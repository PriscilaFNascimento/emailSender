using Domain.ViewModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Org.BouncyCastle.Asn1.Pkcs;
using RazorEngineCore;
using System.Runtime.CompilerServices;
using System.Text;

namespace Domain.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfigModel _emailConfig;
        public EmailService(IOptions<EmailConfigModel> emailConfigAccessor)
        {
            _emailConfig = emailConfigAccessor.Value;
        }
        public void SendEmail(EmailModel request)
        {
            MimeMessage email = new MimeMessage();

            email.To.Add(MailboxAddress.Parse(request.To));
            email.From.Add(MailboxAddress.Parse(_emailConfig.Username));
            email.Subject = request.Subject;

            var body = new BodyBuilder();
            body.HtmlBody = GetEmailTemplate("CancellationEmail");
            email.Body = body.ToMessageBody();

            //email.Body = new TextPart(TextFormat.Html) { Text = request.Body };

            using var smtp = new SmtpClient();

            smtp.Connect(_emailConfig.Server, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_emailConfig.Username, _emailConfig.Password);
            smtp.Send(email); 
            smtp.Disconnect(true);
        }
        public string GetEmailTemplate(string emailTemplate)
        {
            string mailTemplate = LoadTemplate(emailTemplate);

            IRazorEngine razorEngine = new RazorEngine();
            IRazorEngineCompiledTemplate modifiedMailTemplate = razorEngine.Compile(mailTemplate);

            return modifiedMailTemplate.Run();
        }

        public string LoadTemplate(string emailTemplate)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string templateDir = "C:\\YourPath";
            string templatePath = Path.Combine(templateDir, $"{emailTemplate}.cshtml");

            using FileStream fileStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader streamReader = new StreamReader(fileStream, Encoding.Default);

            string mailTemplate = streamReader.ReadToEnd();
            streamReader.Close();

            return mailTemplate;
        }
    }
}
