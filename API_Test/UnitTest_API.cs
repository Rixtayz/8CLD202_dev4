using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

using MVC.Models;
using MVC.Data;


namespace API_Test
{
    public class PostControllerTests
    {
        [Theory]
        [InlineData("Test Title", "User", "C:\\Users\\gui44\\OneDrive\\Bureau\\meme2\\meme1.jpg")]
        [InlineData("Test Title 1", "User 1", "C:\\Users\\gui44\\OneDrive\\Bureau\\meme2\\meme1.jpg")]
        [InlineData("Test Title 2", "User 2", "C:\\Users\\gui44\\OneDrive\\Bureau\\meme2\\meme1.jpg")]
        [InlineData("Test Title 3", "User 3", "C:\\Users\\gui44\\OneDrive\\Bureau\\meme2\\meme1.jpg")]
        [InlineData("Test Title 4", "User 4", "C:\\Users\\gui44\\OneDrive\\Bureau\\meme2\\meme1.jpg")]
        [InlineData("Test Title 5", "User 5", "C:\\Users\\gui44\\OneDrive\\Bureau\\meme2\\meme1.jpg")]
        [InlineData("Test Title 6", "User 6", "C:\\Users\\gui44\\OneDrive\\Bureau\\meme2\\meme1.jpg")]
        [InlineData("Test Title 7", "User 7", "C:\\Users\\gui44\\OneDrive\\Bureau\\meme2\\meme1.jpg")]
        [InlineData("Test Title 8", "User 8", "C:\\Users\\gui44\\OneDrive\\Bureau\\meme2\\meme1.jpg")]
        [InlineData("Test Title 9", "User 9", "C:\\Users\\gui44\\OneDrive\\Bureau\\meme2\\meme1.jpg")]

        public async Task AddPost_Should_Return_Created_Result(string Title, string User, string Image)
        {
            HttpClient _client = new HttpClient
            {
                BaseAddress = new Uri("https://127.00.0.1")
            };

            var imageContent = new ByteArrayContent(File.ReadAllBytes(Image));
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            var content = new MultipartFormDataContent();
            content.Add(imageContent, "Image", "test-image.jpg");
            content.Add(new StringContent(Title), "Title");
            content.Add(new StringContent(Category.Nouvelle.ToString()), "Category");
            content.Add(new StringContent(User), "User");

            var response = await _client.PostAsync("/Posts/Add", content);

            response.EnsureSuccessStatusCode();

            var createdPost = await response.Content.ReadAsAsync<PostReadDTO>();
            Assert.Equal(Title, createdPost.Title);
            Assert.Equal(User, createdPost.User);
        }


    }
}