namespace CelestialOrrery.Models;

public class GuessModel
{
    public string Username { get; set; } = string.Empty;  // Default to an empty string if null is not acceptable
    public string UserGuess { get; set; } = string.Empty;
}

