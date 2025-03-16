using Agroculture.Models;
using Agroculture.Services;
using System;
using System.Collections.Generic;
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

        // Шляхи до файлів JSON (розмістіть їх у папці Data у корені проєкту)
        private string soilsFilePath = "Data/soils.json";
        private string cropsFilePath = "Data/crops.json";
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
            fields = dataService.LoadFields();

            // Прив'язка даних до ComboBox-ів
            SoilComboBox.ItemsSource = soils;
            CurrentCropComboBox.ItemsSource = crops;
            PastCropComboBox.ItemsSource = crops;

            // Прив'язка списку полів
            FieldsListBox.ItemsSource = fields;
        }

        private void FieldsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FieldsListBox.SelectedItem is Field selectedField)
            {
                FieldNameTextBox.Text = selectedField.Name;
                FieldAreaTextBox.Text = selectedField.Area.ToString();
                // Прив'язуємо обраний тип ґрунту за ID
                SoilComboBox.SelectedItem = soils.Find(s => s.ID == selectedField.SelectedSoil?.ID);
                CurrentNTextBox.Text = selectedField.CurrentN.ToString();
                CurrentP2O5TextBox.Text = selectedField.CurrentP2O5.ToString();
                CurrentK2OTextBox.Text = selectedField.CurrentK2O.ToString();
                CurrentCropComboBox.SelectedItem = crops.Find(c => c.ID == selectedField.CurrentCrop?.ID);
                PastCropComboBox.SelectedItem = crops.Find(c => c.ID == selectedField.PastCrop?.ID);
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
            RefreshFieldsList();
        }

        private void DeleteField_Click(object sender, RoutedEventArgs e)
        {
            if (FieldsListBox.SelectedItem is Field selectedField)
            {
                fields.Remove(selectedField);
                RefreshFieldsList();
            }
        }

        private void RefreshFieldsList()
        {
            FieldsListBox.ItemsSource = null;
            FieldsListBox.ItemsSource = fields;
            dataService.SaveFields(fields);
        }

        private void SaveField_Click(object sender, RoutedEventArgs e)
        {
            if (FieldsListBox.SelectedItem is Field selectedField)
            {
                selectedField.Name = FieldNameTextBox.Text;
                if (double.TryParse(FieldAreaTextBox.Text, out double area))
                    selectedField.Area = area;
                selectedField.SelectedSoil = SoilComboBox.SelectedItem as Soil;
                if (double.TryParse(CurrentNTextBox.Text, out double n))
                    selectedField.CurrentN = n;
                if (double.TryParse(CurrentP2O5TextBox.Text, out double p))
                    selectedField.CurrentP2O5 = p;
                if (double.TryParse(CurrentK2OTextBox.Text, out double k))
                    selectedField.CurrentK2O = k;
                selectedField.CurrentCrop = CurrentCropComboBox.SelectedItem as Crop;
                selectedField.PastCrop = PastCropComboBox.SelectedItem as Crop;

                dataService.SaveFields(fields);
                RefreshFieldsList();
            }
        }

        private void SoilComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // При зміні типу ґрунту скидаємо поточні значення поживних речовин до стандартних
            if (SoilComboBox.SelectedItem is Soil selectedSoil)
            {
                CurrentNTextBox.Text = selectedSoil.DefaultN.ToString();
                CurrentP2O5TextBox.Text = selectedSoil.DefaultP2O5.ToString();
                CurrentK2OTextBox.Text = selectedSoil.DefaultK2O.ToString();
            }
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            // Тут буде виклик алгоритму розрахунку потреби в добривах.
            MessageBox.Show("Розрахунок виконано (демонстраційний варіант)");
        }
    }
}
