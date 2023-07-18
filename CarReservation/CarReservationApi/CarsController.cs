namespace CarReservationApi;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class CarsController : ControllerBase
{
    [HttpPost]
    public ActionResult Add(Car car)
    {
        return BadRequest();
    }
}
