// walk_tracker.go
package main

import (
	"encoding/json"
	"flag"
	"fmt"
	"os"
	"time"
)

type Walk struct {
	Date     string  `json:"date"`
	Distance float64 `json:"distance"`
	Duration float64 `json:"duration"`
	Speed    float64 `json:"speed"`
	Weight   float64 `json:"weight"`
	Calories float64 `json:"calories"`
}

type Config struct {
	Weight float64 `json:"weight"`
}

var configFile = "walk_config.json"
var dataFile = "walks.json"
var weight = 70.0
var walks []Walk

func loadConfig() {
	data, err := os.ReadFile(configFile)
	if err != nil {
		return
	}
	var cfg Config
	if err := json.Unmarshal(data, &cfg); err == nil {
		weight = cfg.Weight
	}
}

func saveConfig() {
	cfg := Config{Weight: weight}
	data, _ := json.MarshalIndent(cfg, "", "  ")
	os.WriteFile(configFile, data, 0644)
}

func loadWalks() {
	data, err := os.ReadFile(dataFile)
	if err != nil {
		return
	}
	json.Unmarshal(data, &walks)
}

func saveWalks() {
	data, _ := json.MarshalIndent(walks, "", "  ")
	os.WriteFile(dataFile, data, 0644)
}

func addWalk(dist, dur, w float64) {
	if w == 0 {
		w = weight
	}
	speed := dist / (dur / 60.0)
	var met float64
	switch {
	case speed < 3.0:
		met = 2.0
	case speed < 5.0:
		met = 3.0
	case speed < 6.5:
		met = 3.8
	case speed < 8.0:
		met = 5.0
	default:
		met = 6.0
	}
	cal := met * w * (dur / 60.0)
	entry := Walk{
		Date:     time.Now().Format(time.RFC3339),
		Distance: dist,
		Duration: dur,
		Speed:    speed,
		Weight:   w,
		Calories: cal,
	}
	walks = append(walks, entry)
	saveWalks()
	fmt.Printf("✅ Walk added: %.2f km in %.0f min, %.1f kcal\n", dist, dur, cal)
}

func listWalks() {
	if len(walks) == 0 {
		fmt.Println("No walks recorded.")
		return
	}
	fmt.Println("\n📋 Walks:")
	for i, w := range walks {
		dt := w.Date[:16] // approx
		fmt.Printf("%d. %s | %.2f km | %.0f min | %.1f km/h | %.1f kcal\n",
			i+1, dt, w.Distance, w.Duration, w.Speed, w.Calories)
	}
}

func stats() {
	if len(walks) == 0 {
		fmt.Println("No walks yet.")
		return
	}
	var totalDist, totalDur, totalCal float64
	for _, w := range walks {
		totalDist += w.Distance
		totalDur += w.Duration
		totalCal += w.Calories
	}
	fmt.Println("\n🚶 Walk Summary")
	fmt.Printf("Total walks: %d\n", len(walks))
	fmt.Printf("Total distance: %.2f km\n", totalDist)
	fmt.Printf("Total duration: %.0f min (%.1f h)\n", totalDur, totalDur/60)
	fmt.Printf("Total calories: %.0f kcal\n", totalCal)
}

func exportCSV(filename string) {
	f, err := os.Create(filename)
	if err != nil {
		fmt.Println("Error creating file:", err)
		return
	}
	defer f.Close()
	f.WriteString("Date,Distance (km),Duration (min),Speed (km/h),Weight (kg),Calories (kcal)\n")
	for _, w := range walks {
		f.WriteString(fmt.Sprintf("%s,%.2f,%.0f,%.1f,%.1f,%.1f\n",
			w.Date, w.Distance, w.Duration, w.Speed, w.Weight, w.Calories))
	}
	fmt.Printf("Exported %d walks to %s\n", len(walks), filename)
}

func main() {
	loadConfig()
	loadWalks()

	if len(os.Args) < 2 {
		fmt.Println("Usage: walk_tracker [weight|add|list|stats|export]")
		return
	}
	cmd := os.Args[1]

	switch cmd {
	case "weight":
		if len(os.Args) != 3 {
			fmt.Println("Usage: weight <kg>")
			return
		}
		var w float64
		fmt.Sscanf(os.Args[2], "%f", &w)
		weight = w
		saveConfig()
		fmt.Printf("Default weight set to %.1f kg\n", weight)

	case "add":
		if len(os.Args) < 4 {
			fmt.Println("Usage: add <distance> <duration> [--weight <kg>]")
			return
		}
		var dist, dur, w float64
		fmt.Sscanf(os.Args[2], "%f", &dist)
		fmt.Sscanf(os.Args[3], "%f", &dur)
		// Check optional weight
		for i := 4; i < len(os.Args); i++ {
			if os.Args[i] == "--weight" && i+1 < len(os.Args) {
				fmt.Sscanf(os.Args[i+1], "%f", &w)
				break
			}
		}
		addWalk(dist, dur, w)

	case "list":
		listWalks()

	case "stats":
		stats()

	case "export":
		filename := "walks.csv"
		if len(os.Args) >= 3 {
			filename = os.Args[2]
		}
		exportCSV(filename)

	default:
		fmt.Println("Unknown command")
	}
}
