using System.IO;
using LiteDB;
using Xunit;
using Assert = Xunit.Assert;

using Agroculture.Models;

namespace Agroculture.Tests
{
    public class SoilLiteDbTests
    {
        private const string TestDbFile = "TestSoil.db";

        public SoilLiteDbTests()
        {
            // Перед кожним тестом видаляємо старий файл
            if (File.Exists(TestDbFile))
                File.Delete(TestDbFile);
        }

        [Fact(DisplayName = "ToString returns Name")]
        public void ToString_ReturnsName()
        {
            // Arrange
            var soil = new Soil
            {
                ID = 1,
                Name = "Чорнозем",
                DefaultN = 100,
                DefaultP2O5 = 50,
                DefaultK2O = 80,
                BulkDensity = 1.3
            };

            // Act
            var str = soil.ToString();

            // Assert
            Assert.Equal("Чорнозем", str);
        }

        [Fact(DisplayName = "Can insert and retrieve Soil from LiteDB")]
        public void LiteDb_RoundTrip_Soil()
        {
            // Arrange
            var soil = new Soil
            {
                ID = 7,
                Name = "Підзол",
                DefaultN = 75.5,
                DefaultP2O5 = 65.2,
                DefaultK2O = 70.1,
                BulkDensity = 1.35
            };

            // Act: запис у LiteDB та зчитування
            using (var db = new LiteDatabase(TestDbFile))
            {
                var col = db.GetCollection<Soil>("soils");
                col.Insert(soil);
                var fromDb = col.FindById(soil.ID);

                // Assert
                Assert.NotNull(fromDb);
                Assert.Equal(soil.ID, fromDb.ID);
                Assert.Equal(soil.Name, fromDb.Name);
                Assert.Equal(soil.DefaultN, fromDb.DefaultN);
                Assert.Equal(soil.DefaultP2O5, fromDb.DefaultP2O5);
                Assert.Equal(soil.DefaultK2O, fromDb.DefaultK2O);
                Assert.Equal(soil.BulkDensity, fromDb.BulkDensity);
            }
        }
    }
}
