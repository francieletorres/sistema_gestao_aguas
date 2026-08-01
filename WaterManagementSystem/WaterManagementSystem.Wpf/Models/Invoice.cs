using System;
using System.Collections.Generic;
using System.Text;

namespace WaterManagementSystem.Wpf.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        public int ConsumptionId { get; set; }

        public DateTime IssueDate { get; set; }

        public decimal InvoiceAmount { get; set; }

        public bool IsCancelled { get; set; }

        public bool IsPaid { get; set; }

        public int MeterId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerAddress { get; set; }

        public string CustomerPhone { get; set; }

        public string CustomerEmail { get; set; }

        public string TaxNumber { get; set; }

        public DateTime ReadingDate { get; set; }

        public int MeterReading { get; set; }

        public decimal ConsumedVolume { get; set; }

    }
}
