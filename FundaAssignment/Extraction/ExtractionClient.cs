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
            Console.WriteLine("Fetching all Amsterdam real estate offers for purchase...");
            brokerEntries.AddRange(await GetPurchaseOffersAsync());
            Console.WriteLine("Done with purchase offers.");
            Console.WriteLine("Fetching all Amsterdam real estate offers for rent...");
            brokerEntries.AddRange(await GetRentOffersAsync());
            Console.WriteLine("Done with rent offers.");
            
        }else if (objective == "garden")
        {
            Console.WriteLine("Fetching all garden real estate offers for purchase...");
            brokerEntries.AddRange(await GetPurchaseOffersAsync("tuin/"));
            Console.WriteLine("Done with garden purchase offers.");
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
        using HttpClient client = new HttpClient();
        client.BaseAddress = new Uri($"{BaseUrl}/{_apiKey}/");

        Boolean nextPageAvailable = true;
        int page = 1;
        searchTerm = !string.IsNullOrEmpty(searchTerm) ? $"{searchTerm}/" : "";
        String remainingURl = $"?type=koop&zo=/amsterdam/{searchTerm}&page={page}&pagesize=25";
        HttpResponseMessage responseMessage;

        while (nextPageAvailable)
        {
            if (page % 10 == 0) Console.WriteLine("Fetching page: " + page);
            
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
                    // Sleep for 600 milliseconds to avoid hitting the rate limit of > 100 requests per minute
                    Thread.Sleep(500); 
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
            if (page % 10 == 0) Console.WriteLine("Fetching page: " + page);
            
            try
            {
                responseMessage = await client.GetAsync(remainingURl);
                // TODO: retry logic if the request fails
                if (!responseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch data: {responseMessage.StatusCode} on page {page}");
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
                    // Sleep for 600 milliseconds to avoid hitting the rate limit of > 100 requests per minute
                    Thread.Sleep(500); 
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
            realEstateAgent.OfferType = OfferType.Rent;
        }
        
        // Logic to extract purchase offers
        return realEstateAgents;
    }
}
    