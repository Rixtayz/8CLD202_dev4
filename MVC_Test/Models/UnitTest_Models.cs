using MVC.Models;

namespace MVC_Test.Models
{
    public class UnitTest
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("Guillaume Routhier", "Guillaume Routhier")]
        [InlineData(@"LongString with many !@#%%$&&?(**$?%!@$\\", @"LongString with many !@#%%$&&?(**$?%!@$\\")]

        public void Post_Creation_Title(string title, string expectedTitle)
        {
            // Arrange
            Post post = new Post { Title = title, User = "", Image = new byte[1] };

            // Act


            // Asset
            Assert.Equal(post.Title, expectedTitle);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2, 2)]
        [InlineData(1, 1)]

        public void Post_Creation_Category(int category, int expectedCategory)
        {
            // Arrange
            Post post = new Post { Title = "", User = "", Image = new byte[1], Category = (Category)category };

            // Act


            // Asset
            Assert.True(post.Category == (Category)expectedCategory);
        }
    }
}