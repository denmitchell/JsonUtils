using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.JsonUtils.Tests {
    public class ValidationResultTests {
        private readonly ITestOutputHelper output;

        public ValidationResultTests(ITestOutputHelper output) {
            this.output = output;
        }

        ValidationResults vr = new ValidationResults() {
            PassesJsonStructureTest = true,
            PassesModelValidationTest = false,
            Results = new List<Result>() {
                new Result() {
                    SubmitterId = 1,
                    SubmitterRecordId = 1,
                    AttemptedOperation = "Add",
                    Successful = true,
                    Errors = null
                },
                new Result() {
                    SubmitterId = 1,
                    SubmitterRecordId = 2,
                    AttemptedOperation = "Add",
                    Successful = false,
                    Errors = new List<ErrorRecord>() {
                        new ErrorRecord() {
                            Data = "Position(Worker),IsManager(True)",
                            Code = 110105,
                            Description = "Invalid combination",
                            Properties = new List<string>() {
                                "Position", "IsManager"
                            }
                        }
                    }
                },
                new Result() {
                    SubmitterId = 1,
                    SubmitterRecordId = 3,
                    AttemptedOperation = "Modify",
                    Successful = true,
                    Errors = null
                }
            }
        };

        

        [Fact]
        public void SerializeValidationResult() {

            var expectedJsonFile = $"ValidationResult\\sample1.json";

            var expectedJson = File.ReadAllText(expectedJsonFile);
            var expected = JToken.Parse(expectedJson).ToObject<ValidationResults>();

            string json1 = JsonConvert.SerializeObject(vr,
                    Formatting.Indented, new SafeJsonConverter() {
                });
            string json2 = JsonConvert.SerializeObject(expected,
                    Formatting.Indented, new SafeJsonConverter() {
                });

            output.WriteLine("actual:\n" + json1);

            Assert.Equal(json1, json2);

        }

    }
}
