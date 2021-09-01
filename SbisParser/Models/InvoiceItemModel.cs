﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbisParser.Models
{
    public class InvoiceItemModel
    {
        public string Invoice { get; set; }
        public string Title { get; set; }
        public decimal CountItem { get; set; }
        public decimal PriceIncVat { get; set; }
        public decimal PriceExtVat { get; set; }
        public decimal Price { get; set; }
        public int VAT { get; set; }
        public decimal VATSum { get; set; }
    }
}
