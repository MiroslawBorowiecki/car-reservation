using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CarReservationApi.Tests;

[TestClass]
public class CarTests
{
    private readonly WebApplicationFactory<Program> _factory = new();

    [TestMethod]
    [DataRow(null, "MX-5", "C1", nameof(Car.Make), DisplayName = nameof(Car.Make))]
    [DataRow("Mazda", null, "C1", nameof(Car.Model), DisplayName = nameof(Car.Model))]
    [DataRow("Mazda", "MX-5", null, nameof(Car.Id), DisplayName = nameof(Car.Id))]
    public async Task GivenAFieldIsNotProvided_WhenITryToAddACar(
        string make, string model, string id, string fieldName)
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response
            = await client.PostAsJsonAsync("/cars", CreateTestCar(make, model, id));

        ItDoesNotAddACar(response);
        await ItSaysFieldIsRequired(response, fieldName);
    }

    [TestMethod]
    [DataRow("", "MX-5", "C1", nameof(Car.Make), DisplayName = nameof(Car.Make))]
    [DataRow("Mazda", "", "C1", nameof(Car.Model), DisplayName = nameof(Car.Model))]
    [DataRow("Mazda", "MX-5", "", nameof(Car.Id), DisplayName = nameof(Car.Id))]
    public async Task GivenAFieldIsEmpty_WhenITryToAddACar(
        string make, string model, string id, string fieldName)
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response
            = await client.PostAsJsonAsync("/cars", CreateTestCar(make, model, id));

        ItDoesNotAddACar(response);
        await ItSaysFieldIsRequired(response, fieldName);
    }

    [TestMethod]
    public async Task GivenIdFormatIsIncorrect_WhenITryToAddACar()
    {
        HttpClient client = _factory.CreateClient();
        var car = CreateTestCar("Mazda", "MX-5", "1");

        HttpResponseMessage response = await client.PostAsJsonAsync("/cars", car);

        ItDoesNotAddACar(response);
        await ItSaysFieldIsIncorrect(response, nameof(Car.Id));
    }

    [TestMethod]
    public async Task GivenProperData_WhenIAddACar()
    {
        HttpClient client = _factory.CreateClient();
        Car car = CreateTestCar("Mazda", "MX-5", "C1");

        HttpResponseMessage response = await client.PostAsJsonAsync("/cars", car);

        ItAddsACar(response);
        ItShouldShowCarLocation(response, $"/cars/{car.Id}");
        await ItShouldPreserveCarDetails(response, car);
    }

    private async Task ItShouldPreserveCarDetails(HttpResponseMessage response, Car car)
    {
        var responseCar = await response.Content.ReadFromJsonAsync<Car>();
        Assert.IsNotNull(responseCar);
        Assert.AreEqual(car.Id, responseCar.Id);
        Assert.AreEqual(car.Make, responseCar.Make);
        Assert.AreEqual(car.Model, responseCar.Model);
    }

    private void ItShouldShowCarLocation(HttpResponseMessage response, string carLocation) 
        => Assert.AreEqual(carLocation, response.Headers.Location?.ToString());

    private void ItAddsACar(HttpResponseMessage response) 
        => Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

    private static async Task ItSaysFieldIsIncorrect(HttpResponseMessage response, string field) 
        => StringAssert.Contains(
            await response.Content.ReadAsStringAsync(),
            $"The field {field} ");

    private static async Task ItSaysFieldIsRequired(HttpResponseMessage response, string field) 
        => StringAssert.Contains(
            await response.Content.ReadAsStringAsync(),
            $"The {field} field is required.");

    private static void ItDoesNotAddACar(HttpResponseMessage response) 
        => Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

    private static Car CreateTestCar(string? make = null, string? model = null, string? id = null)
    {
        Car car = new();

        if (make != null) car.Make = make;

        if (model != null) car.Model = model;

        if (id != null) car.Id = id;

        return car;
    }
}