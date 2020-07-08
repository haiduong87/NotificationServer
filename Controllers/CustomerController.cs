using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using NotificationServer.Dto;

namespace NotificationServer.Controllers
{
    [Route("customer")]
    public class CustomerController : Controller
    {
        private readonly ChannelWriter<DatabaseNotification> _notificationChannelWriter;

        public CustomerController(Channel<DatabaseNotification> notificationChannel)
        {
            _notificationChannelWriter = notificationChannel.Writer;
        }

        [HttpPost("{customer}")]
        public IActionResult PublishNotificationAsync([FromRoute] [Required] string customer,
            [FromBody] IEnumerable<DatabaseNotification> databaseNotifications)
        {
            if (databaseNotifications == null)
                return BadRequest("Empty payload");

            foreach (var notification in databaseNotifications)
            {
                notification.Customer = customer;
                _notificationChannelWriter.WriteAsync(notification);
            }

            return Ok();
        }
    }
}