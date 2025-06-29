namespace VesselMovementAPI.Models
{
    public class Vessel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public float SpeedKmph { get; set; }
        public float DistanceKm { get; set; }
        public float TraveledKm { get; set; } = 0;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "In Transit";
        public float ProgressPercentage => DistanceKm > 0 ? Math.Min(100, (TraveledKm / DistanceKm) * 100) : 0;
        public float EstimatedHoursRemaining => Status == "Arrived" ? 0 :
            SpeedKmph > 0 ? Math.Max(0, (DistanceKm - TraveledKm) / SpeedKmph) : 0;
    }
}