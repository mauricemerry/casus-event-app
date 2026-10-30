using EventApp.OrganiserPortal.Models;

namespace Sparkly.OrganiserPortal.Models
{
    public class CreateEventProblems
    {
        private readonly Dictionary<CreateEventError, object[]> entries = [];

        public void AddError(CreateEventError error, params object[] args)
        {
            this.entries[error] = args;
        }

        public bool HasErrors()
        {
            return this.HasErrors(CreateEventTab.ALL);
        }

        public bool HasErrors(CreateEventTab tab)
        {
            foreach (CreateEventError error in this.entries.Keys)
            {
                if (error.IsIn(tab))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasError(CreateEventError error)
        {
            return this.entries.ContainsKey(error);
        }

        public bool HasAnyErrors(params CreateEventError[] errors)
        {
            foreach (CreateEventError error in errors)
            {
                if (this.HasError(error))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasAllErrors(params CreateEventError[] errors)
        {
            foreach (CreateEventError error in errors)
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

            foreach (KeyValuePair<CreateEventError, object[]> entry in entries)
            {
                CreateEventError error = entry.Key;
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
