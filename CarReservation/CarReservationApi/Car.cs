using System.ComponentModel.DataAnnotations;

namespace CarReservationApi;

public class Car
{
    [Required(AllowEmptyStrings = false)]
    public string Make { get; set; } = null!;
}
