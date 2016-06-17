namespace EmailSenderConsoleApplication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using EmailSenderConsoleApplication.Email;

    using ITDM;

    class Program
    {
        //you can invoke it like this: EmailSenderConsoleApplication.exe -help
        // or EmailSenderConsoleApplication.exe "..\..\templates\invoice.html"
        static void Main(string[] args)
        {
            string emailBodyPath;
            EmailMessage emailMessage = null;

            ConsoleCmdLine cmdLine = new ConsoleCmdLine();

            if (args.Length == 1 && !string.Equals(args[0], "-help", StringComparison.OrdinalIgnoreCase))
            {
                emailBodyPath = args[0];
                emailMessage = CreateDefaultEmailMessage(emailBodyPath);
            }
            else
            {
                emailMessage = CreateEmailMessage(cmdLine, args);
            }

            if (emailMessage == null)
            {
                Console.WriteLine("Invalid parameters.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Email message:");
            Console.WriteLine(emailMessage);


            SendEmail(emailMessage);
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void SendEmail(EmailMessage emailMessage)
        {
            IEmailService emailService = new EmailSendGridService();

            Console.WriteLine("Sending email...");

            emailService.SendEmailAsync(emailMessage).Wait();

            Console.WriteLine("Email has been sent");
        }

        private static EmailMessage CreateEmailMessage(ConsoleCmdLine cmdLine, string[] args)
        {
            CmdLineString emailBodyPathParam = new CmdLineString("file", true, "Email body file path.");
            CmdLineString emailAddressParam = new CmdLineString("email", false, "Comma-separated emails: some@example.com;some2@example.com");
            CmdLineString subjectParam = new CmdLineString("subject", false, "Email subject.");
            cmdLine.RegisterParameter(emailBodyPathParam);
            cmdLine.RegisterParameter(emailAddressParam);
            cmdLine.RegisterParameter(subjectParam);
            cmdLine.Parse(args);

            if (!File.Exists(emailBodyPathParam))
            {
                Console.WriteLine("File email body not exists");
                return null;
            }

            string emailBody = File.ReadAllText(emailBodyPathParam);
            var emailMessage = new EmailMessage
            {
                To = GetEmailsOrDefault(emailAddressParam),
                Body = emailBody,
                Subject = subjectParam.Exists ? subjectParam : "Test email"
            };

            return emailMessage;
        }

        private static EmailMessage CreateDefaultEmailMessage(string emailBodyPath)
        {
            if (!File.Exists(emailBodyPath))
            {
                Console.WriteLine("File email body not exists");
                return null;
            }

            string emailBody = File.ReadAllText(emailBodyPath);
            var emailMessage = new EmailMessage
            {
                To = new[] { AppSettings.DefaultEmailTo },
                Body = emailBody,
                Subject = "Test email"
            };

            return emailMessage;
        }

        private static string[] GetEmailsOrDefault(params CmdLineString[] emailParams)
        {
            if (emailParams == null || emailParams.Length == 0)
            {
                return new[] { AppSettings.DefaultEmailTo };
            }

            List<string> emailsResult = new List<string>();
            foreach (string cmdLine in emailParams)
            {
                string[] splitted = cmdLine.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitted.Any())
                {
                    emailsResult.AddRange(splitted);
                }
            }
            if (emailsResult.Any())
            {
                return emailsResult.ToArray();
            }

            return new[] { AppSettings.DefaultEmailTo };
        }
    }
}
