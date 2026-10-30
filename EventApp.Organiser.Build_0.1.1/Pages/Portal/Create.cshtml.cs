
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventApp.OrganiserPortal.Models;
using EventApp.OrganiserPortal.Repositories;
using Sparkly.OrganiserPortal.Models;

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

    public int CurrentStep { get; private set; } = 1;

    public void OnGet()
    {
        Input.Periods.Add(new EventPeriodInputModel
        {
            Name = ""
        });

        Input.Locations.Add(new EventLocationInputModel
        {
            Name = "Locatie 1"
        });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Input.Periods ??= new();
        Input.Locations ??= new();
        Input.Tickets ??= new();
        Input.Tags ??= new();
        Input.AccessibilityOptionDescriptions ??= new();

        CreateEventProblems problems = Input.GetProblems();

        if (problems.HasErrors())
        {
            IList<string> feedback = problems.GetFeedback();

            foreach (string message in feedback)
            {
                ModelState.AddModelError("", message);
            }

            CurrentStep = GetFirstInvalidStep(problems);

            return Page();
        }

        if (!ModelState.IsValid)
        {
            CurrentStep = 1;
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
            foreach (var file in Input.Images)
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
    

    private static int GetFirstInvalidStep(CreateEventProblems problems)
    {
        if (problems.HasErrors(CreateEventTab.NAME_AND_ENVIRONMENT))
        {
            return 1;
        }

        if (problems.HasErrors(CreateEventTab.ACCESSIBILITY))
        {
            return 2;
        }

        if (problems.HasErrors(CreateEventTab.LOCATIONS))
        {
            return 3;
        }

        if (problems.HasErrors(CreateEventTab.DATE_AND_TIME))
        {
            return 4;
        }

        if (problems.HasErrors(CreateEventTab.TICKETS))
        {
            return 5;
        }

        return 1;
    }
}
