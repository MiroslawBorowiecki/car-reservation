using System.ComponentModel.DataAnnotations;

namespace CarReservationApi.Http
{
    // Can be improved by using raw value types. Requires a different version for tests.
    public class ReservationRequest
    {
        [Required] public DateTime? Time { get; set; }
        [Required] public TimeSpan? Duration { get; set; }
    }
}