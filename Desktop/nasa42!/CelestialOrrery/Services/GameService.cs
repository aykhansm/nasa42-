using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CelestialOrrery.Models;
using CelestialOrrery.Services.Interfaces;

namespace CelestialOrrery.Services
{
    public class GameService : IGameService
    {
        private static ConcurrentDictionary<string, GameSession> _sessions = new ConcurrentDictionary<string, GameSession>();
        private static Dictionary<int, string> _correctAnswers = new Dictionary<int, string>
        {
            {1, "Mercury"}, {2, "Venus"}, {3, "Earth"}, {4, "Mars"}, {5, "Jupiter"}
        };

        public GameService()
        {
            // Initialize a session with Alice and Bob
            var session = new GameSession
            {
                SessionId = Guid.NewGuid().ToString(),
                IsSessionActive = true,
                Scores = new Dictionary<string, double> { {"Alice", 0}, {"Bob", 0} },
                RoundNumber = 1,
                PlayersGuessed = new HashSet<string>()
            };
            _sessions.TryAdd(session.SessionId, session);
        }

        public async Task<GameSession> MakeGuess(string username, string guess)
        {
            var session = _sessions.Values.FirstOrDefault(s => s.Scores.ContainsKey(username) && s.IsSessionActive);
            if (session != null && !session.PlayersGuessed.Contains(username))
            {
                session.PlayersGuessed.Add(username);
                var correctAnswer = _correctAnswers[session.RoundNumber];
                if (guess.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    session.Scores[username]++;
                }

                if (AllPlayersGuessed(session.SessionId))
                {
                    AdjustScoresForTie(session);
                    if (session.RoundNumber >= _correctAnswers.Count)
                    {
                        session.IsSessionActive = false;
                    }
                    else
                    {
                        session.RoundNumber++;
                        session.PlayersGuessed.Clear();  // Reset guesses for the new round
                    }
                }

                await Task.CompletedTask;
            }
            return session;
        }

        private void AdjustScoresForTie(GameSession session)
        {
            var scores = session.Scores.Values.ToList();
            if (scores.Count == 2 && scores[0] == scores[1]) // Check if scores are tied
            {
                foreach (var key in session.Scores.Keys.ToList())
                {
                    session.Scores[key] += (int)0.5;  // Increment each score by 0.5 in case of a tie
                }
            }
        }

        private bool AllPlayersGuessed(string sessionId)
        {
            var session = _sessions[sessionId];
            return session.PlayersGuessed.Count == session.Scores.Count;
        }

        public async Task<IEnumerable<string>> GetUsernamesAsync()
        {
            var usernames = _sessions.Values.SelectMany(session => session.Scores.Keys).Distinct().ToList();
            return await Task.FromResult(usernames);
        }

        public async Task<Dictionary<string, double>> GetRoundScoresAsync()
        {
            var scores = new Dictionary<string, double>();
            foreach (var session in _sessions.Values.Where(s => s.IsSessionActive))
            {
                foreach (var score in session.Scores)
                {
                    if (!scores.ContainsKey(score.Key))
                        scores[score.Key] = score.Value;
                }
            }
            return await Task.FromResult(scores);
        }

        public async Task<string> GetGameStatusAsync()
        {
            bool isActive = _sessions.Values.Any(session => session.IsSessionActive);
            return await Task.FromResult(isActive ? "Active" : "Inactive");
        }

        public async Task<string> JoinGameAsync(string username)
{
    // Check if the user is already in a session
    var session = _sessions.Values.FirstOrDefault(s => s.Scores.ContainsKey(username));
    if (session != null)
    {
        return await Task.FromResult($"Player {username} is already in a session.");
    }

    // Create a new session for this user if they are not already in one
    session = _sessions.Values.FirstOrDefault(s => s.Scores.Count < 2);  // Find an existing session with space
    if (session == null)
    {
        session = new GameSession
        {
            SessionId = Guid.NewGuid().ToString(),
            Scores = new Dictionary<string, double> { { username, 0 } },
            PlayersGuessed = new HashSet<string>()
        };
        _sessions.TryAdd(session.SessionId, session);
        return await Task.FromResult($"New session created with ID: {session.SessionId} for player {username}");
    }
    else
    {
        session.Scores.Add(username, 0);
        return await Task.FromResult($"Player {username} joined session with ID: {session.SessionId}");
    }
}

}
}