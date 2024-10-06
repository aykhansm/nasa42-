using Microsoft.AspNetCore.Mvc;
using CelestialOrrery.Services;  // Assuming you have a session service
using System.Threading.Tasks;
using CelestialOrrery.Services;  


namespace CelestialOrrery.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameSessionService _gameSessionService;

        public GameController(IGameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }

        // A simple GET endpoint for Swagger to detect
        [HttpGet("status")]
        public IActionResult GetGameStatus()
        {
            return Ok(new { status = "Game is running" });
        }

        // A POST endpoint to create a new game session
        [HttpPost("create")]
        public IActionResult CreateGame([FromBody] string gameName)
        {
            var sessionId = _gameSessionService.CreateGameSession(gameName);
            return Ok(new { message = $"Game '{gameName}' created successfully.", sessionId });
        }

        // A GET endpoint to fetch session info by sessionId
        [HttpGet("session/{sessionId}")]
        public IActionResult GetSession(string sessionId)
        {
            var session = _gameSessionService.GetSessionById(sessionId);

            if (session != null)
            {
                return Ok(session);
            }
            return NotFound(new { message = "Session not found" });
        }
    }
}
