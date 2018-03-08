using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NutsAndBolts {
    public class Supplier {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? Since { get; set; }
        public string City { get; set; }
        public List<PartSupplier> PartSuppliers { get; set; } = new List<PartSupplier>();
    }
}
