using BaliBotDotNet.Data;
using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using BaliBotDotNet.Modules;
using Moq;

namespace BaliBotTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var mockRepo = new Mock<WordModule>();
            mockRepo.Setup(x => x.GetAlternativeFact(-1)).Returns(GetFacts());
        }

        private List<AlternativeFact> GetFacts()
        {
            BaliBotDbContext db = new BaliBotDbContext();
            List<AlternativeFact> lst = new List<AlternativeFact>();
            var rng = new Random();
            for (int i = 0; i < 100; i++)
            {
                lst.Add(db.AlternativeFacts.FirstOrDefault(x => x.AlternativeFactID == rng.Next(db.AlternativeFacts.Count())));
            }
            return lst;
        }
    }
}