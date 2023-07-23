using CarReservationApi.Reservations;
using Microsoft.AspNetCore.Mvc;

namespace CarReservationApi.Http;

[ApiController]
[Route("[controller]")]
public class ReservationsController : ControllerBase
{
    public const string NoCarsAvailable
        = "Booking with the given time and duration is not possible - no cars are available.";
    private readonly ReservationService _reservationService;

    public ReservationsController(ReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost]
    public ActionResult ReserveCar(ReservationRequest request)
    {
        try
        {
            var result = _reservationService.ReserveCar(request);
            return result != null
                ? Ok(result)
                : Conflict(NoCarsAvailable);
        }
        catch (ArgumentException e)
        {
            return ValidationProblem(e.Message);
        }
    }

    [HttpGet]
    public ActionResult<IEnumerable<ReservationResponse>> GetAll() => _reservationService.GetAll();
}
