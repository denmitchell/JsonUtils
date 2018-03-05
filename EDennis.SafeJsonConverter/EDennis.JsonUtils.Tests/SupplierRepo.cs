using System;
using System.Collections.Generic;
using System.Text;

namespace NutsAndBolts {
    public class SupplierRepo {

        private NutsAndBoltsContext context;

        public SupplierRepo(NutsAndBoltsContext context) {
            this.context = context;
        }

        public List<Part> GetPartsForSupplier(int supplierId) {
            var parts = new List<Part>(); 

            foreach (var ps in context.PartSuppliers) {
                if (ps.SupplierId == supplierId) {
                    parts.Add(ps.Part);
                }
            }
            return parts;
        }



    }
}
