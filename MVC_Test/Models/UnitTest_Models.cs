using System;
using MVC.Models;
using Xunit;

namespace MVC_Test.Models
{
    public class UnitTest_Post
    {
        [Fact]
        public void IncrementLike_ShouldIncrementLike()
        {
            // Arrange
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = "Test Post",
                Category = Category.Humour,
                User = "TestUser",
                BlobImage = Guid.NewGuid(),
                Url = "http://example.com"
            };

            // Act
            post.IncrementLike();

            // Assert
            Assert.Equal(1, post.Like);
        }

        [Fact]
        public void IncrementDislike_ShouldIncrementDislike()
        {
            // Arrange
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = "Test Post",
                Category = Category.Humour,
                User = "TestUser",
                BlobImage = Guid.NewGuid(),
                Url = "http://example.com"
            };

            // Act
            post.IncrementDislike();

            // Assert
            Assert.Equal(1, post.Dislike);
        }

        [Fact]
        public void Approve_ShouldSetIsApprovedToTrue()
        {
            // Arrange
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = "Test Post",
                Category = Category.Humour,
                User = "TestUser",
                BlobImage = Guid.NewGuid(),
                Url = "http://example.com"
            };

            // Act
            post.Approve();

            // Assert
            Assert.True(post.IsApproved);
        }

        [Fact]
        public void Delete_ShouldSetIsDeletedToTrue()
        {
            // Arrange
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = "Test Post",
                Category = Category.Humour,
                User = "TestUser",
                BlobImage = Guid.NewGuid(),
                Url = "http://example.com"
            };

            // Act
            post.Delete();

            // Assert
            Assert.True(post.IsDeleted);
        }

        [Fact]
        public void ToString_ShouldReturnCorrectString()
        {
            // Arrange
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = "Test Post",
                Category = Category.Humour,
                User = "TestUser",
                BlobImage = Guid.NewGuid(),
                Url = "http://example.com"
            };

            var expectedString = "===============\r\n" +
                                 "Title : Test Post\r\n" +
                                 "Category : Humour\r\n" +
                                 "User : TestUser\r\n" +
                                 "Like : 0\r\n" +
                                 "Dislike : 0\r\n" +
                                 "Created : " + post.Created.ToString() + "\r\n" +
                                 "===============";

            // Act
            var result = post.ToString();

            // Assert
            Assert.Equal(expectedString, result);
        }
    }

    public class UnitTest_Comment
    {
        [Fact]
        public void IncrementLike_ShouldIncrementLike()
        {
            // Arrange
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Commentaire = "Test Comment",
                User = "TestUser",
                PostId = Guid.NewGuid(),
                Post = new Post { Id = Guid.NewGuid(), Title = "Test Post", User = "TestUser", BlobImage = Guid.NewGuid(), Url = "" }
            };

            // Act
            comment.IncrementLike();

            // Assert
            Assert.Equal(1, comment.Like);
        }

        [Fact]
        public void IncrementDislike_ShouldIncrementDislike()
        {
            // Arrange
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Commentaire = "Test Comment",
                User = "TestUser",
                PostId = Guid.NewGuid(),
                Post = new Post { Id = Guid.NewGuid(), Title = "Test Post", User = "TestUser", BlobImage = Guid.NewGuid(), Url = "" }
            };

            // Act
            comment.IncrementDislike();

            // Assert
            Assert.Equal(1, comment.Dislike);
        }

        [Fact]
        public void Approve_ShouldSetIsApprovedToTrue()
        {
            // Arrange
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Commentaire = "Test Comment",
                User = "TestUser",
                PostId = Guid.NewGuid(),
                Post = new Post { Id = Guid.NewGuid(), Title = "Test Post", User = "TestUser", BlobImage = Guid.NewGuid(), Url = "" }
            };

            // Act
            comment.Approve();

            // Assert
            Assert.True(comment.IsApproved);
        }

        [Fact]
        public void Delete_ShouldSetIsDeletedToTrue()
        {
            // Arrange
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Commentaire = "Test Comment",
                User = "TestUser",
                PostId = Guid.NewGuid(),
                Post = new Post { Id = Guid.NewGuid(), Title = "Test Post", User = "TestUser", BlobImage = Guid.NewGuid(), Url = "" }
            };

            // Act
            comment.Delete();

            // Assert
            Assert.True(comment.IsDeleted);
        }

        [Fact]
        public void ToString_ShouldReturnCorrectString()
        {
            // Arrange
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Commentaire = "Test Comment",
                User = "TestUser",
                PostId = Guid.NewGuid(),
                Post = new Post { Id = Guid.NewGuid(), Title = "Test Post", User = "TestUser", BlobImage = Guid.NewGuid(), Url = "" }
            };

            var expectedString = "===============\r\n" +
                                 "Comment : Test Comment\r\n" +
                                 "User : TestUser\r\n" +
                                 "Like : 0\r\n" +
                                 "Dislike : 0\r\n" +
                                 "Created : " + comment.Created.ToString() + "\r\n" +
                                 "===============";

            // Act
            var result = comment.ToString();

            // Assert
            Assert.Equal(expectedString, result);
        }
    }
}
