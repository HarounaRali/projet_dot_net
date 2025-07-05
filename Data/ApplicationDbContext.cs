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
        public DbSet<Tarif> Tarifs { get; set; }
        public DbSet<Facture> Factures { get; set; }
        public DbSet<TicketSupport> TicketsSupport { get; set; }
        public DbSet<MessageSupport> MessagesSupport { get; set; }
        public DbSet<FAQ> FAQs { get; set; }

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

            // Configuration pour les notifications
            modelBuilder.Entity<Notification>()
                .Property(n => n.EstLue)
                .HasDefaultValue(false);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Type)
                .HasDefaultValue("Info");

            modelBuilder.Entity<Notification>()
                .Property(n => n.Priorite)
                .HasDefaultValue(1);

            // Configuration pour les tarifs
            modelBuilder.Entity<Tarif>()
                .Property(t => t.EstActif)
                .HasDefaultValue(true);

            modelBuilder.Entity<Tarif>()
                .Property(t => t.DateCreation)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            // Configuration pour les factures
            modelBuilder.Entity<Facture>()
                .Property(f => f.TVA)
                .HasDefaultValue(20);

            modelBuilder.Entity<Facture>()
                .Property(f => f.Statut)
                .HasDefaultValue("En attente");

            modelBuilder.Entity<Facture>()
                .Property(f => f.DateEmission)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            // Configuration pour les tickets de support
            modelBuilder.Entity<TicketSupport>()
                .Property(t => t.Statut)
                .HasDefaultValue("Ouvert");

            modelBuilder.Entity<TicketSupport>()
                .Property(t => t.Priorite)
                .HasDefaultValue("Normale");

            modelBuilder.Entity<TicketSupport>()
                .Property(t => t.DateCreation)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            // Configuration pour les messages de support
            modelBuilder.Entity<MessageSupport>()
                .Property(m => m.ExpediteurType)
                .HasDefaultValue("Client");

            modelBuilder.Entity<MessageSupport>()
                .Property(m => m.EstInterne)
                .HasDefaultValue(false);

            modelBuilder.Entity<MessageSupport>()
                .Property(m => m.DateEnvoi)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            // Configuration pour les FAQ
            modelBuilder.Entity<FAQ>()
                .Property(f => f.EstActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<FAQ>()
                .Property(f => f.DateCreation)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        }
    }
}
