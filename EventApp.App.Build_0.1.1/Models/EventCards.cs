namespace EventApp.Web.Models
{
    public class EventCard
    {

        // waarom string.Empty en niet '??'
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal MinPrice { get; set; }
        public string TicketVariation { get; set; } = string.Empty;
        public int RatingValue { get; set; }
        public string RatingText { get; set; } = string.Empty;
        public bool IsAdultsOnly { get; set; }
        public List<string> Categories { get; set; } = [];
        public string AccessibilityText { get; set; } = string.Empty;

        public static string GetCategoryIcon(String category)
        {
            if (
                category.Contains("house", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("techno", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("edm", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("trance", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("drum", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("hardstyle", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("muziek", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("music", StringComparison.OrdinalIgnoreCase)
            ) return "♪";

            if (
                category.Contains("schilder", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("kunst", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("art", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("tekenen", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("illustratie", StringComparison.OrdinalIgnoreCase)
            ) return "✎";

            if (
                category.Contains("historie", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("history", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("erfgoed", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("museum", StringComparison.OrdinalIgnoreCase)
            ) return "⌛";

            if (
                category.Contains("film", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("cinema", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("documentaire", StringComparison.OrdinalIgnoreCase)
            ) return "◉";

            if (
                category.Contains("theater", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("toneel", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("show", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("performance", StringComparison.OrdinalIgnoreCase)
            ) return "◌";

            if (
                category.Contains("dans", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("dance", StringComparison.OrdinalIgnoreCase)
            ) return "♬";

            if (
                category.Contains("eten", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("food", StringComparison.OrdinalIgnoreCase) ||
                category.Contains("drank", StringComparison.OrdinalIgnoreCase)
            ) return "☕";

            return "•";
        }
    }
}