using System.Net.Http.Headers;
using MVC.Models;

namespace API_Test
{

    public class PostControllerTests
    {
        private const string HostURL = "https://localhost:7101";

        [Theory]
        [InlineData("Test Title", "User", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]
        [InlineData("Test Title 1", "User 1", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg")]
        [InlineData("Test Title 2", "User 2", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg")]
        [InlineData("Test Title 3", "User 3", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg")]
        [InlineData("Test Title 4", "User 4", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg")]
        [InlineData("Test Title 5", "User 5", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg")]
        [InlineData("Test Title 6", "User 6", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg")]
        [InlineData("Test Title 7", "User 7", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg")]
        [InlineData("Test Title 8", "User 8", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg")]
        [InlineData("Test Title 9", "User 9", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg")]
        public async Task AddPost_Should_Return_Created_Result(string Title, string User, string Image)
        {
            // Arrange
            HttpClient _client = new HttpClient
            {
                BaseAddress = new Uri(HostURL)
            };

            MemoryStream _stream = new MemoryStream();
            using (var stream = File.OpenRead(Image))
            {
                stream.CopyTo(_stream);
            }

            _stream.Position = 0;

            var imageContent = new ByteArrayContent(_stream.ToArray());
            _stream.Dispose();

            imageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "Image",
                FileName = "test-image.jpg"
            };
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            var content = new MultipartFormDataContent();
            content.Add(imageContent, "Image", "test-image.jpg");
            content.Add(new StringContent(Title), "Title");
            content.Add(new StringContent(Category.Nouvelle.ToString()), "Category");
            content.Add(new StringContent(User), "User");

            // Act
            var response = await _client.PostAsync("/Posts/Add", content);
            response.EnsureSuccessStatusCode();
            var createdPost = await response.Content.ReadAsAsync<PostReadDTO>();

            // Assert
            Assert.Equal(Title, createdPost.Title);
            Assert.Equal(User, createdPost.User);
            Assert.True(createdPost.Id != Guid.Empty);
        }


        [Theory]
        [InlineData("Test GetPost", "User", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme1.jpg")]
        [InlineData("Test GetPost 1", "User 1", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme2.jpg")]
        [InlineData("Test GetPost 2", "User 2", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme3.jpg")]
        [InlineData("Test GetPost 3", "User 3", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme4.jpg")]
        [InlineData("Test GetPost 4", "User 4", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme5.jpg")]
        [InlineData("Test GetPost 5", "User 5", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme6.jpg")]
        [InlineData("Test GetPost 6", "User 6", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme7.jpg")]
        [InlineData("Test GetPost 7", "User 7", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme8.jpg")]
        [InlineData("Test GetPost 8", "User 8", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme9.jpg")]
        [InlineData("Test GetPost 9", "User 9", @"C:\Users\gui44\OneDrive\Bureau\meme2\meme10.jpg")]
        public async Task GetPost_ShouldConfirmId(string Title, string User, string Image)
        {
            // Arrange
            HttpClient _client = new HttpClient
            {
                BaseAddress = new Uri(HostURL)
            };

            MemoryStream _stream = new MemoryStream();
            using (var stream = File.OpenRead(Image))
            {
                stream.CopyTo(_stream);
            }

            _stream.Position = 0;

            var imageContent = new ByteArrayContent(_stream.ToArray());
            _stream.Dispose();

            imageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "Image",
                FileName = "test-image.jpg"
            };
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            var content = new MultipartFormDataContent();
            content.Add(imageContent, "Image", "test-image.jpg");
            content.Add(new StringContent(Title), "Title");
            content.Add(new StringContent(Category.Nouvelle.ToString()), "Category");
            content.Add(new StringContent(User), "User");

            var response = await _client.PostAsync("/Posts/Add", content);
            response.EnsureSuccessStatusCode();
            var createdPost = await response.Content.ReadAsAsync<PostReadDTO>();

            // Act
            var response2 = await _client.GetAsync($"/Posts/{createdPost.Id}");
            response2.EnsureSuccessStatusCode();
            var postReadDTO = await response2.Content.ReadAsAsync<PostReadDTO>();

            // Assert
            Assert.Equal(postReadDTO.Id, createdPost.Id);
        }

        [Fact]
        public async Task GetPost()
        {
            // Arrange
            HttpClient _client = new HttpClient
            {
                BaseAddress = new Uri(HostURL)
            };

            // Act
            var response = await _client.GetAsync("/Posts/");
            response.EnsureSuccessStatusCode();
            var createdPost = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(createdPost.Length > 0);

        }
    }
}