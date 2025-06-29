using Microsoft.AspNetCore.Mvc;
using VesselMovementAPI.Data;
using VesselMovementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace VesselMovementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VesselsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VesselsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
    public async Task<IActionResult> CreateVessel([FromBody] Vessel vessel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            vessel.StartTime = DateTime.UtcNow;
            vessel.TraveledKm = 0;
            vessel.Status = "In Transit";

            _context.Vessels.Add(vessel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVessel), new { id = vessel.Id }, vessel);
        }
        catch (DbUpdateException dbEx)
        {
            Console.WriteLine("❌ DB Update Exception: " + dbEx.InnerException?.Message ?? dbEx.Message);
            return StatusCode(503, "Database write failed due to transient issue. Please try again.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ General Exception: " + ex.Message);
            return StatusCode(500, $"Unexpected error: {ex.Message}");
        }
    }


        [HttpGet]
        public async Task<IActionResult> GetAllVessels()
        {
            try
            {
                var vessels = await _context.Vessels
                    .AsNoTracking()
                    .ToListAsync();

                // Update vessel positions in memory only
                foreach (var vessel in vessels)
                {
                    UpdateVesselPosition(vessel);
                }

                return Ok(vessels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving vessels: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetVessel(int id)
        {
            try
            {
                var vessel = await _context.Vessels.FindAsync(id);
                if (vessel == null)
                {
                    return NotFound($"Vessel with ID {id} not found");
                }

                UpdateVesselPosition(vessel);
                await _context.SaveChangesAsync();

                return Ok(vessel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving vessel: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVessel(int id)
        {
            try
            {
                var vessel = await _context.Vessels.FindAsync(id);
                if (vessel == null)
                {
                    return NotFound($"Vessel with ID {id} not found");
                }

                _context.Vessels.Remove(vessel);
                await _context.SaveChangesAsync();

                return Ok($"Vessel {vessel.Name} deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting vessel: {ex.Message}");
            }
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetAllVessels()
        {
            try
            {
                var vessels = await _context.Vessels.ToListAsync();
                foreach (var vessel in vessels)
                {
                    vessel.StartTime = DateTime.UtcNow;
                    vessel.TraveledKm = 0;
                    vessel.Status = "In Transit";
                }

                await _context.SaveChangesAsync();
                return Ok("All vessels reset successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error resetting vessels: {ex.Message}");
            }
        }

        private void UpdateVesselPosition(Vessel vessel)
        {
            if (vessel.Status == "Arrived") return;

            var elapsedHours = (DateTime.UtcNow - vessel.StartTime).TotalHours;
            var newTraveledKm = Math.Min(vessel.DistanceKm, (float)(vessel.SpeedKmph * elapsedHours));
            
            vessel.TraveledKm = newTraveledKm;

            if (vessel.TraveledKm >= vessel.DistanceKm)
            {
                vessel.Status = "Arrived";
                vessel.TraveledKm = vessel.DistanceKm;
            }
        }
    }
}