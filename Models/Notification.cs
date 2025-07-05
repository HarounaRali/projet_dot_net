using System.ComponentModel.DataAnnotations;

namespace SuiviLivraison.Models
{
    public class Notification
    {
        public int Id { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public DateTime DateEnvoi { get; set; }
        
        public bool EstLue { get; set; } = false;
        
        public string Type { get; set; } = "Info"; // Info, Success, Warning, Error
        
        public string? DestinataireId { get; set; } // Pour les notifications spécifiques
        
        public int? ColisId { get; set; }
        public Colis? Colis { get; set; }
        
        // Propriétés pour les notifications en temps réel
        public string? ActionUrl { get; set; }
        
        public string? Icone { get; set; }
        
        public int Priorite { get; set; } = 1; // 1=Faible, 2=Normale, 3=Élevée
    }
}
