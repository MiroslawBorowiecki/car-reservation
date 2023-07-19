namespace CarReservationApi.Tests;

public static class It
{
    public static async Task ShouldRequireAField(HttpResponseMessage response, string field)
        => StringAssert.Contains(
            await response.Content.ReadAsStringAsync(),
            $"The {field} field is required.");

    public static void ShouldDenyTheAttempt(
        HttpResponseMessage response, HttpStatusCode expectedCode = HttpStatusCode.BadRequest)
        => Assert.AreEqual(expectedCode, response.StatusCode);

    public static async Task ShouldExplain(HttpResponseMessage response, string explanation)
        => StringAssert.Contains(await response.Content.ReadAsStringAsync(), explanation);
}
