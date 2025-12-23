using System.Web;

namespace Server;

public class ApiKeyHandler(string apiKey) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uriBuilder = new UriBuilder(request.RequestUri!);

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["api-key"] = apiKey;

        uriBuilder.Query = query.ToString();
        request.RequestUri = uriBuilder.Uri;

        return base.SendAsync(request, cancellationToken);
    }
}