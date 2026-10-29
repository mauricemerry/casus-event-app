namespace EventApp.OrganiserPortal.Models
{
    public class AccessibilityOption
    {
        private static readonly Dictionary<string, AccessibilityOption> BY_DESCRIPTION = [];

        public static readonly AccessibilityOption WHEELCHAIR_ACCESSIBLE = new("Rolstoeltoegankelijk");
        public static readonly AccessibilityOption ELEVATOR = new("Lift aanwezig");
        public static readonly AccessibilityOption TOILET = new("Toegankelijk toilet");
        public static readonly AccessibilityOption SIGN_LANGUAGE_INTERPRETER = new("Gebarentolk");
        public static readonly AccessibilityOption REST_ROOM = new("Rustruimte");
        public static readonly AccessibilityOption HARD_OF_HEARING = new("Slechthorenden");
        public static readonly AccessibilityOption VISUALLY_IMPAIRED = new("Slechtzienden");
        public static readonly AccessibilityOption GUIDE_DOG_ALLOWED = new("Geleidehond toegestaan");

        private readonly string description;

        private AccessibilityOption(string description)
        {
            this.description = description;
            BY_DESCRIPTION.Add(description, this);
        }

        public string Description()
        {
            return this.description;
        }

        public static AccessibilityOption? ByDescription(string description)
        {
            return BY_DESCRIPTION.GetValueOrDefault(description);
        }
    }
}
