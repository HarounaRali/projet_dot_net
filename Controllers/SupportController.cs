using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviLivraison.Data;
using SuiviLivraison.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SuiviLivraison.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SupportController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Page d'accueil du support
        public IActionResult Index()
        {
            return View();
        }

        // Liste des FAQ
        public async Task<IActionResult> FAQ()
        {
            var faqs = await _context.FAQs
                .Where(f => f.EstActive)
                .OrderBy(f => f.OrdreAffichage)
                .ThenBy(f => f.Categorie)
                .ToListAsync();

            var faqsParCategorie = faqs.GroupBy(f => f.Categorie).ToList();
            return View(faqsParCategorie);
        }

        // Créer un ticket de support
        [HttpGet]
        public IActionResult CreerTicket()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreerTicket(TicketSupport ticket)
        {
            if (!ModelState.IsValid)
            {
                return View(ticket);
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var user = await _userManager.FindByIdAsync(userId);

            // Déterminer le type d'utilisateur
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            var livreur = await _context.Livreurs.FirstOrDefaultAsync(l => l.IdentityUserId == userId);

            if (client != null)
            {
                ticket.ClientId = client.Id;
            }
            else if (livreur != null)
            {
                ticket.LivreurId = livreur.Id;
            }

            ticket.DateCreation = DateTimeHelper.GetCurrentUtcTime();

            _context.TicketsSupport.Add(ticket);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Votre ticket de support a été créé avec succès. Nous vous répondrons dans les plus brefs délais.";
            return RedirectToAction("MesTickets");
        }

        // Liste des tickets de l'utilisateur
        public async Task<IActionResult> MesTickets()
        {
            var userId = _userManager.GetUserId(User);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            var livreur = await _context.Livreurs.FirstOrDefaultAsync(l => l.IdentityUserId == userId);

            IQueryable<TicketSupport> query;

            if (client != null)
            {
                query = _context.TicketsSupport.Where(t => t.ClientId == client.Id);
            }
            else if (livreur != null)
            {
                query = _context.TicketsSupport.Where(t => t.LivreurId == livreur.Id);
            }
            else
            {
                return NotFound("Utilisateur non trouvé");
            }

            var tickets = await query
                .OrderByDescending(t => t.DateCreation)
                .ToListAsync();

            return View(tickets);
        }

        // Détails d'un ticket
        public async Task<IActionResult> DetailsTicket(int id)
        {
            var userId = _userManager.GetUserId(User);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            var livreur = await _context.Livreurs.FirstOrDefaultAsync(l => l.IdentityUserId == userId);

            var ticket = await _context.TicketsSupport
                .Include(t => t.Messages.OrderBy(m => m.DateEnvoi))
                .Include(t => t.Client)
                .Include(t => t.Livreur)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            // Vérifier que l'utilisateur a accès à ce ticket
            if (client != null && ticket.ClientId != client.Id)
            {
                return Forbid();
            }
            if (livreur != null && ticket.LivreurId != livreur.Id)
            {
                return Forbid();
            }

            return View(ticket);
        }

        // Ajouter un message à un ticket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjouterMessage(int ticketId, string contenu)
        {
            if (string.IsNullOrWhiteSpace(contenu))
            {
                TempData["Error"] = "Le message ne peut pas être vide.";
                return RedirectToAction("DetailsTicket", new { id = ticketId });
            }

            var userId = _userManager.GetUserId(User);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            var livreur = await _context.Livreurs.FirstOrDefaultAsync(l => l.IdentityUserId == userId);

            var ticket = await _context.TicketsSupport.FindAsync(ticketId);
            if (ticket == null)
            {
                return NotFound();
            }

            // Vérifier l'accès
            if (client != null && ticket.ClientId != client.Id)
            {
                return Forbid();
            }
            if (livreur != null && ticket.LivreurId != livreur.Id)
            {
                return Forbid();
            }

            var message = new MessageSupport
            {
                TicketId = ticketId,
                Contenu = contenu,
                ExpediteurId = userId ?? string.Empty,
                ExpediteurType = client != null ? "Client" : "Livreur",
                DateEnvoi = DateTimeHelper.GetCurrentUtcTime()
            };

            _context.MessagesSupport.Add(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message ajouté avec succès.";
            return RedirectToAction("DetailsTicket", new { id = ticketId });
        }

        // Fermer un ticket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FermerTicket(int id)
        {
            var userId = _userManager.GetUserId(User);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            var livreur = await _context.Livreurs.FirstOrDefaultAsync(l => l.IdentityUserId == userId);

            var ticket = await _context.TicketsSupport.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Vérifier l'accès
            if (client != null && ticket.ClientId != client.Id)
            {
                return Forbid();
            }
            if (livreur != null && ticket.LivreurId != livreur.Id)
            {
                return Forbid();
            }

            ticket.Statut = "Fermé";
            ticket.DateFermeture = DateTimeHelper.GetCurrentUtcTime();
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ticket fermé avec succès.";
            return RedirectToAction("MesTickets");
        }
    }
} 