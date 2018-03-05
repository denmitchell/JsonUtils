using System;
using System.Collections.Generic;
using System.Text;

namespace NutsAndBolts.Tests {
    public class ContextFactory {

        public static NutsAndBoltsContext GetContext() {
            var context = new NutsAndBoltsContext();

            context.Parts.Add(new Part { PartId = 1, PartName = "Nut", Material = "Brass", Weight = 12.50, City = "London" });
            context.Parts.Add(new Part { PartId = 2, PartName = "Bolt", Material = "Brass", Weight = 27.00, City = "Paris" });
            context.Parts.Add(new Part { PartId = 3, PartName = "Screw", Material = "Stainless", Weight = 7.25, City = "Rome" });
            context.Parts.Add(new Part { PartId = 4, PartName = "Screw", Material = "Carbon", Weight = 6.75, City = "London" });
            context.Parts.Add(new Part { PartId = 5, PartName = "Washer", Material = "Chrome", Weight = 5.10, City = "Paris" });
            context.Parts.Add(new Part { PartId = 6, PartName = "Bolt", Material = "Stainless", Weight = 25.50, City = "London" });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 1, PartId = 1, Quantity = 300 });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 2, PartId = 1, Quantity = 300 });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 1, PartId = 2, Quantity = 200 });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 2, PartId = 2, Quantity = 400 });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 3, PartId = 2, Quantity = 200 });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 4, PartId = 2, Quantity = 200 });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 1, PartId = 3, Quantity = 400 });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 1, PartId = 4, Quantity = 200 });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 4, PartId = 4, Quantity = 300 });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 1, PartId = 5, Quantity = 100 });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 4, PartId = 5, Quantity = 400 });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 1, PartId = 6, Quantity = 100 });
            context.Suppliers.Add(new Supplier { SupplierId = 1, SupplierName = "Smith", Since = DateTime.Parse("2000-05-05"), City = "London" });
            context.Suppliers.Add(new Supplier { SupplierId = 2, SupplierName = "Jones", Since = DateTime.Parse("2012-10-12"), City = "Paris" });
            context.Suppliers.Add(new Supplier { SupplierId = 3, SupplierName = "Blake", Since = DateTime.Parse("1995-07-23"), City = "Paris" });
            context.Suppliers.Add(new Supplier { SupplierId = 4, SupplierName = "Clark", Since = DateTime.Parse("2006-11-03"), City = "London" });
            context.Suppliers.Add(new Supplier { SupplierId = 5, SupplierName = "Adams", Since = DateTime.Parse("1985-12-23"), City = "Athens" });

            context.PartSuppliers[0].Part = context.Parts[0];
            context.PartSuppliers[1].Part = context.Parts[0];
            context.PartSuppliers[2].Part = context.Parts[1];
            context.PartSuppliers[3].Part = context.Parts[1];
            context.PartSuppliers[4].Part = context.Parts[1];
            context.PartSuppliers[5].Part = context.Parts[1];
            context.PartSuppliers[6].Part = context.Parts[2];
            context.PartSuppliers[7].Part = context.Parts[3];
            context.PartSuppliers[8].Part = context.Parts[3];
            context.PartSuppliers[9].Part = context.Parts[4];
            context.PartSuppliers[10].Part = context.Parts[4];
            context.PartSuppliers[11].Part = context.Parts[5];
            context.PartSuppliers[0].Supplier = context.Suppliers[0];
            context.PartSuppliers[1].Supplier = context.Suppliers[1];
            context.PartSuppliers[2].Supplier = context.Suppliers[0];
            context.PartSuppliers[3].Supplier = context.Suppliers[1];
            context.PartSuppliers[4].Supplier = context.Suppliers[2];
            context.PartSuppliers[5].Supplier = context.Suppliers[3];
            context.PartSuppliers[6].Supplier = context.Suppliers[0];
            context.PartSuppliers[7].Supplier = context.Suppliers[0];
            context.PartSuppliers[8].Supplier = context.Suppliers[3];
            context.PartSuppliers[9].Supplier = context.Suppliers[0];
            context.PartSuppliers[10].Supplier = context.Suppliers[3];
            context.PartSuppliers[11].Supplier = context.Suppliers[0];

            context.Parts[0].PartSuppliers.Add(context.PartSuppliers[0]);
            context.Parts[0].PartSuppliers.Add(context.PartSuppliers[1]);
            context.Parts[1].PartSuppliers.Add(context.PartSuppliers[2]);
            context.Parts[1].PartSuppliers.Add(context.PartSuppliers[3]);
            context.Parts[1].PartSuppliers.Add(context.PartSuppliers[4]);
            context.Parts[1].PartSuppliers.Add(context.PartSuppliers[5]);
            context.Parts[2].PartSuppliers.Add(context.PartSuppliers[6]);
            context.Parts[3].PartSuppliers.Add(context.PartSuppliers[7]);
            context.Parts[3].PartSuppliers.Add(context.PartSuppliers[8]);
            context.Parts[4].PartSuppliers.Add(context.PartSuppliers[9]);
            context.Parts[4].PartSuppliers.Add(context.PartSuppliers[10]);
            context.Parts[5].PartSuppliers.Add(context.PartSuppliers[11]);
            context.Suppliers[0].PartSuppliers.Add(context.PartSuppliers[0]);
            context.Suppliers[1].PartSuppliers.Add(context.PartSuppliers[1]);
            context.Suppliers[0].PartSuppliers.Add(context.PartSuppliers[2]);
            context.Suppliers[1].PartSuppliers.Add(context.PartSuppliers[3]);
            context.Suppliers[2].PartSuppliers.Add(context.PartSuppliers[4]);
            context.Suppliers[3].PartSuppliers.Add(context.PartSuppliers[5]);
            context.Suppliers[0].PartSuppliers.Add(context.PartSuppliers[6]);
            context.Suppliers[0].PartSuppliers.Add(context.PartSuppliers[7]);
            context.Suppliers[3].PartSuppliers.Add(context.PartSuppliers[8]);
            context.Suppliers[0].PartSuppliers.Add(context.PartSuppliers[9]);
            context.Suppliers[3].PartSuppliers.Add(context.PartSuppliers[10]);
            context.Suppliers[0].PartSuppliers.Add(context.PartSuppliers[11]);

            return context;
        }

    }
}
