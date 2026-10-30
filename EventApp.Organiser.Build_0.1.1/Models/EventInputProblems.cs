using EventApp.OrganiserPortal.Models;

namespace Sparkly.OrganiserPortal.Models
{
    public class EventInputProblems
    {
        private readonly Dictionary<EventInputError, object[]> entries = [];

        public void AddError(EventInputError error, params object[] args)
        {
            this.entries[error] = args;
        }

        public bool HasErrors()
        {
            return this.HasErrors(EventFormTab.ALL);
        }

        public bool HasErrors(EventFormTab tab)
        {
            foreach (EventInputError error in this.entries.Keys)
            {
                if (error.IsIn(tab))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasError(EventInputError error)
        {
            return this.entries.ContainsKey(error);
        }

        public bool HasAnyErrors(params EventInputError[] errors)
        {
            foreach (EventInputError error in errors)
            {
                if (this.HasError(error))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasAllErrors(params EventInputError[] errors)
        {
            foreach (EventInputError error in errors)
            {
                if (!this.HasError(error))
                {
                    return false;
                }
            }

            return true;
        }

        public IList<string> GetFeedback()
        {
            IList<string> feedback = [];

            foreach (KeyValuePair<EventInputError, object[]> entry in entries)
            {
                EventInputError error = entry.Key;
                object[] args = entry.Value;

                try
                {
                    feedback.Add(error.FormatFeedback(args));
                }
                catch (Exception e)
                {
                    feedback.Add(error.Feedback() + "[" + string.Join(", ", args) + "] => " + e.Message);
                }
            }

            return feedback;
        }
    }
}
