﻿using Agroculture.Helpers;
using Agroculture.Models;
using Agroculture.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace Agroculture
{
    public partial class MainWindow : Window
    {
        private List<Field> fields;
        private List<Soil> soils;
        private List<Crop> crops;
        private LiteDbDataService dataService;

        // Шляхи до файлів JSON (розміщені в папці Data)
        private string soilsFilePath = "Data/soils.json";
        private string cropsFilePath = "Data/crops.json";
        // fieldsFilePath більше не використовується для збереження, але може бути збережений для першого запуску
        private string fieldsFilePath = "Data/fields.json";

        public MainWindow()
        {
            InitializeComponent();
            dataService = new LiteDbDataService();
            MigrateJsonToLiteDb();
            LoadData();
        }

        private void CurrentCropComboBox_DropDownClosed(object sender, EventArgs e)
        {
            UpdateSelectedField();

            // Виклик з FieldUIHelper
            FieldUIHelper.UpdateCropRotationIndicator(
                FieldsListBox.SelectedItem as Field,
                CropRotationIndicator,
                CurrentCropComboBox);

            FieldUIHelper.UpdateRecommendedCropsList(
                FieldsListBox.SelectedItem as Field,
                RecommendedCropsListBox);
        }


        private void PastCropComboBox_DropDownClosed(object sender, EventArgs e)
        {
            UpdateSelectedField();

            // Виклик з FieldUIHelper
            FieldUIHelper.UpdateCropRotationIndicator(
                FieldsListBox.SelectedItem as Field,
                CropRotationIndicator,
                CurrentCropComboBox);

            FieldUIHelper.UpdateRecommendedCropsList(
                FieldsListBox.SelectedItem as Field,
                RecommendedCropsListBox);
        }

        // Метод для міграції даних з JSON у LiteDB
        // Якщо дані вже є в LiteDB, то нічого не робимо
        private void MigrateJsonToLiteDb()
        {
            try
            {
                // Шлях до Data
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dataDir = Path.Combine(baseDir, "Data");

                // Перевіряємо, що JSON-файли існують
                var soilsFile = Path.Combine(dataDir, "soils.json");
                var cropsFile = Path.Combine(dataDir, "crops.json");
                var fieldsFile = Path.Combine(dataDir, "fields.json");
                if (!File.Exists(soilsFile) || !File.Exists(cropsFile) || !File.Exists(fieldsFile))
                    return; // немає чого мігрувати

                // Міграція тільки якщо колекції порожні
                if (!dataService.LoadSoils().Any())
                {
                    var jsonService = new JsonDataService(soilsFile, cropsFile, fieldsFile);
                    var soils = jsonService.LoadSoils();
                    dataService.SaveSoils(soils);
                }
                if (!dataService.LoadCrops().Any())
                {
                    var jsonService = new JsonDataService(soilsFile, cropsFile, fieldsFile);
                    var crops = jsonService.LoadCrops();
                    dataService.SaveCrops(crops);
                }
                if (!dataService.LoadFields().Any())
                {
                    var jsonService = new JsonDataService(soilsFile, cropsFile, fieldsFile);
                    var fields = jsonService.LoadFields();
                    foreach (var f in fields) dataService.SaveField(f);
                }
            }
            catch (Exception ex)
            {
                // для діагностики логніть або показуйте повідомлення
                MessageBox.Show("Помилка міграції JSON→LiteDB: " + ex.Message);
            }
        }



        private void LoadData()
        {
            // Перевіряємо, чи є дані в LiteDB, якщо ні - мігруємо з JSON
            //MigrateJsonToLiteDb();
            soils = dataService.LoadSoils();
            crops = dataService.LoadCrops();
            fields = dataService.LoadFields();

            SoilComboBox.ItemsSource = soils;
            CurrentCropComboBox.ItemsSource = crops;
            PastCropComboBox.ItemsSource = crops;
            FieldsListBox.ItemsSource = fields;

            if (fields.Count > 0)
            {
                FieldsListBox.SelectedIndex = 0;
            }
        }

        private void CurrentCropComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedField();

            // Виклик з FieldUIHelper
            FieldUIHelper.UpdateCropRotationIndicator(
                FieldsListBox.SelectedItem as Field,
                CropRotationIndicator,
                CurrentCropComboBox);

            FieldUIHelper.UpdateRecommendedCropsList(
                FieldsListBox.SelectedItem as Field,
                RecommendedCropsListBox);
        }

        private void SoilComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FieldsListBox.SelectedItem is Field selectedField && SoilComboBox.SelectedItem is Soil selectedSoil)
            {
                if (!selectedField.IsSoilFixed)  // Перевіряємо, чи не було значення фіксованим
                {
                    selectedField.CurrentN = selectedSoil.DefaultN;
                    selectedField.CurrentP2O5 = selectedSoil.DefaultP2O5;
                    selectedField.CurrentK2O = selectedSoil.DefaultK2O;

                    CurrentNTextBox.Text = selectedField.CurrentN.ToString(CultureInfo.InvariantCulture);
                    CurrentP2O5TextBox.Text = selectedField.CurrentP2O5.ToString(CultureInfo.InvariantCulture);
                    CurrentK2OTextBox.Text = selectedField.CurrentK2O.ToString(CultureInfo.InvariantCulture);
                }

                UpdateSelectedField();
            }
        }
        private void PastCropComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FieldsListBox.SelectedItem is Field selectedField && PastCropComboBox.SelectedItem is Crop selectedPastCrop)
            {
                if (selectedField.Year == 0)  // Дозволяємо зміну тільки якщо рік == 0
                {
                    selectedField.PastCrop = selectedPastCrop;
                    dataService.SaveField(selectedField);
                }

                // Якщо рік > 0, після першої зміни блокуємо редагування
                if (selectedField.Year > 0)
                {
                    PastCropComboBox.IsEnabled = false;
                }
            }
        }

        private void FieldsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FieldsListBox.SelectedItem is Field selectedField)
            {
                FieldNameTextBox.Text = selectedField.Name;
                FieldAreaTextBox.Text = selectedField.Area.ToString(CultureInfo.InvariantCulture);
                SoilComboBox.SelectedItem = soils.Find(s => s.ID == selectedField.SelectedSoil?.ID);

                CurrentNTextBox.Text = selectedField.CurrentN.ToString(CultureInfo.InvariantCulture);
                CurrentP2O5TextBox.Text = selectedField.CurrentP2O5.ToString(CultureInfo.InvariantCulture);
                CurrentK2OTextBox.Text = selectedField.CurrentK2O.ToString(CultureInfo.InvariantCulture);

                CurrentCropComboBox.SelectedItem = crops.Find(c => c.ID == selectedField.CurrentCrop?.ID);
                PastCropComboBox.SelectedItem = crops.Find(c => c.ID == selectedField.PastCrop?.ID);

                YearCounterTextBlock.Text = $"Рік: {selectedField.Year}";

                // Блокуємо зміну минулорічної культури, якщо рік > 0
                PastCropComboBox.IsEnabled = (selectedField.Year == 0);

                // Блокуємо зміну типу ґрунту, якщо він був зафіксований
                SoilComboBox.IsEnabled = !selectedField.IsSoilFixed;
            }
        }

        private void AddField_Click(object sender, RoutedEventArgs e)
        {
            Field newField = new Field
            {
                ID = fields.Count > 0 ? fields[fields.Count - 1].ID + 1 : 1,
                Name = "Нове поле",
                Area = 0,
                SelectedSoil = null,
                CurrentN = 0,
                CurrentP2O5 = 0,
                CurrentK2O = 0,
                CurrentCrop = null,
                PastCrop = null,
                Year = 0,
                IsSoilFixed = false
            };
            fields.Add(newField);
            // Зберігаємо нове поле у saves/ через SaveField
            dataService.SaveField(newField);
            RefreshFieldsList();
        }

        private void DeleteField_Click(object sender, RoutedEventArgs e)
        {
            if (FieldsListBox.SelectedItem is Field selectedField)
            {
                // Видаляємо JSON-файл для цього поля з saves/
                dataService.DeleteField(selectedField.ID);

                // Видаляємо поле з колекції
                fields.Remove(selectedField);
                RefreshFieldsList();
            }
        }

        private void NextYear_Click(object sender, RoutedEventArgs e)
        {
            if (FieldsListBox.SelectedItem is Field selectedField)
            {
                if (selectedField.CurrentCrop == null)
                {
                    MessageBox.Show("Будь ласка, оберіть поточну культуру перед переходом до наступного року.");
                    return;
                }

                // Фіксуємо минулу культуру рівною поточній
                selectedField.PastCrop = selectedField.CurrentCrop;
                // Збільшуємо рік для цього поля
                selectedField.Year++;
                YearCounterTextBlock.Text = $"Рік: {selectedField.Year}";
                PastCropComboBox.IsEnabled = selectedField.Year == 0;

                // Очищення поточної культури для нового року
                selectedField.CurrentCrop = null;
                CurrentCropComboBox.SelectedItem = null;

                dataService.SaveField(selectedField);
                RefreshFieldsList();
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть поле.");
            }
        }
        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            if (FieldsListBox.SelectedItem is Field selectedField)
            {
                if (selectedField.CurrentCrop == null || selectedField.SelectedSoil == null)
                {
                    MessageBox.Show("Будь ласка, оберіть поточну культуру та тип ґрунту для розрахунку.");
                    return;
                }

                try
                {
                    double h = 20.0;
                    double area = selectedField.Area;

                    // Якщо користувач не ввів планову врожайність або ввів ≤ 0, 
                    // використовуємо PlannedYield з Crop
                    if (!double.TryParse(PlannedYieldTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double plannedYield)
                        || plannedYield <= 0)
                    {
                        plannedYield = selectedField.CurrentCrop.PlannedYield;
                    }

                    // Тепер викликаємо метод із CalculationHelper
                    var nResults = CalculationHelper.CalculateDose(
                        plannedYield,
                        selectedField.CurrentCrop.NutrientUptake.N,
                        selectedField.SelectedSoil.DefaultN,
                        selectedField.SelectedSoil.BulkDensity,
                        h,
                        selectedField.CurrentCrop.SoilUtilization.N,
                        selectedField.CurrentCrop.FertilizerUtilization.N,
                        area
                    );

                    var pResults = CalculationHelper.CalculateDose(
                        plannedYield,
                        selectedField.CurrentCrop.NutrientUptake.P2O5,
                        selectedField.SelectedSoil.DefaultP2O5,
                        selectedField.SelectedSoil.BulkDensity,
                        h,
                        selectedField.CurrentCrop.SoilUtilization.P2O5,
                        selectedField.CurrentCrop.FertilizerUtilization.P2O5,
                        area
                    );

                    var kResults = CalculationHelper.CalculateDose(
                        plannedYield,
                        selectedField.CurrentCrop.NutrientUptake.K2O,
                        selectedField.SelectedSoil.DefaultK2O,
                        selectedField.SelectedSoil.BulkDensity,
                        h,
                        selectedField.CurrentCrop.SoilUtilization.K2O,
                        selectedField.CurrentCrop.FertilizerUtilization.K2O,
                        area
                    );

                    RateNTextBox.Text = nResults.dosePerHa.ToString("F2", CultureInfo.InvariantCulture) + " кг/га";
                    TotalNTextBox.Text = nResults.totalDose.ToString("F2", CultureInfo.InvariantCulture) + " кг";

                    RateP2O5TextBox.Text = pResults.dosePerHa.ToString("F2", CultureInfo.InvariantCulture) + " кг/га";
                    TotalP2O5TextBox.Text = pResults.totalDose.ToString("F2", CultureInfo.InvariantCulture) + " кг";

                    RateK2OTextBox.Text = kResults.dosePerHa.ToString("F2", CultureInfo.InvariantCulture) + " кг/га";
                    TotalK2OTextBox.Text = kResults.totalDose.ToString("F2", CultureInfo.InvariantCulture) + " кг";

                    MessageBox.Show("Розрахунок добрив виконано.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при розрахунку: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть поле для розрахунку.");
            }
        }

        private double ValidateAndGetValue(TextBox textBox)
        {
            if (double.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                if (value < 0.01)
                    value = 0.01;
                else if (value > 1000)
                    value = 1000;

                textBox.Text = value.ToString(CultureInfo.InvariantCulture);
                return value;
            }
            return 0;
        }

        private void UpdateSelectedField()
        {
            if (FieldsListBox.SelectedItem is Field selectedField)
            {
                // Оновлюємо назву поля
                selectedField.Name = FieldNameTextBox.Text;

                // Обробка та коригування значення площі
                if (double.TryParse(FieldAreaTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double area))
                {
                    if (area < 0.01)
                    {
                        area = 0.01;
                    }
                    else if (area > 1000000)
                    {
                        area = 1000000;
                    }
                    selectedField.Area = area;
                    // Оновлюємо UI, щоб відобразити кориговане значення
                    FieldAreaTextBox.Text = area.ToString(CultureInfo.InvariantCulture);
                }

                // Оновлюємо обраний тип ґрунту
                selectedField.SelectedSoil = SoilComboBox.SelectedItem as Soil;

                // Обробка та коригування значень поживних речовин
                selectedField.CurrentN = ValidateAndGetValue(CurrentNTextBox);
                selectedField.CurrentP2O5 = ValidateAndGetValue(CurrentP2O5TextBox);
                selectedField.CurrentK2O = ValidateAndGetValue(CurrentK2OTextBox);

                // Оновлюємо поточну культуру, якщо обрано
                if (CurrentCropComboBox.SelectedItem != null)
                    selectedField.CurrentCrop = CurrentCropComboBox.SelectedItem as Crop;

                // Для року 0 дозволено редагування минулої культури
                if (selectedField.Year == 0 && PastCropComboBox.SelectedItem != null)
                    selectedField.PastCrop = PastCropComboBox.SelectedItem as Crop;

                // Перевірка на фіксацію типу ґрунту за поживними речовинами
                if (selectedField.SelectedSoil != null)
                {
                    double tol = 0.001;
                    if (Math.Abs(selectedField.CurrentN - selectedField.SelectedSoil.DefaultN) > tol ||
                        Math.Abs(selectedField.CurrentP2O5 - selectedField.SelectedSoil.DefaultP2O5) > tol ||
                        Math.Abs(selectedField.CurrentK2O - selectedField.SelectedSoil.DefaultK2O) > tol)
                    {
                        selectedField.IsSoilFixed = true;
                    }
                    else
                    {
                        selectedField.IsSoilFixed = false;
                    }
                }

                // Якщо тип ґрунту зафіксовано, блокуємо зміну
                SoilComboBox.IsEnabled = !selectedField.IsSoilFixed;

                // Зберігаємо змінене поле в окремий файл (saves/field_{ID}.json)
                dataService.SaveField(selectedField);
                RefreshFieldsList();
            }
        }

        private void FieldDetails_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateSelectedField();
        }

        private void FieldDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedField();
        }

        private void RefreshFieldsList()
        {
            var selectedField = FieldsListBox.SelectedItem as Field;

            FieldsListBox.ItemsSource = null;
            FieldsListBox.ItemsSource = fields;

            if (selectedField != null && fields.Exists(f => f.ID == selectedField.ID))
            {
                FieldsListBox.SelectedItem = fields.Find(f => f.ID == selectedField.ID);
            }
            else if (fields.Count > 0)
            {
                FieldsListBox.SelectedIndex = 0;
            }
        }
    }
}
