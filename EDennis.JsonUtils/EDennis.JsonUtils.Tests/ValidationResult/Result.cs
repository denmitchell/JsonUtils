using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace EDennis.JsonUtils.Tests {

    /// <summary>
    /// This class is a container for a single result.  It is intended to be the value
    /// associated with a key that represents the SubmitterRecordId for
    /// the submitted form.
    /// </summary>
    public class Result {
        public int SubmitterId { get; set; }
        public int SubmitterRecordId { get; set; }
        public string AttemptedOperation { get; set; }
        public bool Successful { get; set; }
        public List<ErrorRecord> Errors { get; set; }
    }
}
