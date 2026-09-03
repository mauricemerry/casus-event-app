using System.ComponentModel.DataAnnotations;

namespace EventApp.OrganiserPortal.Models
{
    public class IndoorsOutdoors
    {
        public static readonly string[] MESSAGES =
        {
            "Binnen",
            "Buiten",
            "Beide"
        };
        public static readonly string MESSAGES_NATURAL_LIST = "Binnen, Buiten, of Beide";

        public static string GetMessage(int value)
        {
            return MESSAGES[value - 1];
        }

        public static int GetValue(string message)
        {
            int value = -1;

            for (int i = 0; i < MESSAGES.Length; i++)
            {
                if (MESSAGES[i].Equals(message))
                {
                    value = i + 1;
                }
            }

            return value;
        }
    }
}