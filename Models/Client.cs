using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SuiviLivraison.Models
{
    public class Client
    {
        public int Id { get; set; }
        
        [Required]
        public string Nom { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Telephone { get; set; } = string.Empty;
        
        [Required]
        public string IdentityUserId { get; set; } = string.Empty;
        public IdentityUser? IdentityUser { get; set; }

        public ICollection<Colis> Colis { get; set; } = new List<Colis>();
    }
}
