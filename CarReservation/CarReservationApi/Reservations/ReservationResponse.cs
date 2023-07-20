namespace CarReservationApi.Reservations;

public class ReservationResponse
{
    public DateTime? Time { get; set; } = null!;
    public TimeSpan? Duration { get; set; } = null!;
    public Cars.Car Car { get; set; } = null!;

    public static ReservationResponse Create(ReserveCarRequest request, Cars.Car car)
        => new() { Time = request.Time, Duration = request.Duration, Car = car };
}
