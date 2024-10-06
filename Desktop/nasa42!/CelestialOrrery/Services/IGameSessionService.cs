using CelestialOrrery.Models;

namespace CelestialOrrery.Services
{
    public interface IGameSessionService
    {
        string CreateGameSession(string gameName);
        GameSession GetSessionById(string sessionId);
    }
}
