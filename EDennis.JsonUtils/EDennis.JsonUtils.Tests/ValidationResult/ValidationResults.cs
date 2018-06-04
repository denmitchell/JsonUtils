using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDennis.JsonUtils.Tests
{
    /// <summary>
    /// Encapsulates a Results object with some additional metadata
    /// that indicates whether the validations failed the first stage,
    /// failed the second stage, or passed both stages.
    /// </summary>
    public class ValidationResults {

        public ValidationResults() { }

        public bool? PassesJsonStructureTest { get; set; }
        public bool? PassesModelValidationTest { get; set; }

        [JsonIgnore]
        public int ErrorCount {
            get {
                return Results.Where(e => e.Successful != true).Count();
            }
        }

        public List<Result> Results { get; set; } = new List<Result>();
    }

}