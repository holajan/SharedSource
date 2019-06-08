using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Configuration;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IMP.Shared
{
    /// <summary>
    /// Třída pro vytvoření nebo odeslání emailové zprávy pro šablonu
    /// </summary>
    /// <remarks>
    /// <para>Pro odeslání zprávy je nutné provést nastavení parametrů SMTP klienta v app nebo web.config souboru aplikace.</para>
    /// </remarks>
    /// <example>
    /// <para>Příklad konfigurace parametrů pro odesílání emailů:</para>
    /// <code>
    /// &lt;system.net&gt;
    ///   &lt;mailSettings&gt;
    ///     &lt;smtp deliveryMethod="Network" from="noreply@domena.cz"&gt;
    ///       &lt;network
    ///         host="localhost"
    ///         port="25"
    ///         defaultCredentials="false"
    ///       /&gt;
    ///     &lt;/smtp&gt;
    ///   &lt;/mailSettings&gt;
    /// &lt;/system.net&gt;
    /// </code>
    /// </example>
    internal class MailDefinition
    {
        #region member types definition
        private enum EmailReplacementType
        {
            From,
            To,
            CC,
            Bcc
        }
        #endregion

        #region member varible and default property initialization
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string BodyFileName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string From { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string CC { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Bcc { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Subject { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool IsBodyHtml { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public MailPriority Priority { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public System.Globalization.CultureInfo Culture { get; set; }

        private List<EmbeddedMailObject> m_EmbeddedObjects;
        #endregion

        #region action methods
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public MailMessage CreateMailMessage(string recipients, ReplacementCollection replacements)
        {
            return CreateMailMessage((string)null, recipients, replacements, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public MailMessage CreateMailMessage(string recipients, ReplacementCollection replacements, IEnumerable<Attachment> attachments)
        {
            return CreateMailMessage((string)null, recipients, replacements, attachments);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public MailMessage CreateMailMessage(string body, string recipients, ReplacementCollection replacements)
        {
            return CreateMailMessage(body, recipients, replacements, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public MailMessage CreateMailMessage(string body, string recipients, ReplacementCollection replacements, IEnumerable<Attachment> attachments)
        {
            if (body == null && !string.IsNullOrEmpty(this.BodyFileName))
            {
                body = File.ReadAllText(GetBodyFileNameForCulture());
            }

            string from = this.From;
            if (string.IsNullOrEmpty(from))
            {
                from = GetDefaultFrom();
            }
            from = ApplyReplacements(from, replacements);

            if (recipients != null)
            {
                recipients = ApplyEmailReplacement(ApplyReplacements(recipients.Replace(';', ','), replacements), from, EmailReplacementType.From);
            }

            string cc = this.CC;
            if (cc != null)
            {
                cc = ApplyEmailReplacement(ApplyReplacements(cc.Replace(';', ','), replacements), from, EmailReplacementType.From);
            }

            string bcc = this.Bcc;
            if (bcc != null)
            {
                bcc = ApplyEmailReplacement(ApplyReplacements(bcc.Replace(';', ','), replacements), from, EmailReplacementType.From);
            }

            if (!string.IsNullOrEmpty(body))
            {
                //Body replacements
                body = ApplyReplacements(body, replacements, this.IsBodyHtml);
                body = ApplyEmailReplacement(body, from, EmailReplacementType.From, this.IsBodyHtml);
                body = ApplyEmailReplacement(body, recipients, EmailReplacementType.To, this.IsBodyHtml);
                body = ApplyEmailReplacement(body, cc, EmailReplacementType.CC, this.IsBodyHtml);
                body = ApplyEmailReplacement(body, bcc, EmailReplacementType.Bcc, this.IsBodyHtml);
            }

            var message = new MailMessage() { From = new MailAddress(from), IsBodyHtml = this.IsBodyHtml, Priority = this.Priority };

            try
            {
                foreach (var address in ParseRecipients(recipients))
                {
                    message.To.Add(address);
                }
                foreach (var address in ParseRecipients(cc))
                {
                    message.CC.Add(address);
                }
                foreach (var address in ParseRecipients(bcc))
                {
                    message.Bcc.Add(address);
                }

                string subject = this.Subject;
                if (string.IsNullOrEmpty(subject) && this.IsBodyHtml)
                {
                    subject = ExtractSubjectFromHtmlBody(body);
                }

                if (!string.IsNullOrEmpty(subject))
                {
                    //Subject replacements
                    subject = ApplyReplacements(subject, replacements);
                    subject = ApplyEmailReplacement(subject, from, EmailReplacementType.From);
                    subject = ApplyEmailReplacement(subject, recipients, EmailReplacementType.To);
                    subject = ApplyEmailReplacement(subject, cc, EmailReplacementType.CC);
                    subject = ApplyEmailReplacement(subject, bcc, EmailReplacementType.Bcc);

                    message.Subject = subject;
                }

                if (m_EmbeddedObjects != null && m_EmbeddedObjects.Count != 0)
                {
                    message.AlternateViews.Add(GetAlternateView(body));
                }
                else if (!string.IsNullOrEmpty(body))
                {
                    message.Body = body;
                }

                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        message.Attachments.Add(attachment);
                    }
                }
            }
            catch
            {
                message.Dispose();
                throw;
            }

            return message;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public MailMessage CreateMailMessage(Stream body, string recipients, ReplacementCollection replacements)
        {
            return CreateMailMessage(body, recipients, replacements, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public MailMessage CreateMailMessage(Stream body, string recipients, ReplacementCollection replacements, IEnumerable<Attachment> attachments)
        {
            string bodyText = null;
            if (body != null)
            {
                using (var reader = new StreamReader(body))
                {
                    bodyText = reader.ReadToEnd();
                }
            }

            return CreateMailMessage(bodyText, recipients, replacements, attachments);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void Send(string recipients, ReplacementCollection replacements)
        {
            using (var msg = CreateMailMessage((string)null, recipients, replacements, null))
            {
                var client = new SmtpClient();
                client.Send(msg);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void Send(string recipients, ReplacementCollection replacements, IEnumerable<Attachment> attachments)
        {
            using (var msg = CreateMailMessage((string)null, recipients, replacements, attachments))
            {
                var client = new SmtpClient();
                client.Send(msg);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void Send(string body, string recipients, ReplacementCollection replacements)
        {
            using (var msg = CreateMailMessage(body, recipients, replacements, null))
            {
                var client = new SmtpClient();
                client.Send(msg);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void Send(string body, string recipients, ReplacementCollection replacements, IEnumerable<Attachment> attachments)
        {
            using (var msg = CreateMailMessage(body, recipients, replacements, attachments))
            {
                var client = new SmtpClient();
                client.Send(msg);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void Send(Stream body, string recipients, ReplacementCollection replacements)
        {
            using (var msg = CreateMailMessage(body, recipients, replacements, null))
            {
                var client = new SmtpClient();
                client.Send(msg);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void Send(Stream body, string recipients, ReplacementCollection replacements, IEnumerable<Attachment> attachments)
        {
            using (var msg = CreateMailMessage(body, recipients, replacements, attachments))
            {
                var client = new SmtpClient();
                client.Send(msg);
            }
        }
        #endregion

        #region property getters/setters
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public List<EmbeddedMailObject> EmbeddedObjects
        {
            get
            {
                if (m_EmbeddedObjects == null)
                {
                    m_EmbeddedObjects = new List<EmbeddedMailObject>();
                }
                return m_EmbeddedObjects;
            }
            set { m_EmbeddedObjects = value; }
        }
        #endregion

        #region private member functions
        private static string ApplyReplacements(string str, ReplacementCollection replacements, bool IsHtml)
        {
            if (!string.IsNullOrEmpty(str) && replacements != null)
            {
                foreach (var Item in replacements)
                {
                    string Name = Item.Name.StartsWith("<%", StringComparison.Ordinal) ? Item.Name : "<%" + Item.Name + "%>";
                    string Value = Item.Value ?? "";

                    if (IsHtml)
                    {
                        if (Item.HtmlEncoding)
                        {
                            Value = Regex.Replace(System.Web.HttpUtility.HtmlEncode(Value), "\r\n|\r|\n", "<br />");
                        }

                        str = Regex.Replace(str, System.Web.HttpUtility.HtmlEncode(Name), Value, RegexOptions.IgnoreCase);
                    }

                    str = Regex.Replace(str, Name, Value, RegexOptions.IgnoreCase);
                }
            }
            return str;
        }

        private static string ApplyReplacements(string str, ReplacementCollection replacements)
        {
            return ApplyReplacements(str, replacements, false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static string ApplyEmailReplacement(string str, string addresses, EmailReplacementType type, bool isHtml)
        {
            if (!string.IsNullOrEmpty(str))
            {
                switch (type)
                {
                    case EmailReplacementType.From:
                        MailAddress from = null;
                        if (addresses != null)
                        {
                            from = new MailAddress(addresses);
                        }

                        str = Regex.Replace(str, "<%From%>", from == null ? "" : from.Address, RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%FromDisplayName%>", from == null ? "" : string.IsNullOrEmpty(from.DisplayName) ? from.Address : from.DisplayName, RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%FromFullName%>", addresses ?? "", RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%FromFullAddress%>", addresses ?? "", RegexOptions.IgnoreCase);

                        if (isHtml)
                        {
                            str = Regex.Replace(str, "&lt;%From%&gt;", from == null ? "" : from.Address, RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%FromDisplayName%&gt;", from == null ? "" : string.IsNullOrEmpty(from.DisplayName) ? from.Address : from.DisplayName, RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%FromFullName%&gt;", addresses ?? "", RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%FromFullAddress%&gt;", addresses ?? "", RegexOptions.IgnoreCase);
                        }
                        break;
                    case EmailReplacementType.To:
                        var to = ParseRecipients(addresses);

                        str = Regex.Replace(str, "<%Recipients%>", string.Join(",", (from i in to select i.Address).ToArray()), RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%RecipientsDisplayName%>", string.Join(",", (from i in to select string.IsNullOrEmpty(i.DisplayName) ? i.Address : i.DisplayName).ToArray()), RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%RecipientsFullName%>", addresses ?? "", RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%RecipientsAddress%>", addresses ?? "", RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%To%>", string.Join(",", (from i in to select i.Address).ToArray()), RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%ToDisplayName%>", string.Join(",", (from i in to select string.IsNullOrEmpty(i.DisplayName) ? i.Address : i.DisplayName).ToArray()), RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%ToFullName%>", addresses ?? "", RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%ToFullAddress%>", addresses ?? "", RegexOptions.IgnoreCase);

                        if (isHtml)
                        {
                            str = Regex.Replace(str, "&lt;%Recipients%&gt;", string.Join(",", (from i in to select i.Address).ToArray()), RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%RecipientsDisplayName%&gt;", string.Join(",", (from i in to select string.IsNullOrEmpty(i.DisplayName) ? i.Address : i.DisplayName).ToArray()), RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%RecipientsFullName%&gt;", addresses ?? "", RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%RecipientsFullAddress%&gt;", addresses ?? "", RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%To%&gt;", string.Join(",", (from i in to select i.Address).ToArray()), RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%ToDisplayName%&gt;", string.Join(",", (from i in to select string.IsNullOrEmpty(i.DisplayName) ? i.Address : i.DisplayName).ToArray()), RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%ToFullName%&gt;", addresses ?? "", RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%ToFullAddress%&gt;", addresses ?? "", RegexOptions.IgnoreCase);
                        }
                        break;
                    case EmailReplacementType.CC:
                        var cc = ParseRecipients(addresses);

                        str = Regex.Replace(str, "<%CC%>", string.Join(",", (from i in cc select i.Address).ToArray()), RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%CCDisplayName%>", string.Join(",", (from i in cc select string.IsNullOrEmpty(i.DisplayName) ? i.Address : i.DisplayName).ToArray()), RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%CCFullName%>", addresses ?? "", RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%CCFullAddress%>", addresses ?? "", RegexOptions.IgnoreCase);

                        if (isHtml)
                        {
                            str = Regex.Replace(str, "&lt;%CC%&gt;", string.Join(",", (from i in cc select i.Address).ToArray()), RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%CCDisplayName%&gt;", string.Join(",", (from i in cc select string.IsNullOrEmpty(i.DisplayName) ? i.Address : i.DisplayName).ToArray()), RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%CCFullName%&gt;", addresses ?? "", RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%CCFullAddress%&gt;", addresses ?? "", RegexOptions.IgnoreCase);
                        }
                        break;
                    case EmailReplacementType.Bcc:
                        var bcc = ParseRecipients(addresses);

                        str = Regex.Replace(str, "<%Bcc%>", string.Join(",", (from i in bcc select i.Address).ToArray()), RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%BccDisplayName%>", string.Join(",", (from i in bcc select string.IsNullOrEmpty(i.DisplayName) ? i.Address : i.DisplayName).ToArray()), RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%BccFullName%>", addresses ?? "", RegexOptions.IgnoreCase);
                        str = Regex.Replace(str, "<%BccFullAddress%>", addresses ?? "", RegexOptions.IgnoreCase);

                        if (isHtml)
                        {
                            str = Regex.Replace(str, "&lt;%Bcc%&gt;", string.Join(",", (from i in bcc select i.Address).ToArray()), RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%BccDisplayName%&gt;", string.Join(",", (from i in bcc select string.IsNullOrEmpty(i.DisplayName) ? i.Address : i.DisplayName).ToArray()), RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%BccFullName%&gt;", addresses ?? "", RegexOptions.IgnoreCase);
                            str = Regex.Replace(str, "&lt;%BccFullAddress%&gt;", addresses ?? "", RegexOptions.IgnoreCase);
                        }
                        break;
                }
            }

            return str;
        }

        private static string ApplyEmailReplacement(string str, string addresses, EmailReplacementType type)
        {
            return ApplyEmailReplacement(str, addresses, type, false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private AlternateView GetAlternateView(string body)
        {
            string mediaType = this.IsBodyHtml ? "text/html" : "text/plain";
            AlternateView item = AlternateView.CreateAlternateViewFromString(body, null, mediaType);

            foreach (var obj in m_EmbeddedObjects)
            {
                string path = obj.FileName;
                if (string.IsNullOrEmpty(path))
                {
                    throw new InvalidOperationException("Missing FileName in EmbeddedMailObject.");
                }
                if (!Path.IsPathRooted(path) && !string.IsNullOrEmpty(this.BodyFileName))
                {
                    string bodyFileName = GetFullBodyFileName();
                    path = Path.Combine(Path.GetDirectoryName(bodyFileName), path);
                }

                LinkedResource resource = new LinkedResource(path) { ContentId = obj.Name };
                try
                {
                    resource.ContentType.Name = Path.GetFileName(path);
                    item.LinkedResources.Add(resource);
                }
                catch
                {
                    resource.Dispose();
                    throw;
                }
            }
            return item;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static string ExtractSubjectFromHtmlBody(string body)
        {
            if (body == null)
            {
                return null;
            }

            int index = -1;
            int index2 = -1;
            int index3 = -1;
            var sb = new System.Text.StringBuilder();

            foreach (string line in ReadBodyLines(body))
            {
                if (index == -1)
                {
                    index = line.IndexOf("<head>", StringComparison.OrdinalIgnoreCase);

                    if (index == -1)
                    {
                        continue;
                    }
                }

                if (index2 == -1)
                {
                    index2 = line.IndexOf("<title>", index, StringComparison.OrdinalIgnoreCase);

                    if (index2 == -1)
                    {
                        index2 = line.IndexOf("</head>", index, StringComparison.OrdinalIgnoreCase);

                        if (index2 != -1)
                        {
                            break;
                        }
                        continue;
                    }
                }

                index3 = line.IndexOf("</title>", index2, StringComparison.OrdinalIgnoreCase);

                if (index3 != -1)
                {
                    if (index2 != 0)    //line with "<title>"
                    {
                        sb.Append(line.Substring(index2 + "<title>".Length, index3 - index2 - "<title>".Length));
                        break;
                    }

                    sb.Append(line.Substring(0, index3));
                    break;
                }

                if (index2 != 0)    //line with "<title>"
                {
                    sb.Append(line.Substring(index2 + "<title>".Length));
                    index2 = 0;
                    continue;
                }
                sb.Append(line);
            }

            string subject = System.Web.HttpUtility.HtmlDecode(sb.ToString().Trim());
            return subject.Length == 0 ? null : subject;
        }

        private string GetBodyFileNameForCulture()
        {
            string bodyFileName = GetFullBodyFileName();
            string fileName;

            if (this.Culture != null)
            {
                //Jméno souboru dle kultury
                string extension = Path.GetExtension(bodyFileName);

                fileName = Path.ChangeExtension(bodyFileName, this.Culture.Name + (string.IsNullOrEmpty(extension) ? "" : extension));
                if (File.Exists(fileName))
                {
                    return fileName;
                }

                fileName = Path.ChangeExtension(bodyFileName, this.Culture.TwoLetterISOLanguageName + (string.IsNullOrEmpty(extension) ? "" : extension));
                if (File.Exists(fileName))
                {
                    return fileName;
                }
            }

            //Jméno souboru pro výchozí kulturu
            return bodyFileName;
        }

        private string GetFullBodyFileName()
        {
            string path = this.BodyFileName;
            if (!Path.IsPathRooted(path))
            {
                string mailTemplateRoot = System.IO.Path.GetDirectoryName((System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly()).Location);
                path = Path.Combine(mailTemplateRoot, path);
            }

            return path;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static IEnumerable<string> ReadBodyLines(string body)
        {
            using (var reader = new StringReader(body))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        yield break;
                    }

                    yield return line;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static string GetDefaultFrom()
        {
            var smtpCfg = (SmtpSection)System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            if (smtpCfg == null || smtpCfg.Network == null || string.IsNullOrEmpty(smtpCfg.From))
            {
                throw new InvalidOperationException("From address is not specified.");
            }
            return smtpCfg.From;
        }

        private static IEnumerable<MailAddress> ParseRecipients(string addresses)
        {
            if (string.IsNullOrEmpty(addresses))
            {
                yield break;
            }

            for (int i = 0; i < addresses.Length; i++)
            {
                yield return ParseMailAddress(addresses, ref i);
            }
        }

        private static MailAddress ParseMailAddress(string addresses, ref int i)
        {
            int index = addresses.IndexOfAny(new[] { '"', ',' }, i);
            if (index != -1 && addresses[index] == '"')
            {
                index = addresses.IndexOf('"', index + 1);
                if (index == -1)
                {
                    throw new FormatException("Invalid mail address format");
                }

                index = addresses.IndexOf(',', index + 1);
            }

            if (index == -1)
            {
                index = addresses.Length;
            }

            var address = new MailAddress(addresses.Substring(i, index - i).Trim(' ', '\t'));
            i = index;
            return address;
        }
        #endregion
    }

    #region EmbeddedMailObject
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal sealed class EmbeddedMailObject
    {
        #region member varible and default property initialization
        public string Name { get; set; }
        public string FileName { get; set; }
        #endregion

        #region constructors and destructors
        public EmbeddedMailObject(string name, string fileName)
        {
            this.Name = name;
            this.FileName = fileName;
        }
        #endregion
    }
    #endregion

    #region Replacement, ReplacementCollection
    /// <summary>
    /// Dvojice klíč, hodnota pro aplikování do šablony emailové zprávy
    /// </summary>
    internal class Replacement
    {
        #region constructors and destructors
        internal Replacement()
        {
            this.HtmlEncoding = true;
        }
        #endregion

        #region member varible and default property initialization
        /// <summary>
        /// Označení prvku
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Hodnota prvku
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Přiznak zda bude hodnota encodována do těla HTML zprávy
        /// </summary>
        public bool HtmlEncoding { get; set; }
        #endregion

        #region action methods
        /// <summary>
        /// Vrací textovou reprezentaci objektu
        /// </summary>
        /// <returns>Textová reprezentace objektu</returns>
        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0}: {1})", this.Name, this.Value);
        }
        #endregion
    }

    /// <summary>
    /// Kolekce dvojic klíč, hodnota pro aplikování do šablony emailové zprávy
    /// </summary>
    internal class ReplacementCollection : IEnumerable<Replacement>, System.Collections.IEnumerable
    {
        #region member varible and default property initialization
        private Dictionary<string, Replacement> ReplacementsDictionary;
        #endregion

        #region constructors and destructors
        /// <summary>
        /// Výchozí konstruktor třídy <see cref="ReplacementCollection"/>
        /// </summary>
        public ReplacementCollection()
        {
            this.ReplacementsDictionary = new Dictionary<string, Replacement>();
        }
        #endregion

        #region action methods
        /// <summary>
        /// Přidání prvku s klíčem <paramref name="name"/> a hodnotou <paramref name="value"/> do kolekce
        /// </summary>
        /// <param name="name">Označení prvku</param>
        /// <param name="value">Hodnota prvku</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void Add(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException("name is empty string.", "name");
            }

            this.ReplacementsDictionary.Add(name, new Replacement() { Name = name, Value = value });
        }

        /// <summary>
        /// Přidání prvku s klíčem <paramref name="name"/> a hodnotou <paramref name="value"/> do kolekce
        /// </summary>
        /// <param name="name">Označení prvku</param>
        /// <param name="value">Hodnota prvku</param>
        /// <param name="htmlEncoding">Přiznak zda bude hodnota encodována do těla HTML zprávy</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void Add(string name, string value, bool htmlEncoding)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException("name is empty string.", "name");
            }

            this.ReplacementsDictionary.Add(name, new Replacement() { Name = name, Value = value, HtmlEncoding = htmlEncoding });
        }

        /// <summary>
        /// Odstranění prvku s klíčem <paramref name="name"/> s kolekce
        /// </summary>
        /// <param name="name">Označení prvku</param>
        /// <returns><c>true</c> pokud byl prvek k odstranění v kolekci nalezen</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool Remove(string name)
        {
            return this.ReplacementsDictionary.Remove(name);
        }

        /// <summary>
        /// Vrací emunerátor kolekce
        /// </summary>
        /// <returns><see cref="IEnumerator&lt;Replacement&gt;"/> kolekce</returns>
        public IEnumerator<Replacement> GetEnumerator()
        {
            return this.ReplacementsDictionary.Values.GetEnumerator();
        }
        #endregion

        #region property getters/setters
        /// <summary>
        /// Počet prvků kolekce
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public int Count
        {
            get { return this.ReplacementsDictionary.Count; }
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
    #endregion
}