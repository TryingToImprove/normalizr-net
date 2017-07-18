using System;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Trying.Normalizr.Tests
{
    [TestClass]
    public class NormalizrTests
    {
        [TestMethod]
        public void Normalizr_ShouldAddReference()
        {
            var userSchema = Normalizr.Schema<User>("users", x => x.Id(y => y.Id));
            var blogSchema = Normalizr.Schema<Blog>("blogs", x =>
            {
                x.Id(y => y.Id);
                x.Reference(y => y.Author, userSchema);
            });
            
            Assert.IsTrue(blogSchema.References.ContainsKey("Author"));
            Assert.AreEqual(userSchema, blogSchema.References["Author"]);
        }

        [TestMethod]
        public void Normalizr_ShouldBeAbleToNormalize_NestedObject()
        {
            var userSchema = Normalizr.Schema<User>("users", x => x.Id(y => y.Id));
            var blogSchema = Normalizr.Schema<Blog>("blogs", x =>
            {
                x.Id(y => y.Id);
                x.Reference(y => y.Author, userSchema);
            });

            var input = new Blog
            {
                Id = 34,
                Author = new User
                {
                    Id = 21,
                    Name = "TryingToImprove"
                },
                Text = "This is a blog"
            };

            var normalizr = new Normalizr(blogSchema);
            var response = normalizr.Normalize(input);

            Assert.IsNotNull(response);

            Assert.IsTrue(response.Entities.ContainsKey("users"));

            var userEntities = response.Entities["users"];
            Assert.IsTrue(userEntities.ContainsKey(21));

            Assert.IsTrue(response.Entities.ContainsKey("blogs"));

            var blogEntities = response.Entities["blogs"];
            Assert.IsTrue(blogEntities.ContainsKey(34));

            Assert.AreEqual(34, response.Result);
        }

        [TestMethod]
        public void Normalizr_ShouldBeAbleToNormalize_NestedArray()
        {
            var userSchema = Normalizr.Schema<User>("users", x => x.Id(y => y.Id));
            var commentSchema = Normalizr.Schema<Comment>("comments", x =>
            {
                x.Id(y => y.Id);
                x.Reference(y => y.Author, userSchema);
            });
            var blogSchema = Normalizr.Schema<Blog>("blogs", x =>
            {
                x.Id(y => y.Id);
                x.Reference(y => y.Author, userSchema);
                x.Reference(y => y.Comments, commentSchema);
            });

            var input = new Blog
            {
                Id = 34,
                Author = new User
                {
                    Id = 21,
                    Name = "TryingToImprove"
                },
                Text = "This is a blog",
                Comments = new List<Comment>()
                {
                    new Comment
                    {
                        Id = 2,
                        Author = new User
                        {
                            Id = 54,
                            Name = "RandomUser"
                        },
                        Text = "This is a comment 1"
                    },

                    new Comment
                    {
                        Id = 4,
                        Author = new User
                        {
                            Id = 21,
                            Name = "TryingToImprove"
                        },
                        Text = "This is a comment 1"
                    }
                }
            };

            var normalizr = new Normalizr(blogSchema);
            var response = normalizr.Normalize(input);

            Assert.IsNotNull(response);

            Assert.IsTrue(response.Entities.ContainsKey("users"));

            var userEntities = response.Entities["users"];
            Assert.AreEqual(2, userEntities.Count);
            Assert.IsTrue(userEntities.ContainsKey(21));
            Assert.IsTrue(userEntities.ContainsKey(54));

            Assert.IsTrue(response.Entities.ContainsKey("blogs"));

            var blogEntities = response.Entities["blogs"];
            var blog = blogEntities[34] as JToken;
            Assert.IsNotNull(blog);

            var blogComments = blog["Comments"].ToObject<int[]>();
            Assert.AreEqual(2, blogComments.Length);
            Assert.IsTrue(blogComments.Contains(2));
            Assert.IsTrue(blogComments.Contains(4));

            Assert.IsTrue(response.Entities.ContainsKey("comments"));
            var commentEntities = response.Entities["comments"];
            Assert.AreEqual(2, commentEntities.Count);
            Assert.IsTrue(commentEntities.ContainsKey(2));
            Assert.IsTrue(commentEntities.ContainsKey(4));

            Assert.AreEqual(34, response.Result);
        }

        [TestMethod]
        public void Normalizr_ShouldBeAbleToNormalize_Array()
        {
            var userSchema = Normalizr.Schema<User>("users", x => x.Id(y => y.Id));

            var input = new[]
            {
                new User
                {
                    Id = 21,
                    Name = "TryingToImprove"
                },
                new User
                {
                    Id = 54,
                    Name = "RandomUser"
                }
            };

            var normalizr = new Normalizr(userSchema);
            var response = normalizr.Normalize(input);

            Assert.IsNotNull(response);

            Assert.IsTrue(response.Entities.ContainsKey("users"));

            var userEntities = response.Entities["users"];
            Assert.AreEqual(2, userEntities.Count);
            Assert.IsTrue(userEntities.ContainsKey(21));
            Assert.IsTrue(userEntities.ContainsKey(54));

            var result = response.Result as IEnumerable<object>;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains(21));
            Assert.IsTrue(result.Contains(54));
        }

        [TestMethod]
        public void Normalizr_ShouldBeAbleToNormalize_Temporary()
        {
            var userSchema = Normalizr.Schema<User>("users", x => x.Id(y => y.Id));
            var temporarySchema = Normalizr.Temporary(x =>
            {
                x.Reference("Users", userSchema);
            });

            var input = new
            {
                Users = new[]
                {
                    new User
                    {
                        Id = 21,
                        Name = "TryingToImprove"
                    },
                    new User
                    {
                        Id = 54,
                        Name = "RandomUser"
                    }
                }
            };

            var normalizr = new Normalizr(temporarySchema);
            var response = normalizr.Normalize(input);

            Assert.IsNotNull(response);
            Assert.IsNull(response.Result);

            Assert.IsTrue(response.Entities.ContainsKey("users"));

            var userEntities = response.Entities["users"];
            Assert.AreEqual(2, userEntities.Count);
            Assert.IsTrue(userEntities.ContainsKey(21));
            Assert.IsTrue(userEntities.ContainsKey(54));
        }

        public class Blog
        {
            public int Id { get; set; }

            public string Text { get; set; }

            public User Author { get; set; }

            public IEnumerable<Comment> Comments { get; set; }
        }

        public class User
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public class Comment
        {
            public int Id { get; set; }

            public User Author { get; set; }

            public string Text { get; set; }
        }
    }
}
