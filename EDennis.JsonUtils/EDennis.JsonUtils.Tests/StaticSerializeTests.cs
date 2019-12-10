using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using System;
using NutsAndBolts.Tests;

namespace EDennis.JsonUtils.Tests {
    public class StaticSerializeTests {

        private readonly ITestOutputHelper _output;
        public StaticSerializeTests(ITestOutputHelper output) {
            _output = output;
        }

        internal class Parent {
            public int Id { get; set; }
            public List<Child> Children { get; set; }
        }
        internal class Child {
            public int Id { get; set; }
            public Parent Parent { get; set; }
        }

        [Fact]
        public void TestCircular() {
            var obj = new Parent { Id = 1 };
            obj.Children = new List<Child> {
                new Child { Id = 1,  Parent = obj },
                new Child { Id = 2,  Parent = obj },
            };

            var actual = SafeJsonSerializer.Serialize(obj);
            var expected = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<Parent>
                (File.ReadAllText("StaticSerializeTests/TestCircular/0.json")),
                new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });

            Assert.Equal(expected, actual);
        }


        internal class Person {
            public int? Id { get; set; }
            public string Name { get; set; }
            public DateTime? SysStart { get; set; }
            public DateTime? SysEnd { get; set; }
            public List<Address> Addresses { get; set; }
        }

        internal class Address {
            public int? Id { get; set; }
            public string City { get; set; }
            public DateTime? SysStart { get; set; }
            public DateTime? SysEnd { get; set; }
        }


        Person[] persons = new Person[] {
            new Person {
                Id = 1,
                Name = "Moe",
                SysStart = new DateTime(2000,1,1),
                SysEnd = new DateTime(2000,2,1),
                Addresses = new List<Address> {
                    new Address {
                        Id = 1,
                        City = "Hartford",
                        SysStart = new DateTime(2000,1,1),
                        SysEnd = new DateTime(2000,2,1)
                    },
                    new Address {
                        Id = 2,
                        City = "Bridgeport",
                        SysStart = new DateTime(2000,1,1),
                        SysEnd = new DateTime(2000,2,1)
                    }
                }
            },
            new Person {
                Id = 2,
                Name = "Larry",
                SysStart = new DateTime(2001,1,1),
                SysEnd = new DateTime(2001,2,1),
                Addresses = new List<Address> {
                    new Address {
                        Id = 3,
                        City = "Boston",
                        SysStart = new DateTime(2001,1,1),
                        SysEnd = new DateTime(2001,2,1)
                    },
                    new Address {
                        Id = 4,
                        City = "Springfield",
                        SysStart = new DateTime(2001,1,1),
                        SysEnd = new DateTime(2001,2,1)
                    }
                }
            }
        };

        string[][] propertiesToIgnore = new string[][] {
            new string[] {"Id"},
            new string[] {"SysStart","SysEnd"}
        };


        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void TestPropertiesToIgnore(int testCase) {

            var actual = SafeJsonSerializer.Serialize(persons, 99, true, propertiesToIgnore[testCase]);
            var expected = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<List<Person>>
                (File.ReadAllText($"StaticSerializeTests/TestPropertiesToIgnore/{testCase}.json")),
                new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });

            if (actual != expected) {
                var fsc = FileStringComparer.GetSideBySideFileStrings(expected, actual, "EXPECTED", "ACTUAL");
                _output.WriteLine(fsc);
            }


            Assert.Equal(expected, actual);
        }

        internal class A {
            public int? Id { get; set; }
            public B B { get; set; }
            public B[] Bs { get; set; }
        }
        internal class B {
            public int? Id { get; set; }
            public C C { get; set; }
            public C[] Cs { get; set; }
        }

        internal class C {
            public int? Id { get; set; }
            public D D { get; set; }
            public D[] Ds { get; set; }
        }

        internal class D {
            public int? Id { get; set; }
        }

        A[] aRecs = new A[]{
            new A {
                Id = 1,
                B = new B {
                    Id = 2,
                    C = new C {
                        Id = 3,
                        D = new D {
                            Id = 4
                        },
                        Ds = new D[] {
                           new D {
                                Id = 4
                           }
                        }
                    },
                    Cs = new C[] {
                        new C {
                            Id = 3,
                            D = new D {
                                Id = 4
                            },
                            Ds = new D[] {
                               new D {
                                    Id = 4
                               }
                            }
                        }
                    }
                },
                Bs = new B[] {
                    new B {
                    Id = 2,
                    C = new C {
                        Id = 3,
                        D = new D {
                            Id = 4
                        },
                        Ds = new D[] {
                           new D {
                                Id = 4
                           }
                        }
                    },
                    Cs = new C[] {
                        new C {
                            Id = 3,
                            D = new D {
                                Id = 4
                            },
                            Ds = new D[] {
                               new D {
                                    Id = 4
                               }
                            }
                        }
                    }
                }
                },

            }
        };


        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void TestMaxDepth(int testCase) {

            var actual = SafeJsonSerializer.Serialize(aRecs, testCase);
            var expected = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<A[]>
                (File.ReadAllText($"StaticSerializeTests/TestMaxDepth/{testCase}.json")),
                new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });

            if (actual != expected) {
                var fsc = FileStringComparer.GetSideBySideFileStrings(expected, actual, "EXPECTED", "ACTUAL");
                _output.WriteLine(fsc);
            }


            Assert.Equal(expected, actual);
        }



    }
}
