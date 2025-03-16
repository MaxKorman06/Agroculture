using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agroculture.Models
{
    class Soil
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double DefaultN { get; set; }
        public double DefaultP2O5 { get; set; }
        public double DefaultK2O { get; set; }
    }
}
