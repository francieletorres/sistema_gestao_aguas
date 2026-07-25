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
                MessageBox.Show(response.Message);
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
                    MessageBox.Show("Customer registered successfully!",
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

        private void ClearFields()
        {
            txtName.Text = string.Empty;
            txtAddress.Text = string.Empty;
            txtPhone.Text = string.Empty;
            txtTaxNumber.Text = string.Empty;
            txtEmail.Text = string.Empty;
        }

        private bool ValidateForm()
        {
            bool output = true;

            if (string.IsNullOrWhiteSpace(txtName.Text) || !Regex.IsMatch(txtName.Text, @"^[A-Za-zÀ-ÿ ]+$"))
            {
                MessageBox.Show("Please enter a valid customer name.",
                     "Validation error",
                     MessageBoxButton.OK,
                     MessageBoxImage.Error);

                output = false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text) || !Regex.IsMatch(txtPhone.Text, @"^[239]\d{8}$"))
            {
                MessageBox.Show("Please enter a valid phone number.",
                     "Validation error",
                     MessageBoxButton.OK,
                     MessageBoxImage.Error);

                output = false;
            }

            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Please enter the address.",
                    "Validation error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                output |= false;
            }


            if (string.IsNullOrWhiteSpace(txtTaxNumber.Text) || !Regex.IsMatch(txtTaxNumber.Text, @"^\d{9}$"))
            {
                MessageBox.Show("Please enter a valid tax number.",
                    "Validation error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                output = false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !Regex.IsMatch(txtEmail.Text, @"^[\w\.-]+@([\w-]+\.)+[\w-]{2,}$"))
            {
                MessageBox.Show("Please enter a valid email address.",
                    "Validation error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                output = false;
            }

            return output;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private async void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            if(dgCustomers.SelectedItem == null)
            {
                MessageBox.Show("Please select a customer.",
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
                MessageBox.Show("Please select a customer.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            //guarda o cliente selecionado no Datagrid
            Customer customerToDelete = (Customer)dgCustomers.SelectedItem;

            MessageBoxResult confirmation = MessageBox.Show("Are you sure you want to delete this customer?",
                "Confirmation",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);

            if (confirmation == MessageBoxResult.OK)
            {
                //resposta da api, se conseguiu apagaar ou nao
                Response  deleteResponse = await _apiService.DeleteCustomer(customerToDelete.CustomerId);

                if (deleteResponse.IsSuccess)
                {
                    MessageBox.Show("Customer deleted successfully!",
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
