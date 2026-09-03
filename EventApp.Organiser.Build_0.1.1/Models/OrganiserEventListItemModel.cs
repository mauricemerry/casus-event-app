namespace EventApp.OrganiserPortal.Models
{
    public class OrganiserEventListItemModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string IndoorsOutdoors { get; set; } = string.Empty;
        public string Description {  get; set; } = string.Empty;
        public string ImageUrl { get; set;  } = string.Empty;
        public decimal? MinPrice { get; set; }
        public string CategoriesText {  get; set; } = string.Empty;
        public string AccessibilityText { get; set; } = string.Empty; 
        public bool IsCanceled { get; set; }
    }
}
