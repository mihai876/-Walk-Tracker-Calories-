// walk_tracker.js
#!/usr/bin/env node
const fs = require('fs');
const { program } = require('commander');

const CONFIG_FILE = 'walk_config.json';
const DATA_FILE = 'walks.json';

let weight = 70.0;
let walks = [];

function loadConfig() {
    if (fs.existsSync(CONFIG_FILE)) {
        try {
            const cfg = JSON.parse(fs.readFileSync(CONFIG_FILE));
            weight = cfg.weight || 70.0;
        } catch (e) {}
    }
}

function saveConfig() {
    fs.writeFileSync(CONFIG_FILE, JSON.stringify({ weight }));
}

function loadWalks() {
    if (fs.existsSync(DATA_FILE)) {
        try {
            walks = JSON.parse(fs.readFileSync(DATA_FILE));
        } catch (e) {}
    }
}

function saveWalks() {
    fs.writeFileSync(DATA_FILE, JSON.stringify(walks, null, 2));
}

function addWalk(dist, dur, w) {
    if (w === undefined) w = weight;
    const speed = dist / (dur / 60.0);
    let met;
    if (speed < 3.0) met = 2.0;
    else if (speed < 5.0) met = 3.0;
    else if (speed < 6.5) met = 3.8;
    else if (speed < 8.0) met = 5.0;
    else met = 6.0;
    const cal = met * w * (dur / 60.0);
    const entry = {
        date: new Date().toISOString(),
        distance: dist,
        duration: dur,
        speed,
        weight: w,
        calories: Math.round(cal * 10) / 10
    };
    walks.push(entry);
    saveWalks();
    console.log(`✅ Walk added: ${dist} km in ${dur} min, ${entry.calories} kcal`);
}

function listWalks() {
    if (!walks.length) {
        console.log('No walks recorded.');
        return;
    }
    console.log('\n📋 Walks:');
    walks.forEach((w, i) => {
        const dt = w.date.slice(0, 16).replace('T', ' ');
        console.log(`${i+1}. ${dt} | ${w.distance} km | ${w.duration} min | ${w.speed.toFixed(1)} km/h | ${w.calories} kcal`);
    });
}

function stats() {
    if (!walks.length) {
        console.log('No walks yet.');
        return;
    }
    let totalDist = 0, totalDur = 0, totalCal = 0;
    for (const w of walks) {
        totalDist += w.distance;
        totalDur += w.duration;
        totalCal += w.calories;
    }
    console.log('\n🚶 Walk Summary');
    console.log(`Total walks: ${walks.length}`);
    console.log(`Total distance: ${totalDist.toFixed(2)} km`);
    console.log(`Total duration: ${totalDur} min (${(totalDur/60).toFixed(1)} h)`);
    console.log(`Total calories: ${totalCal.toFixed(0)} kcal`);
}

function exportCSV(filename) {
    const header = 'Date,Distance (km),Duration (min),Speed (km/h),Weight (kg),Calories (kcal)\n';
    const rows = walks.map(w =>
        `${w.date},${w.distance},${w.duration},${w.speed.toFixed(1)},${w.weight},${w.calories}`
    ).join('\n');
    fs.writeFileSync(filename, header + rows);
    console.log(`Exported ${walks.length} walks to ${filename}`);
}

program
    .command('weight <kg>')
    .action((kg) => {
        weight = parseFloat(kg);
        saveConfig();
        console.log(`Default weight set to ${weight} kg`);
    });

program
    .command('add <distance> <duration>')
    .option('--weight <kg>', 'Weight for this walk')
    .action((distance, duration, options) => {
        const w = options.weight ? parseFloat(options.weight) : undefined;
        addWalk(parseFloat(distance), parseFloat(duration), w);
    });

program
    .command('list')
    .action(listWalks);

program
    .command('stats')
    .action(stats);

program
    .command('export [filename]')
    .action((filename = 'walks.csv') => exportCSV(filename));

program.parse(process.argv);
