using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agroculture.Models
{
    public class Crop
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double RequiredN { get; set; }
        public double RequiredP2O5 { get; set; }
        public double RequiredK2O { get; set; }
        public string RecomendedNextCrop { get; set; }
    }
}
