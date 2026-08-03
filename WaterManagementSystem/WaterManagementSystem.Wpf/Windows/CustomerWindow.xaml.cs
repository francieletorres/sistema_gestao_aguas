using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
    /// <summary>
    /// Interaction logic for CustomerWindow.xaml
    /// </summary>
    public partial class CustomerWindow : Window
    {
        private readonly ApiService _apiService; //para chamar o Getcustomers na class apiservice
        public CustomerWindow()
        {
            InitializeComponent();

            _apiService = new ApiService();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomers();
        }

        private async Task LoadCustomers() //chamar os customers
        {
            Response response = await _apiService.GetCustomers();  //pede a api service para buscar os clientes

            if (response.IsSuccess)
            {
                List<Customer> customers = (List<Customer>)response.Result;

                dgCustomers.ItemsSource = customers;

            }
            else
            {
                MessageBox.Show(response.Message, "Error",
                       MessageBoxButton.OK,
                       MessageBoxImage.Error);
            }
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Customer newCustomer;

            if (ValidateForm())
            {
                newCustomer = new Customer
                {
                    Name = txtName.Text,
                    Address = txtAddress.Text,
                    Phone = txtPhone.Text,
                    TaxNumber = txtTaxNumber.Text,
                    Email = txtEmail.Text,
                    IsActive = true
                };

                Response response = await _apiService.CreateCustomer(newCustomer);

                if (response.IsSuccess)
                {
                    MessageBox.Show("Cliente registado com sucesso.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    ClearFields();

                    await LoadCustomers();
                }
                else
                {
                    MessageBox.Show(response.Message,"Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }   
            }
        }

        /// <summary>
        /// Clears all customer form fields.
        /// </summary>
        private void ClearFields()
        {
            txtName.Text = string.Empty;
            txtAddress.Text = string.Empty;
            txtPhone.Text = string.Empty;
            txtTaxNumber.Text = string.Empty;
            txtEmail.Text = string.Empty;
        }

        /// <summary>
        /// Validates the customer form fields.
        /// </summary>
        private bool ValidateForm()
        {

            if (string.IsNullOrWhiteSpace(txtName.Text) || !Regex.IsMatch(txtName.Text, @"^[A-Za-zÀ-ÿ ]+$"))
            {
                MessageBox.Show("Por favor, introduza um nome de cliente válido.",
                     "Validation error",
                     MessageBoxButton.OK,
                     MessageBoxImage.Error);

                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text) || !Regex.IsMatch(txtPhone.Text, @"^[239]\d{8}$"))
            {
                MessageBox.Show("Por favor, introduza um contacto válido.",
                     "Validation error",
                     MessageBoxButton.OK,
                     MessageBoxImage.Error);

                return false;
            }

            if (string.IsNullOrWhiteSpace(txtTaxNumber.Text) || !Regex.IsMatch(txtTaxNumber.Text, @"^\d{9}$"))
            {
                MessageBox.Show("Por favor, introduza um NIF válido.",
                    "Validation error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }

            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Por favor, introduza a morada.",
                    "Validation error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !Regex.IsMatch(txtEmail.Text, @"^[\w\.-]+@([\w-]+\.)+[\w-]{2,}$"))
            {
                MessageBox.Show("Por favor, introduza um endereço de e-mail válido.",
                    "Validation error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }

            return true;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            if(dgCustomers.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecione um cliente.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning); 
                return;
            }

            Customer customerToEdit = (Customer)dgCustomers.SelectedItem;

            EditCustomerWindow editCustomerWindow = new EditCustomerWindow(customerToEdit);
            editCustomerWindow.ShowDialog();

            await LoadCustomers();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgCustomers.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecione um cliente.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            //guarda o cliente selecionado no Datagrid
            Customer customerToDelete = (Customer)dgCustomers.SelectedItem;

            MessageBoxResult confirmation = MessageBox.Show("Tem a certeza de que pretende eliminar este cliente?",
                "Confirmation",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);

            if (confirmation == MessageBoxResult.OK)
            {
                //resposta da api, se conseguiu apagaar ou nao
                Response  deleteResponse = await _apiService.DeleteCustomer(customerToDelete.CustomerId);

                if (deleteResponse.IsSuccess)
                {
                    MessageBox.Show("Cliente elimiando com sucesso.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    await LoadCustomers();
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
