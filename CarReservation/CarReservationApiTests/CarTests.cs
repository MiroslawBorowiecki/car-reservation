using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CarReservationApi.Tests;

[TestClass]
public class CarTests
{
    private WebApplicationFactory<Program> _factory = new();

    [TestMethod]
    public async Task GivenMakeIsNotProvided_WhenITryToAddACar()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response 
            = await client.PostAsJsonAsync("/cars", CreateTestCar(model: "MX-5"));

        ItDoesNotAddACar(response);
        await ItSaysFieldIsRequired(response, nameof(Car.Make));
    }

    [TestMethod]
    public async Task GivenModelIsNotProvided_WhenITryToAddACar()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response 
            = await client.PostAsJsonAsync("/cars", CreateTestCar(make: "Mazda"));

        ItDoesNotAddACar(response);
        await ItSaysFieldIsRequired(response, nameof(Car.Model));
    }

    [TestMethod]
    public async Task GivenMakeIsEmpty_WhenITryToAddACar()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response 
            = await client.PostAsJsonAsync("/cars", CreateTestCar(string.Empty, "MX-5"));

        ItDoesNotAddACar(response);
        await ItSaysFieldIsRequired(response, nameof(Car.Make));
    }

    [TestMethod]
    public async Task GivenModelIsEmpty_WhenITryToAddACar()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response
            = await client.PostAsJsonAsync("/cars", CreateTestCar("Mazda", string.Empty));

        ItDoesNotAddACar(response);
        await ItSaysFieldIsRequired(response, nameof(Car.Model));
    }

    private static async Task ItSaysFieldIsRequired(HttpResponseMessage response, string parameter)
    {
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), $"The {parameter} field is required.");
    }

    private static void ItDoesNotAddACar(HttpResponseMessage response)
    {
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static Car CreateTestCar(string? make = null, string? model = null)
    {
        Car car = new();

        if (make != null) car.Make = make;

        if (model != null) car.Model = model;

        return car;
    }
}