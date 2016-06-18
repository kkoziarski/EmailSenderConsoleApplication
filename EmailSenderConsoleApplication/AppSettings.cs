namespace EmailSenderConsoleApplication
{
    using System.Configuration;

    public static class AppSettings
    {
        public static string SmtpServer
        {
            get
            {
                return ConfigurationManager.AppSettings["SmtpServer"];
            }
        }

        public static string DefaultEmailFrom
        {
            get
            {
                return ConfigurationManager.AppSettings["DefaultEmailFrom"];
            }
        }

        public static string DefaultEmailTo
        {
            get
            {
                return ConfigurationManager.AppSettings["DefaultEmailTo"];
            }
        }

        public static string DefaultDisplayName
        {
            get
            {
                return ConfigurationManager.AppSettings["DefaultDisplayName"];
            }
        }

        public static string SendGridUserName
        {
            get
            {
                return ConfigurationManager.AppSettings["SendGridUserName"];
            }
        }

        public static string SendGridPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["SendGridPassword"];
            }
        }
    }
}