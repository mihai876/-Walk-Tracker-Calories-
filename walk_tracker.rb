# walk_tracker.rb
#!/usr/bin/env ruby
require 'json'
require 'date'

CONFIG_FILE = 'walk_config.json'
DATA_FILE = 'walks.json'

class WalkTracker
  attr_reader :weight, :walks

  def initialize
    @weight = 70.0
    @walks = []
    load_config
    load_walks
  end

  def load_config
    if File.exist?(CONFIG_FILE)
      config = JSON.parse(File.read(CONFIG_FILE))
      @weight = config['weight'] || 70.0
    end
  end

  def save_config
    File.write(CONFIG_FILE, JSON.pretty_generate({ 'weight' => @weight }))
  end

  def load_walks
    if File.exist?(DATA_FILE)
      @walks = JSON.parse(File.read(DATA_FILE))
    end
  end

  def save_walks
    File.write(DATA_FILE, JSON.pretty_generate(@walks))
  end

  def add_walk(distance, duration, weight = nil)
    weight ||= @weight
    speed = distance / (duration / 60.0)
    met = if speed < 3.0
            2.0
          elsif speed < 5.0
            3.0
          elsif speed < 6.5
            3.8
          elsif speed < 8.0
            5.0
          else
            6.0
          end
    calories = met * weight * (duration / 60.0)
    entry = {
      'date' => Time.now.iso8601,
      'distance' => distance,
      'duration' => duration,
      'speed' => speed,
      'weight' => weight,
      'calories' => calories.round(1)
    }
    @walks << entry
    save_walks
    puts "✅ Walk added: #{distance} km in #{duration} min, #{calories.round(1)} kcal"
  end

  def list_walks
    if @walks.empty?
      puts "No walks recorded."
      return
    end
    puts "\n📋 Walks:"
    @walks.each_with_index do |w, i|
      dt = w['date'][0..15].gsub('T', ' ')
      puts "#{i+1}. #{dt} | #{w['distance']} km | #{w['duration']} min | #{w['speed'].round(1)} km/h | #{w['calories']} kcal"
    end
  end

  def stats
    if @walks.empty?
      puts "No walks yet."
      return
    end
    total_dist = @walks.sum { |w| w['distance'] }
    total_dur = @walks.sum { |w| w['duration'] }
    total_cal = @walks.sum { |w| w['calories'] }
    puts "\n🚶 Walk Summary"
    puts "Total walks: #{@walks.size}"
    puts "Total distance: #{total_dist.round(2)} km"
    puts "Total duration: #{total_dur} min (#{(total_dur/60.0).round(1)} h)"
    puts "Total calories: #{total_cal.round(0)} kcal"
  end

  def export_csv(filename)
    require 'csv'
    CSV.open(filename, 'w') do |csv|
      csv << ["Date", "Distance (km)", "Duration (min)", "Speed (km/h)", "Weight (kg)", "Calories (kcal)"]
      @walks.each do |w|
        csv << [w['date'], w['distance'], w['duration'], w['speed'].round(1), w['weight'], w['calories']]
      end
    end
    puts "Exported #{@walks.size} walks to #{filename}"
  end
end

if ARGV.empty?
  puts "Usage: walk_tracker.rb [weight|add|list|stats|export]"
  exit
end

tracker = WalkTracker.new
cmd = ARGV.shift

case cmd
when 'weight'
  weight = ARGV.shift.to_f
  tracker.weight = weight
  tracker.save_config
  puts "Default weight set to #{weight} kg"
when 'add'
  if ARGV.size < 2
    puts "Usage: add <distance> <duration> [--weight <kg>]"
    exit
  end
  distance = ARGV.shift.to_f
  duration = ARGV.shift.to_f
  weight = nil
  if ARGV.include?('--weight')
    idx = ARGV.index('--weight')
    weight = ARGV[idx+1].to_f if idx
  end
  tracker.add_walk(distance, duration, weight)
when 'list'
  tracker.list_walks
when 'stats'
  tracker.stats
when 'export'
  filename = ARGV.shift || 'walks.csv'
  tracker.export_csv(filename)
else
  puts "Unknown command"
end
