namespace CarReservationApi.Tests;

using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

[TestClass]
public class CarTests
{
    private WebApplicationFactory<Program> _factory = new();

    [TestMethod]
    public async Task GivenNoDataIsProvided_WhenITryToAddACar()
    {
        HttpClient client = _factory.CreateClient();

        var response = await client.PostAsync("/cars", null);

        ItDoesNotAddACar(response);
    }

    private static void ItDoesNotAddACar(HttpResponseMessage response)
    {
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}