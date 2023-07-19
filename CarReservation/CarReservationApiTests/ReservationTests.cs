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

        private async Task TestMissingField(ReserveCarRequest request, string fieldName)
        {
            HttpClient client = _factory.CreateClient();
            
            HttpResponseMessage response = await client.PostAsJsonAsync(BaseUri, request);

            It.ShouldDenyTheAttempt(response);
            await It.ShouldRequireAField(response, fieldName);
        }
    }
}
