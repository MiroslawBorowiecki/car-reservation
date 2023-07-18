namespace CarReservationApi.Tests;

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

[TestClass]
public class CarTests
{
    private WebApplicationFactory<Program> _factory = new();

    [TestMethod]
    public async Task GivenNoDataIsProvided_WhenITryToAddACar()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsync("/cars", null);

        ItDoesNotAddACar(response);
    }

    [TestMethod]
    public async Task GivenModelIsNotProvided_WhenITryToAddACar()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/cars", new Car());

        ItDoesNotAddACar(response);
        await ItSaysParameterIsMissing(response, nameof(Car.Make));
    }

    private static async Task ItSaysParameterIsMissing(HttpResponseMessage response, string parameter)
    {
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), parameter);
    }

    private static void ItDoesNotAddACar(HttpResponseMessage response)
    {
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}