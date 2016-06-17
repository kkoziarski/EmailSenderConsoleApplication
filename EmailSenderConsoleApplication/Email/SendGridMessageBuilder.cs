namespace EmailSenderConsoleApplication.Email
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using System.Text;
    using System.Text.RegularExpressions;

    using SendGrid;

    /// <summary>
    /// Mail Builder
    /// </summary>
    internal sealed class SendGridMessageBuilder
    {
        /// <summary>
        /// TextBody Unsubscribe Test
        /// </summary>
        private static readonly Regex TextUnsubscribeTest = new Regex(@"<%\s*%>", RegexOptions.Compiled);

        /// <summary>
        /// The html unsubscribe test.
        /// </summary>
        private static readonly Regex HtmlUnsubscribeTest = new Regex(@"<%\s*([^\s%]+\s?)+\s*%>", RegexOptions.Compiled);

        /// <summary>
        /// The hide recipients.
        /// </summary>
        private bool hideRecipients;

        /// <summary>
        /// The template id.
        /// </summary>
        private string sendgridTemplateId;

        /// <summary>
        /// The SendGrid message.
        /// </summary>
        private SendGridMessage sendgrid;

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public static SendGridMessageBuilder Create()
        {
            var mailBuilder = new SendGridMessageBuilder();
            mailBuilder.sendgrid = new SendGridMessage();
            return mailBuilder;
        }

        /// <summary>
        /// Builds the message.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessage"/>.
        /// </returns>
        public SendGridMessage Build()
        {
            var exceptions = new List<Exception>();
            if (string.IsNullOrEmpty(this.sendgrid.Html) && string.IsNullOrEmpty(this.sendgrid.Text) && string.IsNullOrEmpty(this.sendgridTemplateId))
            {
                exceptions.Add(new InvalidOperationException("Mail does not contain a body."));
            }

            if (this.sendgrid.To.Length == 0)
            {
                exceptions.Add(new InvalidOperationException("Mail does not have any recipients."));
            }

            if (this.sendgrid.From == null)
            {
                exceptions.Add(new InvalidOperationException("Mail does not have a valid sender's email address."));
            }

            if (string.IsNullOrEmpty(this.sendgrid.Subject))
            {
                exceptions.Add(new InvalidOperationException("Mail does not have a subject."));
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(
                    "Mail has one or more issues and cannot be built.  Check InnerExceptions for details.",
                    exceptions);
            }

            if (this.hideRecipients)
            {
                this.sendgrid.Header.SetTo(ToListString(this.sendgrid.To));
                this.sendgrid.To = new[] { this.sendgrid.From };
            }

            return this.sendgrid;
        }

        /// <summary>
        /// The hide recipients.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder HideRecipients()
        {
            this.hideRecipients = true;
            return this;
        }

        /// <summary>
        /// The from.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder From(MailAddress address)
        {
            this.sendgrid.From = address;
            return this;
        }

        /// <summary>
        /// The from.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder From(string email)
        {
            this.sendgrid.From = new MailAddress(email);
            return this;
        }

        /// <summary>
        /// The from.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="displayName">
        /// The display name.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder From(string email, string displayName)
        {
            this.sendgrid.From = new MailAddress(email, displayName);
            return this;
        }

        /// <summary>
        /// The to.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder To(MailAddress address)
        {
            return this.To(address.Address, address.DisplayName);
        }

        /// <summary>
        /// The to.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder To(string email)
        {
            this.sendgrid.AddTo(email);
            return this;
        }

        /// <summary>
        /// The to.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="displayName">
        /// The display name.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder To(string email, string displayName)
        {
            this.sendgrid.AddTo(EmailFormat(displayName, email));
            return this;
        }

        /// <summary>
        /// The to.
        /// </summary>
        /// <param name="addresses">
        /// The addresses.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder To(IEnumerable<string> addresses)
        {
            this.sendgrid.AddTo(addresses);
            return this;
        }

        /// <summary>
        /// The to.
        /// </summary>
        /// <param name="addresses">
        /// The addresses.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder To(IEnumerable<MailAddress> addresses)
        {
            return this.To(ToListString(addresses));
        }

        /// <summary>
        /// The to.
        /// </summary>
        /// <param name="addresses">
        /// The addresses.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder To(MailAddressCollection addresses)
        {
            return this.To(addresses.ToList());
        }

        /// <summary>
        /// The cc.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Cc(MailAddress address)
        {
            var currentCc = new List<MailAddress>(this.sendgrid.Cc);
            currentCc.Add(address);
            this.sendgrid.Cc = currentCc.ToArray();
            return this;
        }

        /// <summary>
        /// The cc.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Cc(string email)
        {
            return this.Cc(new MailAddress(email));
        }

        /// <summary>
        /// The cc.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="displayName">
        /// The display name.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Cc(string email, string displayName)
        {
            return this.Cc(new MailAddress(email, displayName));
        }

        /// <summary>
        /// The cc.
        /// </summary>
        /// <param name="addresses">
        /// The addresses.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Cc(IEnumerable<string> addresses)
        {
            var newAddresses = addresses.ToList().ConvertAll(x => { return new MailAddress(x); });
            return this.Cc(newAddresses);
        }

        /// <summary>
        /// The cc.
        /// </summary>
        /// <param name="addresses">
        /// The addresses.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Cc(IEnumerable<MailAddress> addresses)
        {
            var currentCc = new List<MailAddress>(this.sendgrid.Cc);
            currentCc.AddRange(addresses);
            this.sendgrid.Cc = currentCc.ToArray();
            return this;
        }

        /// <summary>
        /// The cc.
        /// </summary>
        /// <param name="addresses">
        /// The addresses.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Cc(MailAddressCollection addresses)
        {
            return this.Cc(addresses.ToList());
        }

        /// <summary>
        /// The bcc.
        /// </summary>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Bcc(MailAddress address)
        {
            var currentBcc = new List<MailAddress>(this.sendgrid.Bcc);
            currentBcc.Add(address);
            this.sendgrid.Bcc = currentBcc.ToArray();
            return this;
        }

        /// <summary>
        /// The bcc.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Bcc(string email)
        {
            return this.Bcc(new MailAddress(email));
        }

        /// <summary>
        /// The bcc.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="displayName">
        /// The display name.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Bcc(string email, string displayName)
        {
            return this.Bcc(new MailAddress(email, displayName));
        }

        /// <summary>
        /// The bcc.
        /// </summary>
        /// <param name="addresses">
        /// The addresses.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Bcc(IEnumerable<string> addresses)
        {
            var newAddresses = addresses.ToList().ConvertAll(x => { return new MailAddress(x); });
            return this.Bcc(newAddresses);
        }

        /// <summary>
        /// The bcc.
        /// </summary>
        /// <param name="addresses">
        /// The addresses.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Bcc(IEnumerable<MailAddress> addresses)
        {
            var currentBcc = new List<MailAddress>(this.sendgrid.Bcc);
            currentBcc.AddRange(addresses);
            this.sendgrid.Bcc = currentBcc.ToArray();
            return this;
        }

        /// <summary>
        /// The bcc.
        /// </summary>
        /// <param name="addresses">
        /// The addresses.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Bcc(MailAddressCollection addresses)
        {
            return this.Bcc(addresses.ToList());
        }

        /// <summary>
        /// The email subject.
        /// </summary>
        /// <param name="subject">
        /// The subject.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Subject(string subject)
        {
            this.sendgrid.Subject = subject;
            return this;
        }

        /// <summary>
        /// The html.
        /// </summary>
        /// <param name="view">
        /// The view.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder HtmlBody(AlternateView view)
        {
            return this.HtmlBody(this.GetAlternateViewAsString(view)).EmbedImages(view.LinkedResources);
        }

        /// <summary>
        /// Sets HTML body.
        /// </summary>
        /// <param name="html">
        /// The html email body.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder HtmlBody(string html)
        {
            this.sendgrid.Html = html;
            return this;
        }

        /// <summary>
        /// The text.
        /// </summary>
        /// <param name="view">
        /// The view.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder TextBody(AlternateView view)
        {
            return this.TextBody(this.GetAlternateViewAsString(view));
        }

        /// <summary>
        /// Sets email text.
        /// </summary>
        /// <param name="text">
        /// The email text.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder TextBody(string text)
        {
            this.sendgrid.Text = text;
            return this;
        }

        /// <summary>
        /// The attach file.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder AttachFile(Stream stream, string name)
        {
            this.sendgrid.AddAttachment(stream, name);
            return this;
        }

        /// <summary>
        /// The attach file.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder AttachFile(string filePath)
        {
            this.sendgrid.AddAttachment(filePath);
            return this;
        }

        /// <summary>
        /// The attach file.
        /// </summary>
        /// <param name="attachment">
        /// The attachment.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder AttachFile(Attachment attachment)
        {
            return this.AttachFile(attachment.ContentStream, attachment.Name);
        }

        /// <summary>
        /// The attach files.
        /// </summary>
        /// <param name="attachments">
        /// The attachments.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder AttachFiles(IEnumerable<Attachment> attachments)
        {
            foreach (var attachment in attachments)
            {
                this.AttachFile(attachment);
            }

            return this;
        }

        /// <summary>
        /// The attach files.
        /// </summary>
        /// <param name="attachments">
        /// The attachments.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder AttachFiles(AttachmentCollection attachments)
        {
            return this.AttachFiles(attachments.ToList());
        }

        /// <summary>
        /// The embed image.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="cid">
        /// The cid.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EmbedImage(Stream stream, string name, string cid)
        {
            this.sendgrid.AddAttachment(stream, name);
            this.sendgrid.EmbedImage(name, cid);
            return this;
        }

        /// <summary>
        /// The embed image.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <param name="cid">
        /// The cid.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EmbedImage(string filePath, string cid)
        {
            this.sendgrid.AddAttachment(filePath);
            this.sendgrid.EmbedImage(new FileInfo(filePath).Name, cid);
            return this;
        }

        /// <summary>
        /// The embed image.
        /// </summary>
        /// <param name="resource">
        /// The resource.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EmbedImage(LinkedResource resource)
        {
            return this.EmbedImage(resource.ContentStream, resource.ContentId, resource.ContentId);
        }

        /// <summary>
        /// The embed images.
        /// </summary>
        /// <param name="resources">
        /// The resources.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EmbedImages(IEnumerable<LinkedResource> resources)
        {
            foreach (var resource in resources)
            {
                this.EmbedImage(resource);
            }

            return this;
        }

        /// <summary>
        /// The embed images.
        /// </summary>
        /// <param name="resources">
        /// The resources.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EmbedImages(LinkedResourceCollection resources)
        {
            return this.EmbedImages(resources.ToList());
        }

        /// <summary>
        /// The add header.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder AddHeader(string key, string value)
        {
            this.sendgrid.AddHeaders(new Dictionary<string, string> { { key, value } });
            return this;
        }

        /// <summary>
        /// The add headers.
        /// </summary>
        /// <param name="headers">
        /// The headers.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder AddHeaders(IDictionary<string, string> headers)
        {
            this.sendgrid.AddHeaders(headers);
            return this;
        }

        /// <summary>
        /// The template substitute.
        /// </summary>
        /// <param name="replacementTag">
        /// The replacement tag.
        /// </param>
        /// <param name="substitutionValues">
        /// The substitution values.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Substitute(string replacementTag, IEnumerable<string> substitutionValues)
        {
            this.sendgrid.AddSubstitution(replacementTag, substitutionValues.ToList());
            return this;
        }

        /// <summary>
        /// The template substitute.
        /// </summary>
        /// <param name="replacementTag">
        /// The replacement tag.
        /// </param>
        /// <param name="substitutionValue">
        /// The substitution value.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Substitute(string replacementTag, string substitutionValue)
        {
            return this.Substitute(replacementTag, new List<string> { substitutionValue });
        }

        /// <summary>
        /// The substitute.
        /// </summary>
        /// <param name="substitutePair">
        /// The substitute pair.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder Substitute(KeyValuePair<string, string> substitutePair)
        {
            return this.Substitute(substitutePair.Key, substitutePair.Value);
        }

        /// <summary>
        /// The include unique arguments.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder IncludeUniqueArg(string key, string value)
        {
            this.sendgrid.AddUniqueArgs(new Dictionary<string, string> { { key, value } });
            return this;
        }

        /// <summary>
        /// The include unique args.
        /// </summary>
        /// <param name="identifiers">
        /// The identifiers.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder IncludeUniqueArgs(IDictionary<string, string> identifiers)
        {
            this.sendgrid.AddUniqueArgs(identifiers);
            return this;
        }

        /// <summary>
        /// The set category.
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder SetCategory(string category)
        {
            this.sendgrid.SetCategory(category);
            return this;
        }

        /// <summary>
        /// The set categories.
        /// </summary>
        /// <param name="categories">
        /// The categories.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder SetCategories(IEnumerable<string> categories)
        {
            this.sendgrid.SetCategories(categories);
            return this;
        }

        /// <summary>
        /// The enable gravatar.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EnableGravatar()
        {
            this.sendgrid.EnableGravatar();
            return this;
        }

        /// <summary>
        /// The enable open tracking.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EnableOpenTracking()
        {
            this.sendgrid.EnableOpenTracking();
            return this;
        }

        /// <summary>
        /// The enable click tracking.
        /// </summary>
        /// <param name="includePlainText">
        /// The include plain text.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EnableClickTracking(bool includePlainText = false)
        {
            this.sendgrid.EnableClickTracking(includePlainText);
            return this;
        }

        /// <summary>
        /// The enable spam check.
        /// </summary>
        /// <param name="score">
        /// The score.
        /// </param>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EnableSpamCheck(int score = 5, string url = null)
        {
            this.sendgrid.EnableSpamCheck(score, url);
            return this;
        }

        /// <summary>
        /// Enables unsubscribing.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EnableUnsubscribe(string text, string html)
        {
            // TODO: Because of a bug in SendGrid 3.0.0, we are directly running the corrected EnableUnsubscribe(text, html) code.  Remove once the new build of SendGrid is available.
            // this.sendgrid.EnableUnsubscribe(text, html);
            var filter = "subscriptiontrack";

            if (!TextUnsubscribeTest.IsMatch(text))
            {
                throw new Exception("Missing substitution replacementTag in text");
            }

            if (!HtmlUnsubscribeTest.IsMatch(html))
            {
                throw new Exception("Missing substitution replacementTag in html");
            }

            this.sendgrid.Header.EnableFilter(filter);
            this.sendgrid.Header.AddFilterSetting(filter, new List<string> { "text/plain" }, text);
            this.sendgrid.Header.AddFilterSetting(filter, new List<string> { "text/html" }, html);

            return this;
        }

        /// <summary>
        /// The enable unsubscribe.
        /// </summary>
        /// <param name="replace">
        /// The replace.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EnableUnsubscribe(string replace)
        {
            this.sendgrid.EnableUnsubscribe(replace);
            return this;
        }

        /// <summary>
        /// The enable footer.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EnableFooter(string text = null, string html = null)
        {
            this.sendgrid.EnableFooter(text, html);
            return this;
        }

        /// <summary>
        /// The enable google analytics.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="medium">
        /// The medium.
        /// </param>
        /// <param name="term">
        /// The term.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <param name="campaign">
        /// The campaign.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EnableGoogleAnalytics(
            string source,
            string medium,
            string term,
            string content = null,
            string campaign = null)
        {
            this.sendgrid.EnableGoogleAnalytics(source, medium, term, content, campaign);
            return this;
        }

        /// <summary>
        /// The enable template.
        /// </summary>
        /// <param name="html">
        /// The html.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EnableTemplate(string html)
        {
            this.sendgrid.EnableTemplate(html);
            return this;
        }

        /// <summary>
        /// The enable template engine.
        /// </summary>
        /// <param name="templateId">
        /// The template id.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EnableTemplateEngine(string templateId)
        {
            this.sendgridTemplateId = templateId;
            this.sendgrid.EnableTemplateEngine(templateId);
            return this;
        }

        /// <summary>
        /// The enable bcc.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EnableBcc(string email)
        {
            this.sendgrid.EnableBcc(email);
            return this;
        }

        /// <summary>
        /// The enable bypass list management.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder EnableBypassListManagement()
        {
            this.sendgrid.EnableBypassListManagement();
            return this;
        }

        /// <summary>
        /// The disable gravatar.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder DisableGravatar()
        {
            this.sendgrid.DisableGravatar();
            return this;
        }

        /// <summary>
        /// The disable open tracking.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder DisableOpenTracking()
        {
            this.sendgrid.DisableOpenTracking();
            return this;
        }

        /// <summary>
        /// The disable click tracking.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder DisableClickTracking()
        {
            this.sendgrid.DisableClickTracking();
            return this;
        }

        /// <summary>
        /// The disable spam check.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder DisableSpamCheck()
        {
            this.sendgrid.DisableSpamCheck();
            return this;
        }

        /// <summary>
        /// The disable unsubscribe.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder DisableUnsubscribe()
        {
            this.sendgrid.DisableUnsubscribe();
            return this;
        }

        /// <summary>
        /// The disable footer.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder DisableFooter()
        {
            this.sendgrid.DisableFooter();
            return this;
        }

        /// <summary>
        /// The disable google analytics.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder DisableGoogleAnalytics()
        {
            this.sendgrid.DisableGoogleAnalytics();
            return this;
        }

        /// <summary>
        /// The disable template.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder DisableTemplate()
        {
            this.sendgrid.DisableTemplate();
            return this;
        }

        /// <summary>
        /// The disable bcc.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder DisableBcc()
        {
            this.sendgrid.DisableBcc();
            return this;
        }

        /// <summary>
        /// The disable bypass list management.
        /// </summary>
        /// <returns>
        /// The <see cref="SendGridMessageBuilder"/>.
        /// </returns>
        public SendGridMessageBuilder DisableBypassListManagement()
        {
            this.sendgrid.DisableBypassListManagement();
            return this;
        }

        /// <summary>
        /// The email format.
        /// </summary>
        /// <param name="displayName">
        /// The display name.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string EmailFormat(string displayName, string email)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} <{1}>", displayName, email);
        }

        /// <summary>
        /// The to list string.
        /// </summary>
        /// <param name="addresses">
        /// The addresses.
        /// </param>
        /// <returns>
        /// The list of MailAddress/>.
        /// </returns>
        private static IEnumerable<string> ToListString(IEnumerable<MailAddress> addresses)
        {
            return
                addresses.ToList()
                    .ConvertAll(
                        address =>
                        string.Format(
                            CultureInfo.InvariantCulture,
                            string.IsNullOrWhiteSpace(address.DisplayName) ? "{1}" : "{0} <{1}>",
                            address.DisplayName,
                            address.Address));
        }

        /// <summary>
        /// The get alternate view as string.
        /// </summary>
        /// <param name="view">
        /// The view.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetAlternateViewAsString(AlternateView view)
        {
            var dataStream = view.ContentStream;
            var byteBuffer = new byte[dataStream.Length];
            var encoding = Encoding.GetEncoding(view.ContentType.CharSet);
            return encoding.GetString(byteBuffer, 0, dataStream.Read(byteBuffer, 0, byteBuffer.Length));
        }
    }
}