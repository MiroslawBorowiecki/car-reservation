using Microsoft.AspNetCore.Mvc;

namespace CarReservationApi.Reservations;

[ApiController]
[Route("[controller]")]
public class ReservationsController : ControllerBase
{
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

        return Problem("not implemented yet");
    }
}
