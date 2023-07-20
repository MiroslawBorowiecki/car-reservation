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

        if (_carRepository.Count == 0) return Conflict(NoCarsAvailable);


        // Assuming no 'break' between reservations is needed. Consult domain experts.
        var conflicts = _reservationRepository.FindAll(
            r => RequestConflictsReservation(request, r));

        if (conflicts.Count > 0) return Conflict(NoCarsAvailable);

        var car = _carRepository.First().Value;
        var response = ReservationResponse.Create(request, car);
        _reservationRepository.Add(response);
        return Ok(response);
    }

    private static bool RequestConflictsReservation(
        ReserveCarRequest request, ReservationResponse reservation)
    {
        // Res:   ----------
        // Rq1:    ------
        // Rq2:  ---
        // Rq3:  -------------
        // Rq4:         -------

        // Rq1 = Rq2 -> RqEnd > ResStart && RqEnd < ResEnd
        // Rq3 -> RqStart < ResStart && RqEnd > ResEnd
        // Rq4 -> RqStart < ResEnd && RqEnd > ResEnd
        DateTime? requestEnd = request.Time + request.Duration;
        DateTime? reservationEnd = reservation.Time + reservation.Duration;

        return (requestEnd > reservation.Time && requestEnd <= reservationEnd)
            || (request.Time <= reservation.Time && requestEnd >= reservationEnd)
            || (request.Time < reservationEnd && requestEnd > reservationEnd);
    }

    [HttpGet]
    public ActionResult<IEnumerable<ReservationResponse>> GetAll() => _reservationRepository;
}
