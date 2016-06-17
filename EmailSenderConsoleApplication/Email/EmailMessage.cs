namespace EmailSenderConsoleApplication.Email
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class EmailMessage
    {
        public string Body { get; set; }

        public string Subject { get; set; }

        public IEnumerable<string> To { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Subject: {0}", this.Subject);
            sb.AppendLine();
            sb.AppendLine("Emails to:");
            foreach (var emailTo in this.To.ToArray())
            {
                sb.AppendLine(" - " + emailTo);
            }


            return sb.ToString();
        }
    }
}