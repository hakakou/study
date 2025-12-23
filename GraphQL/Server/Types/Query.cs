using AutoBogus;
using NewsWeb;

namespace Server.Types;

public record Book(string Title, Author Author)
{
}

public record Author(string Name)
{
}


public class Query
{
    public List<Book> GetBooks()
    {
        return AutoFaker.Generate<List<Book>>();
    }

    public Author GetAuthor(int id)
    {
        return AutoFaker.Generate<Author>();
    }

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

    [GraphQLDescription("Get Geo Coordinates for a given location.")]
    public async Task<Response8> GetGeoCoordinatesAsync(string location, NewsService newsService)
    {
        return await newsService.GetGeoCoordinatesAsync(location);
    }


}


public record NewsCountry(string Id, string Code, string Name)
{
    [ID]
    public string Id { get; set; } = Id;

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

public record NewsArticle(int Id, string Title, string Content)
{
    [ID]
    public int Id { get; set; } = Id;

    public List<Comment> GetComments()
    {
        return AutoFaker.Generate<List<Comment>>();
    }
}

public record Comment(int Id, string Text)
{
    [ID]
    public int Id { get; set; } = Id;
}

 
