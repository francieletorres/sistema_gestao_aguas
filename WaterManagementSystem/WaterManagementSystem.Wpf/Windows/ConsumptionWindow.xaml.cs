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

            dpReadingDate.SelectedDate = DateTime.Now;
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

                MessageBox.Show(response.Message, "Aviso",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
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

                MessageBox.Show(response.Message, "Aviso",
                   MessageBoxButton.OK,
                   MessageBoxImage.Information);
            }
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if(cbMeters.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecione um contador.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if(!int.TryParse(txtMeterReading.Text, out int meterReading))
            {
                MessageBox.Show("Por favor, introduza uma leitura válida no contador.",
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
                MessageBox.Show("Consumo registado com sucesso.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                txtMeterReading.Text = string.Empty;
                txtNotes.Text = string.Empty;

                await LoadConsumptions(selectedMeter.MeterId);
            }
            else
            {
                MessageBox.Show(response.Message, "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Clears the consumption form fields and removes the current
        /// selection from the consumptions DataGrid.
        /// </summary>
        private void ClearFields()
        {
            cbCustomers.SelectedItem = null;
            cbMeters.SelectedItem = null;
            txtMeterReading.Text = string.Empty;
            txtNotes.Text = string.Empty;
            dpReadingDate.SelectedDate = DateTime.Now;
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
                    "Por favor, selecione um consumo.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Consumption consumptionToEdit = (Consumption)dgConsumptions.SelectedItem;

            if (consumptionToEdit.HasInvoice)
            {
                MessageBox.Show("Este consumo já foi faturado e não pode ser editado.",
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
                MessageBox.Show("Por favor, selecione um consumo.",
                    "warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Consumption consumptionToDelete = (Consumption)dgConsumptions.SelectedItem;

            if (consumptionToDelete.HasInvoice)
            {
                MessageBox.Show("Este consumo já foi faturado e não pode ser eliminado.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult confirmation = MessageBox.Show(
                "Tem a certeza de que pretende eliminar este consumo?",
                "Confirmation",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);

            if (confirmation == MessageBoxResult.OK)
            {
                Response deleteResponse =  await _apiService.DeleteConsumption(consumptionToDelete.ConsumptionId);

                if (deleteResponse.IsSuccess)
                {
                    MessageBox.Show(
                        "Consumo eliminado com sucesso.",
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
