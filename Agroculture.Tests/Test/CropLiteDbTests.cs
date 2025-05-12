using LiteDB;
using Xunit;
using Agroculture.Models;
using Agroculture.Services;
using System.IO;

namespace Agroculture.Tests
{
    public class CropLiteDbTests
    {
        private const string TestDbFile = "TestAgroculture.db";

        // Цей метод виконається перед кожним тестом
        public CropLiteDbTests()
        {
            // Якщо є старий файл — видаляємо, щоб тест починався з "чистої" бази
            if (File.Exists(TestDbFile))
                File.Delete(TestDbFile);
        }

        [Fact(DisplayName = "Can insert and retrieve Crop from LiteDB")]
        public void InsertAndRetrieveCrop()
        {
            // Arrange: створюємо об’єкт Crop
            var crop = new Crop
            {
                ID = 42,
                Name = "Тестова культура",
                PlannedYield = 10.5,
                NutrientUptake = new NutrientUptake { N = 1.1, P2O5 = 2.2, K2O = 3.3 },
                SoilUtilization = new SoilUtilization { N = 0.1, P2O5 = 0.2, K2O = 0.3 },
                FertilizerUtilization = new FertilizerUtilization { N = 0.4, P2O5 = 0.5, K2O = 0.6 },
                RecomendedNextCrop = "Наступна1, Наступна2"
            };

            // Act: зберігаємо й читаємо назад
            using (var db = new LiteDatabase(TestDbFile))
            {
                var col = db.GetCollection<Crop>("crops");
                col.Insert(crop);

                // Зчитуємо об’єкт за ID
                var fromDb = col.FindById(crop.ID);

                // Assert: перевіряємо, що поля співпадають
                Assert.NotNull(fromDb);
                Assert.Equal(crop.ID, fromDb.ID);
                Assert.Equal(crop.Name, fromDb.Name);
                Assert.Equal(crop.PlannedYield, fromDb.PlannedYield);
                Assert.Equal(crop.NutrientUptake.N, fromDb.NutrientUptake.N);
                Assert.Equal(crop.NutrientUptake.P2O5, fromDb.NutrientUptake.P2O5);
                Assert.Equal(crop.NutrientUptake.K2O, fromDb.NutrientUptake.K2O);
                Assert.Equal(crop.SoilUtilization.N, fromDb.SoilUtilization.N);
                Assert.Equal(crop.FertilizerUtilization.K2O, fromDb.FertilizerUtilization.K2O);
                Assert.Equal(crop.RecomendedNextCrop, fromDb.RecomendedNextCrop);
            }
        }
    }
}
