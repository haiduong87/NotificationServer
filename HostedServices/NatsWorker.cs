using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationServer.Dto;
using NotificationServer.Miscellaneous;

namespace NotificationServer.HostedServices
{
    public class NatsWorker : BackgroundService
    {
        private readonly Configuration _configuration;
        private readonly NatsConnectionPool _connectionPool;
        private readonly ILogger _logger;

        private readonly ChannelReader<DatabaseNotification> _notificationChannelReader;

        public NatsWorker(Channel<DatabaseNotification> notificationChannel, Configuration configuration,
            ILogger<NatsWorker> logger, NatsConnectionPool connectionPool)
        {
            _notificationChannelReader = notificationChannel;
            _logger = logger;
            _configuration = configuration;
            _connectionPool = connectionPool;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var notification in _notificationChannelReader.ReadAllAsync(stoppingToken)
                .ConfigureAwait(false))
                PushNotification(notification);
        }

        private void PushNotification(DatabaseNotification notification)
        {
            var subject =
                $"{_configuration.NatsSubject}.{notification.Customer.ToLower()}.{notification.Table?.ToLower()}.{notification.Action}";

            var payload = Encoding.UTF8.GetBytes(notification.SerializedString);

            for (var i = 0; i < 3; i++)
                try
                {
                    _connectionPool.Connection.Publish(subject, payload);
                    _configuration.LogObject.Success(notification.Customer);
                    break;
                }
                // ReSharper disable once CatchAllClause
                catch (Exception e)
                {
                    if (i < 2)
                        continue;
                    LogError(notification.Customer,
                        $"Fail to publishing for 3 times: [{notification.SerializedString}], error: {e.GetBaseException().Message}");
                    _configuration.LogObject.Fail(notification.Customer);
                }
        }

        private void LogError(string customer, string message)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["customer"] = customer
            }))
            {
                _logger.LogError(message);
            }
        }
    }
}