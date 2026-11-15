
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Shikhsa.Data;
using Shikhsa.Models.Common;
using Shikhsa.ViewModels;

public class EmailService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IDataProtector _emailProtector;
    public EmailService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IDataProtectionProvider dataProtectionProvider)
    {
        _context = context;
        _configuration = configuration;
        _emailProtector = dataProtectionProvider.CreateProtector("SchoolEmailSetting.Password");
    }
    /*
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
        }*/
    public async Task<bool> SendEmailAsync(string moduleName,long? referenceId,string toEmail,string subject,string body,List<(string FileName, byte[] Content, string ContentType)>? attachments = null,string? ccEmail = null,int? schoolId = null)            // optional: if not passed, the first school is used
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
            schoolId ??= await _context.SchoolMasters
                .Select(s => (int?)s.SchoolId)
                .FirstOrDefaultAsync();

            // Active providers of the school, Priority 1 first
            var providers = await _context.SchoolEmailSettings
                .AsNoTracking()
                .Where(e => e.SchoolId == schoolId && e.IsActive && e.IsEmailActive)
                .OrderBy(e => e.Priority)
                .ToListAsync();

            if (!providers.Any())
            {
                log.Status = EmailStatus.Failed;
                log.FailedOn = DateTime.Now;
                log.ErrorMessage = "No active email provider is configured.";
                await _context.SaveChangesAsync();
                return false;
            }

            var errors = new List<string>();
            bool sent = false;

            foreach (var provider in providers)
            {
                var result = await SendUsingProvider(provider, toEmail, subject, body, attachments, ccEmail);

                if (result.Sent)
                {
                    sent = true;
                    break;                       // sent, no need to try the next provider
                }

                errors.Add($"{provider.ProviderName} (priority {provider.Priority}): {result.Error}");
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
            log.ErrorMessage = string.Join(" | ", errors);

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


    // ---------- 3) SendUsingProvider ----------
    private async Task<(bool Sent, string? Error)> SendUsingProvider(SchoolEmailSetting settings,string toEmail,string subject,string body,List<(string FileName, byte[] Content, string ContentType)>? attachments = null,string? ccEmail = null)
    {
        try
        {
            // password is stored encrypted in the DB
            var password = _emailProtector.Unprotect(settings.Password);

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));

            message.To.Add(MailboxAddress.Parse(toEmail));
            if (!string.IsNullOrWhiteSpace(ccEmail))
            {
                message.Cc.Add(MailboxAddress.Parse(ccEmail));
            }
            message.Subject = subject;

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

            // 465 = implicit SSL, otherwise STARTTLS when "Enable SSL" is on
            var secureOption = settings.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : (settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(settings.Host, settings.Port, secureOption);

            await smtp.AuthenticateAsync(settings.UserName, password);

            await smtp.SendAsync(message);

            await smtp.DisconnectAsync(true);

            return (true, null);
        }
        catch (Exception ex)
        {
            // returned so SendEmailAsync can store it in EmailLog.ErrorMessage
            return (false, ex.Message);
        }
    }
}