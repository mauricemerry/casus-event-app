using EventApp.OrganiserPortal.Models;
using EventApp.OrganiserPortal.Repositories;
using Moq;

namespace EventApp.Organiser.Build_0._1._1.Test
{
    [TestFixture]
    public class Tests
    {
        private Mock<IOrganiserRepository> organiserRepository = new Mock<IOrganiserRepository>();

        [SetUp]
        public void Setup()
        {
            this.organiserRepository.Setup(r => r.GetNextEventIdAsync()).ReturnsAsync(7);
        }

        [Test]
        public async Task Test1()
        {
            Assert.That(await this.organiserRepository.Object.GetNextEventIdAsync(), Is.EqualTo(7));
        }
    }
}
