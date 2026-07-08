using Microsoft.AspNetCore.Hosting;

namespace Shikhsa.Helpers
{
    public class FileUploadHelper
    {
        private readonly IWebHostEnvironment _environment;

        public FileUploadHelper(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadFile(
            IFormFile file,
            string userType,
            string fileType)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            string extension = Path.GetExtension(file.FileName).ToLower();

            string[] allowedExtensions =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".pdf"
            };

            if (!allowedExtensions.Contains(extension))
                throw new Exception("Invalid file type.");

            if (file.Length > 2 * 1024 * 1024)
                throw new Exception("Maximum file size is 2 MB.");

            string folderPath = Path.Combine(
                _environment.WebRootPath,
                "Uploads",
                userType,
                fileType);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName =
                $"{fileType}_{DateTime.Now:ddMMyyyyHHmmssffffff}{extension}";

            string fullPath = Path.Combine(folderPath, fileName);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/Uploads/{userType}/{fileType}/{fileName}";
        }
    }
}