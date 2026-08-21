# walk_tracker.php
#!/usr/bin/env php
<?php

define('CONFIG_FILE', 'walk_config.json');
define('DATA_FILE', 'walks.json');

function loadConfig() {
    if (file_exists(CONFIG_FILE)) {
        $data = json_decode(file_get_contents(CONFIG_FILE), true);
        return $data['weight'] ?? 70.0;
    }
    return 70.0;
}

function saveConfig($weight) {
    file_put_contents(CONFIG_FILE, json_encode(['weight' => $weight], JSON_PRETTY_PRINT));
}

function loadWalks() {
    if (file_exists(DATA_FILE)) {
        return json_decode(file_get_contents(DATA_FILE), true) ?: [];
    }
    return [];
}

function saveWalks($walks) {
    file_put_contents(DATA_FILE, json_encode($walks, JSON_PRETTY_PRINT));
}

function addWalk($dist, $dur, $weight = null) {
    $defWeight = loadConfig();
    if ($weight === null) $weight = $defWeight;
    $speed = $dist / ($dur / 60.0);
    if ($speed < 3.0) $met = 2.0;
    elseif ($speed < 5.0) $met = 3.0;
    elseif ($speed < 6.5) $met = 3.8;
    elseif ($speed < 8.0) $met = 5.0;
    else $met = 6.0;
    $cal = $met * $weight * ($dur / 60.0);
    $entry = [
        'date' => date('c'),
        'distance' => $dist,
        'duration' => $dur,
        'speed' => $speed,
        'weight' => $weight,
        'calories' => round($cal, 1)
    ];
    $walks = loadWalks();
    $walks[] = $entry;
    saveWalks($walks);
    echo "✅ Walk added: $dist km in $dur min, " . round($cal, 1) . " kcal\n";
}

function listWalks() {
    $walks = loadWalks();
    if (empty($walks)) {
        echo "No walks recorded.\n";
        return;
    }
    echo "\n📋 Walks:\n";
    foreach ($walks as $i => $w) {
        $dt = substr($w['date'], 0, 16);
        printf("%d. %s | %.2f km | %.0f min | %.1f km/h | %.1f kcal\n",
            $i+1, $dt, $w['distance'], $w['duration'], $w['speed'], $w['calories']);
    }
}

function stats() {
    $walks = loadWalks();
    if (empty($walks)) {
        echo "No walks yet.\n";
        return;
    }
    $totalDist = $totalDur = $totalCal = 0;
    foreach ($walks as $w) {
        $totalDist += $w['distance'];
        $totalDur += $w['duration'];
        $totalCal += $w['calories'];
    }
    echo "\n🚶 Walk Summary\n";
    echo "Total walks: " . count($walks) . "\n";
    echo "Total distance: " . round($totalDist, 2) . " km\n";
    echo "Total duration: $totalDur min (" . round($totalDur/60, 1) . " h)\n";
    echo "Total calories: " . round($totalCal, 0) . " kcal\n";
}

function exportCSV($filename) {
    $walks = loadWalks();
    $fp = fopen($filename, 'w');
    fputcsv($fp, ['Date', 'Distance (km)', 'Duration (min)', 'Speed (km/h)', 'Weight (kg)', 'Calories (kcal)']);
    foreach ($walks as $w) {
        fputcsv($fp, [$w['date'], $w['distance'], $w['duration'], round($w['speed'], 1), $w['weight'], $w['calories']]);
    }
    fclose($fp);
    echo "Exported " . count($walks) . " walks to $filename\n";
}

if ($argc < 2) {
    die("Usage: php walk_tracker.php [weight|add|list|stats|export]\n");
}

$cmd = $argv[1];
switch ($cmd) {
    case 'weight':
        if ($argc != 3) die("Usage: weight <kg>\n");
        $weight = (float)$argv[2];
        saveConfig($weight);
        echo "Default weight set to $weight kg\n";
        break;
    case 'add':
        if ($argc < 4) die("Usage: add <distance> <duration> [--weight <kg>]\n");
        $dist = (float)$argv[2];
        $dur = (float)$argv[3];
        $weight = null;
        for ($i=4; $i<$argc; $i++) {
            if ($argv[$i] == '--weight' && isset($argv[$i+1])) {
                $weight = (float)$argv[$i+1];
                $i++;
            }
        }
        addWalk($dist, $dur, $weight);
        break;
    case 'list':
        listWalks();
        break;
    case 'stats':
        stats();
        break;
    case 'export':
        $filename = $argv[2] ?? 'walks.csv';
        exportCSV($filename);
        break;
    default:
        echo "Unknown command\n";
}
?>
