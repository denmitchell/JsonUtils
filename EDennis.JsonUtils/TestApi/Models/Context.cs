using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApi.Models {
    public class Context {

        public static List<Person> Persons = new List<Person> {
            new Person { PersonId = 1, FirstName = "Bob", LastName = "Barker" },
            new Person { PersonId = 2, FirstName = "Drew", LastName = "Carey"}
        };
    }
}
