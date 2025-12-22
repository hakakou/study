using AutoBogus;
using Bogus;
using HotChocolate.Configuration;
using HotChocolate.Types.Descriptors.Definitions;
using NewsWeb;
using System;
using System.Xml.Linq;

namespace Server.Types;

public class Query
{
    public string Hello() => "Hello world";

    public List<NewsCountry> GetNewsCountries()
    {
        return new List<NewsCountry>
        {
            new NewsCountry("1", "us", "United States"),
            new NewsCountry("2", "gb", "United Kingdom"),
        };
    }

    //public async Task<List<News3>> GetTopNewsAsync(NewsService newsService)
    //{
    //    var news = await newsService.TopNewsAsync("us", "en", "2025-12-20", true);
    //    return news.Top_news.SelectMany(c => c.News).ToList();
    //}

    //[GraphQLDescription("Retrieve News Articles by Ids.")]
    //public async Task<News2?> RetrieveAsync([ID] string id, NewsService newsService)
    //{
    //    var news = await newsService.RetrieveNewsArticlesByIdsAsync(id);
    //    return news.News.FirstOrDefault();
    //}

    //public async Task<Response8> GetGeoCoordinatesAsync(string location, NewsService newsService)
    //{
    //    return await newsService.GetGeoCoordinatesAsync(location);
    //}

    //[GraphQLDescription("Playlists hand-picked to be featured to all users.")]
    //public async Task<List<PlaylistSimplified>> FeaturedPlaylists(SpotifyService spotifyService)
    //{
    //    var response = await spotifyService.GetFeaturedPlaylistsAsync();
    //    var items = response.Playlists.Items;
    //    return items.ToList();
    //}
}


public class NewsCountry(string id, string code, string name)
{
    [ID]
    public string Id { get; set; } = id;
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;

    private List<NewsArticle>? _newsArticles;
    public async Task<List<NewsArticle>> GetNewsArticlesAsync(NewsService newsService)
    {
        if (_newsArticles == null)
        {
            //Response2 news = await newsService.TopNewsAsync(Code, "en", "2025-12-20", false);
            var news = AutoFaker.Generate<Response2>();

            _newsArticles = news.Top_news.SelectMany(c => c.News)
                .Select(c => new NewsArticle(c.Id, c.Title, c.Text)).ToList();
        }
        return _newsArticles;
    }
}

public class NewsArticle(int id, string title, string content)
{
    [ID]
    public int Id { get; set; } = id;
    public string Title { get; set; } = title;
    public string Content { get; set; } = content;

    public List<Comment> GetComments()
    {
        return AutoFaker.Generate<List<Comment>>();
    }
}

public class Comment(int id, string text)
{
    [ID]
    public int Id { get; set; } = id;
    public string Text { get; set; } = text;
}

public class CommentType : ObjectType<Comment>
{
    protected override FieldCollection<ObjectField> OnCompleteFields(ITypeCompletionContext context, ObjectTypeDefinition definition)
    {
        return base.OnCompleteFields(context, definition);
    }
}
 
//[GraphQLDescription("A curated collection of tracks designed for a specific activity or mood.")]
//public class Playlist
//{
//    [GraphQLDescription("The ID for the playlist.")]
//    [ID]
//    public string Id { get; }

//    [GraphQLDescription("The name of the playlist.")]
//    public string Name { get; set; }

//    [GraphQLDescription("Describes the playlist, what to expect and entices the user to listen.")]
//    public string? Description { get; set; }

//    public Playlist(string id, string name)
//    {
//        Id = id;
//        Name = name;
//    }
//}


//public class Artist
//{
//    [GraphQLDescription("The ID for the playlist.")]
//    [ID]
//    public string Id { get; }

//    public string Name { get; set; }

//    public int? Followers { get; set; }

//    public float? Popularity { get; set; }

//    public Artist(string id, string name)
//    {
//        Id = id;
//        Name = name;
//    }
//}
