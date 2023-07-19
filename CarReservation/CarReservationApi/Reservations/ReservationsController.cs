using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarReservationApi.Reservations;

[ApiController]
[Route("[controller]")]
public class ReservationsController : ControllerBase
{
    public const string DurationValidationError 
        = "Duration must be between 5 minutes and 2 hours.";
    public const string TimeValidationError
        = "The reservation can be taken from 5 minutes up to 24 hours ahead.";

    // The specification did not say - it is my assumption that reservation for less than 5
    // minutes doesn't make sense.
    private static readonly TimeSpan minimumDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan maximumDuration = TimeSpan.FromHours(2);

    // Again - specificaiton mentioned the limit of 24 hours. I have added the minimum of 5 minutes
    // to allow processing to finish.
    // The assumption is that it has to start within the 24 hours but can end past that mark.
    private static readonly int minMinutesAhead = 5;
    private static readonly int maxHoursAhead = 24;

    [HttpPost]
    public ActionResult ReserveCar(ReserveCarRequest request)
    {
        if (request.Duration < minimumDuration || request.Duration > maximumDuration)
            return ValidationProblem(DurationValidationError);

        var now = DateTime.Now;
        var minTime = now.AddMinutes(minMinutesAhead);
        var maxTime = now.AddHours(maxHoursAhead);
        if (request.Time < minTime || request.Time > maxTime)
            return ValidationProblem(TimeValidationError);

        return Problem("not implemented yet");
    }
}
