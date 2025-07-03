using FundaAssignment.Extraction;
using FundaAssignment.Transformation;

namespace FundaAssignmentTest;

public class RealEstateAgentDataTransformerTest
{
    [Fact]
    public void ShouldReturnTopRealEstateAgents()
    {
        List<RealEstateAgent> realEstateAgents = new();
        for (int i = 0; i < 10; i++)
        {
            realEstateAgents.Add(new RealEstateAgent
            {
                MakelaarId = i % 3,
                MakelaarNaam = $"Agent {i % 3}",
                OfferType = OfferType.Purchase,
                Count = 0
            });
        }
        
        List<RealEstateAgent> transformedAgents = realEstateAgents.Transform();
        Assert.NotNull(transformedAgents);
        Assert.Equal(3, transformedAgents.Count);
        Assert.Equal(0 , transformedAgents.First().MakelaarId);
        Assert.All(transformedAgents, agent => Assert.Equal(OfferType.Purchase, agent.OfferType));
        
        transformedAgents = realEstateAgents.TransformByOfferType(OfferType.Purchase);
        Assert.NotNull(transformedAgents);
        Assert.Equal(3, transformedAgents.Count);
        Assert.Equal(0 , transformedAgents.First().MakelaarId);
        Assert.All(transformedAgents, agent => Assert.Equal(OfferType.Purchase, agent.OfferType));
    }

    [Fact]
    public void ShouldReturnTopRealEstateAgentsByOfferType()
    {
        List<RealEstateAgent> realEstateAgents = new();
        for (int i = 0; i < 10; i++)
        {
            realEstateAgents.Add(new RealEstateAgent
            {
                MakelaarId = i,
                MakelaarNaam = $"Agent {i}",
                OfferType = i % 2 == 0? OfferType.Purchase : OfferType.Rent,
                Count = 0
            });
        }
        
        List<RealEstateAgent> transformedAgents = realEstateAgents.TransformByOfferType(OfferType.Purchase);
        Assert.NotNull(transformedAgents);
        Assert.Equal(5, transformedAgents.Count);
        Assert.Equal(0 , transformedAgents.First().MakelaarId);
        Assert.All(transformedAgents, agent => Assert.Equal(OfferType.Purchase, agent.OfferType));
        
        transformedAgents = realEstateAgents.TransformByOfferType(OfferType.Rent);
        Assert.NotNull(transformedAgents);
        Assert.Equal(5, transformedAgents.Count);
        Assert.Equal(1 , transformedAgents.First().MakelaarId);
        Assert.All(transformedAgents, agent => Assert.Equal(OfferType.Rent, agent.OfferType));
    }
    
    [Fact]
    public void ShouldOnlyReturn10Items()
    {
        List<RealEstateAgent> realEstateAgents = new();
        for (int i = 0; i < 20; i++)
        {
            realEstateAgents.Add(new RealEstateAgent
            {
                MakelaarId = i,
                MakelaarNaam = $"Agent {i}",
                OfferType = OfferType.Purchase,
                Count = 0
            });
        }
        
        List<RealEstateAgent> transformedAgents = realEstateAgents.Transform();
        Assert.NotNull(transformedAgents);
        Assert.Equal(10, transformedAgents.Count);
        
        transformedAgents = realEstateAgents.TransformByOfferType(OfferType.Purchase);
        Assert.NotNull(transformedAgents);
        Assert.Equal(10, transformedAgents.Count);
    }
    
}