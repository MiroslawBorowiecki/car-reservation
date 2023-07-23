using Microsoft.Extensions.DependencyInjection;
using CarReservationApi.Cars;
using CarReservationApi.Http;

namespace CarReservationApi.Tests;

[TestClass]
public partial class ReservationTests
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
        await It.ShouldExplain(response, Messages.NoCarsAvailable);
    }

    [TestMethod]
    public async Task GivenOneCar_WhenIRequestAReservationEndingDuringAnExistingOne()
        => await TestConflictingRequest(
            CreateValidRequest(DateTime.Now.AddHours(1), TimeSpan.FromHours(1.0)),
            CreateValidRequest(DateTime.Now.AddHours(0.5), TimeSpan.FromHours(1.0)));

    [TestMethod]
    public async Task GivenOneCar_WhenIRequestAReservationStartingDuringAnExistingOne()
        => await TestConflictingRequest(
            CreateValidRequest(DateTime.Now.AddHours(1.0), TimeSpan.FromHours(1.0)),
            CreateValidRequest(DateTime.Now.AddHours(1.5), TimeSpan.FromHours(1.0)));

    [TestMethod]
    public async Task GivenOneCar_WhenIRequestAReservationEncompassingAnExistingOne()
        => await TestConflictingRequest(
            CreateValidRequest(DateTime.Now.AddHours(1.0), TimeSpan.FromHours(1.0)),
            CreateValidRequest(DateTime.Now.AddHours(0.5), TimeSpan.FromHours(2.0)));

    [TestMethod]
    public async Task GivenOneCar_WhenIRequestAReservationExactlyAsExistingOne()
    {
        var request = CreateValidRequest();
        await TestConflictingRequest(request, request);
    }

    private async Task TestConflictingRequest(
        ReservationRequest originalRequest, ReservationRequest conflictingRequest)
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        await client.PostAsJsonAsync(CarTests.BaseUri, CarTests.MazdaMx5);
        await client.PostAsJsonAsync(BaseUri, originalRequest);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(BaseUri, conflictingRequest);

        // Assert
        It.ShouldDenyTheAttempt(response, HttpStatusCode.Conflict);
        await It.ShouldExplain(response, Messages.NoCarsAvailable);
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
        await client.PostAsJsonAsync(CarTests.BaseUri, CarTests.MazdaMx5);
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
            = requests.Select(req => ReservationResponse.Create(req, CarTests.MazdaMx5));
        await httpResponse.ShouldReturnAll(expected, ReservationComparer.That);
    }

    // TODO: Refactor this into individual, more readable test cases.
    [TestMethod]
    public async Task GivenACoupleOfCars_WhenIMakeSubsequentOverlappingReservations()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        await client.Setup(CarTests.BaseUri,
            CarTests.MazdaMx5, CarTests.OpelAstra, CarTests.Peugeout206, CarTests.DodgeViper);
        var now = DateTime.Now;
        List<ReservationResponse> responses = new();

        // Act 
        ReservationRequest request = CreateValidRequest(now.AddHours(1), new TimeSpan(1, 0, 0));
        HttpResponseMessage response = await SubmitReservation(client, request);
        // Assert (Mazda [1:00-2:00])
        responses.Add(await ItReservesFirstAvailableCar(response, request, CarTests.MazdaMx5));

        // Act: reservation ending during an existing one - unreserved car available
        request = CreateValidRequest(now.AddHours(0.5), new TimeSpan(1, 30, 0));
        response = await SubmitReservation(client, request);
        // Assert (Opel [0:30-2:00])
        responses.Add(await ItReservesFirstAvailableCar(response, request, CarTests.OpelAstra));

        // Act: reservation starting during an existing one - unreserved car available
        request = CreateValidRequest(now.AddHours(1.5), new TimeSpan(1, 0, 0));
        response = await SubmitReservation(client, request);
        // Assert (Peugeout [1:30-2:30])
        responses.Add(await ItReservesFirstAvailableCar(response, request, CarTests.Peugeout206));

        // Act: reservation encompassing an existing one - unresereved car available
        request = CreateValidRequest(now.AddHours(0.5), new TimeSpan(2, 0, 0));
        response = await SubmitReservation(client, request);
        // Assert (Dodge [0:30-2:30])
        responses.Add(await ItReservesFirstAvailableCar(response, request, CarTests.DodgeViper));

        // Act: reservation ending during an existing one - another car available at the time
        request = CreateValidRequest(now.AddHours(0.5), new TimeSpan(1, 0, 0));
        response = await SubmitReservation(client, request);
        // Assert (Peugeot [0:30-1:30] & [1:30-2:30])
        responses.Add(await ItReservesFirstAvailableCar(response, request, CarTests.Peugeout206));

        // Act: reservation starting during an existing one - another car available at the time
        request = CreateValidRequest(now.AddHours(2), new TimeSpan(0, 30, 0));
        response = await SubmitReservation(client, request);
        // Assert (Mazda [1:00-2:00] & [2:00-2:30])
        responses.Add(await ItReservesFirstAvailableCar(response, request, CarTests.MazdaMx5));

        // Act: reservation encompassing (equal) an existing one - other car available at the time
        request = CreateValidRequest(now.AddHours(2), new TimeSpan(0, 30, 0));
        response = await SubmitReservation(client, request);
        // Assert (Opel [0:30-2:00] & [2:00-2:30])
        responses.Add(await ItReservesFirstAvailableCar(response, request, CarTests.OpelAstra));

        // Act: reservation ending during an existing one - no cars available at the time
        request = CreateValidRequest(now.AddHours(0.5), new TimeSpan(1, 30, 0));
        response = await SubmitReservation(client, request);
        // Assert
        It.ShouldDenyTheAttempt(response, HttpStatusCode.Conflict);
        await It.ShouldExplain(response, Messages.NoCarsAvailable);

        // Act: reservation starting during an existing one - no cars available at the time
        request = CreateValidRequest(now.AddHours(1.5), new TimeSpan(1, 0, 0));
        response = await SubmitReservation(client, request);
        // Assert
        It.ShouldDenyTheAttempt(response, HttpStatusCode.Conflict);
        await It.ShouldExplain(response, Messages.NoCarsAvailable);

        // Act: reservation encompassing an existing one - no cars available at the time
        request = CreateValidRequest(now.AddHours(0.5), new TimeSpan(2, 0, 0));
        response = await SubmitReservation(client, request);
        // Assert
        It.ShouldDenyTheAttempt(response, HttpStatusCode.Conflict);
        await It.ShouldExplain(response, Messages.NoCarsAvailable);

        // Act
        HttpResponseMessage httpResponse = await client.GetAsync(BaseUri);
        // Assert
        It.ShouldAllowTheAttempt(httpResponse);
        await httpResponse.ShouldReturnAll(responses, ReservationComparer.That);
    }

    [TestMethod]
    public async Task GivenSomePastReservations_WhenIGetAll()
    {
        // Arrange
        TestDateTimeProvider timeProvider = new();
        HttpClient client = _factory.WithWebHostBuilder(builder => builder.ConfigureServices(
            services => services.AddSingleton<IDateTimeProvider>(timeProvider)))
            .CreateClient();
        await client.Setup(CarTests.BaseUri, CarTests.MazdaMx5);

        var pastReservations = await SendACoupleValidRequests(client, timeProvider!.Now);
        
        timeProvider.Now = timeProvider.Now.AddDays(2.0);
        var expectedReservations = await SendACoupleValidRequests(client, timeProvider.Now);
        // This moves the time so that the first of new reservations is already started.
        timeProvider.Now = timeProvider.Now = expectedReservations[0].Time!.Value.AddMinutes(1.0);
        expectedReservations.RemoveAt(0);

        // Act
        var actualReservations 
            = await client.GetFromJsonAsync<IEnumerable<ReservationResponse>>(BaseUri);

        // Assert
        It.ShouldReturnAll(expectedReservations, actualReservations!, ReservationComparer.That);
    }

    private async Task<List<ReservationResponse>> SendACoupleValidRequests(
        HttpClient client, DateTime now)
    {
        List<ReservationResponse> responses = new()
        {
            await ReserveCar(client, CreateValidRequest(now.AddHours(1.0))),
            await ReserveCar(client, CreateValidRequest(now.AddHours(3.0))),
            await ReserveCar(client, CreateValidRequest(now.AddHours(5.0)))
        };
        return responses;
    }

    private static async Task<ReservationResponse> ReserveCar(
        HttpClient client, ReservationRequest request)
    {
        var response = await client.PostAsJsonAsync(BaseUri, request);
        return (await response.Content.ReadFromJsonAsync<ReservationResponse>())!;
    }

    public static async Task<HttpResponseMessage> SubmitReservation
        (HttpClient client, ReservationRequest request)
    {
        return await client.PostAsJsonAsync(BaseUri, request);
    }

    // - Shouldn't be possible to remove or update a car upcoming reservations.

    private static async Task<ReservationResponse> ItReservesFirstAvailableCar(
        HttpResponseMessage response, ReservationRequest request, Car car)
    {
        response.EnsureSuccessStatusCode();
        var expectedResponse = ReservationResponse.Create(request, car);
        var actualResponse = await response.Content.ReadFromJsonAsync<ReservationResponse>();
        Assert.AreEqual(expectedResponse, actualResponse, ReservationComparer.That);
        return actualResponse!;
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
        await It.ShouldExplain(response, Messages.TimeValidationError);
    }

    private async Task TestDurationValidation(TimeSpan duration)
    {
        var request = CreateValidRequest(duration: duration);
        HttpResponseMessage response = await PostReservation(request);

        It.ShouldDenyTheAttempt(response);
        await It.ShouldExplain(response, Messages.DurationValidationError);
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
    /// Creates a request request with provided parameters.
    /// Defaults to one hour ahead and duration of one hour.
    /// </summary>
    /// <param name="time">The time when the request should start.</param>
    /// <param name="duration">The intended duration.</param>
    /// <returns>Prepared request request.</returns>
    public static ReservationRequest CreateValidRequest(
        DateTime? time = null, TimeSpan? duration = null) => new()
        {
            Time = time ?? DateTime.Now.AddHours(1),
            Duration = duration ?? TimeSpan.FromHours(1)
        };

    public class ReservationComparer : EqualityComparer<ReservationResponse>
    {
        public readonly static ReservationComparer That = new(CarComparer.That);
        private readonly CarComparer _carComparer;

        public ReservationComparer(CarComparer carComparer) : base()
        {
            _carComparer = carComparer;
        }

        public override bool Equals(ReservationResponse? x, ReservationResponse? y)
        {
            if (x == null && y == null) return true;

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
