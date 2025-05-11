using LiteDB;
using Agroculture.Models;
using System.Collections.Generic;
using System.Linq;

namespace Agroculture.Services
{
    public class LiteDbDataService
    {
        private readonly LiteDatabase db;

        public LiteDbDataService(string databasePath = "Agroculture.db")
        {
            // Відкриваємо або створюємо файл бази
            db = new LiteDatabase(databasePath);
        }

        // -------- Soils --------
        public List<Soil> LoadSoils()
        {
            return db.GetCollection<Soil>("soils").FindAll().ToList();
        }

        public void SaveSoils(IEnumerable<Soil> soils)
        {
            var col = db.GetCollection<Soil>("soils");
            col.DeleteAll();
            col.InsertBulk(soils);
        }

        // -------- Crops --------
        public List<Crop> LoadCrops()
        {
            return db.GetCollection<Crop>("crops").FindAll().ToList();
        }

        public void SaveCrops(IEnumerable<Crop> crops)
        {
            var col = db.GetCollection<Crop>("crops");
            col.DeleteAll();
            col.InsertBulk(crops);
        }

        // -------- Fields --------
        public List<Field> LoadFields()
        {
            return db.GetCollection<Field>("fields").Include(x => x.SelectedSoil)
                                                   .Include(x => x.CurrentCrop)
                                                   .Include(x => x.PastCrop)
                                                   .FindAll()
                                                   .ToList();
        }

        public void SaveField(Field field)
        {
            var col = db.GetCollection<Field>("fields");
            col.Upsert(field);
        }

        public void DeleteField(int fieldId)
        {
            var col = db.GetCollection<Field>("fields");
            col.Delete(fieldId);
        }
    }
}
