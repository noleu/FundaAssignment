using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace FundaAssignment.Extraction;
/// <summary>
/// ExtractionClient is responsible for fetching real estate data from the Funda API.
/// It further processes the data to extract information about real estate agents and their offers.
/// </summary>
public class ExtractionClient
{
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
    
    /// <summary>
    /// Retrieves real estate agent data based on the specified objective.
    /// If all amsterdam is specified, it fetches both purchase and rent offers.
    /// </summary>
    /// <param name="objective">What data should be retrieved. Can either bei allamsterdam or garden</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<List<RealEstateAgent>> GetBrokerData(String objective = "allamsterdam")
    {
        List<RealEstateAgent> realEstateAgents = new List<RealEstateAgent>();
        
        if (objective.ToLower() == "allamsterdam")
        {
            Console.WriteLine("Fetching all Amsterdam real estate offers for purchase...");
            realEstateAgents.AddRange(await GetPurchaseOffersAsync());
            Console.WriteLine("Done with purchase offers.");
            
            Console.WriteLine("Fetching all Amsterdam real estate offers for rent...");
            realEstateAgents.AddRange(await GetRentOffersAsync());
            Console.WriteLine("Done with rent offers.");
            
        }else if (objective == "garden")
        {
            Console.WriteLine("Fetching all garden real estate offers for purchase...");
            realEstateAgents.AddRange(await GetPurchaseOffersAsync("tuin/"));
            Console.WriteLine("Done with garden purchase offers.");
        }
        else
        {
            throw new ArgumentException(
                $"Objective '{objective}' is not supported. Please use 'allamsterdam' or 'garden'.");
        }
        
        return realEstateAgents;
    }
    
    /// <summary>
    /// Fetches purchase offers for Amsterdam from the Funda API based on the provided search term.
    /// </summary>
    /// <param name="searchTerm">Should be valid category from the funda API</param>
    /// <returns>List of real estate agents</returns>
    /// <exception cref="InvalidOperationException">If api does not return answer for a page</exception>
    private async Task<List<RealEstateAgent>> GetPurchaseOffersAsync(String searchTerm = "")
    {
        // TODO: check input for searchTerm
        searchTerm = !string.IsNullOrEmpty(searchTerm) ? $"{searchTerm}/" : "";
        String parameter = $"?type=koop&zo=/amsterdam/{searchTerm}&page=1&pagesize=25";
        
        List<RealEstateAgent> realEstateAgents = await FetchRealEstateDataAsync(parameter);
        
        // assign offer type to each real estate agent
        foreach (var realEstateAgent in realEstateAgents)
        {
            realEstateAgent.OfferType = OfferType.Purchase;
        }
        
        // Logic to extract purchase offers
        return realEstateAgents;
    }

    /// <summary>
    /// Fetches rent offers for Amsterdam from the Funda API.
    /// </summary>
    /// <returns>List of real estate agents that placed rental properties in amsterdam</returns>
    /// <exception cref="InvalidOperationException">In case a page does not have any content</exception>
    private async Task<List<RealEstateAgent>> GetRentOffersAsync()
    {
        List<RealEstateAgent> realEstateAgents = await FetchRealEstateDataAsync($"?type=huur&zo=/amsterdam/&page=1&pagesize=25");
        // assign offer type to each real estate agent
        foreach (var realEstateAgent in realEstateAgents)
        {
            realEstateAgent.OfferType = OfferType.Rent;
        }
        
        // Logic to extract purchase offers
        return realEstateAgents;
    }
    
    /// <summary>
    /// Fetches real estate data from the Funda API based on the provided parameter.
    /// </summary>
    /// <param name="parameter">Valid Funda API parameters</param>
    /// <returns>List of real estate agents</returns>
    /// <exception cref="InvalidOperationException">If no objects where found in a requested page</exception>
    private async Task<List<RealEstateAgent>> FetchRealEstateDataAsync(String parameter)
    {
        List<RealEstateAgent> realEstateAgents = new List<RealEstateAgent>();
        using HttpClient client = new HttpClient();
        client.BaseAddress = new Uri($"{BaseUrl}/{_apiKey}/");
        
        Boolean nextPageAvailable = true;
        int page = 1;
        HttpResponseMessage responseMessage;

        
        while (nextPageAvailable)
        {
            if (page % 10 == 0) Console.WriteLine("Fetching page: " + page);
            
            try
            {
                responseMessage = await client.GetAsync(parameter);
                // TODO: retry logic if the request fails
                if (!responseMessage.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch data: {responseMessage.StatusCode} on page {page}");
                    page++;
                    parameter = parameter.Replace($"&page={page}", $"&page={++page}");
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
                    parameter = parameter.Replace($"&page={page}", $"&page={++page}");
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
        
        return realEstateAgents;
    }
}
    