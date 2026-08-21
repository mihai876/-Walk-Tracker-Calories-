# walk_tracker.py
import json
import os
from datetime import datetime
import argparse

CONFIG_FILE = "walk_config.json"
DATA_FILE = "walks.json"

class WalkTracker:
    def __init__(self):
        self.weight = 70.0  # default kg
        self.walks = []
        self.load_config()
        self.load_walks()

    def load_config(self):
        if os.path.exists(CONFIG_FILE):
            with open(CONFIG_FILE, "r") as f:
                config = json.load(f)
                self.weight = config.get("weight", 70.0)

    def save_config(self):
        with open(CONFIG_FILE, "w") as f:
            json.dump({"weight": self.weight}, f)

    def load_walks(self):
        if os.path.exists(DATA_FILE):
            with open(DATA_FILE, "r") as f:
                self.walks = json.load(f)

    def save_walks(self):
        with open(DATA_FILE, "w") as f:
            json.dump(self.walks, f, indent=2)

    def add_walk(self, distance, duration, weight=None):
        if weight is None:
            weight = self.weight
        # Calculate speed km/h
        speed = distance / (duration / 60.0) if duration > 0 else 0
        # MET values based on speed (approx)
        if speed < 3.0:
            met = 2.0
        elif speed < 5.0:
            met = 3.0
        elif speed < 6.5:
            met = 3.8
        elif speed < 8.0:
            met = 5.0
        else:
            met = 6.0
        # Calories: MET * weight(kg) * duration(hours)
        calories = met * weight * (duration / 60.0)
        entry = {
            "date": datetime.now().isoformat(),
            "distance": distance,
            "duration": duration,
            "speed": speed,
            "weight": weight,
            "calories": round(calories, 1)
        }
        self.walks.append(entry)
        self.save_walks()
        print(f"✅ Walk added: {distance} km in {duration} min, {calories:.1f} kcal")

    def list_walks(self):
        if not self.walks:
            print("No walks recorded.")
            return
        print("\n📋 Walks:")
        for i, w in enumerate(self.walks, 1):
            dt = w["date"][:16].replace("T", " ")
            print(f"{i}. {dt} | {w['distance']:.2f} km | {w['duration']} min | {w['speed']:.1f} km/h | {w['calories']:.1f} kcal")

    def stats(self):
        if not self.walks:
            print("No walks yet.")
            return
        total_dist = sum(w["distance"] for w in self.walks)
        total_dur = sum(w["duration"] for w in self.walks)
        total_cal = sum(w["calories"] for w in self.walks)
        print("\n🚶 Walk Summary")
        print(f"Total walks: {len(self.walks)}")
        print(f"Total distance: {total_dist:.2f} km")
        print(f"Total duration: {total_dur} min ({total_dur/60:.1f} h)")
        print(f"Total calories: {total_cal:.0f} kcal")

    def export_csv(self, filename):
        import csv
        with open(filename, 'w', newline='') as f:
            writer = csv.writer(f)
            writer.writerow(["Date", "Distance (km)", "Duration (min)", "Speed (km/h)", "Weight (kg)", "Calories (kcal)"])
            for w in self.walks:
                writer.writerow([w["date"], w["distance"], w["duration"], round(w["speed"], 1), w["weight"], w["calories"]])
        print(f"Exported {len(self.walks)} walks to {filename}")

def main():
    parser = argparse.ArgumentParser(description="Walk Tracker")
    subparsers = parser.add_subparsers(dest="cmd", required=True)

    weight_parser = subparsers.add_parser("weight")
    weight_parser.add_argument("kg", type=float)

    add_parser = subparsers.add_parser("add")
    add_parser.add_argument("distance", type=float)
    add_parser.add_argument("duration", type=float)
    add_parser.add_argument("--weight", type=float, help="Weight for this walk (kg)")

    subparsers.add_parser("list")
    subparsers.add_parser("stats")

    export_parser = subparsers.add_parser("export")
    export_parser.add_argument("filename", default="walks.csv", nargs="?")

    args = parser.parse_args()
    tracker = WalkTracker()

    if args.cmd == "weight":
        tracker.weight = args.kg
        tracker.save_config()
        print(f"Default weight set to {args.kg} kg")
    elif args.cmd == "add":
        tracker.add_walk(args.distance, args.duration, args.weight)
    elif args.cmd == "list":
        tracker.list_walks()
    elif args.cmd == "stats":
        tracker.stats()
    elif args.cmd == "export":
        tracker.export_csv(args.filename)

if __name__ == "__main__":
    main()
