namespace CarReservationApi.Http;

public static class Messages
{
    public const string NoCarsAvailable
        = "Booking with the given time and duration is not possible - no cars are available.";
    public const string DurationValidationError
        = "Duration must be between 5 minutes and 2 hours.";
    public const string TimeValidationError
        = "The reservation can be taken from 5 minutes up to 24 hours ahead.";
    public const string CarReservedError = "The car is either in use or reserved ahead.";
}
