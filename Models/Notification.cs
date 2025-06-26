using System.ComponentModel.DataAnnotations;

namespace SuiviLivraison.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int ColisId { get; set; }
        public Colis? Colis { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;
        public DateTime DateEnvoi { get; set; }
    }
}
