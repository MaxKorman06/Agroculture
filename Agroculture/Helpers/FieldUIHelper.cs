using Agroculture.Models;
using System;
using System.Linq;
using System.Windows.Controls;

namespace Agroculture.Helpers
{
    public static class FieldUIHelper
    {
        public static void UpdateCropRotationIndicator(Field selectedField,
                                                       Label cropRotationIndicator,
                                                       ComboBox currentCropComboBox)
        {
            if (selectedField == null)
            {
                cropRotationIndicator.Content = "";
                return;
            }

            if (selectedField.PastCrop == null)
            {
                cropRotationIndicator.Content = "";
                return;
            }

            if (currentCropComboBox.SelectedItem is Crop currentCrop)
            {
                var recommendedList = selectedField.PastCrop.RecomendedNextCrop
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

                if (recommendedList.Any(r => string.Equals(r, currentCrop.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    cropRotationIndicator.Content = "✔";
                }
                else
                {
                    cropRotationIndicator.Content = "";
                }
            }
            else
            {
                cropRotationIndicator.Content = "";
            }
        }

        public static void UpdateRecommendedCropsList(Field selectedField,
                                                      ListBox recommendedCropsListBox)
        {
            if (selectedField != null && selectedField.PastCrop != null)
            {
                var recommended = selectedField.PastCrop.RecomendedNextCrop
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

                recommendedCropsListBox.ItemsSource = recommended;
            }
            else
            {
                recommendedCropsListBox.ItemsSource = null;
            }
        }
    }

}
