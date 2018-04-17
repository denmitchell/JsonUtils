using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NutsAndBolts {
    public class BadPart {
        public int PartId { get; set; }
        public string PartName { get; set; }
        public string Material { get; set; }

        //purposely invalid data format
        [DisplayFormat(DataFormatString = "{0, n2}", ApplyFormatInEditMode = true)]
        public decimal? Weight { get; set; }
        public string City { get; set; }
        public List<PartSupplier> PartSuppliers { get; set; } = new List<PartSupplier>();
    }
}
