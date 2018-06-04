namespace EDennis.JsonUtils.Tests {
    /// <summary>
    /// Extends the ErrorCode class by adding a Data property.  This
    /// property can be used to provide information about the context
    /// of the error (e.g., current data values).
    /// </summary>
    public class ErrorRecord : ErrorCode {
        public string Data { get; set; }
    }
}
