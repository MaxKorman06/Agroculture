using System.IO;
using LiteDB;
using Xunit;
using Assert = Xunit.Assert;
using Agroculture.Models;

namespace Agroculture.Tests
{
    public class FieldLiteDbTests
    {
        private const string TestDbFile = "TestField.db";

        public FieldLiteDbTests()
        {
            // Перед кожним тестом видаляємо старий файл, щоб база була чистою
            if (File.Exists(TestDbFile))
                File.Delete(TestDbFile);
        }

        [Fact(DisplayName = "Default Field properties")]
        public void Field_DefaultProperties_AreExpected()
        {
            // Arrange & Act
            var field = new Field();

            // Assert
            Assert.Equal(0, field.Year);
            Assert.False(field.IsSoilFixed);
        }

        [Fact(DisplayName = "Can insert and retrieve Field with nested Soil and Crop")]
        public void Field_LiteDb_RoundTrip()
        {
            // Arrange: створюємо обʼєкт Field з вкладеними Soil і Crop
            var soil = new Soil
            {
                ID = 1,
                Name = "Тестовий ґрунт",
                DefaultN = 10,
                DefaultP2O5 = 20,
                DefaultK2O = 30,
                BulkDensity = 1.4
            };
            var crop = new Crop
            {
                ID = 2,
                Name = "Тестова культура",
                PlannedYield = 50,
                NutrientUptake = new NutrientUptake { N = 1, P2O5 = 2, K2O = 3 },
                SoilUtilization = new SoilUtilization { N = 0.1, P2O5 = 0.2, K2O = 0.3 },
                FertilizerUtilization = new FertilizerUtilization { N = 0.4, P2O5 = 0.5, K2O = 0.6 },
                RecomendedNextCrop = "A,B"
            };
            var field = new Field
            {
                ID = 42,
                Name = "Тестове поле",
                Area = 12.34,
                SelectedSoil = soil,
                CurrentN = 11,
                CurrentP2O5 = 22,
                CurrentK2O = 33,
                CurrentCrop = crop,
                PastCrop = crop,
                Year = 3,
                IsSoilFixed = true
            };

            // Act: вставка та читання
            using (var db = new LiteDatabase(TestDbFile))
            {
                var col = db.GetCollection<Field>("fields");
                col.Insert(field);

                var fromDb = col.FindById(field.ID);

                // Assert: перевіряємо всі поля
                Assert.NotNull(fromDb);
                Assert.Equal(field.ID, fromDb.ID);
                Assert.Equal(field.Name, fromDb.Name);
                Assert.Equal(field.Area, fromDb.Area);
                // SelectedSoil
                Assert.NotNull(fromDb.SelectedSoil);
                Assert.Equal(soil.ID, fromDb.SelectedSoil.ID);
                Assert.Equal(soil.Name, fromDb.SelectedSoil.Name);
                Assert.Equal(soil.DefaultN, fromDb.SelectedSoil.DefaultN);
                Assert.Equal(soil.BulkDensity, fromDb.SelectedSoil.BulkDensity);
                // Current values
                Assert.Equal(field.CurrentN, fromDb.CurrentN);
                Assert.Equal(field.CurrentP2O5, fromDb.CurrentP2O5);
                Assert.Equal(field.CurrentK2O, fromDb.CurrentK2O);
                // CurrentCrop
                Assert.NotNull(fromDb.CurrentCrop);
                Assert.Equal(crop.ID, fromDb.CurrentCrop.ID);
                Assert.Equal(crop.Name, fromDb.CurrentCrop.Name);
                // PastCrop
                Assert.NotNull(fromDb.PastCrop);
                Assert.Equal(crop.ID, fromDb.PastCrop.ID);
                // Year and IsSoilFixed
                Assert.Equal(field.Year, fromDb.Year);
                Assert.True(fromDb.IsSoilFixed);
            }
        }
    }
}
