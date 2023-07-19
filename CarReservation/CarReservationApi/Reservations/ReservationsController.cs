using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarReservationApi.Reservations
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationsController : ControllerBase
    {
        [HttpPost]
        public ActionResult ReserveCar(ReserveCarRequest request)
        {
            return Problem("not implemented yet");
        }
    }
}
