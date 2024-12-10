using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

using MVC.Models;
using MVC.Data;

namespace API_Test
{
    public class ApiTestFixture : IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        public HttpClient Client { get; }
        public IServiceProvider Services { get; }

        public ApiTestFixture()
        {
            _factory = new WebApplicationFactory<Program>();
            Client = _factory.CreateClient();
            Services = _factory.Services;
        }

        public void Dispose()
        {
            _factory.Dispose();
        }
    }

    [CollectionDefinition("Api collection")]
    public class ApiCollection : ICollectionFixture<ApiTestFixture> { }


    [Collection("Api collection")]
    public class PostControllerTests
    {
        private readonly HttpClient _client;
        private readonly IServiceProvider _services;

        public PostControllerTests(ApiTestFixture fixture)
        {
            _client = fixture.Client;
            _services = fixture.Services;
        }

        [Fact]
        public async Task AddPost_Should_Return_Created_Result()
        {
            var imageContent = new ByteArrayContent(File.ReadAllBytes("Meme2.png"));
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            var content = new MultipartFormDataContent();
            content.Add(imageContent, "Image", "test-image.jpg");
            content.Add(new StringContent("Test Title"), "Title");
            content.Add(new StringContent(Category.Nouvelle.ToString()), "Category");
            content.Add(new StringContent("User1"), "User");

            var response = await _client.PostAsync("/Posts/Add", content);

            response.EnsureSuccessStatusCode();

            var createdPost = await response.Content.ReadAsAsync<PostReadDTO>();
            Assert.Equal("Test Title", createdPost.Title);
        }
    }
}