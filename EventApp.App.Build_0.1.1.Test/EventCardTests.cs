using EventApp.Web.Models;

namespace EventApp.App.Build_0._1._1.Test
{
    public class EventCardTests
    {
        /// <summary>
        /// Author: Maurice Merry
        /// Test that GetCategoryIcon returns the correct icon for known categories.
        /// </summary>
        // arrange
        [TestCase("house", "♪")]
        [TestCase("kunst", "✎")]
        [TestCase("historie", "⌛")]
        [TestCase("film", "◉")]
        [TestCase("toneel", "◌")]
        [TestCase("dans", "♬")]
        [TestCase("drank", "☕")]
        public void TestEventCard_GetCategoryIcon_CorrectIconForKnownValue(string category, string expectedIcon)
        {
            // arrange
            EventCard eventCard = new();

            // act
            string receivedIcon = eventCard.GetCategoryIcon(category);

            // assert
            Assert.That(receivedIcon, Is.EqualTo(expectedIcon));
        }

        /// <summary>
        /// Author: Maurice Merry
        /// Test that GetCategoryIcon returns the default icon for unknown categories.
        /// </summary>
        // arrange
        [TestCase("thisisnotavalideventcategoryitisjustalongstringimadeup", "•")]
        [TestCase("", "•")]
        public void TestEventCard_GetCategoryIcon_DefaultIconForUnknownValue(string category, string expectedIcon)
        {
            // arrange
            EventCard eventCard = new();

            // act
            string receivedIcon = eventCard.GetCategoryIcon(category);

            // assert
            Assert.That(receivedIcon, Is.EqualTo(expectedIcon));
        }
    }
}
