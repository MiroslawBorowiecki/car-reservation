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
            nameof(ReservationRequest.Time));

    [TestMethod]
    public async Task GivenTheDurationIsNotProvided_WhenITryToReserveACar()
        => await TestMissingField(
            new() { Time = DateTime.Now.AddHours(1) }, 
            nameof(ReservationRequest.Duration));

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
        ReservationRequest reservation = CreateValidRequest();
        await client.PostAsJsonAsync(BaseUri, reservation);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(BaseUri, reservation);

        // Assert
        It.ShouldDenyTheAttempt(response, HttpStatusCode.Conflict);
        await It.ShouldExplain(response, ReservationsController.NoCarsAvailable);
    }

    [TestMethod]
    public async Task GivenExactlyOneAndUnreservedCar_WhenIReserveACar()
    {
        HttpClient client = _factory.CreateClient();
        await client.Setup(CarTests.BaseUri, CarTests.MazdaMx5);
        ReservationRequest reservationRequest = CreateValidRequest();

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

    [TestMethod]
    public async Task GivenACoupleReservations_WhenIGetThem()
    {
        HttpClient client = _factory.CreateClient();
        var cars = new[] { CarTests.MazdaMx5, CarTests.MazdaMx5, CarTests.MazdaMx5 };
        await client.Setup(CarTests.BaseUri, cars);
        var now = DateTime.Now;
        ReservationRequest[] requests = new[]
        {
            CreateValidRequest(now.AddHours(1)),
            CreateValidRequest(now.AddHours(3)),
            CreateValidRequest(now.AddHours(5))
        };
        await client.Setup(BaseUri, requests);

        HttpResponseMessage httpResponse = await client.GetAsync(BaseUri);

        It.ShouldAllowTheAttempt(httpResponse);

        IEnumerable<ReservationResponse> expected 
            = requests.Zip(cars).Select(z => ReservationResponse.Create(z.First, z.Second));
        await httpResponse.ShouldReturnAll(expected, ReservationComparer.That);
    }

    // - More conflict types:
    //   - request end within existing reservation
    //   - request encompasses an existing reservation
    //   - request start within an existing reservation
    // - Finding an unreserved car
    // - Finding the car available in the given period
    // - Get shouldn't return reservations in the past

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
        await It.ShouldExplain(response, ReservationValidator.TimeValidationError);
    }

    private async Task TestDurationValidation(TimeSpan duration)
    {
        var request = CreateValidRequest(duration: duration);
        HttpResponseMessage response = await PostReservation(request);

        It.ShouldDenyTheAttempt(response);
        await It.ShouldExplain(response, ReservationValidator.DurationValidationError);
    }

    private async Task TestMissingField(ReservationRequest request, string fieldName)
    {
        HttpResponseMessage response = await PostReservation(request);

        It.ShouldDenyTheAttempt(response);
        await It.ShouldRequireAField(response, fieldName);
    }

    private async Task<HttpResponseMessage> PostReservation(ReservationRequest request) 
        => await _factory.CreateClient().PostAsJsonAsync(BaseUri, request);

    /// <summary>
    /// Creates a reservation request with provided parameters.
    /// Defaults to one hour ahead and duration of one hour.
    /// </summary>
    /// <param name="time">The time when the reservation should start.</param>
    /// <param name="duration">The intended duration.</param>
    /// <returns>Prepared reservation request.</returns>
    private static ReservationRequest CreateValidRequest(
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
