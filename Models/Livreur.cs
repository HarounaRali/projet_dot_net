using System.ComponentModel.DataAnnotations;

namespace SuiviLivraison.Models
{
    public class Livreur
    {
        public int Id { get; set; }
        
        [Required]
        public string Nom { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Telephone { get; set; } = string.Empty;
        
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Statut { get; set; } = "Disponible";
        
        // Relation avec Identity
        [Required]
        public string IdentityUserId { get; set; } = string.Empty;
        public Microsoft.AspNetCore.Identity.IdentityUser? IdentityUser { get; set; }
        
        // Relations
        public ICollection<Colis> ColisLivres { get; set; } = new List<Colis>();
    }
}
