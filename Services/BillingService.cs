using Microsoft.EntityFrameworkCore;
using SuiviLivraison.Data;
using SuiviLivraison.Models;
using SuiviLivraison.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SuiviLivraison.Services
{
    public interface IBillingService
    {
        Task<decimal> CalculerTarifAsync(int colisId, bool urgence = false);
        Task<Facture> GenererFactureAsync(int colisId, decimal montantHT);
        Task<List<Facture>> GetFacturesClientAsync(int clientId);
        Task<Facture?> GetFactureAsync(int factureId);
        Task MarquerFacturePayeeAsync(int factureId, string methodePaiement);
        Task<List<Tarif>> GetTarifsActifsAsync();
        Task<Tarif> CreerTarifAsync(Tarif tarif);
        Task<Tarif?> GetTarifAsync(int tarifId);
        Task MettreAJourTarifAsync(Tarif tarif);
        Task SupprimerTarifAsync(int tarifId);
    }

    public class BillingService : IBillingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IColisAssignmentService _assignmentService;

        public BillingService(ApplicationDbContext context, IColisAssignmentService assignmentService)
        {
            _context = context;
            _assignmentService = assignmentService;
        }

        public async Task<decimal> CalculerTarifAsync(int colisId, bool urgence = false)
        {
            var colis = await _context.Colis
                .Include(c => c.Livreur)
                .FirstOrDefaultAsync(c => c.Id == colisId);

            if (colis == null)
                return 0;

            // Récupérer le tarif actif
            var tarif = await _context.Tarifs
                .Where(t => t.EstActif)
                .OrderByDescending(t => t.DateCreation)
                .FirstOrDefaultAsync();

            if (tarif == null)
                return 0;

            decimal montant = tarif.PrixBase;

            // Calculer la distance si on a les coordonnées
            if (colis.Livreur != null && colis.Latitude != 0 && colis.Longitude != 0)
            {
                var distance = _assignmentService.CalculateDistance(
                    colis.Livreur.Latitude, colis.Livreur.Longitude,
                    colis.Latitude, colis.Longitude);

                montant += (decimal)distance * tarif.PrixParKm;
            }

            // Ajouter le supplément d'urgence
            if (urgence)
            {
                montant += tarif.PrixUrgence;
            }

            return Math.Round(montant, 2);
        }

        public async Task<Facture> GenererFactureAsync(int colisId, decimal montantHT)
        {
            var colis = await _context.Colis
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == colisId);

            if (colis == null)
                throw new ArgumentException("Colis non trouvé");

            var numeroFacture = $"FACT-{DateTime.UtcNow:yyyyMMdd}-{colisId:D6}";
            var tva = 20m; // 20% de TVA
            var montantTTC = montantHT * (1 + tva / 100);

            var facture = new Facture
            {
                NumeroFacture = numeroFacture,
                ClientId = colis.ClientId,
                ColisId = colisId,
                MontantHT = montantHT,
                TVA = tva,
                MontantTTC = Math.Round(montantTTC, 2),
                Statut = "En attente",
                DateEmission = DateTimeHelper.GetCurrentUtcTime()
            };

            _context.Factures.Add(facture);
            await _context.SaveChangesAsync();

            return facture;
        }

        public async Task<List<Facture>> GetFacturesClientAsync(int clientId)
        {
            return await _context.Factures
                .Include(f => f.Colis)
                .Where(f => f.ClientId == clientId)
                .OrderByDescending(f => f.DateEmission)
                .ToListAsync();
        }

        public async Task<Facture?> GetFactureAsync(int factureId)
        {
            return await _context.Factures
                .Include(f => f.Client)
                .Include(f => f.Colis)
                .FirstOrDefaultAsync(f => f.Id == factureId);
        }

        public async Task MarquerFacturePayeeAsync(int factureId, string methodePaiement)
        {
            var facture = await _context.Factures.FindAsync(factureId);
            if (facture != null)
            {
                facture.Statut = "Payée";
                facture.DatePaiement = DateTimeHelper.GetCurrentUtcTime();
                facture.MethodePaiement = methodePaiement;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Tarif>> GetTarifsActifsAsync()
        {
            return await _context.Tarifs
                .Where(t => t.EstActif)
                .OrderByDescending(t => t.DateCreation)
                .ToListAsync();
        }

        public async Task<Tarif> CreerTarifAsync(Tarif tarif)
        {
            tarif.DateCreation = DateTimeHelper.GetCurrentUtcTime();
            _context.Tarifs.Add(tarif);
            await _context.SaveChangesAsync();
            return tarif;
        }

        public async Task<Tarif?> GetTarifAsync(int tarifId)
        {
            return await _context.Tarifs.FindAsync(tarifId);
        }

        public async Task MettreAJourTarifAsync(Tarif tarif)
        {
            tarif.DateModification = DateTimeHelper.GetCurrentUtcTime();
            _context.Tarifs.Update(tarif);
            await _context.SaveChangesAsync();
        }

        public async Task SupprimerTarifAsync(int tarifId)
        {
            var tarif = await _context.Tarifs.FindAsync(tarifId);
            if (tarif != null)
            {
                _context.Tarifs.Remove(tarif);
                await _context.SaveChangesAsync();
            }
        }
    }
} 