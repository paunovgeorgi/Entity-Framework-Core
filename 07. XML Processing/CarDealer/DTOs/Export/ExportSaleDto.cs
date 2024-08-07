﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CarDealer.DTOs.Export
{
    [XmlType("sale")]
    public class ExportSaleDto
    {
        [XmlElement("car")]
        public ExportCarDto19 Car { get; set; }

        [XmlElement("discount")]
        public decimal Discount { get; set; }

        [XmlElement("customer-name")]
        public string CustomerName { get; set; }

        [XmlElement("price")]
        public double Price { get; set; }

        [XmlElement("price-with-discount")]
        public double PriceWithDiscount { get; set; }


    }
}
