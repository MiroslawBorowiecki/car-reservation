using CarReservationApi.Cars;
using Microsoft.AspNetCore.Mvc;

namespace CarReservationApi.Reservations;

[ApiController]
[Route("[controller]")]
public class ReservationsController : ControllerBase
{
    public const string NoCarsAvailable 
        = "Booking with the given time and duration is not possible - no cars are available.";
    private readonly ReservationRepository _reservationRepository;
    private readonly CarRepository _carRepository;

    public ReservationsController(
        ReservationRepository reservationRepository,
        CarRepository carRepository)
    {
        _reservationRepository = reservationRepository;
        _carRepository = carRepository;
    }

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

        if (_carRepository.Count == 0 || _reservationRepository.Count > 0)
            return Conflict(NoCarsAvailable);

        var car = _carRepository.First().Value;
        var response = ReservationResponse.Create(request, car);
        _reservationRepository.Add(response);
        return Ok(response);
    }

    [HttpGet]
    public ActionResult<IEnumerable<ReservationResponse>> GetAll() => _reservationRepository;
}
