namespace CarReservationApi.Cars;

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

    [HttpPut]
    [Route("{id}")]
    public ActionResult Update([FromRoute] string id, UpdateCarRequest updateCarRequest)
    {
        if (!_cars.ContainsKey(id))
            return NotFound();

        var car = _cars[id];
        car.Make = updateCarRequest.Make;
        car.Model = updateCarRequest.Model;
        return NoContent();
    }

    [HttpDelete]
    [Route("{id}")]
    public ActionResult Remove([FromRoute] string id)
    {
        if (!_cars.ContainsKey(id))
            return NotFound();

        _cars.Remove(id);
        return NoContent();
    }
}
