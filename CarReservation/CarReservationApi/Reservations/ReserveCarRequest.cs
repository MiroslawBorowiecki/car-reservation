using System.ComponentModel.DataAnnotations;

namespace CarReservationApi.Reservations
{
    public class ReserveCarRequest
    {
        [Required] public DateTime? Time { get; set; } = null!;
    }
}