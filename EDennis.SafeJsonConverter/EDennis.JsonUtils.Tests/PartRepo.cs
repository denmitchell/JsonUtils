using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NutsAndBolts {
    public class PartRepo {
        private NutsAndBoltsContext context;

        public PartRepo(NutsAndBoltsContext context) {
            this.context = context;
        }

        public Part GetById(int id) {
            var query = 
                from p in context.Parts
                where p.PartId == id
                select p;
            return query.FirstOrDefault();
        }

    }
}
