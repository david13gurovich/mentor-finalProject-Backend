using Domain.In;
using Domain.Out;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Services;

namespace API.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlgoController : ControllerBase {
        private readonly AlgoService _service;

        public AlgoController(AlgoService service) {
            _service = service;
        }

        // Post: api/Algo
        // returns list of contacts
        [HttpPost]
        public async Task<ActionResult<UiComplete>> NewSchedule(JointMissions missionListSetting)
        {
            UiComplete? missions = await _service.CalculateSchedule(HttpContext.User.Claims.First().Value, missionListSetting);
            if (missions == null)
            {
                return NotFound();
            }
            return missions;
        }
    }
}
