// WalkTracker.java
import java.io.*;
import java.nio.file.*;
import java.time.*;
import java.time.format.*;
import java.util.*;
import com.google.gson.*;

class Config {
    double weight = 70.0;
}

class Walk {
    String date;
    double distance;
    double duration;
    double speed;
    double weight;
    double calories;
}

public class WalkTracker {
    private static final String CONFIG_FILE = "walk_config.json";
    private static final String DATA_FILE = "walks.json";
    private static double weight = 70.0;
    private static List<Walk> walks = new ArrayList<>();
    private static final Gson gson = new GsonBuilder().setPrettyPrinting().create();

    static void loadConfig() {
        try {
            Path path = Paths.get(CONFIG_FILE);
            if (Files.exists(path)) {
                String json = new String(Files.readAllBytes(path));
                Config cfg = gson.fromJson(json, Config.class);
                weight = cfg.weight;
            }
        } catch (Exception e) {}
    }

    static void saveConfig() {
        try {
            Config cfg = new Config();
            cfg.weight = weight;
            Files.write(Paths.get(CONFIG_FILE), gson.toJson(cfg).getBytes());
        } catch (Exception e) {}
    }

    static void loadWalks() {
        try {
            Path path = Paths.get(DATA_FILE);
            if (Files.exists(path)) {
                String json = new String(Files.readAllBytes(path));
                Walk[] arr = gson.fromJson(json, Walk[].class);
                walks = new ArrayList<>(Arrays.asList(arr));
            }
        } catch (Exception e) {}
    }

    static void saveWalks() {
        try {
            Files.write(Paths.get(DATA_FILE), gson.toJson(walks).getBytes());
        } catch (Exception e) {}
    }

    static void addWalk(double dist, double dur, Double w) {
        if (w == null) w = weight;
        double speed = dist / (dur / 60.0);
        double met;
        if (speed < 3.0) met = 2.0;
        else if (speed < 5.0) met = 3.0;
        else if (speed < 6.5) met = 3.8;
        else if (speed < 8.0) met = 5.0;
        else met = 6.0;
        double cal = met * w * (dur / 60.0);
        Walk entry = new Walk();
        entry.date = Instant.now().toString();
        entry.distance = dist;
        entry.duration = dur;
        entry.speed = speed;
        entry.weight = w;
        entry.calories = Math.round(cal * 10) / 10.0;
        walks.add(entry);
        saveWalks();
        System.out.printf("✅ Walk added: %.2f km in %.0f min, %.1f kcal\n", dist, dur, entry.calories);
    }

    static void listWalks() {
        if (walks.isEmpty()) {
            System.out.println("No walks recorded.");
            return;
        }
        System.out.println("\n📋 Walks:");
        for (int i = 0; i < walks.size(); i++) {
            Walk w = walks.get(i);
            String dt = w.date.substring(0, 16).replace('T', ' ');
            System.out.printf("%d. %s | %.2f km | %.0f min | %.1f km/h | %.1f kcal\n",
                i+1, dt, w.distance, w.duration, w.speed, w.calories);
        }
    }

    static void stats() {
        if (walks.isEmpty()) {
            System.out.println("No walks yet.");
            return;
        }
        double totalDist = 0, totalDur = 0, totalCal = 0;
        for (Walk w : walks) {
            totalDist += w.distance;
            totalDur += w.duration;
            totalCal += w.calories;
        }
        System.out.println("\n🚶 Walk Summary");
        System.out.printf("Total walks: %d\n", walks.size());
        System.out.printf("Total distance: %.2f km\n", totalDist);
        System.out.printf("Total duration: %.0f min (%.1f h)\n", totalDur, totalDur/60);
        System.out.printf("Total calories: %.0f kcal\n", totalCal);
    }

    static void exportCSV(String filename) throws IOException {
        Path path = Paths.get(filename);
        try (BufferedWriter writer = Files.newBufferedWriter(path)) {
            writer.write("Date,Distance (km),Duration (min),Speed (km/h),Weight (kg),Calories (kcal)\n");
            for (Walk w : walks) {
                writer.write(String.format("%s,%.2f,%.0f,%.1f,%.1f,%.1f\n",
                    w.date, w.distance, w.duration, w.speed, w.weight, w.calories));
            }
        }
        System.out.printf("Exported %d walks to %s\n", walks.size(), filename);
    }

    public static void main(String[] args) throws Exception {
        loadConfig();
        loadWalks();
        if (args.length < 1) {
            System.out.println("Usage: WalkTracker [weight|add|list|stats|export]");
            return;
        }
        String cmd = args[0];
        switch (cmd) {
            case "weight":
                if (args.length < 2) { System.out.println("Usage: weight <kg>"); return; }
                weight = Double.parseDouble(args[1]);
                saveConfig();
                System.out.printf("Default weight set to %.1f kg\n", weight);
                break;
            case "add":
                if (args.length < 3) { System.out.println("Usage: add <distance> <duration> [--weight <kg>]"); return; }
                double dist = Double.parseDouble(args[1]);
                double dur = Double.parseDouble(args[2]);
                Double w = null;
                for (int i = 3; i < args.length; i++) {
                    if (args[i].equals("--weight") && i+1 < args.length) {
                        w = Double.parseDouble(args[i+1]);
                        i++;
                    }
                }
                addWalk(dist, dur, w);
                break;
            case "list":
                listWalks();
                break;
            case "stats":
                stats();
                break;
            case "export":
                String filename = args.length > 1 ? args[1] : "walks.csv";
                exportCSV(filename);
                break;
            default:
                System.out.println("Unknown command");
        }
    }
}
