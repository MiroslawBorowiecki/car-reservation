namespace CarReservationApi;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class CarsController : ControllerBase
{
    private readonly CarRepository _cars;

    public CarsController(CarRepository cars)
    {
        _cars = cars;
    }

    [HttpPost]
    public ActionResult<Car> Add(Car car)
    {
        if (_cars.ContainsKey(car.Id))
        {
            return Conflict();
        }

        _cars.Add(car.Id, car);
        return Created($"/cars/{car.Id}", car);
    }

    [HttpGet]
    public ActionResult<IEnumerable<Car>> GetAll() => _cars.Values.ToList();
}
