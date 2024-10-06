using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using CelestialOrrery.Services;
using CelestialOrrery.Services.Interfaces;
using CelestialOrrery.Models;

namespace CelestialOrrery.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet("roundscores")]
    public async Task<ActionResult<Dictionary<string, double>>> GetRoundScores()
    {
        var scores = await _gameService.GetRoundScoresAsync();
        return Ok(scores);
    }

        [HttpPost("join")]
        public async Task<ActionResult<string>> JoinGame([FromBody] string username)
        {
            var result = await _gameService.JoinGameAsync(username);
            return Ok(result);
        }

        [HttpPost("guess")]
        public async Task<ActionResult<GameSession>> MakeGuess([FromBody] GuessModel guess)
        {
            var session = await _gameService.MakeGuess(guess.Username, guess.UserGuess);
            return Ok(session);
        }

        [HttpGet("usernames")]
        public async Task<ActionResult<IEnumerable<string>>> GetUsernames()
        {
            var usernames = await _gameService.GetUsernamesAsync();
            return Ok(usernames);
        }

        [HttpGet("status")]
        public async Task<ActionResult<string>> GetGameStatus()
        {
            var status = await _gameService.GetGameStatusAsync();
            return Ok(new { status });
        }
    }

    public class GuessModel
    {
        public string Username { get; set; }
        public string UserGuess { get; set; }
    }
}
