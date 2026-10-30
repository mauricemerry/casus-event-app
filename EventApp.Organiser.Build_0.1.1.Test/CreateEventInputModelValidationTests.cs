using EventApp.OrganiserPortal.Models;
using Sparkly.OrganiserPortal.Models;

namespace EventApp.Organiser.Build_0._1._1.Test;

[TestFixture]
public class CreateEventInputModelValidationTests
{
    [Test]
    public void GetProblems_NameAndEnvironment_RequiresNameAndEnvironment()
    {
        EventInputModel input = new();

        EventInputProblems problems = input.GetProblems(EventFormTab.NAME_AND_ENVIRONMENT);

        Assert.Multiple(() =>
        {
            Assert.That(problems.HasError(EventInputError.NO_NAME), Is.True);
            Assert.That(problems.HasError(EventInputError.NO_ENVIRONMENT), Is.True);
            Assert.That(problems.HasError(EventInputError.NO_IMAGES), Is.True);
        });
    }

    [Test]
    public void GetProblems_DateAndTime_RequiresCompletePeriod()
    {
        EventInputModel input = new();
        input.Periods.Add(new EventPeriodInputModel { Name = "Dag 1" });

        EventInputProblems problems = input.GetProblems(EventFormTab.DATE_AND_TIME);

        Assert.Multiple(() =>
        {
            Assert.That(problems.HasError(EventInputError.PERIOD_NO_START_DATE), Is.True);
            Assert.That(problems.HasError(EventInputError.PERIOD_NO_START_TIME), Is.True);
            Assert.That(problems.HasError(EventInputError.PERIOD_NO_END_DATE), Is.True);
            Assert.That(problems.HasError(EventInputError.PERIOD_NO_END_TIME), Is.True);
        });
    }

    [Test]
    public void GetProblems_Locations_RequiresLocationFields()
    {
        EventInputModel input = new();
        input.Locations.Add(new EventLocationInputModel());

        EventInputProblems problems = input.GetProblems(EventFormTab.LOCATIONS);

        Assert.Multiple(() =>
        {
            Assert.That(problems.HasError(EventInputError.LOCATION_NO_NAME), Is.True);
            Assert.That(problems.HasError(EventInputError.LOCATION_NO_CITY), Is.True);
            Assert.That(problems.HasError(EventInputError.LOCATION_NO_STREET), Is.True);
            Assert.That(problems.HasError(EventInputError.LOCATION_NO_NUMBER), Is.True);
            Assert.That(problems.HasError(EventInputError.LOCATION_NO_POSTAL_CODE), Is.True);
        });
    }

    [Test]
    public void GetProblems_Tickets_FormatsTicketNumber()
    {
        EventInputModel input = new();
        input.Tickets.Add(new TicketInputModel { TypeName = "Dagticket" });

        EventInputProblems problems = input.GetProblems(EventFormTab.TICKETS);

        Assert.That(problems.GetFeedback(), Does.Contain("Ticket 1: vul een naam in."));
    }
}
