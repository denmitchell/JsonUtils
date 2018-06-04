using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.JsonUtils.Tests {
    /// <summary>
    /// This class associates an error code with
    /// its description and a set of properties that
    /// are relevant to the error code.
    /// </summary>
    public class ErrorCode {
        public int Code { get; set; }
        public string Description { get; set; }
        public List<string> Properties { get; set; } = new List<string>();
    }
}
