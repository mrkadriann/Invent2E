using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Data; // Your DbContext
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // For ToFirstOrDefaultAsync

namespace InventoryManagement.Controllers
{
    public class ImageController : Controller
    {
        private readonly InventoryDbContext _context;

        public ImageController(InventoryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetImage(int id)
        {
            var imageData = await _context.Images
                                          .AsNoTracking() // Good for read-only operations
                                          .FirstOrDefaultAsync(i => i.ImageId == id);

            if (imageData != null && imageData.ImageDt != null && imageData.ImageDt.Length > 0)
            {
                string contentType = "image/jpeg";
                if (IsPng(imageData.ImageDt)) contentType = "image/png";
                else if (IsGif(imageData.ImageDt)) contentType = "image/gif";
                // Add more checks if needed

                return File(imageData.ImageDt, contentType);
            }
            return NotFound();
        }
        private bool IsPng(byte[] bytes)
        {
            return bytes.Length > 8 &&
                   bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                   bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;
        }

        private bool IsGif(byte[] bytes)
        {
            return bytes.Length > 6 &&
                   bytes[0] == 'G' && bytes[1] == 'I' && bytes[2] == 'F' && bytes[3] == '8' &&
                   (bytes[4] == '7' || bytes[4] == '9') && bytes[5] == 'a';
        }
    }
}