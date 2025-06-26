using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviLivraison.Data;
using SuiviLivraison.Models;
using SuiviLivraison.Services;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Livreur")]
public class LivreurController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IColisAssignmentService _assignmentService;

    public LivreurController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IColisAssignmentService assignmentService)
    {
        _context = context;
        _userManager = userManager;
        _assignmentService = assignmentService;
    }

    public async Task<IActionResult> MesColisALivrer()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var livreur = await _context.Livreurs.FirstOrDefaultAsync(l => l.IdentityUserId == userId);
        if (livreur == null)
        {
            return NotFound("Livreur non trouvé.");
        }

        var colis = await _context.Colis
            .Include(c => c.Client)
            .Where(c => c.LivreurId == livreur.Id)
            .OrderByDescending(c => c.DateEnvoi)
            .ToListAsync();
        return View(colis);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accepter(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var livreur = await _context.Livreurs.FirstOrDefaultAsync(l => l.IdentityUserId == userId);
        if (livreur == null)
        {
            return NotFound("Livreur non trouvé.");
        }

        var colis = await _context.Colis.FirstOrDefaultAsync(c => c.Id == id && c.LivreurId == livreur.Id);
        if (colis == null)
        {
            return NotFound("Colis non trouvé ou non assigné.");
        }

        if (colis.Statut == "Assigné")
        {
            colis.Statut = "En cours";
            
            // Créer une notification pour le client avec l'heure d'arrivée
            var heureArrivee = colis.HeureLivraisonEstimee?.ToString("HH:mm") ?? "à déterminer";
            var notification = new Notification
            {
                ColisId = colis.Id,
                Message = $"🎉 Votre colis '{colis.Description}' est en route ! Le livreur {livreur.Nom} arrive vers {heureArrivee}.",
                DateEnvoi = DateTimeHelper.GetCurrentUtcTime()
            };
            
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("MesColisALivrer");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rejeter(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var livreur = await _context.Livreurs.FirstOrDefaultAsync(l => l.IdentityUserId == userId);
        if (livreur == null)
        {
            return NotFound("Livreur non trouvé.");
        }

        var colis = await _context.Colis.FirstOrDefaultAsync(c => c.Id == id && c.LivreurId == livreur.Id);
        if (colis == null)
        {
            return NotFound("Colis non trouvé ou non assigné.");
        }

        if (colis.Statut == "Assigné")
        {
            colis.LivreurId = null;
            colis.Statut = "En attente";
            
            // Créer une notification pour le client
            var notification = new Notification
            {
                ColisId = colis.Id,
                Message = $"Le livreur {livreur.Nom} a rejeté votre colis '{colis.Description}'. Il sera réassigné à un autre livreur.",
                DateEnvoi = DateTimeHelper.GetCurrentUtcTime()
            };
            
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            
            // Réassigner le colis à un autre livreur
            await _assignmentService.AssignColisToNearestLivreurAsync(colis);
        }

        return RedirectToAction("MesColisALivrer");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var livreur = await _context.Livreurs.FirstOrDefaultAsync(l => l.IdentityUserId == userId);
        if (livreur == null)
        {
            return NotFound("Livreur non trouvé.");
        }

        var colis = await _context.Colis.FirstOrDefaultAsync(c => c.Id == id && c.LivreurId == livreur.Id);
        if (colis == null)
        {
            return NotFound("Colis non trouvé ou non assigné.");
        }
        return View(colis);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Colis updatedColis)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var livreur = await _context.Livreurs.FirstOrDefaultAsync(l => l.IdentityUserId == userId);
        if (livreur == null)
        {
            return NotFound("Livreur non trouvé.");
        }

        var colis = await _context.Colis.FirstOrDefaultAsync(c => c.Id == id && c.LivreurId == livreur.Id);
        if (colis == null)
        {
            return NotFound("Colis non trouvé ou non assigné.");
        }
        colis.Statut = updatedColis.Statut;
        colis.Latitude = updatedColis.Latitude;
        colis.Longitude = updatedColis.Longitude;
        await _context.SaveChangesAsync();
        return RedirectToAction("MesColisALivrer");
    }

    public async Task<IActionResult> ColisDisponibles()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var livreur = await _context.Livreurs.FirstOrDefaultAsync(l => l.IdentityUserId == userId);
        if (livreur == null)
        {
            return NotFound("Livreur non trouvé.");
        }

        var colisDisponibles = await _context.Colis
            .Include(c => c.Client)
            .Where(c => c.LivreurId == null && c.Statut == "En attente")
            .OrderBy(c => c.DateEnvoi)
            .ToListAsync();

        // Calculer la distance pour chaque colis
        var colisAvecDistance = colisDisponibles.Select(c => new
        {
            Colis = c,
            Distance = _assignmentService.CalculateDistance(livreur.Latitude, livreur.Longitude, c.Latitude, c.Longitude)
        })
        .OrderBy(x => x.Distance)
        .Select(x => x.Colis)
        .ToList();

        return View(colisAvecDistance);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DemanderColis(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var livreur = await _context.Livreurs.FirstOrDefaultAsync(l => l.IdentityUserId == userId);
        if (livreur == null)
        {
            return NotFound("Livreur non trouvé.");
        }

        var colis = await _context.Colis.FirstOrDefaultAsync(c => c.Id == id && c.LivreurId == null);
        if (colis == null)
        {
            return NotFound("Colis non disponible.");
        }

        colis.LivreurId = livreur.Id;
        colis.Statut = "Assigné";
        
        // Créer une notification pour le client
        var notification = new Notification
        {
            ColisId = colis.Id,
            Message = $"Le livreur {livreur.Nom} a demandé votre colis '{colis.Description}'",
            DateEnvoi = DateTimeHelper.GetCurrentUtcTime()
        };
        
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return RedirectToAction("MesColisALivrer");
    }
} 