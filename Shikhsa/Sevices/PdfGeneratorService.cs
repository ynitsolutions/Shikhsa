using Microsoft.AspNetCore.Components;
using PuppeteerSharp;
using PuppeteerSharp.Media;
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
    }
}