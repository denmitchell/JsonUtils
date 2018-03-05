using Newtonsoft.Json;

namespace NutsAndBolts {
    public class PartSupplier {
        public int SupplierId { get; set; }
        public int PartId { get; set; }
        public int? Quantity { get; set; }
        public Part Part { get; set; }
        public Supplier Supplier { get; set; }
    }
}
