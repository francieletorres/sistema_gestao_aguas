using System;
using System.Collections.Generic;
using System.Text;

namespace WaterManagementSystem.Wpf.Models
{
    public class Meter
    {
        public int MeterId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public DateTime InstallationDate { get; set; }

        public bool IsActive { get; set; }

        public override string ToString()
        {
            return $"Contador Número: {MeterId}";
        }
    }
}
