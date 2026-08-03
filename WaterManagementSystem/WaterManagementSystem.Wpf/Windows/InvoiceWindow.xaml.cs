using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Policy;
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
    
    public partial class InvoiceWindow : Window
    {
        private readonly ApiService _apiService;

        public InvoiceWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomers();
            await LoadInvoices();
        }

        /// <summary>
        /// Loads all invoices from the API and displays them in the invoices DataGrid.
        /// </summary>
        private async Task LoadInvoices()
        {
            Response response = await _apiService.GetInvoices();

            if (response.IsSuccess)
            {
                var invoices = (List<Invoice>)response.Result;

                dgInvoices.ItemsSource = invoices;
            }
            else
            {
                MessageBox.Show(response.Message, "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads all customers from the API and displays them in the customer
        /// selection and filter ComboBoxes.
        /// </summary>
        private async Task LoadCustomers()
        {
            Response response = await _apiService.GetCustomers();

            if (response.IsSuccess)
            {
                var customers = (List<Customer>)response.Result;

                cbCustomers.ItemsSource = customers;
                cbFilterCustomer.ItemsSource = customers;
            }
            else
            {
                MessageBox.Show(response.Message, "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void cbCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCustomers.SelectedItem == null)
            {
                return;
            }

            cbMeters.ItemsSource = null;
            cbConsumptions.ItemsSource = null;

            Customer selectedCustomer =
                (Customer)cbCustomers.SelectedItem;

            await LoadMeters(selectedCustomer.CustomerId);
        }

        /// <summary>
        /// Loads the meters associated with the specified customer
        /// and displays them in the meter ComboBox.
        /// </summary>
        private async Task LoadMeters(int customerId)
        {
            Response response =
            await _apiService.GetMetersByCustomer(customerId);

            if (response.IsSuccess)
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

        private async void cbMeters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbMeters.SelectedItem == null)
            {
                return;
            }

            cbConsumptions.ItemsSource = null;

            Meter selectedMeter = (Meter)cbMeters.SelectedItem;

            await LoadConsumptions(selectedMeter.MeterId);
        }


        /// <summary>
        /// Loads the consumptions associated with the specified meter
        /// and displays them in the consumption ComboBox.
        /// </summary>
        private async Task LoadConsumptions(int meterId)
        {
            Response response = await _apiService.GetConsumptionsByMeter(meterId);

            if (response.IsSuccess)
            {
                List<Consumption> consumptions = (List<Consumption>)response.Result;

                cbConsumptions.ItemsSource = consumptions;
            }
            else
            {
                cbConsumptions.ItemsSource = null;

                MessageBox.Show(response.Message, "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Clears the selected customer and removes the available meters
        /// and consumptions from the ComboBoxes.
        /// </summary>
        private void ClearFields()
        {
            cbCustomers.SelectedItem = null;
            cbMeters.ItemsSource = null;
            cbConsumptions.ItemsSource = null;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private async void btnCreateInvoice_Click(object sender, RoutedEventArgs e)
        {

            if (cbConsumptions.SelectedItem == null)
            {
                MessageBox.Show("Selecione um consumo.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            Consumption selectedConsumption = (Consumption)cbConsumptions.SelectedItem;

            Invoice newInvoice;

            newInvoice = new Invoice
            {
                ConsumptionId = selectedConsumption.ConsumptionId
            };

            Response response = await _apiService.CreateInvoice(newInvoice);

            if (response.IsSuccess)
            {
                ClearFields();

                await SearchInvoicesAsync();

                MessageBox.Show("Fatura registada com sucesso.",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(response.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        ///  Clears all invoice filter fields and reloads the complete invoice list.
        /// </summary>
        private async Task ClearFilterFields()
        {
            cbFilterCustomer.SelectedItem = null;
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            cbPaymentStatus.SelectedIndex = 0;

            await LoadInvoices();
        }

        private async void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            await ClearFilterFields();
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            await SearchInvoicesAsync();
        }


        /// <summary>
        /// Applies the selected invoice filters and displays the matching results.
        /// </summary>
        private async Task SearchInvoicesAsync()
        {
            int? customerId = null;
            DateTime? startDate = dpStartDate.SelectedDate;
            DateTime? endDate = dpEndDate.SelectedDate;
            bool? isPaid = null;
            bool? isCancelled = null;

            if (cbFilterCustomer.SelectedItem != null)
            {
                Customer selectedCustomer = (Customer)cbFilterCustomer.SelectedItem;

                customerId = selectedCustomer.CustomerId;
            }


            if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
            {
                MessageBox.Show("A data inicial não pode ser posterior à data final.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (cbPaymentStatus.SelectedIndex == 1)
            {
                isPaid = true;
                isCancelled = false;
            }
            else if (cbPaymentStatus.SelectedIndex == 2)
            {
                isPaid = false;
                isCancelled = false;
            }
            else if (cbPaymentStatus.SelectedIndex == 3)
            {
                isCancelled = true;
            }

            Response response = await _apiService.SearchInvoices(
                    customerId,
                    startDate,
                    endDate,
                    isPaid,
                    isCancelled);

            if (response.IsSuccess)
            {
                List<Invoice> invoices = (List<Invoice>)response.Result;

                dgInvoices.ItemsSource = invoices;
            }
            else
            {
                dgInvoices.ItemsSource = null;

                MessageBox.Show(response.Message,
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }


        private async void btnMarkAsPaid_Click(object sender, RoutedEventArgs e)
        {
            if (dgInvoices.SelectedItem == null)
            {
                MessageBox.Show("Selecione uma fatura.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Invoice selectedInvoice = (Invoice)dgInvoices.SelectedItem;

            if (selectedInvoice.IsCancelled)
            {
                MessageBox.Show("Não é possível pagar uma fatura cancelada.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (selectedInvoice.IsPaid)
            {
                MessageBox.Show("Esta fatura já está paga.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult result = MessageBox.Show("Deseja marcar esta fatura como paga?",
                "Confirmação",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            selectedInvoice.IsPaid = true;
            selectedInvoice.IsCancelled = false;

            Response response = await _apiService.UpdateInvoice(selectedInvoice);

            if (response.IsSuccess)
            {
                await SearchInvoicesAsync();

                MessageBox.Show("Fatura marcada como paga.",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(response.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void btnCancelInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (dgInvoices.SelectedItem == null)
            {
                MessageBox.Show("Selecione uma fatura.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Invoice selectedInvoice = (Invoice)dgInvoices.SelectedItem;

            if (selectedInvoice.IsCancelled)
            {
                MessageBox.Show("Esta fatura já está cancelada.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (selectedInvoice.IsPaid)
            {
                MessageBox.Show("Não é possível cancelar uma fatura paga.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult result = MessageBox.Show("Deseja realmente cancelar esta fatura?",
                "Confirmação",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                return;
            }

            selectedInvoice.IsCancelled = true;
            selectedInvoice.IsPaid = false;

            Response response = await _apiService.UpdateInvoice(selectedInvoice);

            if (response.IsSuccess)
            {
                await SearchInvoicesAsync();

                MessageBox.Show("Fatura cancelada com sucesso.",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(response.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void btnViewInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (dgInvoices.SelectedItem == null)
            {
                MessageBox.Show("Selecione uma fatura.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Invoice selectedInvoice = (Invoice)dgInvoices.SelectedItem;

            Response response = await _apiService.GetInvoiceById(selectedInvoice.InvoiceId);

            if (response.IsSuccess)
            {
                Invoice invoice =
                    (Invoice)response.Result;

                InvoiceDetailsWindow invoiceDetailsWindow = new InvoiceDetailsWindow(invoice);
                invoiceDetailsWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show(response.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

        }
    }
}
