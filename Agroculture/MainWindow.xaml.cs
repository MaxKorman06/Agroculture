using Agroculture.Models;
using Agroculture.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        private string fieldsFilePath = "Data/fields.json";

        // Лічильник років для вибраного поля
        private int currentYear = 0;

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
            fields = dataService.LoadFields(); // Тепер завантажує з "saves/"

            // Прив'язка до ComboBox-ів
            SoilComboBox.ItemsSource = soils;
            CurrentCropComboBox.ItemsSource = crops;
            PastCropComboBox.ItemsSource = crops;

            // Прив'язка списку полів
            FieldsListBox.ItemsSource = fields;

            // Автоматичний вибір першого поля, якщо воно є
            if (fields.Count > 0)
            {
                FieldsListBox.SelectedIndex = 0;
            }
        }


        private void SoilComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SoilComboBox.SelectedItem is Soil selectedSoil)
            {

                CurrentNTextBox.Text = selectedSoil.DefaultN.ToString();
                CurrentP2O5TextBox.Text = selectedSoil.DefaultP2O5.ToString();
                CurrentK2OTextBox.Text = selectedSoil.DefaultK2O.ToString();
            }
            // Автоматичне збереження змін після вибору типу ґрунту
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

                // Виправлення проблеми з PastCropComboBox
                PastCropComboBox.SelectedItem = crops.Find(c => c.ID == selectedField.PastCrop?.ID);

                PastCropComboBox.IsEnabled = currentYear == 0; // Блокуємо, якщо рік більше 0
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
                PastCrop = null
            };
            fields.Add(newField);
            // При створенні нового поля зберігаємо його окремо у saves/ через SaveField
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

                // Оновлюємо минулу культуру
                selectedField.PastCrop = selectedField.CurrentCrop;
                currentYear++;
                YearCounterTextBlock.Text = $"Рік: {currentYear}";
                PastCropComboBox.IsEnabled = currentYear == 0;

                // Очищення поточної культури для нового року
                selectedField.CurrentCrop = null;
                CurrentCropComboBox.SelectedItem = null;

                // Оновлення списку культур
                PastCropComboBox.SelectedItem = crops.Find(c => c.ID == selectedField.PastCrop?.ID);

                // Збереження змін
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
        /// Оновлює властивості вибраного поля з даних, введених у форму,
        /// і автоматично зберігає зміни у файл fields.json.
        /// Викликається при зміні даних (події LostFocus та SelectionChanged).
        /// </summary>
        private void UpdateSelectedField()
        {
            if (FieldsListBox.SelectedItem is Field selectedField)
            {
                selectedField.Name = FieldNameTextBox.Text;

                if (double.TryParse(FieldAreaTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double area))
                    selectedField.Area = area;

                selectedField.SelectedSoil = SoilComboBox.SelectedItem as Soil;

                if (double.TryParse(CurrentNTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double n))
                    selectedField.CurrentN = n;

                if (double.TryParse(CurrentP2O5TextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double p))
                    selectedField.CurrentP2O5 = p;

                if (double.TryParse(CurrentK2OTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double k))
                    selectedField.CurrentK2O = k;

                // Оновлюємо поточну культуру тільки якщо щось вибрано
                if (CurrentCropComboBox.SelectedItem != null)
                    selectedField.CurrentCrop = CurrentCropComboBox.SelectedItem as Crop;

                // Для року 0 дозволяємо ручне задання минулої культури
                if (currentYear == 0 && PastCropComboBox.SelectedItem != null)
                    selectedField.PastCrop = PastCropComboBox.SelectedItem as Crop;

                dataService.SaveField(selectedField);
                RefreshFieldsList();
            }
        }



        private void CurrentCropComboBox_DropDownClosed(object sender, EventArgs e)
        {
            // Якщо користувач обрав культуру, оновлюємо поле
            if (CurrentCropComboBox.SelectedItem != null)
            {
                UpdateSelectedField();
            }
        }

        private void PastCropComboBox_DropDownClosed(object sender, EventArgs e)
        {
            // Для року 0 – дозволено ручне задання минулої культури
            if (PastCropComboBox.SelectedItem != null && currentYear == 0)
            {
                UpdateSelectedField();
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
        /// Подія для SelectionChanged у ComboBox'ах.
        /// </summary>
        private void FieldDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedField();
        }

        /// <summary>
        /// Оновлює прив'язку списку полів (для відображення змін) і зберігає дані.
        /// </summary>
        private void RefreshFieldsList()
        {
            var selectedField = FieldsListBox.SelectedItem as Field;

            FieldsListBox.ItemsSource = null;
            FieldsListBox.ItemsSource = fields;

            if (selectedField != null)
            {
                FieldsListBox.SelectedItem = fields.FirstOrDefault(f => f.ID == selectedField.ID);
            }
        }

    }
}
