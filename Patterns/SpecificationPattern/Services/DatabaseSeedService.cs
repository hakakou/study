using Bogus;
using Microsoft.EntityFrameworkCore;
using SpecificationPattern.Database;
using SpecificationPattern.Database.Entities;

namespace SpecificationPattern.Services;

public static class DatabaseSeedService
{
    // Set one random seed from Bogus for consistency
    private const int RandomSeed = 250;

    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        await dbContext.Database.MigrateAsync();

        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        // Set the random seed for Bogus
        Randomizer.Seed = new Random(RandomSeed);

        // Create categories
        var categories = await SeedCategoriesAsync(dbContext);

        // Create 100 users
        var users = await SeedUsersAsync(dbContext, 100);

        // Create 500 posts by random users
        var posts = await SeedPostsAsync(dbContext, users, categories);

        // Create 50 comments on each post
        await SeedCommentsAsync(dbContext, posts, users);

        // Create 100 likes per post
        await SeedLikesAsync(dbContext, posts);

        await dbContext.SaveChangesAsync();
    }

    private static async Task<List<Category>> SeedCategoriesAsync(ApplicationDbContext dbContext)
    {
        var categoryNames = new List<string>
        {
            ".NET",
            "Architecture",
            "JavaScript",
            "Python",
            "Java",
            "C++",
            "Go",
            "Rust",
            "Ruby",
            "Swift",
            "Kotlin"
        };

        var categories = new List<Category>();
        foreach (var name in categoryNames)
        {
            var category = new Category { Name = name };
            categories.Add(category);
            dbContext.Categories.Add(category);
        }

        await dbContext.SaveChangesAsync();
        return categories;
    }

    private static async Task<List<User>> SeedUsersAsync(ApplicationDbContext dbContext, int count)
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Username, f => f.Internet.UserName());

        var users = userFaker.Generate(count);
        dbContext.Users.AddRange(users);
        await dbContext.SaveChangesAsync();
        return users;
    }

    private static async Task<List<Post>> SeedPostsAsync(ApplicationDbContext dbContext, List<User> users, List<Category> categories)
    {
	    var posts = new Faker<Post>()
		    .RuleFor(p => p.CreatedAtUtc, f => f.Date.Recent(14).ToUniversalTime())
		    .RuleFor(p => p.Title, f => f.Lorem.Sentence())
		    .RuleFor(p => p.Content, f => f.Lorem.Paragraphs(3))
		    .RuleFor(p => p.CategoryId, f => f.Random.Int(1, categories.Count))
		    .RuleFor(p => p.Category, (f, p) => categories.FirstOrDefault(c => c.Id == p.CategoryId)!)
		    .RuleFor(p => p.Category, f => f.PickRandom(categories))
		    .RuleFor(p => p.User, f => f.PickRandom(users))
		    .Generate(500);

        dbContext.Posts.AddRange(posts);
        await dbContext.SaveChangesAsync();
        return posts;
    }

    private static async Task SeedCommentsAsync(ApplicationDbContext dbContext, List<Post> posts, List<User> users)
    {
        var commentFaker = new Faker<Comment>()
            .RuleFor(c => c.Text, f => f.Lorem.Paragraph())
            .RuleFor(c => c.CreatedAt, f => f.Date.Recent(14).ToUniversalTime());

        var comments = new List<Comment>();

        foreach (var post in posts)
        {
            // Add 50 comments for each post
            for (var i = 0; i < Random.Shared.Next(20, 120); i++)
            {
                var comment = commentFaker.Generate();
                comment.PostId = post.Id;
                comment.Post = post;

                // Assign random user
                var randomUser = users[new Random(RandomSeed + post.Id * 100 + i).Next(users.Count)];
                comment.UserId = randomUser.Id;
                comment.User = randomUser;

                comments.Add(comment);
            }
        }

        // Add comments in batches to avoid memory issues
        const int batchSize = 1000;
        for (var i = 0; i < comments.Count; i += batchSize)
        {
            var batch = comments.Skip(i).Take(batchSize).ToList();
            dbContext.Comments.AddRange(batch);
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task SeedLikesAsync(ApplicationDbContext dbContext, List<Post> posts)
    {
        var likeFaker = new Faker<Like>();
        var likes = new List<Like>();

        foreach (var post in posts)
        {
            // Add 100 likes for each post
            for (var i = 0; i < Random.Shared.Next(20, 250); i++)
            {
                var like = likeFaker.Generate();
                like.PostId = post.Id;
                like.Post = post;

                likes.Add(like);
            }
        }

        // Add likes in batches to avoid memory issues
        const int batchSize = 1000;
        for (var i = 0; i < likes.Count; i += batchSize)
        {
            var batch = likes.Skip(i).Take(batchSize).ToList();
            dbContext.Likes.AddRange(batch);
            await dbContext.SaveChangesAsync();
        }
    }
}
