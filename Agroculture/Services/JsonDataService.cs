using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Agroculture.Models;

namespace Agroculture.Services
{
    class JsonDataService
    {
        private readonly string soilsFilePath;
        private readonly string cropsFilePath;
        private readonly string fieldsFilePath;

        public JsonDataService(string soilsFilePath, string cropsFilePath, string fieldsFilePath)
        {
            this.soilsFilePath = soilsFilePath;
            this.cropsFilePath = cropsFilePath;
            this.fieldsFilePath = fieldsFilePath;
        }

        public List<Soil> LoadSoils()
        {
            if (!File.Exists(soilsFilePath))
                return new List<Soil>();
            var json = File.ReadAllText(soilsFilePath);
            return JsonConvert.DeserializeObject<List<Soil>>(json);
        }

        public void SaveSoils(List<Soil> soils)
        {
            var json = JsonConvert.SerializeObject(soils, Formatting.Indented);
            File.WriteAllText(soilsFilePath, json);
        }

        public List<Crop> LoadCrops()
        {
            if (!File.Exists(cropsFilePath))
                return new List<Crop>();
            var json = File.ReadAllText(cropsFilePath);
            return JsonConvert.DeserializeObject<List<Crop>>(json);
        }

        public void SaveCrops(List<Crop> crops)
        {
            var json = JsonConvert.SerializeObject(crops, Formatting.Indented);
            File.WriteAllText(cropsFilePath, json);
        }

        public List<Field> LoadFields()
        {
            if (!File.Exists(fieldsFilePath))
                return new List<Field>();
            var json = File.ReadAllText(fieldsFilePath);
            return JsonConvert.DeserializeObject<List<Field>>(json);
        }

        public void SaveFields(List<Field> fields)
        {
            var json = JsonConvert.SerializeObject(fields, Formatting.Indented);
            File.WriteAllText(fieldsFilePath, json);
        }
    }
}
