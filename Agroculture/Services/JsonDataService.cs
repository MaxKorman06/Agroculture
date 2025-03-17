using System;
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
        private readonly string savesDirectory = "saves";

        public JsonDataService(string soilsFilePath, string cropsFilePath, string fieldsFilePath)
        {
            this.soilsFilePath = soilsFilePath;
            this.cropsFilePath = cropsFilePath;
            this.fieldsFilePath = fieldsFilePath;

            if (!Directory.Exists(savesDirectory))
                Directory.CreateDirectory(savesDirectory);
        }

        public List<Soil> LoadSoils()
        {
            if (!File.Exists(soilsFilePath))
                return new List<Soil>();
            return JsonConvert.DeserializeObject<List<Soil>>(File.ReadAllText(soilsFilePath));
        }

        public void SaveSoils(List<Soil> soils)
        {
            File.WriteAllText(soilsFilePath, JsonConvert.SerializeObject(soils, Formatting.Indented));
        }

        public List<Crop> LoadCrops()
        {
            if (!File.Exists(cropsFilePath))
                return new List<Crop>();
            return JsonConvert.DeserializeObject<List<Crop>>(File.ReadAllText(cropsFilePath));
        }

        public void SaveCrops(List<Crop> crops)
        {
            File.WriteAllText(cropsFilePath, JsonConvert.SerializeObject(crops, Formatting.Indented));
        }

        public List<Field> LoadFields()
        {
            string savesDirectory = "saves";

            if (!Directory.Exists(savesDirectory))
            {
                Directory.CreateDirectory(savesDirectory);
            }

            List<Field> fields = new List<Field>();

            foreach (string file in Directory.GetFiles(savesDirectory, "field_*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    Field field = JsonConvert.DeserializeObject<Field>(json);
                    if (field != null)
                    {
                        fields.Add(field);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка при читанні {file}: {ex.Message}");
                }
            }

            return fields;
        }


        public void SaveField(Field field)
        {
            string savesDirectory = "saves";

            // Створюємо папку, якщо її немає
            if (!Directory.Exists(savesDirectory))
            {
                Directory.CreateDirectory(savesDirectory);
            }

            string filePath = Path.Combine(savesDirectory, $"field_{field.ID}.json");

            try
            {
                string json = JsonConvert.SerializeObject(field, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка збереження файлу {filePath}: {ex.Message}");
            }
        }


        public Field LoadField(int fieldID)
        {
            string fieldFilePath = Path.Combine(savesDirectory, $"field_{fieldID}.json");
            if (!File.Exists(fieldFilePath)) return null;
            return JsonConvert.DeserializeObject<Field>(File.ReadAllText(fieldFilePath));
        }

        public void DeleteField(int fieldID)
        {
            string fieldFilePath = Path.Combine("saves", $"field_{fieldID}.json");
            if (File.Exists(fieldFilePath))
            {
                File.Delete(fieldFilePath);
            }
        }
    }
}
