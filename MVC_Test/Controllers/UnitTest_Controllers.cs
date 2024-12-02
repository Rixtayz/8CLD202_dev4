using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using MVC;


//Nuget package :
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

//https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-9.0

namespace MVC_Test.Controllers
{
    public class UnitTest_Controllers : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _httpClient;

        public UnitTest_Controllers(WebApplicationFactory<Program> factory)
        {
            _httpClient = factory.CreateClient();
        }

        public class Home : UnitTest_Controllers
        { 
            public Home(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

            [Fact]
            public async Task Home_OK()
            {
                // Act 
                var result = await _httpClient.GetAsync("/");

                // Assert
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            }

            [Fact]
            public async Task Posts_OK()
            {
                // Act 
                var result = await _httpClient.GetAsync("/Posts/");

                // Assert
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            }

            [Fact]
            public async Task Comments_OK()
            {
                // Act 
                var result = await _httpClient.GetAsync("/Comments/");

                // Assert
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            }
        }


    }
}
