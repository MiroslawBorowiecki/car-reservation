using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CarReservationApi.Tests;

[TestClass]
public class CarTests
{
    private WebApplicationFactory<Program> _factory = new();

    [TestMethod]
    [DataRow(null, "MX-5", nameof(Car.Make), DisplayName = nameof(Car.Make))]
    [DataRow("Mazda", null, nameof(Car.Model), DisplayName = nameof(Car.Model))]
    public async Task GivenAFieldIsNotProvided_WhenITryToAddACar(
        string make, string model, string fieldName)
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response
            = await client.PostAsJsonAsync("/cars", CreateTestCar(make, model));

        ItDoesNotAddACar(response);
        await ItSaysFieldIsRequired(response, fieldName);
    }

    [TestMethod]
    [DataRow("", "MX-5", nameof(Car.Make), DisplayName = nameof(Car.Make))]
    [DataRow("Mazda", "", nameof(Car.Model), DisplayName = nameof(Car.Model))]
    public async Task GivenAFieldIsEmpty_WhenITryToAddACar(
        string make, string model, string fieldName)
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response
            = await client.PostAsJsonAsync("/cars", CreateTestCar(make, model));

        ItDoesNotAddACar(response);
        await ItSaysFieldIsRequired(response, fieldName);
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