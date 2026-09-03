using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventApp.OrganiserPortal.Models;
using EventApp.OrganiserPortal.Repositories;

namespace EventApp.OrganiserPortal.Pages.Portal
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
        private const long MaxImageSizeBytes = 5 * 1024 * 1024;

        private readonly IOrganiserRepository _organiserRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public CreateModel(
            IOrganiserRepository organiserRepository,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _organiserRepository = organiserRepository;
            _environment = environment;
            _configuration = configuration;
        }

        [BindProperty]
        public CreateEventInputModel Input { get; set; } = new();

        [BindProperty]
        public List<IFormFile> Images { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.Tickets == null)
            {
                Input.Tickets = new();
            }

            Input.Tickets = Input.Tickets
                .Where(x => !string.IsNullOrWhiteSpace(x.TicketType))
                .ToList();

            if (!Input.Tickets.Any())
            {
                Input.Tickets.Add(new CreateTicketInputModel
                {
                    TicketType = "Gratis",
                    Price = 0
                });
            }

            if (!Images.Any())
            {
                ModelState.AddModelError(string.Empty, "Upload minimaal één afbeelding.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var organiserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(organiserIdClaim, out var organiserId))
            {
                return Unauthorized();
            }

            // Use wwwroot/uploads for uploaded images - works on any machine
            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads");

            var cacheRoot = Path.Combine(
                uploadsRoot,
                "events",
                organiserId.ToString(),
                "cache");

            Directory.CreateDirectory(cacheRoot);

            var cachedFiles = new List<(string TempPath, string FileName)>();

            try
            {
                for (var i = 0; i < Images.Count; i++)
                {
                    var image = Images[i];

                    if (image.Length <= 0)
                    {
                        ModelState.AddModelError(string.Empty, $"Afbeelding {i + 1} is leeg.");
                        return Page();
                    }

                    if (image.Length > MaxImageSizeBytes)
                    {
                        ModelState.AddModelError(string.Empty, $"Afbeelding {i + 1} is groter dan 5 MB.");
                        return Page();
                    }

                    var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                    if (!AllowedImageExtensions.Contains(extension))
                    {
                        ModelState.AddModelError(string.Empty, $"Afbeelding {i + 1} moet jpg, jpeg, png of webp zijn.");
                        return Page();
                    }

                    var fileName = $"{Guid.NewGuid():N}{extension}";
                    var tempPath = Path.Combine(cacheRoot, fileName);

                    await using (var stream = System.IO.File.Create(tempPath))
                    {
                        await image.CopyToAsync(stream);
                    }

                    cachedFiles.Add((tempPath, fileName));
                }

                var eventId = await _organiserRepository.GetNextEventIdAsync();
                await _organiserRepository.CreateEventAsync(organiserId, eventId, Input);

                var finalRoot = Path.Combine(
                    uploadsRoot,
                    "events",
                    organiserId.ToString(),
                    eventId.ToString(),
                    "uploads");

                Directory.CreateDirectory(finalRoot);

                var finalImageUrls = new List<string>();

                foreach (var cachedFile in cachedFiles)
                {
                    var finalPath = Path.Combine(finalRoot, cachedFile.FileName);

                    if (System.IO.File.Exists(finalPath))
                    {
                        System.IO.File.Delete(finalPath);
                    }

                    System.IO.File.Move(cachedFile.TempPath, finalPath);

                    finalImageUrls.Add($"/uploads/events/{organiserId}/{eventId}/uploads/{cachedFile.FileName}");
                }

                await _organiserRepository.AddEventImagesAsync(eventId, finalImageUrls);

                return RedirectToPage("/Portal/Dashboard");
            }
            catch (Exception ex)
            {
                foreach (var cachedFile in cachedFiles)
                {
                    if (System.IO.File.Exists(cachedFile.TempPath))
                    {
                        System.IO.File.Delete(cachedFile.TempPath);
                    }
                }

                ModelState.AddModelError(string.Empty, $"Het evenement kon niet opgeslagen worden. {ex.Message}");
                return Page();
            }
        }
    }
}