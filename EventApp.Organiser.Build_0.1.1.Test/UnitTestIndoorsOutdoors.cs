using EventApp.OrganiserPortal.Models;
using EventApp.OrganiserPortal.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Moq;

namespace EventApp.Organiser.Build_0._1._1.Test
{
    [TestFixture]
    public class InOutDoorsTests
    {   
        //jesse
        //Arrange
        [TestCase(1, "Binnen")]
        [TestCase(2, "Buiten")]
        [TestCase(3, "Beide")]
        [TestCase(-1, "why not?")]
        public void GetMessage_WithValidValue_ReturnsCorrectMessage(int value, string expected)
        {
            // Act
            var result = IndoorsOutdoors.GetMessage(value);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }
        //jesse
        //Assert
        [TestCase("Binnen", 1)]
        [TestCase("Buiten", 2)]
        [TestCase("Beide", 3)]
        [TestCase("", -1)]
        public void GetValue_WithValidMessage_ReturnsCorrectValue(string message, int expected)
        {
            // Act
            var result = IndoorsOutdoors.GetValue(message);
            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
