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
            fields = dataService.LoadFields(); // Завантаження з saves/

            // Прив'язка даних до ComboBox-ів
            SoilComboBox.ItemsSource = soils;
            CurrentCropComboBox.ItemsSource = crops;
            PastCropComboBox.ItemsSource = crops;

            // Прив'язка списку полів
            FieldsListBox.ItemsSource = fields;

            // Якщо поля існують, автоматично вибираємо перше
            if (fields.Count > 0)
            {
                FieldsListBox.SelectedIndex = 0;
            }
        }

        private void SoilComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SoilComboBox.SelectedItem is Soil selectedSoil)
            {
                // При зміні типу ґрунту автоматично задаємо дефолтні значення поживних речовин
                CurrentNTextBox.Text = selectedSoil.DefaultN.ToString();
                CurrentP2O5TextBox.Text = selectedSoil.DefaultP2O5.ToString();
                CurrentK2OTextBox.Text = selectedSoil.DefaultK2O.ToString();
            }
            UpdateSelectedField();
        }

        private void FieldsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FieldsListBox.SelectedItem is Field selectedField)
            {
                // Оновлення UI при виборі поля
                FieldNameTextBox.Text = selectedField.Name;
                FieldAreaTextBox.Text = selectedField.Area.ToString(CultureInfo.InvariantCulture);
                SoilComboBox.SelectedItem = soils.Find(s => s.ID == selectedField.SelectedSoil?.ID);
                CurrentNTextBox.Text = selectedField.CurrentN.ToString(CultureInfo.InvariantCulture);
                CurrentP2O5TextBox.Text = selectedField.CurrentP2O5.ToString(CultureInfo.InvariantCulture);
                CurrentK2OTextBox.Text = selectedField.CurrentK2O.ToString(CultureInfo.InvariantCulture);
                CurrentCropComboBox.SelectedItem = crops.Find(c => c.ID == selectedField.CurrentCrop?.ID);
                PastCropComboBox.SelectedItem = crops.Find(c => c.ID == selectedField.PastCrop?.ID);
                YearCounterTextBlock.Text = $"Рік: {selectedField.Year}";
                // Блокуємо вибір типу ґрунту, якщо для цього поля зафіксовано значення поживних речовин
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

                // Валідація площі
                if (double.TryParse(FieldAreaTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double area))
                {
                    if (area < 0.01 || area > 1000000)
                    {
                        MessageBox.Show("Площа має бути в межах від 0.01 до 1000000.");
                        return;
                    }
                    selectedField.Area = area;
                }

                selectedField.SelectedSoil = SoilComboBox.SelectedItem as Soil;

                // Валідація поживних речовин
                if (double.TryParse(CurrentNTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double n))
                {
                    if (n < 0.01 || n > 1000)
                    {
                        MessageBox.Show("Значення N має бути в межах від 0.01 до 1000.");
                        return;
                    }
                    selectedField.CurrentN = n;
                }

                if (double.TryParse(CurrentP2O5TextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double p))
                {
                    if (p < 0.01 || p > 1000)
                    {
                        MessageBox.Show("Значення P₂O₅ має бути в межах від 0.01 до 1000.");
                        return;
                    }
                    selectedField.CurrentP2O5 = p;
                }

                if (double.TryParse(CurrentK2OTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double k))
                {
                    if (k < 0.01 || k > 1000)
                    {
                        MessageBox.Show("Значення K₂O має бути в межах від 0.01 до 1000.");
                        return;
                    }
                    selectedField.CurrentK2O = k;
                }

                if (CurrentCropComboBox.SelectedItem != null)
                    selectedField.CurrentCrop = CurrentCropComboBox.SelectedItem as Crop;

                if (selectedField.Year == 0 && PastCropComboBox.SelectedItem != null)
                    selectedField.PastCrop = PastCropComboBox.SelectedItem as Crop;

                // Якщо тип ґрунту вибрано, перевіряємо дефолтні значення для цього ґрунту.
                // Якщо поточні поживні значення не співпадають з дефолтними, фіксуємо тип ґрунту.
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

                // Оновлюємо стан елемента SoilComboBox згідно з прапорцем фіксації
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
