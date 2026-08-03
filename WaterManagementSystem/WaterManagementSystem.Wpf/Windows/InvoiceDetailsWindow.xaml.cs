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

namespace WaterManagementSystem.Wpf.Windows
{
    
    public partial class InvoiceDetailsWindow : Window
    {
        private Invoice _invoice;
        public InvoiceDetailsWindow(Invoice invoice)
        {
            InitializeComponent();
            _invoice = invoice;


            LoadInvoiceDetails();
            
        }

        /// <summary>
        /// Displays the selected invoice details in the form fields.
        /// </summary>
        private void LoadInvoiceDetails()
        {
            txtCustomerName.Text = _invoice.CustomerName;
            txtTaxNumber.Text = _invoice.TaxNumber;
            txtCustomerAddress.Text = _invoice.CustomerAddress;
            txtCustomerPhone.Text = _invoice.CustomerPhone;
            txtCustomerEmail.Text = _invoice.CustomerEmail;

            txtMeterId.Text = _invoice.MeterId.ToString();
            txtReadingDate.Text = _invoice.ReadingDate.ToString("dd/MM/yyyy");
            txtMeterReading.Text = _invoice.MeterReading.ToString();
            txtConsumedVolume.Text = _invoice.ConsumedVolume.ToString("F2") + " m³";

            txtInvoiceId.Text = _invoice.InvoiceId.ToString();
            txtIssueDate.Text = _invoice.IssueDate.ToString("dd/MM/yyyy");
            txtInvoiceAmount.Text = _invoice.InvoiceAmount.ToString("F2");

            if (_invoice.IsCancelled)
            {
                txtInvoiceStatus.Text = "Cancelada";
            }
            else if (_invoice.IsPaid)
            {
                txtInvoiceStatus.Text = "Paga";
            }
            else
            {
                txtInvoiceStatus.Text = "Não paga";
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
