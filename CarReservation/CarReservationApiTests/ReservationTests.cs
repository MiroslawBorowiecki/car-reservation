using CarReservationApi.Reservations;

namespace CarReservationApi.Tests;

[TestClass]
public class ReservationTests
{
    private const string BaseUri = "/reservations";
    private readonly WebApplicationFactory<Program> _factory = new();

    [TestMethod]
    public async Task GivenTheDateIsNotProvided_WhenITryToReserveACar()
        => await TestMissingField(
            new() { Duration = TimeSpan.FromHours(2) }, 
            nameof(ReserveCarRequest.Time));

    [TestMethod]
    public async Task GivenTheDurationIsNotProvided_WhenITryToReserveACar()
        => await TestMissingField(
            new() { Time = DateTime.Now.AddHours(1) }, 
            nameof(ReserveCarRequest.Duration));

    [TestMethod]
    public async Task GivenDurationIsLessThan5Minutes_WhenITryToReserveACar()
        => await TestDurationValidation(new TimeSpan(0, 0, 4, 59, 999));

    [TestMethod]
    public async Task GivenDurationIsMoreThan2Hours_WhenITryToReserveACar()
        => await TestDurationValidation(new TimeSpan(0, 2, 0, 0, 1));

    [TestMethod]
    public async Task GivenStartTimeIsMoreThan24HoursAhead_WhenITryToReserveACar()
        => await TestTimeValidation(DateTime.Now.AddDays(1).AddSeconds(10));

    [TestMethod]
    public async Task GivenStartTimeIsLessThan5MinutesAhead_WhenITryToReserveACar()
        => await TestTimeValidation(DateTime.Now.AddMinutes(5));

    private async Task TestTimeValidation(DateTime time)
    {
        // TODO: Stub DateTime to test at the edges
        var request = CreateRequest(time: time);
        HttpResponseMessage response = await PostReservation(request);

        It.ShouldDenyTheAttempt(response);
        await It.ShouldExplain(response, ReserveCarRequestValidator.TimeValidationError);
    }

    private async Task TestDurationValidation(TimeSpan duration)
    {
        var request = CreateRequest(duration: duration);
        HttpResponseMessage response = await PostReservation(request);

        It.ShouldDenyTheAttempt(response);
        await It.ShouldExplain(response, ReserveCarRequestValidator.DurationValidationError);
    }

    private async Task TestMissingField(ReserveCarRequest request, string fieldName)
    {
        HttpResponseMessage response = await PostReservation(request);

        It.ShouldDenyTheAttempt(response);
        await It.ShouldRequireAField(response, fieldName);
    }

    private async Task<HttpResponseMessage> PostReservation(ReserveCarRequest request) 
        => await _factory.CreateClient().PostAsJsonAsync(BaseUri, request);

    private static ReserveCarRequest CreateRequest(
        DateTime? time = null, TimeSpan? duration = null) => new () { 
            Time = time ?? DateTime.Now.AddHours(1), 
            Duration = duration ?? TimeSpan.FromHours(1)};
}
