namespace CelestialOrrery.Models
{
    public class GameSession
    {
    public string SessionId { get; set; } = Guid.NewGuid().ToString(); // Ensures every session has a unique ID
            public string GameName { get; set; }  // Add this line
        public Dictionary<string, int> Scores { get; set; }  // Stores scores for each player
        public int RoundNumber { get; set; }  // Current round number
        public bool IsSessionActive { get; set; }  // Indicates if the session is currently active

        public GameSession()
        {
            Scores = new Dictionary<string, int>();
            RoundNumber = 1;
            IsSessionActive = true;
        }
    }
}
