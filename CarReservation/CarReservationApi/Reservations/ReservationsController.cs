using CarReservationApi.Cars;
using CarReservationApi.Cars.Persistence;
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
    public ActionResult ReserveCar(ReservationRequest request)
    {
        try
        {
            ReservationValidator.Validate(request);
        }
        catch (ArgumentException e)
        {
            return ValidationProblem(e.Message);
        }

        if (_carRepository.Count == 0) return Conflict(NoCarsAvailable);

        var availableCar = FindAvailableCar(request);

        if (availableCar == null) return Conflict(NoCarsAvailable);

        var response = ReservationResponse.Create(request, availableCar);
        _reservationRepository.Add(response);
        return Ok(response);
    }

    private Car? FindAvailableCar(ReservationRequest request)
    {
        DateTime? requestEnd = request.Time + request.Duration;

        // Assuming no 'break' between reservations is needed. Consult domain experts.
        var conflictingCarReservations =
            from reservation in _reservationRepository
            let reservationEnd = reservation.Time + reservation.Duration
            // Requested reservation would end during an existing one.
            where (reservation.Time < requestEnd && reservationEnd >= requestEnd)
            // Requested reservation would encompass an existing one.
            || (reservation.Time >= request.Time && reservationEnd <= requestEnd)
            // Requested reservation would start during an existing one.
            || (reservation.Time <= request.Time && reservationEnd > request.Time)
            select reservation.Car;

        return _carRepository.Values
            .Except(conflictingCarReservations, CarComparer.That)
            .FirstOrDefault();
    }

    [HttpGet]
    public ActionResult<IEnumerable<ReservationResponse>> GetAll() => _reservationRepository;
}
