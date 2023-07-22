namespace CarReservationApi.Reservations;

public static class ReservationValidator
{
    public const string DurationValidationError
        = "Duration must be between 5 minutes and 2 hours.";
    public const string TimeValidationError
        = "The reservation can be taken from 5 minutes up to 24 hours ahead.";

    // The specification does not say - it is my assumption that reservation for less than 5
    // minutes doesn't make sense.
    private static readonly TimeSpan minimumDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan maximumDuration = TimeSpan.FromHours(2);

    // Again - specificaiton mentioned the limit of 24 hours. I have added the minimum of 5 minutes
    // to allow processing to finish.
    // The assumption is that it has to start within the 24 hours but can end past that mark.
    private static readonly int minMinutesAhead = 5;
    private static readonly int maxHoursAhead = 24;

    public static void Validate(ReservationRequest request)
    {
        ValidateDuration(request.Duration);
        ValidateTime(request.Time);        
    }

    private static void ValidateDuration(TimeSpan? duration)
    {
        if (duration < minimumDuration || duration > maximumDuration)
            throw new ArgumentOutOfRangeException(DurationValidationError);
    }

    private static void ValidateTime(DateTime? time)
    {
        var now = DateTime.Now;
        var minTime = now.AddMinutes(minMinutesAhead);
        var maxTime = now.AddHours(maxHoursAhead);
        if (time < minTime || time > maxTime)
            throw new ArgumentOutOfRangeException(TimeValidationError);
    }
}
