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
    
    public partial class EditCustomerWindow : Window
    {
        private Customer _customerToEdit;

        private ApiService _apiService;

        public EditCustomerWindow(Customer customerToEdit)
        {
            InitializeComponent();
            _apiService = new ApiService();
            _customerToEdit = customerToEdit;

            txtName.Text = customerToEdit.Name;
            txtAddress.Text = customerToEdit.Address;
            txtPhone.Text = customerToEdit.Phone;
            txtTaxNumber.Text = customerToEdit.TaxNumber;
            txtEmail.Text = customerToEdit.Email;
            chkIsActive.IsChecked = customerToEdit.IsActive;
        }

        /// <summary>
        /// Validates the customer form fields.
        /// </summary>
        private bool ValidateForm()
        {
            bool output = true;

            if (string.IsNullOrWhiteSpace(txtName.Text) || !Regex.IsMatch(txtName.Text, @"^[A-Za-zÀ-ÿ ]+$"))
            {
                MessageBox.Show("Por favor, introduza um nome de cliente válido.",
                     "Validation error",
                     MessageBoxButton.OK,
                     MessageBoxImage.Error);

                output = false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text) || !Regex.IsMatch(txtPhone.Text, @"^[239]\d{8}$"))
            {
                MessageBox.Show("Por favor, introduza um contacto válido.",
                     "Validation error",
                     MessageBoxButton.OK,
                     MessageBoxImage.Error);

                output = false;
            }

            if (string.IsNullOrWhiteSpace(txtTaxNumber.Text) || !Regex.IsMatch(txtTaxNumber.Text, @"^\d{9}$"))
            {
                MessageBox.Show("Por favor, introduza um NIF válido.",
                    "Validation error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                output = false;
            }

            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Por favor, introduza a morada.",
                    "Validation error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                output |= false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !Regex.IsMatch(txtEmail.Text, @"^[\w\.-]+@([\w-]+\.)+[\w-]{2,}$"))
            {
                MessageBox.Show("Por favor, introduza um endereço de e-mail válido.",
                    "Validation error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                output = false;
            }

            return output;
        }
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                _customerToEdit.Name = txtName.Text;
                _customerToEdit.Address = txtAddress.Text;
                _customerToEdit.Phone = txtPhone.Text;
                _customerToEdit.TaxNumber = txtTaxNumber.Text;
                _customerToEdit.Email = txtEmail.Text;
                _customerToEdit.IsActive = chkIsActive.IsChecked == true;

                Response response = await _apiService.UpdateCustomer(_customerToEdit);

                if (response.IsSuccess)
                {
                    MessageBox.Show("Cliente atualizado com sucesso.",
                        "Sucess",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    this.Close();
                    
                }
                else
                {
                    MessageBox.Show(response.Message, "Error",
                     MessageBoxButton.OK,
                     MessageBoxImage.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
