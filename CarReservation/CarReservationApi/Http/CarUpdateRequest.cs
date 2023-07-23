using System.ComponentModel.DataAnnotations;

namespace CarReservationApi.Http;

public class CarUpdateRequest
{
    [Required(AllowEmptyStrings = false)] public string Make { get; set; } = null!;
    [Required(AllowEmptyStrings = false)] public string Model { get; set; } = null!;
}
