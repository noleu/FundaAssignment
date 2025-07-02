namespace FundaAssignment.Extraction;

// public record BrokerEntry(Int32 brokerId, String name, OfferType offerType);

public record RealEstateData
{
    public List<RealEstateAgent> Objects { get; init; }
    public PagingInfo Paging { get; init; }
}

public record RealEstateAgent
{
    public int MakelaarId { get; init; }
    public string MakelaarNaam { get; init; }
    // not included in the response, but to avoid code duplication
    public OfferType OfferType { get; set; }
    public int Count { get; set; }
}

public record PagingInfo
{
    public int AantalPaginas { get; init; }
    public int HuidigePagina { get; init; }
    public string? VolgendeUrl { get; init; }
    public string? VorigeUrl { get; init; }
}