using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CarDealer.DTOs.Import
{
    public class ImportSupplierDto
    {
        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        public bool isImporter { get; set; }
    }
}
