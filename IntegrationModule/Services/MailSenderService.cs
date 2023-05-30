using DAL.Models;
using System.Net.Mail;

namespace IntegrationModule.Services
{
    public class MailSenderService : IHostedService, IDisposable
    {
        private Timer? timer;
        private readonly IConfiguration configuration;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<MailSenderService> logger;

        public MailSenderService(ILogger<MailSenderService> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            int interval = configuration.GetValue<int>("MailSender:timerInterval");
            timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(interval));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            int? count = configuration.GetValue<int>("MailSender:count");
            string smtpServer = configuration.GetValue<string>("MailSender:smtpServer");
            int smtpPort = configuration.GetValue<int>("MailSender:smtpPort");
            string sender = configuration.GetValue<string>("MailSender:sender");

            using (var scope = scopeFactory.CreateScope())
            {
                var _dbContext = scope.ServiceProvider.GetRequiredService<RwaMoviesContext>();

                try
                {
                    var client = new SmtpClient(smtpServer, smtpPort);

                    var unsentNotifications =
                        _dbContext.Notifications
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

                            sendSuccessCount++;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError($"Sending failed: {ex.Message}");
                            sendFailCount++;
                        }
                    }

                    _dbContext.SaveChanges();

                    logger.LogInformation($"Success count: {sendSuccessCount}");
                    logger.LogInformation($"Fail count: {sendFailCount}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Batch sending failed: {ex.Message}");
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            timer?.Change(Timeout.Infinite, 0);

            logger.LogInformation("MailSenderService stopped.");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
