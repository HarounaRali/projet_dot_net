using System.ComponentModel.DataAnnotations;

namespace SuiviLivraison.Models
{
    public class TicketSupport
    {
        public int Id { get; set; }
        
        [Required]
        public string Titre { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public string Categorie { get; set; } = "Général"; // Général, Technique, Facturation, Livraison
        
        public string Priorite { get; set; } = "Normale"; // Faible, Normale, Élevée, Critique
        
        public string Statut { get; set; } = "Ouvert"; // Ouvert, En cours, Résolu, Fermé
        
        public int? ClientId { get; set; }
        public Client? Client { get; set; }
        
        public int? LivreurId { get; set; }
        public Livreur? Livreur { get; set; }
        
        public string? AssigneA { get; set; } // ID de l'admin assigné
        
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        
        public DateTime? DateResolution { get; set; }
        
        public DateTime? DateFermeture { get; set; }
        
        public ICollection<MessageSupport> Messages { get; set; } = new List<MessageSupport>();
    }
    
    public class MessageSupport
    {
        public int Id { get; set; }
        
        [Required]
        public string Contenu { get; set; } = string.Empty;
        
        public int TicketId { get; set; }
        public TicketSupport? Ticket { get; set; }
        
        public string ExpediteurId { get; set; } = string.Empty;
        
        public string ExpediteurType { get; set; } = "Client"; // Client, Livreur, Admin
        
        public DateTime DateEnvoi { get; set; } = DateTime.UtcNow;
        
        public bool EstInterne { get; set; } = false; // Pour les notes internes
    }
    
    public class FAQ
    {
        public int Id { get; set; }
        
        [Required]
        public string Question { get; set; } = string.Empty;
        
        [Required]
        public string Reponse { get; set; } = string.Empty;
        
        public string Categorie { get; set; } = "Général";
        
        public int OrdreAffichage { get; set; } = 0;
        
        public bool EstActive { get; set; } = true;
        
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        
        public DateTime? DateModification { get; set; }
    }
} 