using System.Globalization;
using System.Text.Json;
using CsvHelper;
using FundaAssignment.Extraction;

namespace FundaAssignment.Load;

public static class OutputCreator
{
    public static void CreateOutput(this List<RealEstateAgent> realEstateAgents, String outputDir, String fileName)
    {
        var outputPath = Path.Combine(outputDir, fileName);
        
        if (!Directory.Exists(outputDir))
        { 
            Directory.CreateDirectory(outputDir);
        }
        
        // Write to CSV
        Console.WriteLine($"Writing output to {outputPath}.csv");
        realEstateAgents.WriteToCsv(outputPath);
        // Write to JSON
        Console.WriteLine($"Writing output to {outputPath}.csv");
        realEstateAgents.WriteToJson(outputPath);
        // Write to Console
        Console.WriteLine($"Writing output to {outputPath}.csv");
        realEstateAgents.WriteToConsole();
    }
    
    
    private static void WriteToCsv(this List<RealEstateAgent> realEstateAgents, String outputFile)
    {
        if (File.Exists(outputFile))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Output file {outputFile}.csv already exists. Deleting it to create a new one.");
            Console.ResetColor();
            File.Delete(outputFile);
        }

        using var writer = new StreamWriter($"{outputFile}.csv");
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(realEstateAgents);
    }
    
    private static void WriteToJson(this List<RealEstateAgent> realEstateAgents, String outputFile)
    {
        if (File.Exists(outputFile))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Output file {outputFile}.json already exists. Deleting it to create a new one.");
            Console.ResetColor();
            File.Delete(outputFile);
        } 
        string jsonString = JsonSerializer.Serialize(realEstateAgents);
        File.WriteAllText($"{outputFile}.json", jsonString);
    }
    
    private static void WriteToConsole(this List<RealEstateAgent> realEstateAgent)
    {
        Console.WriteLine("Real Estate Agents:");
        foreach (var agent in realEstateAgent)
        {
            Console.WriteLine($"ID: {agent.MakelaarId}, Name: {agent.MakelaarNaam}, Offer Type: {agent.OfferType}, Count: {agent.Count}");
        }
    }
}