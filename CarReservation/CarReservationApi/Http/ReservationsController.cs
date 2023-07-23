using Microsoft.AspNetCore.Mvc;
using CarReservationApi.Reservations;

namespace CarReservationApi.Http;

[ApiController]
[Produces("application/json")]
[Route("[controller]")]
public class ReservationsController : ControllerBase
{
    
    private readonly ReservationService _reservationService;

    public ReservationsController(ReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    /// <summary>
    /// Reserves a car at the specified time and for the specified duration.
    /// </summary>
    /// <param name="request">Reservation data.</param>
    /// <returns>The complete reservation details, including the car.</returns>
    /// <response code="200">The newly created reservation.</response>
    /// <response code="400">Invalid/missing data: Time or Duration. Time must fit within 24h, and duration is from 5 minutes to 2 hours.</response>
    /// <response code="409">No cars are available for the reservation.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult ReserveCar(ReservationRequest request)
    {
        try
        {
            var result = _reservationService.ReserveCar(request);
            return result != null
                ? Ok(result)
                : Conflict(Messages.NoCarsAvailable);
        }
        catch (ArgumentException e)
        {
            return ValidationProblem(e.Message);
        }
    }

    /// <summary>
    /// Retrieves all upcoming reservations.
    /// </summary>
    /// <returns>All upcoming reservations, including car details.</returns>
    /// <response code="200">All existing cars.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ReservationResponse>> GetAll() => _reservationService.GetAll();
}
