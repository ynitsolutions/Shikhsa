//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Caching.Memory;
//using Shikhsa.Data;
//using Shikhsa.Helpers;
//using Shikhsa.Models.Common;
//using Shikhsa.Models.Notification;

//public class NotificationService
//{
//    private readonly ApplicationDbContext _context;
//    private readonly EmailService _emailService;
//    private readonly IMemoryCache _cache;
//    public NotificationService(
//        ApplicationDbContext context,
//        EmailService emailService, IMemoryCache cache)
//    {
//        _context = context;
//        _emailService = emailService;
//        _cache = cache;
//    }

//    public async Task<ResponseModel> SendAsync(
//        string templateCode,
//        string toEmail,
//        long? referenceId = null,
//        params object[] entities)
//    {
//        ResponseModel response = new();

//        var template = await _context.NotificationTemplates
//            .FirstOrDefaultAsync(x => x.TemplateCode == templateCode && x.IsActive);

//        if (template == null)
//        {
//            response.Status = 0;
//            response.Message = "Notification template not found.";
//            return response;
//        }
//        var school = await GetSchoolInfo();

//        var objects = entities.ToList();

//        if (school != null)
//        {
//            objects.Add(school);
//        }

//        var placeholders = PlaceholderHelper.CreateDictionary(objects.ToArray());


//        // Automatically create placeholders from all objects
//        //var placeholders = PlaceholderHelper.CreateDictionary(entities);

//        string subject = PlaceholderHelper.ReplacePlaceholders(
//            template.Subject ?? "",
//            placeholders);

//        string body = PlaceholderHelper.ReplacePlaceholders(
//            template.Body,
//            placeholders);

//        bool sent = await _emailService.SendEmailAsync(
//            "Notification",
//            referenceId,
//            toEmail,
//            subject,
//            body);

//        response.Status = sent ? 1 : 0;
//        response.Message = sent
//            ? "Email sent successfully."
//            : "Email sending failed.";

//        return response;
//    }

//    public async Task<SchoolMaster> GetSchoolInfo()
//    {
//        return await _cache.GetOrCreateAsync("SchoolInfo", async entry =>
//        {
//            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12);

//            return await _context.SchoolMasters.FirstAsync();
//        });
//    }
//}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Shikhsa.Data;
using Shikhsa.Helpers;
using Shikhsa.Models.Common;
using Shikhsa.Models.Notification;

public class NotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;
    private readonly IMemoryCache _cache;

    public NotificationService(
        ApplicationDbContext context,
        EmailService emailService, IMemoryCache cache)
    {
        _context = context;
        _emailService = emailService;
        _cache = cache;
    }


    // ============================================
    // ORIGINAL METHOD — BILKUL UNCHANGED
    // (Student Registration aur baaki sab existing calls
    //  isi ko use karte rahenge, koi break nahi hoga)
    // ============================================

    public async Task<ResponseModel> SendAsync(
        string templateCode,
        string toEmail,
        long? referenceId = null,
        params object[] entities)
    {
        return await SendInternalAsync(templateCode, toEmail, referenceId, null, entities);
    }


    // ============================================
    // NAYA METHOD — ATTACHMENT KE SAATH
    // (Fee Receipt jaisi jagah use hoga jaha PDF attach karna hai)
    // ============================================

    public async Task<ResponseModel> SendWithAttachmentAsync(
        string templateCode,
        string toEmail,
        long? referenceId,
        List<(string FileName, byte[] Content, string ContentType)>? attachments,
        params object[] entities)
    {
        return await SendInternalAsync(templateCode, toEmail, referenceId, attachments, entities);
    }


    // ============================================
    // COMMON INTERNAL LOGIC (dono methods yahi call karte hain)
    // ============================================

    private async Task<ResponseModel> SendInternalAsync(
        string templateCode,
        string toEmail,
        long? referenceId,
        List<(string FileName, byte[] Content, string ContentType)>? attachments,
        object[] entities)
    {
        ResponseModel response = new();

        var template = await _context.NotificationTemplates
            .FirstOrDefaultAsync(x => x.TemplateCode == templateCode && x.IsActive);

        if (template == null)
        {
            response.Status = 0;
            response.Message = "Notification template not found.";
            return response;
        }

        var school = await GetSchoolInfo();

        var objects = entities.ToList();

        if (school != null)
        {
            objects.Add(school);
        }

        var placeholders = PlaceholderHelper.CreateDictionary(objects.ToArray());

        string subject = PlaceholderHelper.ReplacePlaceholders(template.Subject ?? "", placeholders);
        string body = PlaceholderHelper.ReplacePlaceholders(template.Body, placeholders);

        bool sent = await _emailService.SendEmailAsync(
            "Notification",
            referenceId,
            toEmail,
            subject,
            body,
            attachments);

        response.Status = sent ? 1 : 0;
        response.Message = sent
            ? "Email sent successfully."
            : "Email sending failed.";

        return response;
    }


    public async Task<SchoolMaster> GetSchoolInfo()
    {
        return await _cache.GetOrCreateAsync("SchoolInfo", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12);
            return await _context.SchoolMasters.FirstAsync();
        });
    }
}