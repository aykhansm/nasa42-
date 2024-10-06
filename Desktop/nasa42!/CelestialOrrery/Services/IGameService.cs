using System.Collections.Generic;
using System.Threading.Tasks;
using CelestialOrrery.Models;

namespace CelestialOrrery.Services.Interfaces
{
    public interface IGameService
    {
        Task<GameSession> MakeGuess(string username, string guess);
        Task<string> JoinGameAsync(string username);
                Task<string> GetGameStatusAsync();
        Task<Dictionary<string, double>> GetRoundScoresAsync();
        Task<IEnumerable<string>> GetUsernamesAsync();
    }
}
