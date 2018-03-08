using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.JsonUtils.Tests {

    public class Person {

        public Person() {
            SysStart = DateTime.Now;
            SysEnd = DateTime.MaxValue;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }
    }
}
