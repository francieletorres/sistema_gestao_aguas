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

            if(response.IsSuccess)
            {
                List<Customer> customers = (List<Customer>)response.Result; 

                dgCustomers.ItemsSource = customers;

            }
            else
            {
                MessageBox.Show(response.Message);
            }
        }

        
    }
}
