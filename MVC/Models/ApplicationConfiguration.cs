

using System.Security;

namespace MVC.Models
{
    public class ApplicationConfiguration
    {
        public int FontSize { get; set; } = 10;

        public required string FontColor { get; set; } = "blue";

        public required string WelcomePhrase { get; set; } = "Bienvenue sur le merveilleux site !!!";

        // Connection String pour le Blob qui sera acquis du AppConfig
        
        public required string BlobConnectionString { get; set; }

        // Nom du blob pour les images non valider
        public required string UnvalidatedBlob { get; set; }

        // Nom du blob pour les images valider
        public required string ValidatedBlob { get; set; }

        public int Sentinel { get; set; } = 0;
    }
}
