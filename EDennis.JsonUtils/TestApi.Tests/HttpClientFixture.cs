using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace TestApi.Tests {
    public class HttpClientFixture : IDisposable {

        public TestServer Server { get; }
        public HttpClient Client { get; }

        public HttpClientFixture() {
            Server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            Client = Server.CreateClient();
        }

        public void Dispose() {
            Client.Dispose();
            Server.Dispose();
        }
    }
}
