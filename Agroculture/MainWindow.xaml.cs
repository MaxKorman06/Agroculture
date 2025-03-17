using Agroculture.Models;
using Agroculture.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Agroculture
{
    public partial class MainWindow : Window
    {
        private List<Field> fields;
        private List<Soil> soils;
        private List<Crop> crops;
        private JsonDataService dataService;

        // Шляхи до файлів JSON (розміщені в папці Data)
        private string soilsFilePath = "Data/soils.json";
        private string cropsFilePath = "Data/crops.json";
        // fieldsFilePath більше не використовується для збереження, але може бути збережений для першого запуску
        private string fieldsFilePath = "Data/fields.json";

        public MainWindow()
        {
            InitializeComponent();
            dataService = new JsonDataService(soilsFilePath, cropsFilePath, fieldsFilePath);
            LoadData();
        }

        private void LoadData()
{
    soils = dataService.LoadSoils();
    crops = dataService.LoadCrops();
    fields = dataService.LoadFields(); // Завантаження збережених значень

    SoilComboBox.ItemsSource = soils;
    CurrentCropComboBox.ItemsSource = crops;
    PastCropComboBox.ItemsSource = crops;
    FieldsListBox.ItemsSource = fields;

    if (fields.Count > 0)
    {
        FieldsListBox.SelectedIndex = 0;
    }
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
            MessageBox.Show("Розрахунок виконано (демонстраційний варіант)");
        }

        /// <summary>
        /// Оновлює властивості вибраного поля з даних форми, перевіряє валідність введених чисел,
        /// встановлює прапорець фіксації типу ґрунту, якщо поживні значення відрізняються від дефолтних,
        /// та зберігає зміни у відповідний файл saves/field_{ID}.json.
        /// </summary>
        private void UpdateSelectedField()
        {
            if (FieldsListBox.SelectedItem is Field selectedField)
            {
                selectedField.Name = FieldNameTextBox.Text;

                if (double.TryParse(FieldAreaTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double area))
                {
                    if (area < 0.01 || area > 1000000)
                    {
                        MessageBox.Show("Площа має бути в межах від 0.01 до 1000000.");
                        return;
                    }
                    selectedField.Area = area;
                }

                if (double.TryParse(CurrentNTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double n) &&
                    double.TryParse(CurrentP2O5TextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double p) &&
                    double.TryParse(CurrentK2OTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double k))
                {
                    if (n < 0.01 || n > 1000 || p < 0.01 || p > 1000 || k < 0.01 || k > 1000)
                    {
                        MessageBox.Show("Значення поживних речовин повинні бути від 0.01 до 1000.");
                        return;
                    }

                    // Перевіряємо, чи відрізняються поживні речовини від стандартних значень ґрунту
                    if (selectedField.SelectedSoil != null)
                    {
                        double tol = 0.001;
                        if (Math.Abs(n - selectedField.SelectedSoil.DefaultN) > tol ||
                            Math.Abs(p - selectedField.SelectedSoil.DefaultP2O5) > tol ||
                            Math.Abs(k - selectedField.SelectedSoil.DefaultK2O) > tol)
                        {
                            selectedField.IsSoilFixed = true;
                        }
                        else
                        {
                            selectedField.IsSoilFixed = false;
                        }
                    }

                    selectedField.CurrentN = n;
                    selectedField.CurrentP2O5 = p;
                    selectedField.CurrentK2O = k;
                }

                if (CurrentCropComboBox.SelectedItem != null)
                    selectedField.CurrentCrop = CurrentCropComboBox.SelectedItem as Crop;

                if (selectedField.Year == 0 && PastCropComboBox.SelectedItem != null)
                    selectedField.PastCrop = PastCropComboBox.SelectedItem as Crop;

                // Якщо тип ґрунту був зафіксований, більше не даємо його змінювати
                SoilComboBox.IsEnabled = !selectedField.IsSoilFixed;

                dataService.SaveField(selectedField);
                RefreshFieldsList();
            }
        }


        /// <summary>
        /// Подія для LostFocus у текстових полях.
        /// </summary>
        private void FieldDetails_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateSelectedField();
        }

        /// <summary>
        /// Подія для SelectionChanged у ComboBox‑ах.
        /// </summary>
        private void FieldDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedField();
        }

        /// <summary>
        /// Оновлює прив'язку списку полів (для відображення змін).
        /// </summary>
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
