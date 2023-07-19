﻿using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CarReservationApi.Tests;

[TestClass]
public class CarTests
{
    private const string BaseUri = "/cars";
    private static readonly Car MazdaMx5 = CreateTestCar("Mazda", "MX-5", "C1");
    private static readonly Car OpelAstra = CreateTestCar("Opel", "Astra", "C2");
    private static readonly Car Peugeout206 = CreateTestCar("Peugeout", "206", "C3");
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

    [TestMethod]
    public async Task GivenACarWithProvidedIdAlreadyExists_WhenITryToAddACar()
    {
        HttpClient client = _factory.CreateClient();
        await client.PostAsJsonAsync(BaseUri, MazdaMx5);

        var idConflictCar = CreateTestCar("make", "model", MazdaMx5.Id);
        HttpResponseMessage response = await client.PostAsJsonAsync(BaseUri, idConflictCar);

        ItDoesNotAddACar(response, HttpStatusCode.Conflict);
        await ItPreservesTheExistingCar(client, MazdaMx5);
    }

    [TestMethod]
    public async Task GivenSomeCars_WhenIGetThem()
    {
        HttpClient client = _factory.CreateClient();
        await client.PostAsJsonAsync($"{BaseUri}", MazdaMx5);
        await client.PostAsJsonAsync($"{BaseUri}", OpelAstra);
        await client.PostAsJsonAsync($"{BaseUri}", Peugeout206);

        HttpResponseMessage response = await client.GetAsync(BaseUri);

        ItShouldSayOK(response);
        await ItShouldReturnAllCars(response, new[] { MazdaMx5, OpelAstra, Peugeout206 });
    }

    [TestMethod]
    public async Task GivenNoCarsAddedYet_WhenITryToGetThem()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(BaseUri);

        ItShouldSayOK(response);
        await ItShouldReturnNoCars(response);
    }

    private static async Task ItPreservesTheExistingCar(HttpClient client, Car mazdaMx5)
    {
        var existingCar = (await client.GetFromJsonAsync<IEnumerable<Car>>(BaseUri))?
            .SingleOrDefault();
        Assert.IsNotNull(existingCar);
        Assert.AreEqual(mazdaMx5, existingCar, new CarComparer());
    }

    private static async Task ItShouldReturnNoCars(HttpResponseMessage response)
    {
        var results = await response.Content.ReadFromJsonAsync<IEnumerable<Car>>();
        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count());
    }

    private static async Task ItShouldReturnAllCars(HttpResponseMessage response, Car[] cars)
    {
        var temp = await response.Content.ReadFromJsonAsync<IEnumerable<Car>>();
        Assert.IsNotNull(temp);
        var comparer = new CarComparer();
        HashSet<Car> retrievedCars = new(temp, comparer);
        HashSet<Car> expectedCars = new(cars, comparer);
        Assert.IsTrue(expectedCars.SetEquals(retrievedCars));
    }

    private static void ItShouldSayOK(HttpResponseMessage response) 
        => Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

    private static async Task ItShouldPreserveCarDetails(HttpResponseMessage response, Car car)
    {
        var responseCar = await response.Content.ReadFromJsonAsync<Car>();
        Assert.IsNotNull(responseCar);
        Assert.AreEqual(car.Id, responseCar.Id);
        Assert.AreEqual(car.Make, responseCar.Make);
        Assert.AreEqual(car.Model, responseCar.Model);
    }

    private void ItShouldShowCarLocation(HttpResponseMessage response, string carLocation) 
        => Assert.AreEqual(carLocation, response.Headers.Location?.ToString());

    private static void ItAddsACar(HttpResponseMessage response) 
        => Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

    private static async Task ItSaysFieldIsIncorrect(HttpResponseMessage response, string field) 
        => StringAssert.Contains(
            await response.Content.ReadAsStringAsync(),
            $"The field {field} ");

    private static async Task ItSaysFieldIsRequired(HttpResponseMessage response, string field) 
        => StringAssert.Contains(
            await response.Content.ReadAsStringAsync(),
            $"The {field} field is required.");

    private static void ItDoesNotAddACar(
        HttpResponseMessage response, HttpStatusCode expectedCode = HttpStatusCode.BadRequest) 
        => Assert.AreEqual(expectedCode, response.StatusCode);

    private static Car CreateTestCar(string? make = null, string? model = null, string? id = null)
    {
        Car car = new();

        if (make != null) car.Make = make;

        if (model != null) car.Model = model;

        if (id != null) car.Id = id;

        return car;
    }

    private class CarComparer : EqualityComparer<Car>
    {
        public override bool Equals(Car? x, Car? y)
        {
            if (x == null && y == null) return true;

            if (x == null || y == null) return false;            

            return x.Id == y.Id && x.Make == y.Make && x.Model == y.Model;
        }

        public override int GetHashCode([DisallowNull] Car obj)
        {
            return (obj.Id + obj.Make + obj.Model).GetHashCode();
        }
    }
}