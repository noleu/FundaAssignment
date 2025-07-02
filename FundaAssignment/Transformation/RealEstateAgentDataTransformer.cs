using FundaAssignment.Extraction;
using System.Linq;

namespace FundaAssignment.Transformation;

public static class RealEstateAgentDataTransformer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="realEstateAgents"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<RealEstateAgent> Transform(this List<RealEstateAgent> realEstateAgents)
    {
        ArgumentNullException.ThrowIfNull(realEstateAgents);

        return realEstateAgents
            .GroupBy(realEstateAgent => realEstateAgent.MakelaarId)
            .Select(group => new RealEstateAgent
                {
                    MakelaarId = group.Key,
                    MakelaarNaam = group.First().MakelaarNaam,
                    OfferType = group.First().OfferType,
                    Count = group.Count()
                })
            .OrderByDescending(group => group.Count)
            .Take(10)
            .ToList();
    }
    
    public static List<RealEstateAgent> TransformByOfferType(this List<RealEstateAgent> realEstateAgents, OfferType offerType)
    {
        ArgumentNullException.ThrowIfNull(realEstateAgents);

        return realEstateAgents
            .Where(realEstateAgent => realEstateAgent.OfferType == offerType)
            .GroupBy(realEstateAgent => realEstateAgent.MakelaarId)
            .Select(group => new RealEstateAgent
                {
                    MakelaarId = group.Key,
                    MakelaarNaam = group.First().MakelaarNaam,
                    OfferType = group.First().OfferType,
                    Count = group.Count()
                })
            .OrderByDescending(group => group.Count)
            .Take(10)
            .ToList();
    }
}