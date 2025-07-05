using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviLivraison.Data;
using SuiviLivraison.Models;
using SuiviLivraison.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

[Authorize]
public class ColisController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IColisAssignmentService _assignmentService;
    private readonly IMapService _mapService;

    public ColisController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IColisAssignmentService assignmentService, IMapService mapService)
    {
        _context = context;
        _userManager = userManager;
        _assignmentService = assignmentService;
        _mapService = mapService;
    }

    public async Task<IActionResult> MesColis()
    {
        var userId = _userManager.GetUserId(User);
        var client = await _context.Clients.Include(c => c.Colis).FirstOrDefaultAsync(c => c.IdentityUserId == userId);
        if (client == null)
        {
            return NotFound("Client non trouvé.");
        }
        var colis = client.Colis.ToList();
        return View(colis);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Colis colis, string AdresseDepart, string AdresseArrivee)
    {
        // Géocodage du point de départ si adresse fournie
        if (!string.IsNullOrWhiteSpace(AdresseDepart))
        {
            using (var httpClient = new HttpClient())
            {
                var url = $"https://nominatim.openstreetmap.org/search?format=json&q={System.Net.WebUtility.UrlEncode(AdresseDepart)}&limit=1";
                httpClient.DefaultRequestHeaders.Add("User-Agent", "SuiviLivraison/1.0");
                var response = await httpClient.GetStringAsync(url);
                var results = JsonDocument.Parse(response).RootElement;
                if (results.GetArrayLength() > 0)
                {
                    var result = results[0];
                    if (result.TryGetProperty("lat", out var latProp) && result.TryGetProperty("lon", out var lonProp))
                    {
                        if (double.TryParse(latProp.GetString(), out double lat) && double.TryParse(lonProp.GetString(), out double lon))
                        {
                            colis.LatitudeDepart = lat;
                            colis.LongitudeDepart = lon;
                        }
                    }
                }

            }
        }
        // Géocodage du point d'arrivée si adresse fournie
        if (!string.IsNullOrWhiteSpace(AdresseArrivee))
        {
            using (var httpClient = new HttpClient())
            {
                var url = $"https://nominatim.openstreetmap.org/search?format=json&q={System.Net.WebUtility.UrlEncode(AdresseArrivee)}&limit=1";
                httpClient.DefaultRequestHeaders.Add("User-Agent", "SuiviLivraison/1.0");
                var response = await httpClient.GetStringAsync(url);
                var results = JsonDocument.Parse(response).RootElement;
                if (results.GetArrayLength() > 0)
                {
                    var result = results[0];
                    if (result.TryGetProperty("lat", out var latProp) && result.TryGetProperty("lon", out var lonProp))
                    {
                        if (double.TryParse(latProp.GetString(), out double lat) && double.TryParse(lonProp.GetString(), out double lon))
                        {
                            colis.LatitudeArrivee = lat;
                            colis.LongitudeArrivee = lon;
                        }
                    }
                }

            }
        }




        if (!ModelState.IsValid)
        {
            return View(colis);
        }
        var userId = _userManager.GetUserId(User);
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
        if (client == null)
        {
            return NotFound("Client non trouvé.");
        }
        colis.ClientId = client.Id;
        colis.DateEnvoi = DateTimeHelper.GetCurrentUtcTime();
        colis.Statut = "En attente";
        _context.Colis.Add(colis);
        await _context.SaveChangesAsync();
        await _assignmentService.AssignColisToNearestLivreurAsync(colis);
        return RedirectToAction("MesColis");
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
        if (client == null)
        {
            return NotFound("Client non trouvé.");
        }
        var colis = await _context.Colis.Include(c => c.Notifications)
            .FirstOrDefaultAsync(c => c.Id == id && c.ClientId == client.Id);
        if (colis == null)
        {
            return NotFound("Colis non trouvé ou accès refusé.");
        }
        return View(colis);
    }

    public async Task<IActionResult> Notifications()
    {
        var userId = _userManager.GetUserId(User);
        var client = await _context.Clients.Include(c => c.Colis)
                                           .ThenInclude(colis => colis.Notifications)
                                           .FirstOrDefaultAsync(c => c.IdentityUserId == userId);
        if (client == null)
        {
            return NotFound("Client non trouvé.");
        }
        var notifications = client.Colis.SelectMany(c => c.Notifications).OrderByDescending(n => n.DateEnvoi).ToList();
        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Annuler(int id)
    {
        var userId = _userManager.GetUserId(User);
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
        if (client == null)
        {
            return NotFound("Client non trouvé.");
        }
        var colis = await _context.Colis.FirstOrDefaultAsync(c => c.Id == id && c.ClientId == client.Id);
        if (colis == null)
        {
            return NotFound("Colis non trouvé ou accès refusé.");
        }
        if (colis.Statut == "En attente" || colis.Statut == "En cours")
        {
            colis.Statut = "Annulé";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Le colis a été annulé avec succès.";
        }
        return RedirectToAction("MesColis");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemettreEnCours(int id)
    {
        var userId = _userManager.GetUserId(User);
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
        if (client == null)
        {
            return NotFound("Client non trouvé.");
        }
        var colis = await _context.Colis.FirstOrDefaultAsync(c => c.Id == id && c.ClientId == client.Id);
        if (colis == null)
        {
            return NotFound("Colis non trouvé ou accès refusé.");
        }
        if (colis.Statut == "Annulé")
        {
            colis.Statut = "En attente";
            await _context.SaveChangesAsync();
            
            // Réassigner le colis à un livreur
            await _assignmentService.AssignColisToNearestLivreurAsync(colis);
            
            TempData["Success"] = "Le colis a été remis en cours et sera réassigné à un livreur.";
        }
        return RedirectToAction("MesColis");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Supprimer(int id)
    {
        var userId = _userManager.GetUserId(User);
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
        if (client == null)
        {
            return NotFound("Client non trouvé.");
        }
        var colis = await _context.Colis.FirstOrDefaultAsync(c => c.Id == id && c.ClientId == client.Id);
        if (colis == null)
        {
            return NotFound("Colis non trouvé ou accès refusé.");
        }
        if (colis.Statut == "Annulé")
        {
            // Supprimer les notifications associées
            var notifications = await _context.Notifications.Where(n => n.ColisId == id).ToListAsync();
            _context.Notifications.RemoveRange(notifications);
            
            // Supprimer le colis
            _context.Colis.Remove(colis);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Le colis a été supprimé définitivement.";
        }
        else
        {
            TempData["Error"] = "Seuls les colis annulés peuvent être supprimés.";
        }
        return RedirectToAction("MesColis");
    }
} 