using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CelestialOrrery.Models;

namespace CelestialOrrery.Hubs
{
    public class GameHub : Hub
    {
        // Thread-safe dictionary to track active game sessions
        private static ConcurrentDictionary<string, GameSession> sessions = new ConcurrentDictionary<string, GameSession>();
        private static HashSet<string> playersGuessed = new HashSet<string>();

        // Method to handle a player joining the game
        public async Task JoinGame(string username)
        {
            var connectionId = Context.ConnectionId;

            // Create a new session if fewer than two players are connected
            if (sessions.Count < 2)
            {
                if (!sessions.ContainsKey(connectionId))
                {
                    // Create a new game session for the player
                    var session = new GameSession
                    {
                        SessionId = connectionId,  // Using connectionId temporarily for the session
                        Scores = new Dictionary<string, int> { { username, 0 } }
                    };

                    sessions.TryAdd(connectionId, session);
                    await Groups.AddToGroupAsync(connectionId, "GameRoom");

                    // Notify other clients that a player has joined
                    await Clients.Group("GameRoom").SendAsync("PlayerJoined", username);
                }

                // Start the game session once two players have joined
                if (sessions.Count == 2)
                {
                    await Clients.Group("GameRoom").SendAsync("StartGame", "The game is starting now."); 

                    foreach (var session in sessions)
                    {
                        session.Value.IsSessionActive = true;
                    }
                }
            }
        }

        // Method to handle a player making a guess
        public async Task MakeGuess(string username, string guess)
        {
            var connectionId = Context.ConnectionId;

            if (sessions.TryGetValue(connectionId, out var session) && session.IsSessionActive)
            {
                // Update the player's score based on their guess (simple logic)
                session.Scores[username]++;  // Increment score as an example

                // Mark the player as having guessed this round
                playersGuessed.Add(connectionId);

                await Clients.Group("GameRoom").SendAsync("UpdateScores", session.Scores);

                // Check if all players have guessed for the current round
                if (AllPlayersGuessed())
                {
                    await Clients.Group("GameRoom").SendAsync("RoundComplete", "Proceeding to next round.");
                    ResetRoundGuesses();
                }
            }
        }

        // Helper method to check if all players have made their guess for the current round
        private bool AllPlayersGuessed()
        {
            return playersGuessed.Count == sessions.Count;
        }

        // Helper method to reset guesses for the next round
        private void ResetRoundGuesses()
        {
            playersGuessed.Clear();
        }

        // Handle disconnections (optional but useful in multiplayer games)
        public override async Task OnDisconnectedAsync(Exception exception)
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
