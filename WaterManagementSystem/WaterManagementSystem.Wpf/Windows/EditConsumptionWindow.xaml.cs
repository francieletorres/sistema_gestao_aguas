using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WaterManagementSystem.Wpf.Models;
using WaterManagementSystem.Wpf.Services;

namespace WaterManagementSystem.Wpf.Windows
{
    public partial class EditConsumptionWindow : Window
    {
        private Consumption _consumptionToEdit;

        private ApiService _apiService;
        public EditConsumptionWindow(Consumption consumptionToEdit)
        {
            InitializeComponent();
            _consumptionToEdit = consumptionToEdit;
            _apiService = new ApiService();

            txtCustomerName.Text = consumptionToEdit.CustomerName;
            txtMeterNumber.Text = consumptionToEdit.MeterId.ToString();
            txtReadingDate.Text = consumptionToEdit.ReadingDate.ToString("dd/MM/yyyy");
            txtMeterReading.Text = consumptionToEdit.MeterReading.ToString();
            txtNotes.Text = consumptionToEdit.Notes;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(!int.TryParse(txtMeterReading.Text, out int meterReading))
            {
                MessageBox.Show(
                    "Por favor, introduza uma leitura válida no contador.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            _consumptionToEdit.MeterReading = meterReading;
            _consumptionToEdit.Notes = txtNotes.Text;

            Response response =  await _apiService.UpdateConsumption(_consumptionToEdit);

            if (response.IsSuccess)
            {
                MessageBox.Show(
                    "Consumo atualizado com sucesso.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
            }
            else
            {
                MessageBox.Show(
                    response.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
