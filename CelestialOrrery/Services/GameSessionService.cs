using System.Collections.Concurrent;
using CelestialOrrery.Models;

namespace CelestialOrrery.Services
{
    public class GameSessionService : IGameSessionService
    {
        private static readonly ConcurrentDictionary<string, GameSession> sessions = new ConcurrentDictionary<string, GameSession>();

        public string CreateGameSession(string gameName)
        {
            var sessionId = Guid.NewGuid().ToString();
            var newSession = new GameSession
            {
                SessionId = sessionId,
                GameName = gameName,
                Scores = new Dictionary<string, int>()
            };
            sessions.TryAdd(sessionId, newSession);
            return sessionId;
        }

        public GameSession GetSessionById(string sessionId)
        {
            sessions.TryGetValue(sessionId, out var session);
            return session;
        }
    }
}
