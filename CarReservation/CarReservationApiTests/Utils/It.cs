namespace CarReservationApiTests.Utils;

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

    public static void ShouldAllowTheAttempt(
        HttpResponseMessage response, HttpStatusCode expectedCode = HttpStatusCode.OK)
        => Assert.AreEqual(expectedCode, response.StatusCode);

    public static void ShouldShowTheLocation(HttpResponseMessage response, string location)
        => Assert.AreEqual(location, response.Headers.Location?.ToString());

    public static async Task ShouldReturnNo<T>(HttpResponseMessage response)
    {
        var results = await response.Content.ReadFromJsonAsync<IEnumerable<T>>();
        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count());
    }

    public static void ShouldReturnAll<T>(
        IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T> comparer)
    {
        HashSet<T> expectedSet = new(expected, comparer);
        Assert.IsTrue(expectedSet.SetEquals(actual), $"Not all {typeof(T).Name}s were returned.");
    }
}
