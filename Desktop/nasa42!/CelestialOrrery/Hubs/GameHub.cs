using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CelestialOrrery.Models;

namespace CelestialOrrery.Hubs
{
    public class GameHub : Hub
    {
        private static ConcurrentDictionary<string, GameSession> sessions = new ConcurrentDictionary<string, GameSession>();
        private static HashSet<string> playersGuessed = new HashSet<string>();

        public async Task JoinGame(string username)
        {
            var connectionId = Context.ConnectionId;
            if (sessions.Count < 2)
            {
                if (!sessions.ContainsKey(connectionId))
                {
                    var session = new GameSession
                    {
                        SessionId = connectionId,
                        Scores = new Dictionary<string, double> { { username, 0 } }
                    };
                    sessions.TryAdd(connectionId, session);
                    await Groups.AddToGroupAsync(connectionId, "GameRoom");
                    await Clients.Group("GameRoom").SendAsync("PlayerJoined", username);
                }
                if (sessions.Count == 2)
                {
                    foreach (var session in sessions)
                    {
                        session.Value.IsSessionActive = true;
                        session.Value.RoundNumber = 1;  // Initialize round number
                    }
                    await Clients.Group("GameRoom").SendAsync("StartGame", "The game is starting now.");
                }
            }
        }

        public async Task MakeGuess(string username, string guess)
        {
            var connectionId = Context.ConnectionId;
            if (sessions.TryGetValue(connectionId, out var session) && session.IsSessionActive)
            {
                bool isCorrect = ValidateGuess(guess, session);  // Validate the guess
                if (isCorrect)
                {
                    session.Scores[username]++;
                }
                playersGuessed.Add(connectionId);
                await Clients.Group("GameRoom").SendAsync("UpdateScores", session.Scores);

                if (AllPlayersGuessed())
                {
                    if (session.RoundNumber >= 5)
                    {
                        session.IsSessionActive = false;
                        await Clients.Group("GameRoom").SendAsync("EndGame", "Game over!");
                        sessions.Clear();  // Reset sessions for a new game
                    }
                    else
                    {
                        session.RoundNumber++;
                        await Clients.Group("GameRoom").SendAsync("RoundComplete", $"Proceeding to round {session.RoundNumber}.");
                        ResetRoundGuesses();
                    }
                }
            }
        }

        private bool ValidateGuess(string guess, GameSession session)
        {
            // Logic to validate the guess against the correct answer for the current round
            return true;  // Simplified example
        }

        private bool AllPlayersGuessed()
        {
            return playersGuessed.Count == sessions.Count;
        }

        private void ResetRoundGuesses()
        {
            playersGuessed.Clear();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            if (sessions.TryRemove(connectionId, out var session))
            {
                await Groups.RemoveFromGroupAsync(connectionId, "GameRoom");
                await Clients.Group("GameRoom").SendAsync("PlayerDisconnected", session.SessionId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
