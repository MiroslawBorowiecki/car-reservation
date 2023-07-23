using CarReservationApi.Cars.Persistence;
using CarReservationApi.Http;
using CarReservationApi.Reservations;

namespace CarReservationApi.Cars;

public class CarService
{
    private readonly CarRepository _cars;
    private readonly ReservationService _reservationService;

    public CarService(CarRepository cars, ReservationService reservationService)
    {
        _cars = cars;
        _reservationService = reservationService;
    }

    public bool Add(Car car)
    {
        if (_cars.ContainsKey(car.Id))
            return false;

        _cars.Add(car.Id, car);
        return true;
    }

    public List<Car> GetAll() => _cars.Values.ToList();

    public Result Update(string id, string make, string model)
    {
        if (!_cars.TryGetValue(id, out Car? car))
            return new(Status.NotFound);

        if (_reservationService.CarHasUpcomingOrOngoingReservation(id))
            return new(Status.Conflict, Messages.CarReservedError);

        car.Make = make;
        car.Model = model;
        return new(Status.Success);
    }

    public Result Remove(string id)
    {
        if (!_cars.ContainsKey(id)) return new(Status.NotFound);

        if (_reservationService.CarHasUpcomingOrOngoingReservation(id))
            return new(Status.Conflict, Messages.CarReservedError);

        _cars.Remove(id);
        return new(Status.Success);
    }
}
