using System.ComponentModel.DataAnnotations;

namespace EventApp.OrganiserPortal.Models
{
    public class EventEnvironment
    {
        public static readonly int FLAG_INDOORS  = 0b001;
        public static readonly int FLAG_OUTDOORS = 0b010;
        public static readonly int FLAG_ONLINE   = 0b100;

        public static int GetFlags(bool indoors, bool outdoors, bool online)
        {
            int flags = 0;

            if (indoors)
            {
                flags |= FLAG_INDOORS;
            }
            if (outdoors)
            {
                flags |= FLAG_OUTDOORS;
            }
            if (online)
            {
                flags |= FLAG_ONLINE;
            }

            return flags;
        }

        public static string GetDescription(bool indoors, bool outdoors, bool online)
        {
            IList<string> names = [];

            if (indoors)
            {
                names.Add("Binnen");
            }
            if (outdoors)
            {
                names.Add("Buiten");
            }
            if (online)
            {
                names.Add("Online");
            }

            return string.Join(", ", names);
        }

        public static bool IsIndoors(int flags)
        {
            return (flags & FLAG_INDOORS) != 0;
        }

        public static bool IsOutdoors(int flags)
        {
            return (flags & FLAG_OUTDOORS) != 0;
        }

        public static bool IsOnline(int flags)
        {
            return (flags & FLAG_ONLINE) != 0;
        }

        public static string GetDescription(int flags)
        {
            return GetDescription(
                IsIndoors(flags),
                IsOutdoors(flags),
                IsOnline(flags)
            );
        }
    }
}