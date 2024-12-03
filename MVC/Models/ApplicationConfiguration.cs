

namespace MVC.Models
{
    public class ApplicationConfiguration
    {
        public int FontSize { get; set; } = 10;

        public required string FontColor { get; set; } = "blue";

        public required string WelcomePhrase { get; set; } = "Bienvenue sur le merveilleux site !!!";

        public int Sentinel { get; set; } = 0;
    }
}
