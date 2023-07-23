namespace CarReservationApi.Http;

public class ReservationResponse
{
    public DateTime? Time { get; set; } = null!;
    public TimeSpan? Duration { get; set; } = null!;
    public Cars.Car Car { get; set; } = null!;

    public static ReservationResponse Create(ReservationRequest request, Cars.Car car)
        => new() { Time = request.Time, Duration = request.Duration, Car = car };
}
