using Microsoft.AspNetCore.Mvc;

namespace CarReservationApi.Reservations;

[ApiController]
[Route("[controller]")]
public class ReservationsController : ControllerBase
{
    public const string NoCarsAvailable 
        = "Booking with the given time and duration is not possible - no cars are available.";

    [HttpPost]
    public ActionResult ReserveCar(ReserveCarRequest request)
    {
        try
        {
            ReserveCarRequestValidator.Validate(request);
        }
        catch (ArgumentException e)
        {
            return ValidationProblem(e.Message);
        }

        return Conflict(NoCarsAvailable);
    }
}
