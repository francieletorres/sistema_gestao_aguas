using System;
using System.Collections.Generic;
using System.Text;

namespace WaterManagementSystem.Wpf.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string TaxNumber { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }


    }
}
