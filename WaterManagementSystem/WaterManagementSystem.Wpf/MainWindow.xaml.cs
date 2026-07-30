using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WaterManagementSystem.Wpf.Windows;

namespace WaterManagementSystem.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnCustomers_Click(object sender, RoutedEventArgs e)
        {
            CustomerWindow customerWindow = new CustomerWindow();
            customerWindow.ShowDialog();
        }

        private void btnMeters_Click(object sender, RoutedEventArgs e)
        {
            MeterWindow meterWindow = new MeterWindow();
            meterWindow.ShowDialog();
        }

        private void btnConsumptions_Click(object sender, RoutedEventArgs e)
        {
            ConsumptionWindow consumptionWindow = new ConsumptionWindow();
            consumptionWindow.ShowDialog();
        }

        private void btnInvoices_Click(object sender, RoutedEventArgs e)
        {
            InvoiceWindow invoiceWindow = new InvoiceWindow();
            invoiceWindow.ShowDialog();
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
    }
}