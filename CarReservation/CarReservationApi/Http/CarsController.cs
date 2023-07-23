using CarReservationApi.Cars;

namespace CarReservationApi.Http;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class CarsController : ControllerBase
{
    private readonly CarService _carService;

    public CarsController(CarService carService)
    {
        _carService = carService;
    }

    [HttpPost]
    public ActionResult<Car> Add(Car car) => _carService.Add(car)
            ? Created($"/cars/{car.Id}", car)
            : Conflict();

    [HttpGet]
    public ActionResult<IEnumerable<Car>> GetAll() => _carService.GetAll();

    // I didn't provide any GET /{id} method, as the specification did not list as required.

    // I have assumed that Update doesn't include car's ID for simplicity.
    // The same can still be achieved by adding new car first and then removing the previous one.
    [HttpPut]
    [Route("{id}")]
    public ActionResult Update([FromRoute] string id, UpdateCarRequest updateCarRequest)
        => _carService.Update(id, updateCarRequest.Make, updateCarRequest.Model)
            ? NoContent()
            : NotFound();

    [HttpDelete]
    [Route("{id}")]
    public ActionResult Remove([FromRoute] string id) => _carService.Remove(id)
        ? NoContent()
        : NotFound();
}
