using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agroculture.Models
{
    class Field
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double Area { get; set; }
        public Soil SelectedSoil { get; set; }
        public double CurrentN { get; set; }
        public double CurrentP2O5 { get; set; }
        public double CurrentK2O { get; set; }
        public Crop CurrentCrop { get; set; }
        public Crop PastCrop { get; set; }

        // Додаємо властивість для року поля
        public int Year { get; set; } = 0;

        public bool IsSoilFixed { get; set; }
    }
}
