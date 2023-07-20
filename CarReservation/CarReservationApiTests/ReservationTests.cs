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

    [TestMethod]
    public async Task GivenNoCarsAddedYet_WhenITryToReserveACar()
    {
        var request = CreateValidRequest(DateTime.Now.AddHours(1), TimeSpan.FromHours(1));
        
        var response = await PostReservation(request);

        It.ShouldDenyTheAttempt(response, HttpStatusCode.Conflict);
        await It.ShouldExplain(response, ReservationsController.NoCarsAvailable);
    }

    [TestMethod]
    public async Task GivenOneCarAndAConflictingReservation_WhenITryToReserveACar()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        await client.PostAsJsonAsync(CarTests.BaseUri, CarTests.MazdaMx5);
        ReserveCarRequest reservation = CreateValidRequest();
        await client.PostAsJsonAsync(BaseUri, reservation);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(BaseUri, reservation);

        // Assert
        It.ShouldDenyTheAttempt(response, HttpStatusCode.Conflict);
        await It.ShouldExplain(response, ReservationsController.NoCarsAvailable);
    }

    [TestMethod]
    public async Task GivenOnlyOneUnreservedCar_WhenIReserveACar()
    {
        HttpClient client = _factory.CreateClient();
        await client.PostAsJsonAsync(CarTests.BaseUri, CarTests.MazdaMx5);
        ReserveCarRequest reservationRequest = CreateValidRequest();

        HttpResponseMessage response = await client.PostAsJsonAsync(BaseUri, reservationRequest);

        It.ShouldAllowTheAttempt(response, HttpStatusCode.OK);
        await ItShouldReturnReservationDetails(
            response, ReservationResponse.Create(reservationRequest, CarTests.MazdaMx5));
    }

    [TestMethod]
    public async Task GivenNoReservationsMadeYet_WhenITryToGetThem()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage responseMessage = await client.GetAsync(BaseUri);

        It.ShouldAllowTheAttempt(responseMessage);
        await It.ShouldReturnNo<ReservationResponse>(responseMessage);
    }

    private static async Task ItShouldReturnReservationDetails(
        HttpResponseMessage response, ReservationResponse expectedResponse)
    {
        var actualResponse = await response.Content.ReadFromJsonAsync<ReservationResponse>();
        Assert.AreEqual(expectedResponse, actualResponse, ReservationComparer.That);
    }

    private async Task TestTimeValidation(DateTime time)
    {
        // TODO: Stub DateTime to test at the edges
        var request = CreateValidRequest(time: time);
        HttpResponseMessage response = await PostReservation(request);

        It.ShouldDenyTheAttempt(response);
        await It.ShouldExplain(response, ReserveCarRequestValidator.TimeValidationError);
    }

    private async Task TestDurationValidation(TimeSpan duration)
    {
        var request = CreateValidRequest(duration: duration);
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

    private static ReserveCarRequest CreateValidRequest(
        DateTime? time = null, TimeSpan? duration = null) => new() 
        {
            Time = time ?? DateTime.Now.AddHours(1),
            Duration = duration ?? TimeSpan.FromHours(1) 
        };

    public class ReservationComparer : EqualityComparer<ReservationResponse>
    {
        public readonly static ReservationComparer That = new(new CarTests.CarComparer());
        private readonly CarTests.CarComparer _carComparer;

        public ReservationComparer(CarTests.CarComparer carComparer) : base()
        {
            _carComparer = carComparer;
        }

        public override bool Equals(ReservationResponse? x, ReservationResponse? y)
        {
            if(x == null && y == null) return true;

            if (x == null || y == null) return false;

            return x.Time == y.Time && x.Duration == y.Duration && _carComparer.Equals(x.Car, y.Car);
        }

        public override int GetHashCode([DisallowNull] ReservationResponse obj)
        {
            return obj.Time.GetHashCode() 
                ^ obj.Duration.GetHashCode() 
                ^ _carComparer.GetHashCode(obj.Car);
        }
    }
}
