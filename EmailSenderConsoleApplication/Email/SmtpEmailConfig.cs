namespace EmailSenderConsoleApplication.Email
{
    public class SmtpEmailConfig : EmailConfig
    {
        public string Smtp { get; set; }

        public string SmtpUserName { get; set; }

        public string SmtpPassword { get; set; }

        public static SmtpEmailConfig CreateDefault()
        {
            return new SmtpEmailConfig
            {
                FromAddress = AppSettings.DefaultEmailFrom,
                FromDisplayName = AppSettings.DefaultDisplayName,
                SmtpUserName = AppSettings.SendGridUserName,
                SmtpPassword = AppSettings.SendGridPassword
            };
        }
    }
}