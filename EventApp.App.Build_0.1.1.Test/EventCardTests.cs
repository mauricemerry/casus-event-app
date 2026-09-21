using EventApp.Web.Models;

namespace EventApp.App.Build_0._1._1.Test
{
    public class EventCardTests
    {
        /// <summary>
        /// Author: Maurice Merry
        /// Test that GetCategoryIcon returns the correct icon for known categories.
        /// </summary>
        [Test]
        public void TestEventCard_GetCategoryIcon_CorrectIconForKnownValue()
        {
            // arrange
            string category = "house";
            string expectedIcon = "♪";

            // act
            string receivedIcon = EventCard.GetCategoryIcon(category);

            // assert
            Assert.That(receivedIcon, Is.EqualTo(expectedIcon));
        }

        /// <summary>
        /// Author: Maurice Merry
        /// Test that GetCategoryIcon returns the default icon for unknown categories.
        /// </summary>
        [Test]
        public void TestEventCard_GetCategoryIcon_DefaultIconForUnknownValue()
        {
            // arrange
            string category = "thisisnotavalideventcategoryitisjustalongstringimadeup";
            string expectedIcon = "•";

            // act
            string receivedIcon = EventCard.GetCategoryIcon(category);

            // assert
            Assert.That(receivedIcon, Is.EqualTo(expectedIcon));
        }
    }
}
