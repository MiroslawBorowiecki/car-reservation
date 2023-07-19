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
            => await TestMissingField(new() { }, nameof(ReserveCarRequest.Time));

        private async Task TestMissingField(ReserveCarRequest request, string fieldName)
        {
            HttpClient client = _factory.CreateClient();
            
            HttpResponseMessage response = await client.PostAsJsonAsync(BaseUri, request);

            It.ShouldDenyTheAttempt(response);
            await It.ShouldRequireAField(response, fieldName);
        }
    }
}
