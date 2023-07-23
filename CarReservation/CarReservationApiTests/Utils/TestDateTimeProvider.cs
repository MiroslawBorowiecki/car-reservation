using CarReservationApi;

namespace CarReservationApiTests.Utils;

public class TestDateTimeProvider : IDateTimeProvider
{
    public TestDateTimeProvider() => Now = new DateTime(2023, 7, 20);
    public DateTime Now { get; set; }
}
