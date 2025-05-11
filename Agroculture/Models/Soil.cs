namespace Agroculture.Models
{
    public class Soil
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double DefaultN { get; set; }
        public double DefaultP2O5 { get; set; }
        public double DefaultK2O { get; set; }

        // Нова властивість для об'ємної маси
        public double BulkDensity { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
