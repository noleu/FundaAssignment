using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace FundaAssignment.Extraction;

public class ExtractionClient
{
    // private static IHttpClientFactory _httpClientFactory;
    private readonly String _apiKey;
    private static readonly String BaseUrl = "https://partnerapi.funda.nl/feeds/Aanbod.svc/json";
    
    public ExtractionClient()
    {
        // _httpClientFactory = new ClientFactory();
        String? apiKeyEnv = Environment.GetEnvironmentVariable("FUNDA_API_KEY");
        if (String.IsNullOrEmpty(apiKeyEnv))
        {
            throw new InvalidOperationException("FundaApiKey environment variable is not set.");
        }
        _apiKey = apiKeyEnv;
    }
    
    public async Task<List<RealEstateAgent>> GetBrokerData(String objective = "allamsterdam")
    {
        List<RealEstateAgent> brokerEntries = new List<RealEstateAgent>();
        
        if (objective.ToLower() == "allamsterdam")
        {
            brokerEntries.AddRange(await GetPurchaseOffersAsync());
            brokerEntries.AddRange(await GetRentOffersAsync());
            
        }else if (objective == "garden")
        {
            brokerEntries.AddRange(await GetPurchaseOffersAsync("tuin/"));
        }
        else
        {
            throw new ArgumentException(
                $"Objective '{objective}' is not supported. Please use 'allamsterdam' or 'garden'.");
        }
        
        return brokerEntries;
    }
    
    private async Task<List<RealEstateAgent>> GetPurchaseOffersAsync(String searchTerm = "")
    {
        List<RealEstateAgent> realEstateAgents = new List<RealEstateAgent>();
        // setup http client to fetch data from the API or website
        // HttpClient client = _httpClientFactory.CreateClient("purchase");
        using HttpClient client = new HttpClient();
        client.BaseAddress = new Uri($"{BaseUrl}/{_apiKey}/");

        Boolean nextPageAvailable = true;
        int page = 1;
        searchTerm = !string.IsNullOrEmpty(searchTerm) ? $"{searchTerm}/" : "";
        String remainingURl = $"?type=koop&zo=/amsterdam/{searchTerm}&page={page}&pagesize=25";
        HttpResponseMessage responseMessage;

        while (nextPageAvailable)
        {
            try
            {
                responseMessage = await client.GetAsync(remainingURl);
                // TODO: retry logic if the request fails
                if (!responseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch data: {responseMessage.StatusCode} on page {page}");
                    // return new List<RealEstateAgent>();
                    page++;
                    remainingURl = remainingURl.Replace("&page=", $"&page={page}");
                    continue;
                }

                RealEstateData? responseContent = await responseMessage.Content.ReadFromJsonAsync<RealEstateData>();
                // Check if responseContent is null or Objects is null
                if (responseContent == null)
                {
                    Console.WriteLine("Response content is null. No data received.");
                    throw new InvalidOperationException("Response content is null. No data received from the API.");
                }
                realEstateAgents.AddRange(responseContent.Objects);
                
                if (responseContent.Paging.HuidigePagina <= responseContent.Paging.AantalPaginas 
                    && !string.IsNullOrEmpty(responseContent.Paging.VolgendeUrl))
                {
                    nextPageAvailable = true;
                    remainingURl = remainingURl.Replace($"&page={page}", $"&page={++page}");
                }
                else
                {
                    nextPageAvailable = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        foreach (var realEstateAgent in realEstateAgents)
        {
            realEstateAgent.OfferType = OfferType.Purchase;
        }
        
        // Logic to extract purchase offers
        return realEstateAgents;
    }

    private async Task<List<RealEstateAgent>> GetRentOffersAsync()
    {
         List<RealEstateAgent> realEstateAgents = new List<RealEstateAgent>();
        // setup http client to fetch data from the API or website
        using HttpClient client = new HttpClient();
        client.BaseAddress = new Uri($"{BaseUrl}/{_apiKey}/");
        
        Boolean nextPageAvailable = true;
        int page = 1;
        String remainingURl = $"?type=huur&zo=/amsterdam/&page={page}&pagesize=25";
        HttpResponseMessage responseMessage;

        while (nextPageAvailable)
        {
            try
            {
                responseMessage = await client.GetAsync(remainingURl);
                // TODO: retry logic if the request fails
                if (!responseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch data: {responseMessage.StatusCode} on page {page}");
                    // return new List<RealEstateAgent>();
                    page++;
                    remainingURl = remainingURl.Replace("&page=", $"&page={page}");
                    continue;
                }

                RealEstateData? responseContent = await responseMessage.Content.ReadFromJsonAsync<RealEstateData>();
                // Check if responseContent is null or Objects is null
                if (responseContent == null)
                {
                    Console.WriteLine("Response content is null. No data received.");
                    throw new InvalidOperationException("Response content is null. No data received from the API.");
                }
                realEstateAgents.AddRange(responseContent.Objects);
                
                if (responseContent.Paging.HuidigePagina <= responseContent.Paging.AantalPaginas 
                    && !string.IsNullOrEmpty(responseContent.Paging.VolgendeUrl))
                {
                    nextPageAvailable = true;
                    remainingURl = remainingURl.Replace($"&page={page}", $"&page={page++}");
                }
                else
                {
                    nextPageAvailable = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        foreach (var realEstateAgent in realEstateAgents)
        {
            realEstateAgent.OfferType = OfferType.Purchase;
        }
        
        // Logic to extract purchase offers
        return realEstateAgents;
    }
}
    