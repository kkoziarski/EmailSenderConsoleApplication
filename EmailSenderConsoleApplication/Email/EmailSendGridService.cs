namespace EmailSenderConsoleApplication.Email
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    using SendGrid;

    public interface IEmailService
    {
        Task SendEmailAsync(EmailMessage message);
    }

    /// <summary>
    /// EmailSendGridService class
    /// </summary>
    public class EmailSendGridService : IEmailService
    {
        private readonly SmtpEmailConfig emailConfig;

        private readonly Web transportWeb;

        public EmailSendGridService()
            : this(SmtpEmailConfig.CreateDefault())
        {
        }


        public EmailSendGridService(SmtpEmailConfig emailConfig)
        {
            this.emailConfig = emailConfig;

            // Create credentials, specifying your user name and password.
            var credentials = new NetworkCredential(emailConfig.SmtpUserName, emailConfig.SmtpPassword);

            // Create an Web transport for sending email.
            this.transportWeb = new Web(credentials);
        }

        public async Task SendEmailAsync(EmailMessage message)
        {
            // Create the email object first, then add the properties.
            SendGridMessage email = this.CreateEmailMessage(message);

            string path = @"email-" + email.Subject + "-" + DateTime.Now.Ticks + ".html";
            System.IO.File.WriteAllText(path, message.Body);

            await this.SendAsync(email);
        }

        private SendGridMessage CreateEmailMessage(EmailMessage message)
        {
            var mailBuilder = this.BuildBaseEmail(message);

            return mailBuilder.Build();
        }

        private SendGridMessageBuilder BuildBaseEmail(EmailMessage message)
        {
            return SendGridMessageBuilder.Create()
                    .To(message.To)
                    .From(this.emailConfig.FromAddress, this.emailConfig.FromDisplayName)
                    .Subject(message.Subject)
                    .HtmlBody(message.Body ?? string.Empty);

            // classic way, without external builder helper
            /*
            string emailFrom = this.emailConfig.FromAddress;

            var email = new SendGridMessage();

            email.AddTo(message.To);
            email.From = new System.Net.Mail.MailAddress(emailFrom, this.emailConfig.FromDisplayName);
            email.Subject = message.Subject;

            email.Html = message.Body ?? string.Empty;

            email.DisableClickTracking();
            return email;
            */
        }

        private async Task SendAsync(SendGridMessage email)
        {
            if (this.transportWeb != null)
            {
                await this.transportWeb.DeliverAsync(email);
            }
        }
    }
}