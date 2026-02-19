using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        [HttpPost]
        [Filters.Permission("Product", "Write")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            const long maxFileSize = 10 * 1024 * 1024; // 10MB limit for raw upload before processing
            if (file.Length > maxFileSize)
                return BadRequest("Dosya boyutu çok büyük (Maksimum 10MB).");

            try
            {
                var currentDir = Directory.GetCurrentDirectory();
                var possiblePaths = new[] 
                { 
                    Path.Combine(currentDir, "web"),
                    Path.Combine(currentDir, "..", "web"),
                    Path.Combine(currentDir, "..", "..", "web")
                };

                string webPath = null;
                foreach (var path in possiblePaths)
                {
                    if (Directory.Exists(path))
                    {
                        webPath = Path.GetFullPath(path);
                        break;
                    }
                }

                if (webPath == null)
                    return BadRequest("'web' klasörü bulunamadı.");

                var uploadsPath = Path.Combine(webPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var newFileName = $"{fileName}_{Guid.NewGuid()}.jpg"; // Always save as jpg for consistency
                var filePath = Path.Combine(uploadsPath, newFileName);

                // Image Processing with ImageSharp
                using (var image = await Image.LoadAsync(file.OpenReadStream()))
                {
                    const int maxWidth = 800;
                    const int maxHeight = 800;

                    if (image.Width > maxWidth || image.Height > maxHeight)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(maxWidth, maxHeight),
                            Mode = ResizeMode.Max
                        }));
                    }

                    // Save as optimized JPEG
                    var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                    {
                        Quality = 80 // 80% quality is usually enough
                    };
                    await image.SaveAsync(filePath, encoder);
                }

                var relativePath = $"uploads/{newFileName}";
                return Ok(new { url = relativePath });
            }
            catch (Exception ex)
            {
                return BadRequest($"Resim işleme hatası: {ex.Message}. Lütfen geçerli bir resim dosyası yükleyin.");
            }
        }
    }
}
