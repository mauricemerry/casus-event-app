using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventApp.OrganiserPortal.Models;
using EventApp.OrganiserPortal.Repositories;

namespace EventApp.OrganiserPortal.Pages.Portal
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IOrganiserRepository _organiserRepository;
        private readonly IWebHostEnvironment _environment;

        public EditModel(IOrganiserRepository organiserRepository, IWebHostEnvironment environment)
        {
            _organiserRepository = organiserRepository;
            _environment = environment;
        }

        [BindProperty]
        public CreateEventInputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var organiserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(organiserIdValue, out var organiserId))
            {
                return Unauthorized();
            }

            var eventData = await _organiserRepository.GetEventForEditAsync(organiserId, Id);
            if (eventData == null)
            {
                return NotFound();
            }

            Input = eventData;

            if (Input.Locations.Count == 0)
            {
                Input.Locations.Add(new EventLocationInputModel());
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var organiserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(organiserIdValue, out var organiserId))
            {
                return Unauthorized();
            }

            if (Input.Tickets == null)
            {
                Input.Tickets = new();
            }

            Input.Tickets = Input.Tickets
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .ToList();

            if (!Input.Tickets.Any())
            {
                Input.Tickets.Add(new CreateTicketInputModel
                {
                    Name = "Gratis",
                    Price = 0
                });
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _organiserRepository.UpdateEventAsync(organiserId, Id, Input);
            await _organiserRepository.RemoveEventImagesAsync(Id, Input.KeptImageUrls);

            if (Input.Images.Count > 0)
            {
                string uploadDirectory = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "events",
                    organiserId.ToString(),
                    Id.ToString(),
                    "uploads");
                Directory.CreateDirectory(uploadDirectory);

                List<string> imageUrls = new();
                foreach (IFormFile image in Input.Images.Take(10))
                {
                    string extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                    if (image.Length <= 0 || image.Length > 5 * 1024 * 1024 ||
                        extension is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
                    {
                        continue;
                    }

                    string fileName = $"{Guid.NewGuid():N}{extension}";
                    string filePath = Path.Combine(uploadDirectory, fileName);
                    await using FileStream stream = System.IO.File.Create(filePath);
                    await image.CopyToAsync(stream);
                    imageUrls.Add($"/uploads/events/{organiserId}/{Id}/uploads/{fileName}");
                }

                if (imageUrls.Count > 0)
                {
                    await _organiserRepository.AddEventImagesAsync(Id, imageUrls);
                }
            }

            return RedirectToPage("/Portal/Dashboard");
        }

        public async Task<IActionResult> OnPostCancelAsync()
        {
            var organiserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(organiserIdValue, out var organiserId))
            {
                return Unauthorized();
            }

            await _organiserRepository.CancelEventAsync(organiserId, Id);
            return RedirectToPage("/Portal/Dashboard");
        }
    }
}