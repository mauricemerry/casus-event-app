
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventApp.OrganiserPortal.Models;
using EventApp.OrganiserPortal.Repositories;

namespace EventApp.OrganiserPortal.Pages.Portal;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IOrganiserRepository _imagesRepository;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public CreateModel(
        IOrganiserRepository imagesRepository,
        IWebHostEnvironment env,
        IConfiguration config)
    {
        _imagesRepository = imagesRepository;
        _env = env;
        _config = config;
    }

    [BindProperty]
    public CreateEventInputModel Input { get; set; } = new();

    [BindProperty]
    public List<IFormFile> Images { get; set; } = new();

    public void OnGet()
    {
        Input.Periods.Add(new EventPeriodInputModel
        {
            Name = "Openingsdag"
        });

        Input.Locations.Add(new EventLocationInputModel
        {
            Name = "Hoofdingang"
        });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Input.Periods ??= new();
        Input.Locations ??= new();
        Input.Tickets ??= new();
        Input.Tags ??= new();
        Input.AccessibilityOptions ??= new();
        Images ??= new();

        // OMGEVING

        Input.IndoorsOutdoors =
            Input.IsIndoor && Input.IsOutdoor ? "Beide" :
            Input.IsIndoor ? "Binnen" :
            Input.IsOutdoor ? "Buiten" : "Beide";

        ModelState.Remove("Input.IndoorsOutdoors");

        if (!Input.IsIndoor &&
            !Input.IsOutdoor &&
            !Input.IsOnline)
        {
            ModelState.AddModelError(
                "",
                "Kies minimaal één omgeving.");
        }

        // PERIODES CONTROLEREN

        if (Input.Periods.Count == 0)
        {
            ModelState.AddModelError(
                "",
                "Voeg minimaal één periode toe.");
        }

        for (int i = 0; i < Input.Periods.Count; i++)
        {
            var p = Input.Periods[i];

            if (string.IsNullOrWhiteSpace(p.Name) ||
                p.Name.Length > 100 ||
                !p.StartDate.HasValue ||
                !p.EndDate.HasValue ||
                !p.StartTime.HasValue ||
                !p.EndTime.HasValue)
            {
                ModelState.AddModelError(
                    "",
                    $"Periode {i + 1}: vul naam, begin en einde in " +
                    "(maximaal 100 tekens voor naam).");

                continue;
            }

            DateTime start =
                p.StartDate.Value.Date.Add(p.StartTime.Value);

            DateTime end =
                p.EndDate.Value.Date.Add(p.EndTime.Value);

            if (end <= start)
            {
                ModelState.AddModelError(
                    "",
                    $"Periode {i + 1}: het eindmoment " +
                    "moet na het beginmoment liggen.");
            }
        }

        // LOCATIES CONTROLEREN

        if (Input.Locations.Count == 0)
        {
            ModelState.AddModelError(
                "",
                "Voeg een hoofdlocatie toe.");
        }

        for (int i = 0; i < Input.Locations.Count; i++)
        {
            var l = Input.Locations[i];

            if (string.IsNullOrWhiteSpace(l.Name) ||
                l.Name.Length > 100 ||
                l.City?.Length > 100 ||
                l.Street?.Length > 150 ||
                l.Number?.Length > 20 ||
                l.Addition?.Length > 20 ||
                l.PostalCode?.Length > 20)
            {
                ModelState.AddModelError(
                    "",
                    $"Locatie {i + 1}: controleer de naam " +
                    "en de lengte van de velden.");
            }
        }

        if (Input.Locations.Count > 0)
        {
            var first = Input.Locations[0];

            if (!first.Latitude.HasValue ||
                !first.Longitude.HasValue ||
                first.Latitude < -90 ||
                first.Latitude > 90 ||
                first.Longitude < -180 ||
                first.Longitude > 180)
            {
                ModelState.AddModelError(
                    "",
                    "Zet een pin op de kaart voor de hoofdlocatie.");
            }

            Input.Latitude = first.Latitude;
            Input.Longitude = first.Longitude;

            Input.Address = string.Join(
                " ",
                new[]
                {
                    first.Street,
                    first.Number,
                    first.City
                }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        // TICKETS CONTROLEREN

        string[] types =
        [
            "Gratis",
            "Dagticket",
            "Periode",
            "Tijdslot"
        ];

        for (int i = 0; i < Input.Tickets.Count; i++)
        {
            var t = Input.Tickets[i];

            if (string.IsNullOrWhiteSpace(t.TicketType) ||
                t.TicketType.Length > 100 ||
                !types.Contains(t.DateType))
            {
                ModelState.AddModelError(
                    "",
                    $"Ticket {i + 1}: geef een geldige " +
                    "naam en een type.");

                continue;
            }

            bool free =
                t.DateType == "Gratis" ||
                t.TicketType.Trim().Equals(
                    "gratis",
                    StringComparison.OrdinalIgnoreCase);

            if (free)
            {
                t.Price = 0;
            }

            if (t.DateType == "Dagticket" &&
                !t.TicketDate.HasValue)
            {
                ModelState.AddModelError(
                    "",
                    $"Ticket {i + 1}: kies een datum.");
            }

            if (t.DateType == "Periode" &&
                (t.PeriodDates == null ||
                 !t.PeriodDates.Any(d => d.HasValue)))
            {
                ModelState.AddModelError(
                    "",
                    $"Ticket {i + 1}: voeg minimaal één dag toe.");
            }

            if (t.DateType == "Tijdslot" &&
                (!t.TicketDate.HasValue ||
                 !t.SlotStart.HasValue ||
                 !t.SlotEnd.HasValue ||
                 t.SlotEnd <= t.SlotStart))
            {
                ModelState.AddModelError(
                    "",
                    $"Ticket {i + 1}: kies een datum " +
                    "en een geldig tijdslot.");
            }
        }

        // TOEGANKELIJKHEID

        string[] fixedOptions =
        [
            "Rolstoeltoegankelijk",
            "Lift aanwezig",
            "Toegankelijk toilet",
            "Gebarentolk",
            "Rustruimte",
            "Slechthorenden",
            "Slechtzienden",
            "Geleidehond toegestaan"
        ];

        if (Input.AccessibilityOptions.Any(
            o => !fixedOptions.Contains(o)))
        {
            ModelState.AddModelError(
                "",
                "Ongeldige toegankelijkheidsoptie.");
        }

        if (Input.HasOtherAccessibility &&
            (string.IsNullOrWhiteSpace(
                 Input.OtherAccessibilityDescription) ||
             Input.OtherAccessibilityDescription.Length > 240))
        {
            ModelState.AddModelError(
                "",
                "Geef bij Overige een toelichting " +
                "(maximaal 240 tekens).");
        }

        if (Input.SelectedCategory?.Length > 100 ||
            Input.Tags.Any(t => t.Length > 100))
        {
            ModelState.AddModelError(
                "",
                "Categorie en tags mogen maximaal 100 tekens hebben.");
        }

        // AFBEELDINGEN

        if (Images.Count < 1 || Images.Count > 10)
        {
            ModelState.AddModelError(
                "",
                "Upload minimaal 1 en maximaal 10 afbeeldingen.");
        }

        string[] allowed =
        [
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        ];

        if (Images.Any(i =>
            i.Length == 0 ||
            i.Length > 5 * 1024 * 1024 ||
            !allowed.Contains(
                Path.GetExtension(i.FileName).ToLowerInvariant())))
        {
            ModelState.AddModelError(
                "",
                "Afbeeldingen moeten JPG, PNG of WEBP zijn, " +
                "maximaal 5 MB per stuk.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // GEBRUIKER CONTROLEREN

        if (!int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out int organiserId))
        {
            return Unauthorized();
        }

        // AFBEELDINGEN TIJDELIJK BEWAREN

        string uploadsRoot = Path.Combine(
            _env.WebRootPath,
            "uploads",
            "events",
            organiserId.ToString());

        string cache = Path.Combine(
            uploadsRoot,
            "cache");

        Directory.CreateDirectory(cache);

        var staged =
            new List<(string Path, string FileName)>();

        try
        {
            foreach (var file in Images)
            {
                string name =
                    Guid.NewGuid().ToString("N") +
                    Path.GetExtension(file.FileName)
                        .ToLowerInvariant();

                string path = Path.Combine(
                    cache,
                    name);

                await using (var stream =
                    System.IO.File.Create(path))
                {
                    await file.CopyToAsync(stream);
                }

                staged.Add((path, name));
            }

            // EVENEMENT IN SQL OPSLAAN

            int eventId =
                await new EventWizardWriter(_config)
                    .SaveAsync(organiserId, Input);

            // AFBEELDINGEN NAAR DEFINITIEVE MAP

            string final = Path.Combine(
                uploadsRoot,
                eventId.ToString(),
                "uploads");

            Directory.CreateDirectory(final);

            var urls = new List<string>();

            foreach (var f in staged)
            {
                System.IO.File.Move(
                    f.Path,
                    Path.Combine(final, f.FileName));

                urls.Add(
                    $"/uploads/events/{organiserId}/" +
                    $"{eventId}/uploads/{f.FileName}");
            }

            await _imagesRepository.AddEventImagesAsync(
                eventId,
                urls);

            return RedirectToPage("/Portal/Dashboard");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                "",
                "Opslaan mislukt: " + ex.Message);

            return Page();
        }
        finally
        {
            foreach (var f in staged)
            {
                if (System.IO.File.Exists(f.Path))
                {
                    System.IO.File.Delete(f.Path);
                }
            }
        }
    }
}
