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
    private const int SleepTime = 500; // milliseconds
    private const int MaxRetries = 3; // maximum number of retries for failed pages
    
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
        String parameter = $"?type=koop&zo=/amsterdam/{searchTerm}&page=0&pagesize=25";
        
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
        List<RealEstateAgent> realEstateAgents = await FetchRealEstateDataAsync($"?type=huur&zo=/amsterdam/&page=0&pagesize=25");
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
        using var client = CreateHttpClient();
        
        Boolean nextPageAvailable = true;
        int page = 0;
        HttpResponseMessage responseMessage;
        List<int> failedPages = [];

        while (nextPageAvailable)
        {
            if (page % 10 == 0) Console.WriteLine("Fetching page: " + page);
            
            try
            {
                responseMessage = await client.GetAsync(parameter);
                // TODO: retry logic if the request fails
                if (!responseMessage.IsSuccessStatusCode)
                {
                    failedPages.Add(page);
                    parameter = parameter.Replace($"&page={page}", $"&page={++page}");
                    // Sleep for 500 milliseconds to avoid hitting the rate limit of > 100 requests per minute
                    Thread.Sleep(SleepTime);
                    continue;
                }

                RealEstateData? responseContent = await responseMessage.Content.ReadFromJsonAsync<RealEstateData>();
                // Check if responseContent is null or Objects is null
                if (responseContent == null)
                {
                    failedPages.Add(page);
                    // Sleep for 500 milliseconds to avoid hitting the rate limit of > 100 requests per minute
                    Thread.Sleep(SleepTime); 
                }
                realEstateAgents.AddRange(responseContent.Objects);
                
                if (responseContent.Paging.HuidigePagina <= responseContent.Paging.AantalPaginas 
                    && !string.IsNullOrEmpty(responseContent.Paging.VolgendeUrl))
                {
                    nextPageAvailable = true;
                    parameter = parameter.Replace($"&page={page}", $"&page={++page}");
                    // Sleep for 500 milliseconds to avoid hitting the rate limit of > 100 requests per minute
                    Thread.Sleep(SleepTime); 
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

        // retry logic for failed pages
        if (failedPages.Count > 0)
        {
            realEstateAgents.AddRange(RetryFetchEstateDataAsync(failedPages, parameter).GetAwaiter().GetResult());
        }

        return realEstateAgents;
    }

    private async Task<List<RealEstateAgent>> RetryFetchEstateDataAsync(List<Int32> pagesToRetry, String parameter)
    {
        using HttpClient client = CreateHttpClient();
        List<RealEstateAgent> realEstateAgents = new List<RealEstateAgent>();
        List<int> failedPages = new List<int>();
        int numberOfFailedPages = pagesToRetry.Count;
        
        for (int i = 0; i < MaxRetries; i++)
        {
            foreach (int page in pagesToRetry)
            {
                try
                {
                    Console.WriteLine($"Retrying page {page}...");
                    parameter = parameter.Replace($"&page=0", $"&page={page}");
                    HttpResponseMessage responseMessage = await client.GetAsync(parameter);
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        failedPages.Add(page);
                        continue;
                    }

                    RealEstateData? responseContent = await responseMessage.Content.ReadFromJsonAsync<RealEstateData>();
                    // Check if responseContent is null or Objects is null
                    if (responseContent == null)
                    {
                        failedPages.Add(page);
                        continue;
                    }

                    realEstateAgents.AddRange(responseContent.Objects);
                    // Thread.Sleep(400);
                    Thread.Sleep(SleepTime);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error fetching page {page}: {e.Message}");
                    failedPages.Add(page);
                }
            }

            pagesToRetry = failedPages;
        }

        Console.WriteLine($"Succesfully retrieved {numberOfFailedPages - failedPages.Count}/{numberOfFailedPages} failed pages on retry.");
        return realEstateAgents;
    }
    
    private HttpClient CreateHttpClient()
    {
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri($"{BaseUrl}/{_apiKey}/");
        return client;
    }
}
    