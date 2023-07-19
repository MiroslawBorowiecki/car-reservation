using CarReservationApi.Reservations;

namespace CarReservationApi.Tests
{
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

        private async Task TestDurationValidation(TimeSpan duration)
        {
            var request = CreateRequest(duration: duration);
            HttpResponseMessage response = await PostReservation(request);

            It.ShouldDenyTheAttempt(response);
            await It.ShouldExplain(response, ReservationsController.DurationValidationError);
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
}
