using PuppeteerSharp;
using PuppeteerSharp.Media;
using Shikhsa.ViewModels;
using System.Text;

namespace Shikhsa.Sevices
{
    public class StaffIdCardPdfService
    {
        public async Task<byte[]> GenerateIdCardAsync(StaffProfileViewModel profile)
        {
            var html = BuildIdCardHtml(profile);

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(html, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Width = "3.4in",
                Height = "5.4in",
                PrintBackground = true,
                MarginOptions = new MarginOptions { Top = "0", Bottom = "0", Left = "0", Right = "0" }
            });

            return pdfBytes;
        }
        private string BuildIdCardHtml(StaffProfileViewModel profile)
        {
            var s = profile.Staff;
            var photoPath = s.PhotoPath ?? "";

            var sb = new StringBuilder();
            sb.Append($@"<!DOCTYPE html><html><head><meta charset='utf-8' />
<style>
    * {{ margin:0; padding:0; box-sizing:border-box; font-family: Arial, sans-serif; }}
    body {{ width: 3.4in; padding: 10px; text-align:center; }}
    .school-name {{ font-size: 13px; font-weight:bold; }}
    .sub {{ font-size: 9px; color:#555; margin-bottom:8px; }}
    .photo {{ width: 90px; height: 100px; object-fit:cover; border:1px solid #ccc; margin: 6px auto; display:block; }}
    .name {{ font-size: 13px; font-weight:bold; margin-top:6px; }}
    .row {{ font-size: 10px; margin-top:3px; text-align:left; padding-left: 8px; }}
    .footer {{ font-size: 7px; color:#777; margin-top:10px; }}
</style>
</head>
<body>
    <div class='school-name'>YOUR SCHOOL NAME</div>
    <div class='sub'>Staff Identity Card</div>

    <img class='photo' src='{photoPath}' />

    <div class='name'>{s.FullName}</div>
    <div class='row'>Staff Code: {s.StaffCode}</div>
    <div class='row'>Designation: {profile.ExtraInfo.DesignationName}</div>
    <div class='row'>Department: {profile.ExtraInfo.DepartmentName}</div>
    <div class='row'>Mobile: {s.MobileNo}</div>
    <div class='row'>Blood Group: {profile.ExtraInfo.BloodGroupName}</div>

    <div class='footer'>Valid till employment continues</div>
</body>
</html>");

            return sb.ToString();
        }
    }
}
