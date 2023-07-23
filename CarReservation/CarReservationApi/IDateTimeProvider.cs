namespace CarReservationApi;

public interface IDateTimeProvider
{
    public DateTime Now { get; }
}

public class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}
