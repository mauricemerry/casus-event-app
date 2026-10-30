
using Sparkly.OrganiserPortal.Models;
using System.ComponentModel.DataAnnotations;

namespace EventApp.OrganiserPortal.Models;

public class EventInputModel
{
    // Basis
    [Required, StringLength(255)]
    public string EventName { get; set; } = "";

    [StringLength(50)]
    public string? EventType { get; set; }

    [StringLength(500)]
    public string? ShortDescription { get; set; }

    // Meerdere omgevingen tegelijk mogelijk.
    public bool IsIndoor { get; set; }
    public bool IsOutdoor { get; set; }
    public bool IsOnline { get; set; }

    // Details
    [Required]
    public string Description { get; set; } = "";

    public List<IFormFile> Images { get; set; } = new();
    public List<string> ExistingImageUrls { get; set; } = new();
    public List<string> KeptImageUrls { get; set; } = new();
    public List<ImageCropPositionInputModel> ImageCropPositions
    { get; set; } = new();

    // Periodes
    public List<EventPeriodInputModel> Periods
    { get; set; } = new();

    // Globaal bereik, voor bestaande databasecode.
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }

    // Extra informatie
    public string? Notes { get; set; }

    public List<string> AccessibilityOptionDescriptions { get; set; } = new();
    public List<AccessibilityOption> AccessibilityOptions { get; set; } = new();

    public bool HasCustomAccessibilityOptions { get; set; }

    public string? CustomAccessibilityOptionDescriptions
    { get; set; }

    public string? CategoryName { get; set; } = EventCategory.MISCELLANEOUS.Name();
    public EventCategory? Category { get; set; } = EventCategory.MISCELLANEOUS;

    public List<string> Tags { get; set; } = new();

    // Locaties
    public List<EventLocationInputModel> Locations
    { get; set; } = new();

    // Oude velden behouden voor Edit.cshtml.
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    // Bestaande voorzieningen.
    public bool? HasStandingPlaces { get; set; }
    public bool? HasSittingPlaces { get; set; }
    public bool? DisabledParkingAvailable { get; set; }
    public bool? DisabledToiletAvailable { get; set; }
    public bool? WheelchairAccessibleToilet { get; set; }

    // Tickets
    public List<TicketInputModel> Tickets { get; set; } = new();

    public EventInputProblems GetProblems()
    {
        return this.GetProblems(EventFormTab.ALL);
    }

    public EventInputProblems GetProblems(EventFormTab tab)
    {
        EventInputProblems problems = new();

        if (tab == EventFormTab.ALL || tab == EventFormTab.NAME_AND_ENVIRONMENT)
        {
            if (string.IsNullOrWhiteSpace(this.EventName))
            {
                problems.AddError(EventInputError.NO_NAME);
            }
            else if (this.EventName.Length > 255)
            {
                problems.AddError(EventInputError.NAME_TOO_LONG);
            }
            if (!this.IsIndoor && !this.IsOutdoor && !this.IsOnline)
            {
                problems.AddError(EventInputError.NO_ENVIRONMENT);
            }
            if (this.Images.Count == 0 || this.Images.Count > 10)
            {
                problems.AddError(EventInputError.NO_IMAGES);
            }
            foreach (IFormFile image in this.Images)
            {
                if (image.Length == 0)
                {
                    problems.AddError(EventInputError.IMAGE_EMPTY, image.Name);
                }
                else if (image.Length > 5 * 1024 * 1024)
                {
                    problems.AddError(EventInputError.IMAGE_TOO_LARGE);
                }

                string[] allowedFormats =
                [
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                ];

                if (!allowedFormats.Contains(Path.GetExtension(image.FileName).ToLowerInvariant()))
                {
                    problems.AddError(EventInputError.IMAGE_INVALID_FORMAT, image.Name, string.Join(", ", allowedFormats));
                }
            }
        }
        if (tab == EventFormTab.ALL || tab == EventFormTab.DATE_AND_TIME)
        {
            if (this.Periods.Count == 0)
            {
                problems.AddError(EventInputError.NO_PERIODS);
            }
            else
            {
                for (int i = 0; i < this.Periods.Count; i++)
                {
                    int periodNumber = i + 1; // 1-indexed for the user

                    EventPeriodInputModel period = this.Periods[i];

                    if (string.IsNullOrWhiteSpace(period.Name))
                    {
                        problems.AddError(EventInputError.PERIOD_NO_NAME, periodNumber);
                    }
                    else if (period.Name.Length > 100)
                    {
                        problems.AddError(EventInputError.PERIOD_NAME_TOO_LONG, periodNumber);
                    }

                    if (period.StartDate.HasValue && period.StartTime.HasValue && period.EndDate.HasValue && period.EndTime.HasValue)
                    {
                        DateTime start = period.StartDate.Value.Date.Add(period.StartTime.Value);
                        DateTime end = period.EndDate.Value.Date.Add(period.EndTime.Value);

                        if (start > end)
                        {
                            problems.AddError(EventInputError.PERIOD_START_AFTER_END, periodNumber);
                        }
                    }
                    else
                    {
                        if (!period.StartDate.HasValue)
                        {
                            problems.AddError(EventInputError.PERIOD_NO_START_DATE, periodNumber);
                        }
                        if (!period.StartTime.HasValue)
                        {
                            problems.AddError(EventInputError.PERIOD_NO_START_TIME, periodNumber);
                        }
                        if (!period.EndDate.HasValue)
                        {
                            problems.AddError(EventInputError.PERIOD_NO_END_DATE, periodNumber);
                        }
                        if (!period.EndTime.HasValue)
                        {
                            problems.AddError(EventInputError.PERIOD_NO_END_TIME, periodNumber);
                        }
                    }
                }
            }
        }
        if (tab == EventFormTab.ALL || tab == EventFormTab.LOCATIONS)
        {
            if (this.Locations.Count == 0)
            {
                problems.AddError(EventInputError.NO_LOCATIONS);
            }
            else
            {
                for (int i = 0; i < this.Locations.Count; i++)
                {
                    int locationNumber = i + 1; // 1-indexed for the user

                    EventLocationInputModel location = this.Locations[i];

                    if (string.IsNullOrWhiteSpace(location.Name))
                    {
                        problems.AddError(EventInputError.LOCATION_NO_NAME, locationNumber);
                    }
                    else if (location.Name.Length > 100)
                    {
                        problems.AddError(EventInputError.LOCATION_NAME_TOO_LONG, locationNumber);
                    }

                    if (string.IsNullOrWhiteSpace(location.City))
                    {
                        problems.AddError(EventInputError.LOCATION_NO_CITY, locationNumber);
                    }
                    else if (location.City.Length > 100)
                    {
                        problems.AddError(EventInputError.LOCATION_CITY_TOO_LONG, locationNumber);
                    }

                    if (string.IsNullOrWhiteSpace(location.Street))
                    {
                        problems.AddError(EventInputError.LOCATION_NO_STREET, locationNumber);
                    }
                    else if (location.Street.Length > 150)
                    {
                        problems.AddError(EventInputError.LOCATION_STREET_TOO_LONG, locationNumber);
                    }

                    if (string.IsNullOrWhiteSpace(location.Number))
                    {
                        problems.AddError(EventInputError.LOCATION_NO_NUMBER, locationNumber);
                    }
                    else if (location.Number.Length > 20)
                    {
                        problems.AddError(EventInputError.LOCATION_NUMBER_TOO_LONG, locationNumber);
                    }

                    if (!string.IsNullOrWhiteSpace(location.Addition) && location.Addition.Length > 20)
                    {
                        problems.AddError(EventInputError.LOCATION_ADDITION_TOO_LONG, locationNumber);
                    }

                    if (string.IsNullOrWhiteSpace(location.PostalCode))
                    {
                        problems.AddError(EventInputError.LOCATION_NO_POSTAL_CODE, locationNumber);
                    }
                    else if (location.PostalCode.Length > 20)
                    {
                        problems.AddError(EventInputError.LOCATION_POSTAL_CODE_TOO_LONG, locationNumber);
                    }
                    
                    if (location.Latitude.HasValue && location.Longitude.HasValue)
                    {
                        if (location.Latitude < -90 || location.Latitude > 90 || location.Longitude < -180 || location.Longitude > 180)
                        {
                            problems.AddError(EventInputError.LOCATION_INVALID_COORDINATES, locationNumber);
                        }
                    }
                }
            }
        }
        if (tab == EventFormTab.ALL || tab == EventFormTab.TICKETS)
        {
            for (int i = 0; i < this.Tickets.Count; i++)
            {
                int ticketNumber = i + 1; // 1-indexed for the user

                TicketInputModel ticket = this.Tickets[i];

                if (string.IsNullOrWhiteSpace(ticket.Name))
                {
                    problems.AddError(EventInputError.TICKET_NO_NAME, ticketNumber);
                }
                else if (ticket.Name.Length > 100)
                {
                    problems.AddError(EventInputError.TICKET_NAME_TOO_LONG, ticketNumber);
                }

                if (string.IsNullOrWhiteSpace(ticket.TypeName))
                {
                        problems.AddError(EventInputError.TICKET_NO_TYPE, ticketNumber);
                }
                else
                {
                    ticket.Type = TicketType.ByName(ticket.TypeName);
                }

                if (ticket.Type == null)
                {
                    problems.AddError(EventInputError.TICKET_NO_TYPE, ticketNumber);
                }
                else
                {
                    if (ticket.Type == TicketType.FREE)
                    {
                        ticket.Price = 0;
                    }
                    if (ticket.Type == TicketType.DAY_TICKET && !ticket.Date.HasValue)
                    {
                        problems.AddError(EventInputError.TICKET_NO_DATE, ticketNumber);
                    }
                    if (ticket.Type == TicketType.PERIOD && !ticket.PeriodDates.Any(d => d.HasValue))
                    {
                        problems.AddError(EventInputError.TICKET_NO_PERIOD, ticketNumber);
                    }
                    if (ticket.Type == TicketType.TIME_SLOT)
                    {
                        if (ticket.SlotStart.HasValue && ticket.SlotEnd.HasValue)
                        {
                            if (ticket.SlotStart > ticket.SlotEnd)
                            {
                                problems.AddError(EventInputError.TICKET_SLOT_START_AFTER_END, ticketNumber);
                            }
                        }
                        else
                        {
                            if (!ticket.SlotStart.HasValue)
                            {
                                problems.AddError(EventInputError.TICKET_NO_SLOT_START, ticketNumber);
                            }
                            if (!ticket.SlotEnd.HasValue)
                            {
                                problems.AddError(EventInputError.TICKET_NO_SLOT_END, ticketNumber);
                            }
                        }
                    }
                }
            }
        }
        if (tab == EventFormTab.ALL || tab == EventFormTab.ACCESSIBILITY)
        {
            this.AccessibilityOptions = this.AccessibilityOptionDescriptions
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => AccessibilityOption.ByDescription(d))
                .Where(o => o != null)
                .ToList();
            List<string> invalidOptions = this.AccessibilityOptionDescriptions
                .Where(d => string.IsNullOrWhiteSpace(d) || AccessibilityOption.ByDescription(d) == null)
                .ToList();

            if (invalidOptions.Count > 0)
            {
                problems.AddError(EventInputError.INVALID_ACCESSIBILITY_OPTION);
            }
            if (this.HasCustomAccessibilityOptions && string.IsNullOrWhiteSpace(this.CustomAccessibilityOptionDescriptions))
            {
                problems.AddError(EventInputError.CUSTOM_ACCESSIBILITY_NO_DESCRIPTION);
            }

            if (string.IsNullOrWhiteSpace(this.CategoryName))
            {
                problems.AddError(EventInputError.NO_CATEGORY);
            }
            else
            {
                this.Category = EventCategory.ByName(this.CategoryName);
            }

            if (this.Category == null)
            {
                problems.AddError(EventInputError.NO_CATEGORY);
            }

            if (this.Tags.Any(t => t.Length > 100))
            {
                problems.AddError(EventInputError.TAG_TOO_LONG);
            }
        }

        return problems;
    }
}
