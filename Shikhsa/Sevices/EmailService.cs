
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Shikhsa.Data;
using Shikhsa.Models.Common;
using Shikhsa.ViewModels;

public class EmailService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public EmailService(
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<bool> SendEmailAsync( string moduleName, long? referenceId,string toEmail,string subject,string body, List<(string FileName, byte[] Content, string ContentType)>? attachments = null,
    string? ccEmail = null)
    {
        var log = new EmailLog
        {
            ModuleName = moduleName,
            ReferenceId = referenceId,
            ToEmail = toEmail,
            Subject = subject,
            Body = body,
            Status = EmailStatus.Pending,
            CreatedOn = DateTime.Now,
            IsActive = true
        };

        _context.EmailLogs.Add(log);
        await _context.SaveChangesAsync();

        try
        {
            // First Try Zoho
            var sent = await SendUsingProvider(
                "EmailSettings:Zoho",
                toEmail,
                subject,
                body,
                attachments,
                ccEmail);

            // If Zoho fails then Gmail
            if (!sent)
            {
                sent = await SendUsingProvider(
                    "EmailSettings:Gmail",
                    toEmail,
                    subject,
                    body,
                    attachments,
                ccEmail);
            }

            if (sent)
            {
                log.Status = EmailStatus.Sent;
                log.SentOn = DateTime.Now;

                await _context.SaveChangesAsync();

                return true;
            }

            log.Status = EmailStatus.Failed;
            log.FailedOn = DateTime.Now;

            await _context.SaveChangesAsync();

            return false;
        }
        catch (Exception ex)
        {
            log.Status = EmailStatus.Failed;
            log.FailedOn = DateTime.Now;
            log.ErrorMessage = ex.ToString();
            log.RetryCount++;

            await _context.SaveChangesAsync();

            return false;
        }

    }
    private async Task<bool> SendUsingProvider(string providerSection,string toEmail,string subject,string body, List<(string FileName, byte[] Content, string ContentType)>? attachments = null,
        string? ccEmail = null)
    {
        try
        {
            var settings = new EmailSettings();

            _configuration.GetSection(providerSection).Bind(settings);

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(settings.FromName,settings.FromEmail));

            message.To.Add(MailboxAddress.Parse(toEmail));
            if (!string.IsNullOrWhiteSpace(ccEmail))
            {
                message.Cc.Add(MailboxAddress.Parse(ccEmail));
            }
            message.Subject = subject;

            //message.Body = new TextPart("html")
            //{
            //    Text = body
            //};

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            if (attachments != null)
            {
                foreach (var att in attachments)
                {
                    builder.Attachments.Add(
                        att.FileName,
                        att.Content,
                        MimeKit.ContentType.Parse(att.ContentType));   // fully-qualified, System.Net.Mime clash na ho
                }
            }
            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync( settings.Host,settings.Port,SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(settings.UserName,settings.Password);

            await smtp.SendAsync(message);

            await smtp.DisconnectAsync(true);

            return true;
        }
        catch
        {
            return false;
        }
    }
}