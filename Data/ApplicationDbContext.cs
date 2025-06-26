using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SuiviLivraison.Models; // très important si tes modèles sont dans ce dossier

namespace SuiviLivraison.Data // assure-toi que ça correspond
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Livreur> Livreurs { get; set; }
        public DbSet<Colis> Colis { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration pour les dates avec PostgreSQL
            modelBuilder.Entity<Colis>()
                .Property(c => c.DateEnvoi)
                .HasColumnType("timestamp with time zone");

            // Configuration pour les autres entités avec des dates si nécessaire
            modelBuilder.Entity<Notification>()
                .Property(n => n.DateEnvoi)
                .HasColumnType("timestamp with time zone");
        }
    }
}
