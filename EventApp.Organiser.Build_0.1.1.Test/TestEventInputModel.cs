using EventApp.OrganiserPortal.Models;
using Sparkly.OrganiserPortal.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventApp.Organiser.Build_0._1._1.Test
{
    internal class TestEventInputModel
    {

        /// <summary>
        /// author: Maurice Merry
        /// </summary>
        [TestCase("test", true, true, true)]
        [TestCase("test", false, true, false)]
        [TestCase("dit is een heeeeeeeeeele lange naam, maar dan bedoel ik ook echt pittig lang", true, true, true)]
        public void TestEventInputModel_Tab_NAME_AND_ENVIRONMENT_NoErrors(string name, bool indoor, bool outdoor, bool online)
        {
            // arrange
            EventFormTab tab = EventFormTab.NAME_AND_ENVIRONMENT;
            EventInputModel input = new()
            {
                EventName = name,
                IsIndoor = indoor,
                IsOutdoor = outdoor,
                IsOnline = online
            };

            // act
            EventInputProblems problems = input.GetProblems(tab);
            bool errors = problems.HasErrors();

            // assert
            Assert.That(errors, Is.EqualTo(false), "problems: " + string.Join(" | ", problems.GetFeedback()));
        }

        /// <summary>
        /// author: Maurice Merry
        /// </summary>
        [TestCase("", true, true, true)]
        [TestCase("test", false, false, false)]
        [TestCase("dit is een heeeeeeeeeele lange naam, maar dan bedoel ik ook echt pittig lang, zo lang dat het de maximale lengte voor een eenementnaam overschrijdt, hopelijk is dit genoeg want ik ben niet aan het tellen", true, true, true)]
        public void TestEventInputModel_Tab_NAME_AND_ENVIRONMENT_WithErrors(string name, bool indoor, bool outdoor, bool online)
        {
            // arrange
            EventFormTab tab = EventFormTab.NAME_AND_ENVIRONMENT;
            EventInputModel input = new()
            {
                EventName = name,
                IsIndoor = indoor,
                IsOutdoor = outdoor,
                IsOnline = online
            };

            // act
            EventInputProblems problems = input.GetProblems(tab);
            bool errors = problems.HasErrors();

            // assert
            Assert.That(errors, Is.EqualTo(true), "problems: " + string.Join(" | ", problems.GetFeedback()));
        }
    }
}
