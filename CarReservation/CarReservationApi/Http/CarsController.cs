using CarReservationApi.Cars;
using Microsoft.AspNetCore.Mvc;

namespace CarReservationApi.Http;

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
    public ActionResult Update([FromRoute] string id, CarUpdateRequest updateCarRequest)
    {
        Result result = _carService.Update(id, updateCarRequest.Make, updateCarRequest.Model);
        return result.Status switch
        {
            Status.Success => NoContent(),
            Status.NotFound => NotFound(),
            Status.Conflict => Conflict(result.Message),
            _ => throw new InvalidOperationException()
        };
    }        

    [HttpDelete]
    [Route("{id}")]
    public ActionResult Remove([FromRoute] string id)
    {
        Result result = _carService.Remove(id);
        return result.Status switch
        {
            Status.Success => NoContent(),
            Status.NotFound => NotFound(),
            Status.Conflict => Conflict(result.Message),
            _ => throw new InvalidOperationException()
        };
    } 
}
