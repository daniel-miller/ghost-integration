namespace GhostIntegration;

internal sealed partial class GhostApiHelper
{
    public HttpClient Client { get; }

    public string Url { get; }

    public string Token { get; }

    public GhostApiHelper(HttpClient httpClient, string url, string token)
    {
        Client = httpClient;
        Url = url;
        Token = token;
    }
}