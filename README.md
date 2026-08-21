🚶 Walk Tracker (Calories) — Multi‑Language Fitness Logger
8 languages, one powerful walk logger – track your walks, estimate calories burned, and monitor your progress – right from your terminal.

✨ Features
🚶 Log walks – record distance (km) and duration (minutes)

🔥 Calorie estimation – uses MET (Metabolic Equivalent) based on walking speed

⚖️ Custom weight – set your weight once (default 70 kg)

📊 Summary stats – total distance, total time, total calories

📋 List all walks – with date, distance, duration, speed, and calories

📤 Export to CSV – for further analysis in spreadsheets

💾 Persistent storage – all data saved in walks.json

🧰 Supported Languages & Files
Language	File	Dependencies
Python	walk_tracker.py	none (stdlib)
Go	walk_tracker.go	none (stdlib)
JavaScript (Node)	walk_tracker.js	commander (optional), fs
Ruby	walk_tracker.rb	json, date
PHP	walk_tracker.php	none (extensions)
Java	WalkTracker.java	Java 8+ (uses java.nio, java.time)
C#	WalkTracker.cs	.NET Core 3.1+
C++	walk_tracker.cpp	nlohmann/json
🚀 Quick Start
All implementations follow the same CLI pattern:

bash
# Set default weight (optional)
<command> weight 75

# Add a walk (distance in km, duration in minutes)
<command> add 5.2 30

# Add a walk with a different weight (overrides default)
<command> add 3.8 25 --weight 80

# List all walks
<command> list

# Show summary statistics
<command> stats

# Export all walks to CSV
<command> export walks.csv
Commands:

weight <kg> – set default weight (saved in config)

add <distance> <duration> [--weight <kg>] – log a walk

list – show all recorded walks

stats – display totals

export <filename> – export to CSV

📸 Example Output
text
🚶 Walk Summary
Total walks: 3
Total distance: 12.5 km
Total duration: 75 min
Total calories: 415 kcal

📋 Walks:
2026-08-21 14:30 | 5.2 km | 30 min | 10.4 km/h | 175 kcal
2026-08-21 10:15 | 3.8 km | 25 min | 9.1 km/h | 120 kcal
2026-08-20 18:00 | 3.5 km | 20 min | 10.5 km/h | 120 kcal
📁 Repository Structure
text
.
├── README.md
├── python/
│   └── walk_tracker.py
├── go/
│   └── walk_tracker.go
├── javascript/
│   └── walk_tracker.js
├── ruby/
│   └── walk_tracker.rb
├── php/
│   └── walk_tracker.php
├── java/
│   └── WalkTracker.java
├── csharp/
│   └── WalkTracker.cs
└── cpp/
    └── walk_tracker.cpp
