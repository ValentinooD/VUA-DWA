using DAL.Models;
using IntegrationModule.Models.Requests;
using IntegrationModule.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace IntegrationModule.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly RwaMoviesContext ctx;
        private readonly IConfiguration configuration;

        public NotificationsController(RwaMoviesContext ctx, IConfiguration configuration)
        {
            this.ctx = ctx;
            this.configuration = configuration;
        }

        [HttpGet("[action]")]
        public ActionResult<int> GetUnsentCount()
        {
            return Ok(ctx.Notifications.Where(x => x.SentAt == null).Count());
        }

        [HttpGet("[action]")]
        public ActionResult<IEnumerable<NotificationResponse>> GetAll()
        {
            try
            {
                var allNotifications =
                    ctx.Notifications.Select(dbNotification =>
                    new NotificationResponse
                    {
                        Id = dbNotification.Id,
                        ReceiverEmail = dbNotification.ReceiverEmail,
                        Subject = dbNotification.Subject,
                        Body = dbNotification.Body,
                        SentAt = dbNotification.SentAt
                    });
                return Ok(allNotifications);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<NotificationResponse> Get(int id)
        {
            try
            {
                var dbNotification = ctx.Notifications.FirstOrDefault(x => x.Id == id);
                if (dbNotification == null)
                    return NotFound();

                return Ok(new NotificationResponse
                {
                    Id = dbNotification.Id,
                    ReceiverEmail = dbNotification.ReceiverEmail,
                    Subject = dbNotification.Subject,
                    Body = dbNotification.Body,
                    SentAt = dbNotification.SentAt
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("[action]")]
        public ActionResult<NotificationResponse> Create(NotificationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var dbNotification = new Notification
                {
                    ReceiverEmail = request.ReceiverEmail,
                    Subject = request.Subject,
                    Body = request.Body
                };

                ctx.Notifications.Add(dbNotification);

                ctx.SaveChanges();

                return Ok(new NotificationResponse
                {
                    Id = dbNotification.Id,
                    ReceiverEmail = dbNotification.ReceiverEmail,
                    Subject = dbNotification.Subject,
                    Body = dbNotification.Body
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<NotificationResponse> Remove(int id)
        {
            try
            {
                var dbNotification = ctx.Notifications.FirstOrDefault(x => x.Id == id);
                if (dbNotification == null)
                    return NotFound();

                ctx.Notifications.Remove(dbNotification);

                ctx.SaveChanges();

                return Ok(new NotificationResponse
                {
                    Id = dbNotification.Id,
                    ReceiverEmail = dbNotification.ReceiverEmail,
                    Subject = dbNotification.Subject,
                    Body = dbNotification.Body,
                    SentAt = dbNotification.SentAt
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("[action]")]
        public ActionResult SendAllNotifications()
        {
            var client = new SmtpClient(configuration.GetValue<string>("MailSender:smtpServer"), configuration.GetValue<int>("MailSender:smtpPort"));
            var sender = configuration.GetValue<string>("MailSender:sender");

            int countSuccess = 0;
            int countFailed = 0;

            try
            {
                var unsentNotifications =
                    ctx.Notifications.Where(
                        x => !x.SentAt.HasValue);

                foreach (var notification in unsentNotifications)
                {
                    try
                    {
                        var mail = new MailMessage(
                            from: new MailAddress(sender),
                            to: new MailAddress(notification.ReceiverEmail));

                        mail.Subject = notification.Subject;
                        mail.Body = notification.Body;

                        client.Send(mail);

                        notification.SentAt = DateTime.UtcNow;

                        countSuccess++;
                    }
                    catch (Exception)
                    {
                        countFailed++;
                    }
                }

                ctx.SaveChanges();

                return Ok(new SendNotificationsResponse()
                {
                    SuccessCount = countSuccess,
                    FailCount = countFailed
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("[action]/{count}")]
        public ActionResult<SendNotificationsResponse> SendNotificationBatch(int? count)
        {
            var client = new SmtpClient(configuration.GetValue<string>("MailSender:smtpServer"), configuration.GetValue<int>("MailSender:smtpPort"));
            var sender = configuration.GetValue<string>("MailSender:sender");

            try
            {
                var unsentNotifications =
                    ctx.Notifications
                        .Where(x => !x.SentAt.HasValue)
                        .OrderBy(x => x.CreatedAt)
                        .AsQueryable();

                if (count.HasValue)
                    unsentNotifications = unsentNotifications.Take(count.Value);

                int sendSuccessCount = 0;
                int sendFailCount = 0;
                foreach (var notification in unsentNotifications)
                {
                    try
                    {
                        var mail = new MailMessage(
                            from: new MailAddress(sender),
                            to: new MailAddress(notification.ReceiverEmail));

                        mail.Subject = notification.Subject;
                        mail.Body = notification.Body;

                        client.Send(mail);

                        notification.SentAt = DateTime.UtcNow;
                        ctx.SaveChanges();

                        sendSuccessCount++;
                    }
                    catch (Exception)
                    {
                        sendFailCount++;
                    }
                }

                return Ok(new SendNotificationsResponse
                {
                    SuccessCount = sendSuccessCount,
                    FailCount = sendFailCount
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
