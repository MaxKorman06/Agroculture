using Xunit;
using Assert = Xunit.Assert;

using Agroculture.Helpers;

namespace Agroculture.Tests
{
    public class CalculationHelperTests
    {
        [Fact(DisplayName = "CalculateDose returns correct positive values")]
        public void CalculateDose_PositiveCase()
        {
            // Arrange
            double plannedYield = 50.0;        // ц/га
            double nutrientUptake = 2.0;       // кг/ц
            double defaultContent = 100.0;     // мг/100г
            double bulkDensity = 1.4;          // г/см³
            double h = 20.0;                   // см
            double soilUtilization = 0.1;      // частки від ґрунту
            double fertilizerUtilization = 0.5;// частки від добрив
            double area = 10.0;                // га

            // Manually compute expected:
            // dosePerHa = (50*2 - ((100/10)*1.4*20)*0.1) / 0.5
            //           = (100 - (10*1.4*20)*0.1) / 0.5
            //           = (100 - (280)*0.1) / 0.5
            //           = (100 - 28) / 0.5 = 72 / 0.5 = 144
            // totalDose = 144 * 10 = 1440

            double expectedDosePerHa = 144.0;
            double expectedTotalDose = 1440.0;

            // Act
            var (dosePerHa, totalDose) = CalculationHelper.CalculateDose(
                plannedYield,
                nutrientUptake,
                defaultContent,
                bulkDensity,
                h,
                soilUtilization,
                fertilizerUtilization,
                area);

            // Assert
            Assert.Equal(expectedDosePerHa, dosePerHa, precision: 3);
            Assert.Equal(expectedTotalDose, totalDose, precision: 3);
        }

        [Fact(DisplayName = "CalculateDose clamps negative dose to zero")]
        public void CalculateDose_NegativeCase_ClampedToZero()
        {
            // Arrange: такі параметри, що (U*Uptake - term) < 0
            double plannedYield = 10.0;
            double nutrientUptake = 1.0;
            double defaultContent = 1000.0;    // великий дефолт
            double bulkDensity = 2.0;
            double h = 20.0;
            double soilUtilization = 1.0;      // максимально забирає з ґрунту
            double fertilizerUtilization = 0.5;
            double area = 5.0;

            // Numerator = 10*1 - ((1000/10)*2*20)*1 = 10 - (100*2*20)=10 - 4000 = -3990
            // dosePerHa -> clamped to 0; totalDose = 0*5 = 0

            // Act
            var (dosePerHa, totalDose) = CalculationHelper.CalculateDose(
                plannedYield,
                nutrientUptake,
                defaultContent,
                bulkDensity,
                h,
                soilUtilization,
                fertilizerUtilization,
                area);

            // Assert
            Assert.Equal(0.0, dosePerHa);
            Assert.Equal(0.0, totalDose);
        }
    }
}
