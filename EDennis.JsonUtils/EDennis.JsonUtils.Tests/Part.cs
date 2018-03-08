using System.Collections.Generic;

namespace NutsAndBolts {
    public class Part {
        public int PartId { get; set; }
        public string PartName { get; set; }
        public string Material { get; set; }
        public double? Weight { get; set; }
        public string City { get; set; }
        public List<PartSupplier> PartSuppliers { get; set; } = new List<PartSupplier>();
    }
}
