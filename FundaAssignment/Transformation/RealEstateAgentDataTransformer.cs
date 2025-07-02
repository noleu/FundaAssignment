using FundaAssignment.Extraction;
using System.Linq;

namespace FundaAssignment.Transformation;

public static class RealEstateAgentDataTransformer
{
    /// <summary>
    /// Transforms a list of real estate agents by grouping them by their ID and counting the number of offers per real estate agent.
    /// Reduces the list to the top 10 real estate agents based on the number of offers.
    /// </summary>
    /// <param name="realEstateAgents">List of real estate agent to transform</param>
    /// <exception cref="ArgumentNullException">Thrown if real estate agents is null</exception>
    /// <returns>List to the top 10 real estate agents based on the number of offers</returns>
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
    
    /// <summary>
    /// Transforms a list of real estate agents by filtering them based on the specified offer type (Purchase or Rent).
    /// Reduces the list to the top 10 real estate agents based on the number of offers for the specified offer type.
    /// </summary>
    /// <param name="realEstateAgents">List of real estate agent to transform</param>
    /// <param name="offerType">Type of offer they have placed on funda</param>
    /// <exception cref="ArgumentNullException">Thrown if real estate agents is null</exception>
    /// <returns>List to the top 10 real estate agents based on the number of offers</returns>
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