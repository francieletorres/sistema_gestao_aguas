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
    /// Interaction logic for EditMeterWindow.xaml
    /// </summary>
    public partial class EditMeterWindow : Window
    {

        private Meter _meterToEdit;

        private ApiService _apiService;

        public EditMeterWindow(Meter meterToEdit)
        {
            InitializeComponent();

            _apiService = new ApiService();
            _meterToEdit = meterToEdit;

            txtCustomerName.Text = _meterToEdit.CustomerName;
            dpInstallationDate.SelectedDate = _meterToEdit.InstallationDate;
            chkIsActive.IsChecked = _meterToEdit.IsActive;

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
          
            _meterToEdit.IsActive = chkIsActive.IsChecked == true;

            Response response = await _apiService.UpdateMeter(_meterToEdit);

            if (response.IsSuccess)
            {
                MessageBox.Show("Meter updated successfully!",
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
}

