using EDennis.NetCoreTestingUtilities.Extensions;
using Microsoft.AspNetCore.TestHost;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestApi.Models;
using Xunit;
using Xunit.Abstractions;

namespace TestApi.Tests {
    public class ItemControllerIntegrationTests : IClassFixture<HttpClientFixture> {

        private readonly ITestOutputHelper output;
        private readonly TestServer _server;
        private readonly HttpClient _client;


        public ItemControllerIntegrationTests(ITestOutputHelper output, HttpClientFixture fixture) {
            _server = fixture.Server;
            _client = fixture.Client;
            this.output = output;
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task  GetPerson(int personId) {

            Person expected = new Person().FromJsonPath($"PersonController\\GetPerson\\expected{personId}.json");

            var response = await _client.GetAsync($"/api/Person/{personId}");//.Result;
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();//.Result;
            Person actual = new Person().FromJsonString(responseString);

            Assert.True(actual.IsEqualOrWrite(expected,output));

            return;
        }


        [Theory]
        [InlineData(1)]
        public async Task GetPersons(int personId) {

            List<Person> expected = new List<Person>().FromJsonPath($"PersonController\\GetPersons\\expected{personId}.json");

            var response = await _client.GetAsync($"/api/Person");//.Result;
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();//.Result;
            List<Person> actual = new List<Person>().FromJsonString(responseString);

            Assert.True(actual.IsEqualOrWrite(expected, output));

            return;
        }

        [Theory]
        [InlineData("1")]
        public async Task  PostItem(string testCase) {


            string newItemJson = File.ReadAllText($"PersonController\\PostPerson\\new{testCase}.json");
            List<Person> expected = new List<Person>().FromJsonPath($"PersonController\\PostPerson\\expected{testCase}.json");

            var content = new StringContent(newItemJson, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"/api/Person", content); //.Result;

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync(); //.Result;

            List<Person> actual = new List<Person>().FromJsonString(responseString);
            Assert.True(actual.IsEqualOrWrite(expected, output));


            return; //optional

        }



    }
}
