using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestApi.Models;

namespace TestApi.Controllers {
    [Route("api/[controller]")]
    public class PersonController : Controller {
        // GET api/Person
        [HttpGet]
        public IEnumerable<Person> GetPersons() {
            return Context.Persons;
        }

        // GET api/Person/5
        [HttpGet("{id}")]
        public Person GetPerson(int id) {
            return Context.Persons[id - 1];
        }

        // POST api/Person
        [HttpPost]
        public IEnumerable<Person> PostPerson([FromBody]Person person) {
            Context.Persons.Add(person);
            return Context.Persons;
        }

    }
}
