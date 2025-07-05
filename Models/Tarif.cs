using System.ComponentModel.DataAnnotations;

namespace SuiviLivraison.Models
{
    public class Tarif
    {
        public int Id { get; set; }
        
        [Required]
        public string Nom { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal PrixBase { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal PrixParKm { get; set; } = 0;
        
        [Range(0, double.MaxValue)]
        public decimal PrixUrgence { get; set; } = 0;
        
        public bool EstActif { get; set; } = true;
        
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        
        public DateTime? DateModification { get; set; }
    }
    
    public class Facture
    {
        public int Id { get; set; }
        
        [Required]
        public string NumeroFacture { get; set; } = string.Empty;
        
        public int ClientId { get; set; }
        public Client? Client { get; set; }
        
        public int ColisId { get; set; }
        public Colis? Colis { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal MontantHT { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal TVA { get; set; } = 20; // 20% par défaut
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal MontantTTC { get; set; }
        
        public string Statut { get; set; } = "En attente"; // En attente, Payée, Annulée
        
        public DateTime DateEmission { get; set; } = DateTime.UtcNow;
        
        public DateTime? DatePaiement { get; set; }
        
        public string? MethodePaiement { get; set; }
        
        public string? Notes { get; set; }
    }
} 