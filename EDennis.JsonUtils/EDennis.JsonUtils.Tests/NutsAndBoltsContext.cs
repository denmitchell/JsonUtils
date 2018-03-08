using System.Collections.Generic;
using System.Text;

namespace NutsAndBolts {
    public class NutsAndBoltsContext {
        public List<Part> Parts { get; set; } = new List<Part>();
        public List<PartSupplier> PartSuppliers { get; set; } = new List<PartSupplier>();
        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();
    }
}
