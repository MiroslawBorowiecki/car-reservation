namespace CarReservationApi;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class CarsController : ControllerBase
{
    [HttpPost]
    public ActionResult<Car> Add(Car car)
    {
        return Created($"/cars/{car.Id}", car);
    }
}
