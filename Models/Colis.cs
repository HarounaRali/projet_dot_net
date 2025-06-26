using System.ComponentModel.DataAnnotations;

namespace SuiviLivraison.Models
{
    public class Colis
    {
        public int Id { get; set; }
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public string Statut { get; set; } = "En attente";
        
        public DateTime? DateEnvoi { get; set; }
        
        public DateTime? HeureLivraisonEstimee { get; set; }
        
        public string? AdresseLivraison { get; set; }

        public int ClientId { get; set; }
        public Client? Client { get; set; }

        public int? LivreurId { get; set; }
        public Livreur? Livreur { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Point de départ
        public double LatitudeDepart { get; set; }
        public double LongitudeDepart { get; set; }
        public string? AdresseDepart { get; set; }

        // Point d'arrivée
        public double LatitudeArrivee { get; set; }
        public double LongitudeArrivee { get; set; }
        public string? AdresseArrivee { get; set; }

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
