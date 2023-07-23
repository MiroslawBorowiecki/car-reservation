using System.ComponentModel.DataAnnotations;

namespace CarReservationApi.Http;

public class UpdateCarRequest
{
    [Required(AllowEmptyStrings = false)] public string Make { get; set; } = null!;
    [Required(AllowEmptyStrings = false)] public string Model { get; set; } = null!;
}
