using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace NotificationServer.Miscellaneous
{
    public class Configuration
    {
        public readonly int BatchSize;
        public readonly JsonSerializerOptions JsonSerializerOptions;
        public readonly LogObject LogObject;
        public readonly int NatsConnectionPoolSize;
        public readonly string[] NatsServers;
        public readonly string NatsSubject;
        public readonly bool UseConnectionPool;

        private Configuration()
        {
            LogObject = new LogObject();
            JsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                Converters = {new DateTimeConverter()}
            };
        }

        public Configuration(IConfiguration configuration)
            : this()
        {
            NatsSubject = configuration.GetValue<string>("Settings:Subject");
            BatchSize = configuration.GetValue<int>("Settings:Batch");
            UseConnectionPool = configuration.GetValue<bool>("Settings:UseConnectionPool");
            NatsConnectionPoolSize = configuration.GetValue<int>("Settings:NatsConnectionPoolSize");
            NatsServers = configuration.GetSection("ConnectionStrings:NATSServers").Get<string[]>();
        }
    }

    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"));
        }
    }


    public class LogObject
    {
        private static readonly string ReportHeader = $"{"Customer",-50}{"Success",-20}{"Fail",-20}";

        public readonly ConcurrentDictionary<string, StrongBox<long>> CustomerFailCount =
            new ConcurrentDictionary<string, StrongBox<long>>();

        public readonly ConcurrentDictionary<string, StrongBox<long>> CustomerSuccessCount =
            new ConcurrentDictionary<string, StrongBox<long>>();

        private long _successCount, _failCount;

        public long FailCount => _failCount;
        public long SuccessCount => _successCount;
        public DateTimeOffset StartTime { get; set; }

        public void Success(string customer)
        {
            // ReSharper disable once ExceptionNotDocumented
            Interlocked.Increment(ref _successCount);
            // ReSharper disable once ExceptionNotDocumented
            Interlocked.Increment(ref GetCache(customer, CustomerSuccessCount).Value);
        }

        public void Fail(string customer)
        {
            // ReSharper disable once ExceptionNotDocumented
            Interlocked.Increment(ref _failCount);
            // ReSharper disable once ExceptionNotDocumented
            Interlocked.Increment(ref GetCache(customer, CustomerFailCount).Value);
        }

        private StrongBox<long> GetCache(string customer, ConcurrentDictionary<string, StrongBox<long>> dictionary)
        {
            if (dictionary.ContainsKey(customer)) return dictionary[customer];
            dictionary.TryAdd(customer, new StrongBox<long>(0));
            return dictionary[customer];
        }

        public string GetCustomerStatistic()
        {
            var customers = CustomerSuccessCount.Keys.Union(CustomerSuccessCount.Keys).Distinct()
                .OrderBy(customer => customer);

            // ReSharper disable once ExceptionNotDocumented
            var customerStatistic = string.Join('\n',
                customers.Select(customer =>
                    $"{customer,-50}{(CustomerSuccessCount.ContainsKey(customer) ? CustomerSuccessCount[customer].Value : 0),-20}{(CustomerFailCount.ContainsKey(customer) ? CustomerFailCount[customer].Value : 0),-20}"));
            return $"{ReportHeader}\n{customerStatistic}";
        }
    }
}