namespace CarReservationApiTests.Utils;

internal static class HttpExtensions
{
    internal static async Task Setup<T>(this HttpClient client, string uri, params T[] items)
    {
        foreach (var item in items)
            (await client.PostAsJsonAsync(uri, item)).EnsureSuccessStatusCode();
    }

    internal static async Task ShouldReturnAll<T>(this HttpResponseMessage httpResponse,
        IEnumerable<T> expected, IEqualityComparer<T> comparer)
    {
        var actual = await httpResponse.Content.ReadFromJsonAsync<IEnumerable<T>>();
        Assert.IsNotNull(actual);
        It.ShouldReturnAll(expected, actual, comparer);
    }
}
