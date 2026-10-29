namespace EventApp.OrganiserPortal.Models
{
    public class CreateEventError
    {
        public static readonly CreateEventError NO_NAME = new(CreateEventTab.NAME_AND_ENVIRONMENT, "Vul een naam voor het evenement in (maximaal 255 tekens).");
        public static readonly CreateEventError NAME_TOO_LONG = new(CreateEventTab.NAME_AND_ENVIRONMENT, "Naam voor het evenement mag maximaal 255 tekens bevatten.");
        public static readonly CreateEventError NO_ENVIRONMENT = new(CreateEventTab.NAME_AND_ENVIRONMENT, "Kies minimaal één omgeving.");
        public static readonly CreateEventError NO_IMAGES = new(CreateEventTab.NAME_AND_ENVIRONMENT, "Upload minimaal 1 en maximaal 10 afbeeldingen.");
        public static readonly CreateEventError IMAGE_EMPTY = new(CreateEventTab.NAME_AND_ENVIRONMENT, "Afbeelding {} is leeg. Raar!.");
        public static readonly CreateEventError IMAGE_TOO_LARGE = new(CreateEventTab.NAME_AND_ENVIRONMENT, "Afbeelding {} is te groot (maximaal 1024x1024).");
        public static readonly CreateEventError IMAGE_INVALID_FORMAT = new(CreateEventTab.NAME_AND_ENVIRONMENT, "Afbeelding {} heeft een ongeldig formaat. Alleen {} zijn toegestaan.");
        public static readonly CreateEventError NO_PERIODS = new(CreateEventTab.DATE_AND_TIME, "Voeg minimaal één periode toe.");
        public static readonly CreateEventError PERIOD_NO_NAME = new(CreateEventTab.DATE_AND_TIME, "Periode {0}: vul een naam in (maximaal 100 tekens).");
        public static readonly CreateEventError PERIOD_NO_START_DATE = new(CreateEventTab.DATE_AND_TIME, "Periode {0}: vul een begindatum in.");
        public static readonly CreateEventError PERIOD_NO_START_TIME = new(CreateEventTab.DATE_AND_TIME, "Periode {0}: vul een begintijd in.");
        public static readonly CreateEventError PERIOD_NO_END_DATE = new(CreateEventTab.DATE_AND_TIME, "Periode {0}: vul een einddatum in.");
        public static readonly CreateEventError PERIOD_NO_END_TIME = new(CreateEventTab.DATE_AND_TIME, "Periode {0}: vul een eindtijd.");
        public static readonly CreateEventError PERIOD_NAME_TOO_LONG = new(CreateEventTab.DATE_AND_TIME, "Periode {0}: naam mag maximaal 100 tekens bevatten.");
        public static readonly CreateEventError PERIOD_START_AFTER_END = new(CreateEventTab.DATE_AND_TIME, "Periode {0}: beginmoment moet voor eindmoment liggen.");
        public static readonly CreateEventError NO_LOCATIONS = new(CreateEventTab.LOCATIONS, "Voeg minimaal één locatie toe.");
        public static readonly CreateEventError LOCATION_NO_NAME = new(CreateEventTab.LOCATIONS, "Locatie {0}: vul een naam in (maximaal 100 tekens).");
        public static readonly CreateEventError LOCATION_NO_CITY = new(CreateEventTab.LOCATIONS, "Locatie {0}: vul een stad in.");
        public static readonly CreateEventError LOCATION_NO_STREET = new(CreateEventTab.LOCATIONS, "Locatie {0}: vul een straatnaam in.");
        public static readonly CreateEventError LOCATION_NO_NUMBER = new(CreateEventTab.LOCATIONS, "Locatie {0}: vul een huisnummer in.");
        public static readonly CreateEventError LOCATION_NO_POSTAL_CODE = new(CreateEventTab.LOCATIONS, "Locatie {0}: vul een postcode in.");
        public static readonly CreateEventError LOCATION_NAME_TOO_LONG = new(CreateEventTab.LOCATIONS, "Locatie {0}: naam mag maximaal 100 tekens bevatten.");
        public static readonly CreateEventError LOCATION_CITY_TOO_LONG = new(CreateEventTab.LOCATIONS, "Locatie {0}: stad mag maximaal 100 tekens bevatten.");
        public static readonly CreateEventError LOCATION_STREET_TOO_LONG = new(CreateEventTab.LOCATIONS, "Locatie {0}: straatnaam mag maximaal 150 tekens bevatten.");
        public static readonly CreateEventError LOCATION_NUMBER_TOO_LONG = new(CreateEventTab.LOCATIONS, "Locatie {0}: huisnummer mag maximaal 20 tekens bevatten.");
        public static readonly CreateEventError LOCATION_ADDITION_TOO_LONG = new(CreateEventTab.LOCATIONS, "Locatie {0}: toevoeging mag maximaal 20 tekens bevatten.");
        public static readonly CreateEventError LOCATION_POSTAL_CODE_TOO_LONG = new(CreateEventTab.LOCATIONS, "Locatie {0}: postcode mag maximaal 20 tekens bevatten.");
        public static readonly CreateEventError LOCATION_INVALID_COORDINATES = new(CreateEventTab.LOCATIONS, "Locatie {0}: ongeldige coordinaten. Zet een pin op de kaart om een locatie te kiezen.");
        public static readonly CreateEventError TICKET_NO_NAME = new(CreateEventTab.TICKETS, "Ticket {0}: vul een naam in.");
        public static readonly CreateEventError TICKET_NO_TYPE = new(CreateEventTab.TICKETS, "Ticket {0}: kies een ticket type.");
        public static readonly CreateEventError TICKET_NO_DATE = new(CreateEventTab.TICKETS, "Ticket {0}: kies een datum.");
        public static readonly CreateEventError TICKET_NO_PERIOD = new(CreateEventTab.TICKETS, "Ticket {0}: voeg minimaal één dag toe.");
        public static readonly CreateEventError TICKET_NO_SLOT_START = new(CreateEventTab.TICKETS, "Ticket {0}: kies een startmoment voor het tijdslot.");
        public static readonly CreateEventError TICKET_NO_SLOT_END = new(CreateEventTab.TICKETS, "Ticket {0}: kies een eindmoment voor het tijdslot.");
        public static readonly CreateEventError TICKET_NAME_TOO_LONG = new(CreateEventTab.TICKETS, "Ticket {0}: naam mag maximaal 100 tekens bevatten.");
        public static readonly CreateEventError TICKET_SLOT_START_AFTER_END = new(CreateEventTab.TICKETS, "Ticket {0}: tijslot startmoment moet voor eindmoment liggen.");
        public static readonly CreateEventError CUSTOM_ACCESSIBILITY_NO_DESCRIPTION = new(CreateEventTab.ACCESSIBILITY, "Geef bij Overige een toelichting (maximaal 240 tekens).");
        public static readonly CreateEventError INVALID_ACCESSIBILITY_OPTION = new(CreateEventTab.ACCESSIBILITY, "Ongeldige toegankelijkheidsoptie(s): {}.");
        public static readonly CreateEventError NO_CATEGORY = new(CreateEventTab.ACCESSIBILITY, "Kies een categorie.");
        public static readonly CreateEventError TAG_TOO_LONG = new(CreateEventTab.ACCESSIBILITY, "Tags mogen maximaal 100 tekens bevatten.");

        private readonly CreateEventTab tab;
        private readonly string feedback;

        private CreateEventError(CreateEventTab tab, string feedback)
        {
            if (tab == CreateEventTab.NONE || tab == CreateEventTab.ALL)
            {
                throw new ArgumentException("tab cannot be NONE or ALL");
            }

            this.tab = tab;
            this.feedback = feedback;
        }

        public bool IsIn(CreateEventTab tab)
        {
            return tab == CreateEventTab.ALL || this.tab == tab;
        }

        public string Feedback()
        {
            return this.feedback;
        }

        public string FormatFeedback(params object[] args)
        {
            return string.Format(this.feedback, args);
        }
    }
}
