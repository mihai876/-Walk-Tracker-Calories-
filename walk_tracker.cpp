// walk_tracker.cpp
#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <ctime>
#include <iomanip>
#include <sstream>
#include <nlohmann/json.hpp>

using namespace std;
using json = nlohmann::json;

struct Walk {
    string date;
    double distance;
    double duration;
    double speed;
    double weight;
    double calories;
};

double weight = 70.0;
vector<Walk> walks;
const string configFile = "walk_config.json";
const string dataFile = "walks.json";

void loadConfig() {
    ifstream f(configFile);
    if (f.is_open()) {
        json j;
        f >> j;
        if (j.contains("weight")) weight = j["weight"];
        f.close();
    }
}

void saveConfig() {
    json j = {{"weight", weight}};
    ofstream f(configFile);
    f << setw(2) << j << endl;
}

void loadWalks() {
    ifstream f(dataFile);
    if (f.is_open()) {
        json j;
        f >> j;
        for (auto& item : j) {
            Walk w;
            w.date = item["date"];
            w.distance = item["distance"];
            w.duration = item["duration"];
            w.speed = item["speed"];
            w.weight = item["weight"];
            w.calories = item["calories"];
            walks.push_back(w);
        }
        f.close();
    }
}

void saveWalks() {
    json j = json::array();
    for (auto& w : walks) {
        j.push_back({{"date", w.date}, {"distance", w.distance}, {"duration", w.duration},
                     {"speed", w.speed}, {"weight", w.weight}, {"calories", w.calories}});
    }
    ofstream f(dataFile);
    f << setw(2) << j << endl;
}

string currentTime() {
    time_t t = time(nullptr);
    char buf[30];
    strftime(buf, sizeof(buf), "%Y-%m-%dT%H:%M:%S%z", localtime(&t));
    return string(buf);
}

void addWalk(double dist, double dur, double w) {
    if (w == 0) w = weight;
    double speed = dist / (dur / 60.0);
    double met;
    if (speed < 3.0) met = 2.0;
    else if (speed < 5.0) met = 3.0;
    else if (speed < 6.5) met = 3.8;
    else if (speed < 8.0) met = 5.0;
    else met = 6.0;
    double cal = met * w * (dur / 60.0);
    Walk entry{currentTime(), dist, dur, speed, w, cal};
    walks.push_back(entry);
    saveWalks();
    cout << "✅ Walk added: " << dist << " km in " << dur << " min, " << cal << " kcal\n";
}

void listWalks() {
    if (walks.empty()) {
        cout << "No walks recorded.\n";
        return;
    }
    cout << "\n📋 Walks:\n";
    for (size_t i = 0; i < walks.size(); i++) {
        auto& w = walks[i];
        string dt = w.date.substr(0, 16);
        cout << i+1 << ". " << dt << " | " << w.distance << " km | " << w.duration << " min | "
             << fixed << setprecision(1) << w.speed << " km/h | " << w.calories << " kcal\n";
    }
}

void stats() {
    if (walks.empty()) {
        cout << "No walks yet.\n";
        return;
    }
    double totalDist = 0, totalDur = 0, totalCal = 0;
    for (auto& w : walks) {
        totalDist += w.distance;
        totalDur += w.duration;
        totalCal += w.calories;
    }
    cout << "\n🚶 Walk Summary\n";
    cout << "Total walks: " << walks.size() << "\n";
    cout << "Total distance: " << fixed << setprecision(2) << totalDist << " km\n";
    cout << "Total duration: " << totalDur << " min (" << totalDur/60 << " h)\n";
    cout << "Total calories: " << (int)totalCal << " kcal\n";
}

void exportCSV(const string& filename) {
    ofstream f(filename);
    f << "Date,Distance (km),Duration (min),Speed (km/h),Weight (kg),Calories (kcal)\n";
    for (auto& w : walks) {
        f << w.date << "," << w.distance << "," << w.duration << "," << fixed << setprecision(1) << w.speed
          << "," << w.weight << "," << w.calories << "\n";
    }
    f.close();
    cout << "Exported " << walks.size() << " walks to " << filename << "\n";
}

int main(int argc, char* argv[]) {
    loadConfig();
    loadWalks();
    if (argc < 2) {
        cerr << "Usage: walk_tracker [weight|add|list|stats|export]\n";
        return 1;
    }
    string cmd = argv[1];
    if (cmd == "weight") {
        if (argc < 3) { cerr << "Usage: weight <kg>\n"; return 1; }
        weight = stod(argv[2]);
        saveConfig();
        cout << "Default weight set to " << weight << " kg\n";
    } else if (cmd == "add") {
        if (argc < 4) { cerr << "Usage: add <distance> <duration> [--weight <kg>]\n"; return 1; }
        double dist = stod(argv[2]);
        double dur = stod(argv[3]);
        double w = 0;
        for (int i = 4; i < argc; i++) {
            if (string(argv[i]) == "--weight" && i+1 < argc) {
                w = stod(argv[i+1]);
                i++;
            }
        }
        addWalk(dist, dur, w);
    } else if (cmd == "list") {
        listWalks();
    } else if (cmd == "stats") {
        stats();
    } else if (cmd == "export") {
        string filename = "walks.csv";
        if (argc > 2) filename = argv[2];
        exportCSV(filename);
    } else {
        cerr << "Unknown command\n";
        return 1;
    }
    return 0;
}
