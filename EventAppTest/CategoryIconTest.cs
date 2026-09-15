using EventApp.Web.Models;

namespace EventAppTest
{
    public class Tests
    {

        [Test]
        public void CategoryEvent_EventCard_ReturnsCorrectCategory()
        {
            // Arrange
            var eventCard = new EventCard();
            var category = "House";

            // Act
            var result = eventCard.GetCategoryIcon(category);

            // Assert
            Assert.That(result, Is.EqualTo("♪"));
        }
    }
}
