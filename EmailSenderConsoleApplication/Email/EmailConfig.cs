namespace EmailSenderConsoleApplication.Email
{
    public class EmailConfig
    {
        public string FromAddress { get; set; }

        public string FromDisplayName { get; set; }

        public string DefaultBccAddress { get; set; }

        public string SupportAddress { get; set; }
    }
}