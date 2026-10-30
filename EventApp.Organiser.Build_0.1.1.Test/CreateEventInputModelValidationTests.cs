using EventApp.OrganiserPortal.Models;
using Sparkly.OrganiserPortal.Models;

namespace EventApp.Organiser.Build_0._1._1.Test;

[TestFixture]
public class CreateEventInputModelValidationTests
{
    [Test]
    public void GetProblems_NameAndEnvironment_RequiresNameAndEnvironment()
    {
        CreateEventInputModel input = new();

        CreateEventProblems problems = input.GetProblems(CreateEventTab.NAME_AND_ENVIRONMENT);

        Assert.Multiple(() =>
        {
            Assert.That(problems.HasError(CreateEventError.NO_NAME), Is.True);
            Assert.That(problems.HasError(CreateEventError.NO_ENVIRONMENT), Is.True);
            Assert.That(problems.HasError(CreateEventError.NO_IMAGES), Is.True);
        });
    }

    [Test]
    public void GetProblems_DateAndTime_RequiresCompletePeriod()
    {
        CreateEventInputModel input = new();
        input.Periods.Add(new EventPeriodInputModel { Name = "Dag 1" });

        CreateEventProblems problems = input.GetProblems(CreateEventTab.DATE_AND_TIME);

        Assert.Multiple(() =>
        {
            Assert.That(problems.HasError(CreateEventError.PERIOD_NO_START_DATE), Is.True);
            Assert.That(problems.HasError(CreateEventError.PERIOD_NO_START_TIME), Is.True);
            Assert.That(problems.HasError(CreateEventError.PERIOD_NO_END_DATE), Is.True);
            Assert.That(problems.HasError(CreateEventError.PERIOD_NO_END_TIME), Is.True);
        });
    }

    [Test]
    public void GetProblems_Locations_RequiresLocationFields()
    {
        CreateEventInputModel input = new();
        input.Locations.Add(new EventLocationInputModel());

        CreateEventProblems problems = input.GetProblems(CreateEventTab.LOCATIONS);

        Assert.Multiple(() =>
        {
            Assert.That(problems.HasError(CreateEventError.LOCATION_NO_NAME), Is.True);
            Assert.That(problems.HasError(CreateEventError.LOCATION_NO_CITY), Is.True);
            Assert.That(problems.HasError(CreateEventError.LOCATION_NO_STREET), Is.True);
            Assert.That(problems.HasError(CreateEventError.LOCATION_NO_NUMBER), Is.True);
            Assert.That(problems.HasError(CreateEventError.LOCATION_NO_POSTAL_CODE), Is.True);
        });
    }

    [Test]
    public void GetProblems_Tickets_FormatsTicketNumber()
    {
        CreateEventInputModel input = new();
        input.Tickets.Add(new CreateTicketInputModel { TypeName = "Dagticket" });

        CreateEventProblems problems = input.GetProblems(CreateEventTab.TICKETS);

        Assert.That(problems.GetFeedback(), Does.Contain("Ticket 1: vul een naam in."));
    }
}
