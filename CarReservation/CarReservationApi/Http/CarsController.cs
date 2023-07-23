using Microsoft.AspNetCore.Mvc;
using CarReservationApi.Cars;

namespace CarReservationApi.Http;

[ApiController]
[Produces("application/json")]
[Route("[controller]")]
public class CarsController : ControllerBase
{
    private readonly CarService _carService;

    public CarsController(CarService carService)
    {
        _carService = carService;
    }

    /// <summary>
    /// Creates a car.
    /// </summary>
    /// <param name="car">Data of the car.</param>
    /// <returns>The created car.</returns>
    /// <response code="201">The newly created car.</response>
    /// <response code="400">Invalid/missing data: Id, Make, or Model.</response>
    /// <response code="409">Car with specified ID exists.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<Car> Add(Car car) => _carService.Add(car)
            ? Created($"/cars/{car.Id}", car)
            : Conflict();

    /// <summary>
    /// Retrieves all existing cars.
    /// </summary>
    /// <returns>All existing cars.</returns>
    /// <response code="200">All existing cars.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<Car>> GetAll() => _carService.GetAll();

    // I didn't provide any GET /{id} method, as the specification did not list as required.

    /// <summary>
    /// Updates specified car's data.
    /// </summary>
    /// <param name="id">Id of the updated car.</param>
    /// <param name="updateCarRequest">New data of the car.</param>
    /// <returns></returns>
    /// <response code="204">Car successfully updated.</response>
    /// <response code="400">Invalid/missing data: Make, or Model.</response>
    /// <response code="404">Car not found.</response>
    /// <response code="409">Car has an onging or upcoming reservation.</response>
    [HttpPut]
    [Route("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Removes the specified car.
    /// </summary>
    /// <param name="id">Id of the car.</param>
    /// <returns></returns>
    /// <response code="204">Car successfully removed.</response>
    /// <response code="404">Car not found.</response>
    /// <response code="409">Car has an onging or upcoming reservation.</response>
    [HttpDelete]
    [Route("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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
