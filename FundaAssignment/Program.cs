// See https://aka.ms/new-console-template for more information

using FundaAssignment.Extraction;
using FundaAssignment.Load;
using FundaAssignment.Transformation;

// TODO: Add CLI arguments for output directory
ExtractionClient extractionClient = new ExtractionClient();
String outputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR") ?? "output";

// Load general Amsterdam data
List<RealEstateAgent> realEstateAgents = extractionClient.GetBrokerData().GetAwaiter().GetResult();

// create result table  
Console.WriteLine("Transforming real estate agent data for Amsterdam...");
List<RealEstateAgent> overAllResults = realEstateAgents.Transform();
List<RealEstateAgent> purchaseResults = realEstateAgents.TransformByOfferType(OfferType.Purchase);
List<RealEstateAgent> rentResults = realEstateAgents.TransformByOfferType(OfferType.Rent);
Console.WriteLine("Done");

// load garden data 
realEstateAgents = extractionClient.GetBrokerData("garden").GetAwaiter().GetResult();

// create result table  
realEstateAgents = realEstateAgents.Transform();

// create output file for Amsterdam data
overAllResults.CreateOutput(outputDir, "amsterdam_overall");
purchaseResults.CreateOutput(outputDir, "amsterdam_purchase_only");
rentResults.CreateOutput(outputDir, "amsterdam_rent_only");

// create output file for garden data
realEstateAgents.CreateOutput(outputDir, "garden_overall");