using EventApp.OrganiserPortal.Models;
using EventApp.OrganiserPortal.Repositories;
using Moq;

namespace EventApp.Organiser.Build_0._1._1.Test
{
    [TestFixture]
    public class InOutDoorsTests
    {
        [Test]
        public void GetMessage_WithValue1_ReturnsBinnen()
        {
            // Arrange
            int value = 1;

            // Act
            var result = IndoorsOutdoors.GetMessage(value);

            // Assert
            Assert.That(result, Is.EqualTo("Binnen"));
        }
        [Test]
        public void GetMessage_WithValue1_Returnsbijde()
        {
            // Arrange
            int value = 3;

            // Act
            var result = IndoorsOutdoors.GetMessage(value);

            // Assert
            Assert.That(result, Is.EqualTo("Beide"));
        }
        [Test]
        public void GetMessage_WithValue1_ReturnsBuiten()
        {
            // Arrange
            int value = 2;

            // Act
            var result = IndoorsOutdoors.GetMessage(value);

            // Assert
            Assert.That(result, Is.EqualTo("Buiten"));
        }
    }
}
