using System;
using System.Collections.Generic;
using System.Text;

namespace WaterManagementSystem.Wpf.Models
{
    public class Consumption
    {
        public int ConsumptionId { get; set; }

        public int MeterId { get; set; }

        public string CustomerName { get; set; }

        public int MeterReading { get; set; }

        public DateTime ReadingDate { get; set; }

        public decimal ConsumedVolume { get; set; }

        public string Notes { get; set; }

        public bool HasInvoice { get; set; }

        public override string ToString()
        {
            return ReadingDate.ToString("dd/MM/yyyy")
                + " - "
                + ConsumedVolume.ToString("F2")
                + " m³";
        }

    }
}
