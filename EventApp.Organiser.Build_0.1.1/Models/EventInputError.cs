namespace EventApp.OrganiserPortal.Models
{
    public class EventInputError
    {
        public static readonly EventInputError NO_NAME = new(EventFormTab.NAME_AND_ENVIRONMENT, "Vul een naam voor het evenement in (maximaal 255 tekens).");
        public static readonly EventInputError NAME_TOO_LONG = new(EventFormTab.NAME_AND_ENVIRONMENT, "Naam voor het evenement mag maximaal 255 tekens bevatten.");
        public static readonly EventInputError NO_ENVIRONMENT = new(EventFormTab.NAME_AND_ENVIRONMENT, "Kies minimaal één omgeving.");
        public static readonly EventInputError NO_IMAGES = new(EventFormTab.NAME_AND_ENVIRONMENT, "Upload minimaal 1 en maximaal 10 afbeeldingen.");
        public static readonly EventInputError IMAGE_EMPTY = new(EventFormTab.NAME_AND_ENVIRONMENT, "Afbeelding {} is leeg. Raar!.");
        public static readonly EventInputError IMAGE_TOO_LARGE = new(EventFormTab.NAME_AND_ENVIRONMENT, "Afbeelding {} is te groot (maximaal 1024x1024).");
        public static readonly EventInputError IMAGE_INVALID_FORMAT = new(EventFormTab.NAME_AND_ENVIRONMENT, "Afbeelding {} heeft een ongeldig formaat. Alleen {} zijn toegestaan.");
        public static readonly EventInputError NO_PERIODS = new(EventFormTab.DATE_AND_TIME, "Voeg minimaal één periode toe.");
        public static readonly EventInputError PERIOD_NO_NAME = new(EventFormTab.DATE_AND_TIME, "Periode {0}: vul een naam in (maximaal 100 tekens).");
        public static readonly EventInputError PERIOD_NO_START_DATE = new(EventFormTab.DATE_AND_TIME, "Periode {0}: vul een begindatum in.");
        public static readonly EventInputError PERIOD_NO_START_TIME = new(EventFormTab.DATE_AND_TIME, "Periode {0}: vul een begintijd in.");
        public static readonly EventInputError PERIOD_NO_END_DATE = new(EventFormTab.DATE_AND_TIME, "Periode {0}: vul een einddatum in.");
        public static readonly EventInputError PERIOD_NO_END_TIME = new(EventFormTab.DATE_AND_TIME, "Periode {0}: vul een eindtijd.");
        public static readonly EventInputError PERIOD_NAME_TOO_LONG = new(EventFormTab.DATE_AND_TIME, "Periode {0}: naam mag maximaal 100 tekens bevatten.");
        public static readonly EventInputError PERIOD_START_AFTER_END = new(EventFormTab.DATE_AND_TIME, "Periode {0}: beginmoment moet voor eindmoment liggen.");
        public static readonly EventInputError NO_LOCATIONS = new(EventFormTab.LOCATIONS, "Voeg minimaal één locatie toe.");
        public static readonly EventInputError LOCATION_NO_NAME = new(EventFormTab.LOCATIONS, "Locatie {0}: vul een naam in (maximaal 100 tekens).");
        public static readonly EventInputError LOCATION_NO_CITY = new(EventFormTab.LOCATIONS, "Locatie {0}: vul een stad in.");
        public static readonly EventInputError LOCATION_NO_STREET = new(EventFormTab.LOCATIONS, "Locatie {0}: vul een straatnaam in.");
        public static readonly EventInputError LOCATION_NO_NUMBER = new(EventFormTab.LOCATIONS, "Locatie {0}: vul een huisnummer in.");
        public static readonly EventInputError LOCATION_NO_POSTAL_CODE = new(EventFormTab.LOCATIONS, "Locatie {0}: vul een postcode in.");
        public static readonly EventInputError LOCATION_NAME_TOO_LONG = new(EventFormTab.LOCATIONS, "Locatie {0}: naam mag maximaal 100 tekens bevatten.");
        public static readonly EventInputError LOCATION_CITY_TOO_LONG = new(EventFormTab.LOCATIONS, "Locatie {0}: stad mag maximaal 100 tekens bevatten.");
        public static readonly EventInputError LOCATION_STREET_TOO_LONG = new(EventFormTab.LOCATIONS, "Locatie {0}: straatnaam mag maximaal 150 tekens bevatten.");
        public static readonly EventInputError LOCATION_NUMBER_TOO_LONG = new(EventFormTab.LOCATIONS, "Locatie {0}: huisnummer mag maximaal 20 tekens bevatten.");
        public static readonly EventInputError LOCATION_ADDITION_TOO_LONG = new(EventFormTab.LOCATIONS, "Locatie {0}: toevoeging mag maximaal 20 tekens bevatten.");
        public static readonly EventInputError LOCATION_POSTAL_CODE_TOO_LONG = new(EventFormTab.LOCATIONS, "Locatie {0}: postcode mag maximaal 20 tekens bevatten.");
        public static readonly EventInputError LOCATION_INVALID_COORDINATES = new(EventFormTab.LOCATIONS, "Locatie {0}: ongeldige coordinaten. Zet een pin op de kaart om een locatie te kiezen.");
        public static readonly EventInputError TICKET_NO_NAME = new(EventFormTab.TICKETS, "Ticket {0}: vul een naam in.");
        public static readonly EventInputError TICKET_NO_TYPE = new(EventFormTab.TICKETS, "Ticket {0}: kies een ticket type.");
        public static readonly EventInputError TICKET_NO_DATE = new(EventFormTab.TICKETS, "Ticket {0}: kies een datum.");
        public static readonly EventInputError TICKET_NO_PERIOD = new(EventFormTab.TICKETS, "Ticket {0}: voeg minimaal één dag toe.");
        public static readonly EventInputError TICKET_NO_SLOT_START = new(EventFormTab.TICKETS, "Ticket {0}: kies een startmoment voor het tijdslot.");
        public static readonly EventInputError TICKET_NO_SLOT_END = new(EventFormTab.TICKETS, "Ticket {0}: kies een eindmoment voor het tijdslot.");
        public static readonly EventInputError TICKET_NAME_TOO_LONG = new(EventFormTab.TICKETS, "Ticket {0}: naam mag maximaal 100 tekens bevatten.");
        public static readonly EventInputError TICKET_SLOT_START_AFTER_END = new(EventFormTab.TICKETS, "Ticket {0}: tijslot startmoment moet voor eindmoment liggen.");
        public static readonly EventInputError CUSTOM_ACCESSIBILITY_NO_DESCRIPTION = new(EventFormTab.ACCESSIBILITY, "Geef bij Overige een toelichting (maximaal 240 tekens).");
        public static readonly EventInputError INVALID_ACCESSIBILITY_OPTION = new(EventFormTab.ACCESSIBILITY, "Ongeldige toegankelijkheidsoptie(s): {}.");
        public static readonly EventInputError NO_CATEGORY = new(EventFormTab.ACCESSIBILITY, "Kies een categorie.");
        public static readonly EventInputError TAG_TOO_LONG = new(EventFormTab.ACCESSIBILITY, "Tags mogen maximaal 100 tekens bevatten.");

        private readonly EventFormTab tab;
        private readonly string feedback;

        private EventInputError(EventFormTab tab, string feedback)
        {
            if (tab == EventFormTab.NONE || tab == EventFormTab.ALL)
            {
                throw new ArgumentException("tab cannot be NONE or ALL");
            }

            this.tab = tab;
            this.feedback = feedback;
        }

        public bool IsIn(EventFormTab tab)
        {
            return tab == EventFormTab.ALL || this.tab == tab;
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
