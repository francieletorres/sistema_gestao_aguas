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
using System.Xml.Linq;
using WaterManagementSystem.Wpf.Models;
using WaterManagementSystem.Wpf.Services;

namespace WaterManagementSystem.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for ConsumptionWindow.xaml
    /// </summary>
    public partial class ConsumptionWindow : Window
    {
        private readonly ApiService _apiService;
        public ConsumptionWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();

            dpReadingDate.SelectedDate = DateTime.Today;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomers();
        }

        private async Task LoadCustomers()
        {
            Response response = await _apiService.GetCustomers();

            if (response.IsSuccess)
            {
                List<Customer> customers = (List<Customer>)response.Result;

                cbCustomers.ItemsSource = customers;
            }
            else
            {
                MessageBox.Show(response.Message, "error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void cbCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cbCustomers.SelectedItem == null)
            {
                return;
            }

            cbMeters.ItemsSource = null;

            Customer selectedCustomer = (Customer)cbCustomers.SelectedItem;

            await LoadMeters(selectedCustomer.CustomerId);
        }

        private async Task LoadMeters(int customerId)
        {
            Response response = await _apiService.GetMetersByCustomer(customerId);

            if(response.IsSuccess)
            {
                List<Meter> meters = (List<Meter>)response.Result;
                
                cbMeters.ItemsSource = meters;
            }
            else
            {
                cbMeters.ItemsSource = null;

                MessageBox.Show(response.Message, "error",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }


        private async Task LoadConsumptions(int meterId)
        {
            Response response = await _apiService.GetConsumptionsByMeter(meterId);

            if (response.IsSuccess)
            {
                List<Consumption> consumptions = (List<Consumption>)response.Result;

                dgConsumptions.ItemsSource = consumptions;
            }
            else
            {
                dgConsumptions.ItemsSource = null; //limpa os dados em caso de erro

                MessageBox.Show(response.Message, "error",
                   MessageBoxButton.OK,
                   MessageBoxImage.Error);
            }
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if(cbMeters.SelectedItem == null)
            {
                MessageBox.Show("Please select a meter",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if(!int.TryParse(txtMeterReading.Text, out int meterReading))
            {
                MessageBox.Show("Please enter a valid meter reading.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Meter selectedMeter = (Meter)cbMeters.SelectedItem;

            Consumption newConsumption;

            newConsumption = new Consumption
            {
                MeterId = selectedMeter.MeterId,
                MeterReading = meterReading,
                ReadingDate = dpReadingDate.SelectedDate.Value,
                Notes = txtNotes.Text
            };

            Response response = await _apiService.CreateConsumption(newConsumption);

            if (response.IsSuccess)
            {
                MessageBox.Show("Consumption registered successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await LoadConsumptions(selectedMeter.MeterId);

                ClearFields();
 
            }
            else
            {
                MessageBox.Show(response.Message, "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ClearFields()
        {
            txtMeterReading.Text = string.Empty;
            txtNotes.Text = string.Empty;
            dpReadingDate.SelectedDate = DateTime.Today;
            dgConsumptions.SelectedItem = null; //tira a seleção da linha que estiver marcada na DataGrid
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private async void cbMeters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cbMeters.SelectedItem == null)
            {
                dgConsumptions.ItemsSource = null;
                return;
            }

            Meter selectedMeter = (Meter)cbMeters.SelectedItem;

            await LoadConsumptions(selectedMeter.MeterId);
        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgConsumptions.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select a consumption.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Consumption consumptionToEdit = (Consumption)dgConsumptions.SelectedItem;

            if (consumptionToEdit.HasInvoice)
            {
                MessageBox.Show("This consumption has already been invoiced and cannot be edited.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            EditConsumptionWindow editConsumptionWindow = new EditConsumptionWindow(consumptionToEdit);
            editConsumptionWindow.ShowDialog();

            await LoadConsumptions(consumptionToEdit.MeterId);

        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if(dgConsumptions.SelectedItem == null)
            {
                MessageBox.Show("Please select a consumption.",
                    "warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Consumption consumptionToDelete = (Consumption)dgConsumptions.SelectedItem;

            if (consumptionToDelete.HasInvoice)
            {
                MessageBox.Show("This consumption has already been invoiced and cannot be deleted.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult confirmation = MessageBox.Show(
                "Are you sure you want to delete this consumption?",
                "Confirmation",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);

            if (confirmation == MessageBoxResult.OK)
            {
                Response deleteResponse =  await _apiService.DeleteConsumption(consumptionToDelete.ConsumptionId);

                if (deleteResponse.IsSuccess)
                {
                    MessageBox.Show(
                        "Consumption deleted successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    await LoadConsumptions(consumptionToDelete.MeterId);

                    ClearFields();
                }
                else
                {
                    MessageBox.Show(
                        deleteResponse.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}
