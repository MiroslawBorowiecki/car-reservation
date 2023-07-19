using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarReservationApi.Reservations
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationsController : ControllerBase
    {
        public const string DurationValidationError
        = "Duration must be between 5 minutes and 2 hours.";

        // The specification did not say - it is my assumption that reservation for less than 5
        // minutes doesn't make sense.
        private static readonly TimeSpan minimumDuration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan maximumDuration = TimeSpan.FromHours(2);

        [HttpPost]
        public ActionResult ReserveCar(ReserveCarRequest request)
        {
            if (request.Duration < minimumDuration || request.Duration > maximumDuration)
                return ValidationProblem(DurationValidationError);

            return Problem("not implemented yet");
        }
    }
}
