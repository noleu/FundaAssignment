using System.Globalization;
using System.Text.Json;
using CsvHelper;
using FundaAssignment.Extraction;

namespace FundaAssignment.Load;

public static class OutputCreator
{
    /// <summary>
    /// Creates output files and console output for the list of real estate agents.
    /// </summary>
    /// <param name="realEstateAgents">List of real estate agent records</param>
    /// <param name="outputDir">Output directory for the files</param>
    /// <param name="fileName">File name without an extension</param>
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
        Console.WriteLine($"Writing output to {outputPath}.json");
        realEstateAgents.WriteToJson(outputPath);
        // Write to Console
        Console.WriteLine($"Writing output to console");
        realEstateAgents.WriteToConsole();
    }
    
    /// <summary>
    /// Writes the list of real estate agents to a CSV file.
    /// </summary>
    /// <param name="realEstateAgents">List of real estate agent records</param>
    /// <param name="outputFile">Path to the outputfile including the name</param>
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
    
    /// <summary>
    /// Writes the list of real estate agents to a JSON file.
    /// </summary>
    /// <param name="realEstateAgents">List of real estate agent records</param>
    /// <param name="outputFile">Path to the outputfile including the name</param>
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
    
    /// <summary>
    /// Writes the list of real estate agents to the console.
    /// </summary>
    /// <param name="realEstateAgent">List of real estate agents to print</param>
    private static void WriteToConsole(this List<RealEstateAgent> realEstateAgent)
    {
        Console.WriteLine("Real Estate Agents:");
        foreach (var agent in realEstateAgent)
        {
            Console.WriteLine($"ID: {agent.MakelaarId}, Name: {agent.MakelaarNaam}, Offer Type: {agent.OfferType}, Count: {agent.Count}");
        }
    }
}