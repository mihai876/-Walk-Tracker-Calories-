// WalkTracker.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

class Config
{
    [JsonPropertyName("weight")]
    public double Weight { get; set; } = 70.0;
}

class Walk
{
    [JsonPropertyName("date")]
    public string Date { get; set; }
    [JsonPropertyName("distance")]
    public double Distance { get; set; }
    [JsonPropertyName("duration")]
    public double Duration { get; set; }
    [JsonPropertyName("speed")]
    public double Speed { get; set; }
    [JsonPropertyName("weight")]
    public double Weight { get; set; }
    [JsonPropertyName("calories")]
    public double Calories { get; set; }
}

class WalkTracker
{
    private static readonly string ConfigFile = "walk_config.json";
    private static readonly string DataFile = "walks.json";
    private static double weight = 70.0;
    private static List<Walk> walks = new List<Walk>();
    private static readonly JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };

    static void LoadConfig()
    {
        if (!File.Exists(ConfigFile)) return;
        string json = File.ReadAllText(ConfigFile);
        Config cfg = JsonSerializer.Deserialize<Config>(json);
        if (cfg != null) weight = cfg.Weight;
    }

    static void SaveConfig()
    {
        Config cfg = new Config { Weight = weight };
        string json = JsonSerializer.Serialize(cfg, options);
        File.WriteAllText(ConfigFile, json);
    }

    static void LoadWalks()
    {
        if (!File.Exists(DataFile)) return;
        string json = File.ReadAllText(DataFile);
        walks = JsonSerializer.Deserialize<List<Walk>>(json) ?? new List<Walk>();
    }

    static void SaveWalks()
    {
        string json = JsonSerializer.Serialize(walks, options);
        File.WriteAllText(DataFile, json);
    }

    static void AddWalk(double distance, double duration, double? weightOverride)
    {
        double w = weightOverride ?? weight;
        double speed = distance / (duration / 60.0);
        double met;
        if (speed < 3.0) met = 2.0;
        else if (speed < 5.0) met = 3.0;
        else if (speed < 6.5) met = 3.8;
        else if (speed < 8.0) met = 5.0;
        else met = 6.0;
        double cal = met * w * (duration / 60.0);
        Walk entry = new Walk
        {
            Date = DateTime.Now.ToString("o"),
            Distance = distance,
            Duration = duration,
            Speed = speed,
            Weight = w,
            Calories = Math.Round(cal, 1)
        };
        walks.Add(entry);
        SaveWalks();
        Console.WriteLine($"✅ Walk added: {distance} km in {duration} min, {entry.Calories} kcal");
    }

    static void ListWalks()
    {
        if (!walks.Any())
        {
            Console.WriteLine("No walks recorded.");
            return;
        }
        Console.WriteLine("\n📋 Walks:");
        for (int i = 0; i < walks.Count; i++)
        {
            var w = walks[i];
            string dt = w.Date.Substring(0, 16).Replace('T', ' ');
            Console.WriteLine($"{i+1}. {dt} | {w.Distance} km | {w.Duration} min | {w.Speed:F1} km/h | {w.Calories} kcal");
        }
    }

    static void Stats()
    {
        if (!walks.Any())
        {
            Console.WriteLine("No walks yet.");
            return;
        }
        double totalDist = walks.Sum(w => w.Distance);
        double totalDur = walks.Sum(w => w.Duration);
        double totalCal = walks.Sum(w => w.Calories);
        Console.WriteLine("\n🚶 Walk Summary");
        Console.WriteLine($"Total walks: {walks.Count}");
        Console.WriteLine($"Total distance: {totalDist:F2} km");
        Console.WriteLine($"Total duration: {totalDur} min ({totalDur/60:F1} h)");
        Console.WriteLine($"Total calories: {totalCal:F0} kcal");
    }

    static void ExportCSV(string filename)
    {
        using var writer = new StreamWriter(filename);
        writer.WriteLine("Date,Distance (km),Duration (min),Speed (km/h),Weight (kg),Calories (kcal)");
        foreach (var w in walks)
        {
            writer.WriteLine($"{w.Date},{w.Distance},{w.Duration},{w.Speed:F1},{w.Weight},{w.Calories}");
        }
        Console.WriteLine($"Exported {walks.Count} walks to {filename}");
    }

    static void Main(string[] args)
    {
        LoadConfig();
        LoadWalks();
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: WalkTracker [weight|add|list|stats|export]");
            return;
        }
        string cmd = args[0];
        switch (cmd)
        {
            case "weight":
                if (args.Length < 2) { Console.WriteLine("Usage: weight <kg>"); return; }
                weight = double.Parse(args[1]);
                SaveConfig();
                Console.WriteLine($"Default weight set to {weight} kg");
                break;
            case "add":
                if (args.Length < 3) { Console.WriteLine("Usage: add <distance> <duration> [--weight <kg>]"); return; }
                double dist = double.Parse(args[1]);
                double dur = double.Parse(args[2]);
                double? w = null;
                for (int i = 3; i < args.Length; i++)
                {
                    if (args[i] == "--weight" && i+1 < args.Length)
                    {
                        w = double.Parse(args[i+1]);
                        i++;
                    }
                }
                AddWalk(dist, dur, w);
                break;
            case "list":
                ListWalks();
                break;
            case "stats":
                Stats();
                break;
            case "export":
                string filename = args.Length > 1 ? args[1] : "walks.csv";
                ExportCSV(filename);
                break;
            default:
                Console.WriteLine("Unknown command");
                break;
        }
    }
}
