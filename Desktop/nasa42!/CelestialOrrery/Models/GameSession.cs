namespace CelestialOrrery.Models
{
    public class GameSession
    {
     public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string GameName { get; set; }
        public Dictionary<string, double> Scores { get; set; } = new Dictionary<string, double>(); // Changed to double for fractional scores
        public int RoundNumber { get; set; } = 1;
        public bool IsSessionActive { get; set; } = true;
        public HashSet<string> PlayersGuessed { get; set; } = new HashSet<string>(); // Added this property to track guesses
   
        public GameSession()
        {
            Scores = new Dictionary<string, double>();
            RoundNumber = 1;
            IsSessionActive = true;
        }
    }
}
