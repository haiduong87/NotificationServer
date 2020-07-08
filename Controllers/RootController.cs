using Microsoft.AspNetCore.Mvc;
using NotificationServer.Miscellaneous;

namespace NotificationServer.Controllers
{
    [Route("/")]
    public class RootController : Controller
    {
        private readonly Configuration _configuration;

        public RootController(Configuration configuration, NatsConnectionPool natsConnectionPool)
        {
            _configuration = configuration;
        }

        public string Get()
        {
            return $@"
Service:                NotificationServer
Author:                 Nguyen Hai Duong <duongngh@mycompany.com>
Version:                1.0.0
Start time:             {_configuration.LogObject.StartTime:O}
Total success publish:  {_configuration.LogObject.SuccessCount}
Total fail publish:     {_configuration.LogObject.FailCount}
Customer statistic:
{_configuration.LogObject.GetCustomerStatistic()}";
        }
    }
}