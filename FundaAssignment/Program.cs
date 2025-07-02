// See https://aka.ms/new-console-template for more information

using FundaAssignment.Extraction;
using FundaAssignment.Load;
using FundaAssignment.Transformation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
ExtractionClient extractionClient = new ExtractionClient();
String outputDir = Environment.GetEnvironmentVariable("OUTPUT_DIR") ?? "output";

// Load general Amsterdam data
List<RealEstateAgent> realEstateAgents = extractionClient.GetBrokerData().GetAwaiter().GetResult();

// create result table  
List<RealEstateAgent> overAllResults = realEstateAgents.Transform();
List<RealEstateAgent> purchaseResults = realEstateAgents.TransformByOfferType(OfferType.Purchase);
List<RealEstateAgent> rentResults = realEstateAgents.TransformByOfferType(OfferType.Rent);

// create output file
overAllResults.CreateOutput(outputDir, "amsterdam_overall");
purchaseResults.CreateOutput(outputDir, "amsterdam_purchase_only");
rentResults.CreateOutput(outputDir, "amsterdam_rent_only");

// load garden data 
// Load general Amsterdam data
realEstateAgents = extractionClient.GetBrokerData("garden").GetAwaiter().GetResult();

// create result table  
realEstateAgents = realEstateAgents.Transform();
// create output file
realEstateAgents.CreateOutput(outputDir, "garden_overall");

