namespace FundaAssignment.Extraction;

public class ClientFactory : IHttpClientFactory 
{
    private readonly String _apiKey;
    private static readonly String BaseUrl = "https://partnerapi.funda.nl/feeds/Aanbod.svc/json";
    
    public ClientFactory()
    {
        String? apiKeyEnv = Environment.GetEnvironmentVariable("FUNDA_API_KEY");
        if (String.IsNullOrEmpty(apiKeyEnv))
        {
            throw new InvalidOperationException("FundaApiKey environment variable is not set.");
        }
        _apiKey = apiKeyEnv;
    }
    
    /// <summary>
    /// Creates an HttpClient instance based on the provided name. Names can be "purchase" or "rent".
    /// Clients are configured with the base URL and API key for the Funda API.
    /// </summary>
    /// <param name="name">Name of the client may be "purchase" or "rent"</param>
    /// <returns>Respective HttpClient</returns>
    /// <exception cref="ArgumentException">If another value than purchase or rent is given</exception>
    public HttpClient CreateClient(string name)
    {
        HttpClient client = new HttpClient();
        switch (name.ToLower())
        {
            // create a client for purchase offers
            case "purchase":
                client.BaseAddress = new Uri($"{BaseUrl}/{_apiKey}/?type=koop&zo=/amsterdam/");
                return client;
            // create a client for rent offers
            case "rent":
                client.BaseAddress = new Uri($"{BaseUrl}/{_apiKey}/?type=huur&zo=/amsterdam/");
                return client;
            // no other clients are supported
            default:
                throw new ArgumentException($"Client name '{name}' is not supported. Please use 'purchase' or 'rent'.");
        }
    }
}