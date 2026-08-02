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
using WaterManagementSystem.Wpf.Windows;

namespace WaterManagementSystem.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for MeterWindow.xaml
    /// </summary>
    public partial class MeterWindow : Window
    {
        private readonly ApiService _apiService;
        public MeterWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();

            dpInstallationDate.SelectedDate = DateTime.Today;
        }

        private async Task LoadMeters(int customerId) 
        {
            Response response = await _apiService.GetMetersByCustomer(customerId);

            if (response.IsSuccess)
            {
                List<Meter> meters = (List<Meter>)response.Result;

                dgMeters.ItemsSource = meters;
            }
            else
            {
                dgMeters.ItemsSource = null;

                MessageBox.Show(response.Message, "Error",
                       MessageBoxButton.OK,
                       MessageBoxImage.Error);
            }
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
                MessageBox.Show(response.Message, "Error",
                       MessageBoxButton.OK,
                       MessageBoxImage.Error);
            }

        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
           await LoadCustomers();
        }

        private async void cbCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cbCustomers.SelectedItem == null)
            {
                return;
            }

            Customer selectedCustomer = (Customer)cbCustomers.SelectedItem;

            await LoadMeters(selectedCustomer.CustomerId);
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {

            if (cbCustomers.SelectedItem == null)
            {
                MessageBox.Show("Please select a customer.",
                   "Warning",
                   MessageBoxButton.OK,
                   MessageBoxImage.Warning);
                return;
            }

            Customer selectedCustomer = (Customer)cbCustomers.SelectedItem;

            Meter newMeter;

            newMeter = new Meter
            {
                CustomerId = selectedCustomer.CustomerId,
                InstallationDate = DateTime.Now,
                IsActive = true
            };

            Response response = await _apiService.CreateMeter(newMeter);

            if (response.IsSuccess)
            {
                MessageBox.Show("Meter registered successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await LoadMeters(selectedCustomer.CustomerId);
            }
            else
            {
                MessageBox.Show(response.Message, "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
            }

        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if(dgMeters.SelectedItem == null)
            {
                MessageBox.Show("Please select a meter.",
                   "Warning",
                   MessageBoxButton.OK,
                   MessageBoxImage.Warning);
                return;
            }

            Meter meterToEdit = (Meter)dgMeters.SelectedItem;

            EditMeterWindow editMeterWindow = new EditMeterWindow(meterToEdit);
            editMeterWindow.ShowDialog();

            await LoadMeters(meterToEdit.CustomerId);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            cbCustomers.SelectedItem = null;
            dgMeters.ItemsSource = null;
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if(dgMeters.SelectedItem == null)
            {
                MessageBox.Show("Please select a meter.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Meter meterToDelete = (Meter)dgMeters.SelectedItem;

            MessageBoxResult confirmation = MessageBox.Show("Are you sure you want to delete this meter?",
                "Confirmation",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);

            if (confirmation == MessageBoxResult.OK)
            {
                //resposta da api, se conseguiu apagaar ou nao
                Response deleteResponse = await _apiService.DeleteMeter(meterToDelete.MeterId);

                if (deleteResponse.IsSuccess)
                {
                    MessageBox.Show("Meter deleted successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    await LoadMeters(meterToDelete.CustomerId);
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

