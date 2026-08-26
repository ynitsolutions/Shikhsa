using Microsoft.AspNetCore.Components;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Shikhsa.ViewModels;
using System.Text;
using PuppeteerNavigationOptions = PuppeteerSharp.NavigationOptions;
namespace Shikhsa.Services
{
    public class PdfGeneratorService
    {
        private static bool _browserDownloaded = false;
        private static readonly SemaphoreSlim _downloadLock = new(1, 1);

        public async Task<byte[]> GeneratePdfFromHtmlAsync(string html)
        {
            await EnsureBrowserDownloadedAsync();

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox"
                }
            };

            await using var browser = await Puppeteer.LaunchAsync(launchOptions);
            await using var page = await browser.NewPageAsync();

            await page.SetContentAsync(html, new PuppeteerNavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.A5,
                Landscape = true,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "5mm",
                    Bottom = "5mm",
                    Left = "5mm",
                    Right = "5mm"
                }
            };

            var pdfBytes = await page.PdfDataAsync(pdfOptions);

            return pdfBytes;
        }

        // ============================================
        // Chromium ko sirf EK BAAR download karo
        // (application lifetime me), har request pe nahi
        // ============================================

        private static async Task EnsureBrowserDownloadedAsync()
        {
            if (_browserDownloaded)
                return;

            await _downloadLock.WaitAsync();

            try
            {
                if (_browserDownloaded)
                    return;

                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                _browserDownloaded = true;
            }
            finally
            {
                _downloadLock.Release();
            }
        }
        public async Task<byte[]> GenerateA4PdfFromHtmlAsync(string html)
        {
            await EnsureBrowserDownloadedAsync();

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            };

            await using var browser = await Puppeteer.LaunchAsync(launchOptions);
            await using var page = await browser.NewPageAsync();

            await page.SetContentAsync(html, new PuppeteerSharp.NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.A4,
                Landscape = false,
                PrintBackground = true,
                MarginOptions = new MarginOptions { Top = "10mm", Bottom = "10mm", Left = "8mm", Right = "8mm" }
            };

            return await page.PdfDataAsync(pdfOptions);
        }
        public async Task<byte[]> GenerateIdCardAsync(StudentProfileViewModel profile)
        {
            var html = BuildIdCardHtml(profile);

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(html, new PuppeteerSharp.NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Width = "3.4in",   // ID card jaisa narrow-tall size — apne school
                Height = "5.4in",  // ke actual card size ke hisaab se adjust kar lein
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "0",
                    Bottom = "0",
                    Left = "0",
                    Right = "0"
                }
            });

            return pdfBytes;
        }

        private string BuildIdCardHtml(StudentProfileViewModel profile)
        {
            var s = profile.Student;
            var photoPath = profile.PhotoDocument?.FilePath ?? "";

            var sb = new StringBuilder();
            sb.Append($@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8' />
<style>
    * {{ margin:0; padding:0; box-sizing:border-box; font-family: Arial, sans-serif; }}
    body {{ width: 3.4in; padding: 10px; text-align:center; }}
    .school-name {{ font-size: 13px; font-weight:bold; }}
    .session {{ font-size: 9px; color:#555; margin-bottom:8px; }}
    .photo {{ width: 90px; height: 100px; object-fit:cover; border:1px solid #ccc; margin: 6px auto; display:block; }}
    .name {{ font-size: 13px; font-weight:bold; margin-top:6px; }}
    .row {{ font-size: 10px; margin-top:3px; text-align:left; padding-left: 8px; }}
    .footer {{ font-size: 7px; color:#777; margin-top:10px; }}
</style>
</head>
<body>
    <div class='school-name'>YOUR SCHOOL NAME</div>
    <div class='session'>Session: {s.CurrentBatchName}</div>

    <img class='photo' src='{photoPath}' />

    <div class='name'>{s.FirstName} {s.MiddleName} {s.LastName}</div>
    <div class='row'>Class: {s.ClassName} - {s.SectionName}</div>
    <div class='row'>Scholar No: {s.ScholarNumber}</div>
    <div class='row'>App No: {s.ApplicationNo}</div>
    <div class='row'>DOB: {s.DOB:dd-MM-yyyy}</div>

    <div class='footer'>Valid for Current Academic Session</div>
</body>
</html>");

            return sb.ToString();
        }
    }
}