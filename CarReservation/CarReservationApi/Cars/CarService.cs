using CarReservationApi.Cars.Persistence;

namespace CarReservationApi.Cars;

public class CarService
{
    private readonly CarRepository _cars;

    public CarService(CarRepository _cars)
    {
        this._cars = _cars;
    }

    public bool Add(Car car)
    {
        if (_cars.ContainsKey(car.Id))
            return false;

        _cars.Add(car.Id, car);
        return true;
    }

    public List<Car> GetAll() => _cars.Values.ToList();

    public bool Update(string id, string make, string model)
    {
        if (!_cars.TryGetValue(id, out Car? car))
            return false;

        car.Make = make;
        car.Model = model;
        return true;
    }

    public bool Remove(string id) => _cars.Remove(id);
}
