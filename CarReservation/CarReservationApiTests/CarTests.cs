using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CarReservationApi.Tests;

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
        await ItSaysParameterIsRequired(response, nameof(Car.Make));
    }

    private static async Task ItSaysParameterIsRequired(HttpResponseMessage response, string parameter)
    {
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), $"The {parameter} field is required.");
    }

    private static void ItDoesNotAddACar(HttpResponseMessage response)
    {
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}