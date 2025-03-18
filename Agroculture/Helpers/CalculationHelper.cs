using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agroculture.Helpers
{
    public static class CalculationHelper
    {

        public static (double dosePerHa, double totalDose) CalculateDose(
            double plannedYield,
            double nutrientUptake,
            double defaultContent,
            double bulkDensity,
            double h,
            double soilUtilization,
            double fertilizerUtilization,
            double area)
        {
            // Формула
            double dosePerHa = (plannedYield * nutrientUptake
                - ((defaultContent / 10) * bulkDensity * h) * soilUtilization)
                / fertilizerUtilization;

            if (dosePerHa < 0)
                dosePerHa = 0;

            double totalDose = dosePerHa * area;
            return (dosePerHa, totalDose);
        }
    }

}
