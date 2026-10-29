using EventApp.OrganiserPortal.Models;
using EventApp.OrganiserPortal.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Moq;
using NUnit.Framework.Internal;

namespace EventApp.Organiser.Build_0._1._1.Test
{
    [TestFixture]
    public class InOutDoorsTests
    {   
        //jesse
        //Arrange
        [TestCase(1, "Binnen")]
        [TestCase(2, "Buiten")]
        [TestCase(3, "Binnen, Buiten")]
        [TestCase(4, "Online")]
        [TestCase(5, "Binnen, Online")]
        [TestCase(6, "Buiten, Online")]
        [TestCase(7, "Binnen, Buiten, Online")]
        public void GetMessage_WithValidValue_ReturnsCorrectMessage(int flags, string expected)
        {
            // Act
            var result = EventEnvironment.GetDescription(flags);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }
        //jesse
        //Assert
        [TestCase(false, false, false, 0)]
        [TestCase(true, false, false, 1)]
        [TestCase(false, true, false, 2)]
        [TestCase(true, true, false, 3)]
        [TestCase(false, false, true, 4)]
        [TestCase(true, false, true, 5)]
        [TestCase(false, true, true, 6)]
        [TestCase(true, true, true, 7)]
        public void GetValue_WithValidMessage_ReturnsCorrectValue(bool indoors, bool outdoors, bool online, int expected)
        {
            // Act
            var result = EventEnvironment.GetFlags(indoors, outdoors, online);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
