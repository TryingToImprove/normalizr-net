Normalizr.NET
==================

This project is inspired by https://github.com/paularmstrong/normalizr and is used for normalizing data structure.

Motivation
---------------------------

The main motivation for this project is to save the bytes sent from the server, and to remove duplicate objects.


##### Structure
```
class User {
    public int Id { get; set; }
    public string Name { get; set }
}

class Comment {
    public int Id { get; set; }
    public User Author { get; set; }
    public string Text { get; set; }
}

class Blog {
    public int Id { get; set; }
    public User Author { get; set; }
    public string Text { get; set; }
    public IEnumerable<Comment> Comments { get; set; }
}

```

##### Test data
```
var mainAuthor = new User {
    Id = 1,
    Name = "TryingToImprove"
};

var blog = new Blog {
    Id = 23,
    Author = mainAuthor,
    Text = "This is a blog post",
    Comments = new [] {
        new Comment {
            Id = 1,
            Author = new User { Id = 3, Name = "RandomUser" },
            Text = "This is a comment #1"
        },
        new Comment {
            Id = 3,
            Author = new User { Id = 6, Name = "RandomUser2" },
            Text = "This is a comment #2"
        },
        new Comment {
            Id = 54,
            Author = mainAuthor,
            Text = "This is a comment #3"
        },
    } 
};
```

#### Using standard JSON.NET
If you were to use JSON.net and serialize the blog, then you would return `mainAuthor` 2 times which will give you duplicate data.

```
{
  "Id": 23,
  "Author": { ///###
    "Id": 1,
    "Name": "TryingToImprove"
  },
  "Text": "This is a blog post",
  "Comments": [
    {
      "Id": 1,
      "Author": {
        "Id": 3,
        "Name": "RandomUser"
      },
      "Text": "This is a comment #1"
    },
    {
      "Id": 3,
      "Author": {
        "Id": 6,
        "Name": "RandomUser2"
      },
      "Text": "This is a comment #2"
    },
    {
      "Id": 54,
      "Author": {  ///### already returned above
        "Id": 1,
        "Name": "TryingToImprove"
      },
      "Text": "This is a comment #3"
    }
  ]
}
```

#### Using standard Normalizr.NET
To prevent returning duplicates we will use the idea of normalizing the data. We will do this by setup the schema for the data structure and use the `Normalizr.Normalize`-method

```
var userSchema = Normalizr.Schema<User>("users", x => x.Id(y => y.Id));
var blogSchema = Normalizr.Schema<Blog>("blogs", x =>
{
    x.Id(y => y.Id);
    x.Reference(y => y.Author, userSchema);
});
var commentSchema = Normalizr.Schema<Comment>("comments", x =>
{
    x.Id(y => y.Id);
    x.Reference(y => y.Author, userSchema);
});

var normalizedResult = Normalizr.Normalize(blogSchema, blog);
```

```
{
  "entities": {
    "blogs": { // Set of blogs
      "23": {
        "Id": 23,
        "Author": 1,
        "Text": "This is a blog post",
        "Comments": [ // A array of all the comments
          1,
          3,
          54
        ]
      }
    },
    "users": { // Set of users
      "1": {
        "Id": 1,
        "Name": "TryingToImprove"
      },
      "3": {
        "Id": 3,
        "Name": "RandomUser"
      },
      "6": {
        "Id": 6,
        "Name": "RandomUser2"
      }
    },
    "comments": { // Set of comments
      "1": {
        "Id": 1,
        "Author": 3, // Id of the author
        "Text": "This is a comment #1"
      },
      "3": {
        "Id": 3,
        "Author": 6,
        "Text": "This is a comment #2"
      },
      "54": {
        "Id": 54,
        "Author": 1,
        "Text": "This is a comment #3"
      }
    }
  },
  "result": 23 // The id of the main entity (blog)
}
```

API
---------------------------

To be described