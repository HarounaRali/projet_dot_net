using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviLivraison.Data;
using SuiviLivraison.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Dashboard avec statistiques
    public async Task<IActionResult> Dashboard()
    {
        var stats = new AdminStatsViewModel
        {
            TotalColis = await _context.Colis.CountAsync(),
            ColisEnCours = await _context.Colis.CountAsync(c => c.Statut == "En cours"),
            ColisLivres = await _context.Colis.CountAsync(c => c.Statut == "Livré"),
            TotalClients = await _context.Clients.CountAsync(),
            TotalLivreurs = await _context.Livreurs.CountAsync(),
            ColisParStatut = await _context.Colis
                .GroupBy(c => c.Statut)
                .Select(g => new { Statut = g.Key, Count = g.Count() })
                .ToListAsync()
        };

        return View(stats);
    }

    // === GESTION DES LIVREURS ===
    
    // Liste des livreurs
    public async Task<IActionResult> Livreurs()
    {
        var livreurs = await _context.Livreurs
            .Include(l => l.IdentityUser)
            .ToListAsync();
        return View(livreurs);
    }

    // Créer un nouveau livreur
    public IActionResult CreateLivreur()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLivreur(CreateLivreurViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Créer l'utilisateur Identity
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Ajouter le rôle Livreur
                await _userManager.AddToRoleAsync(user, "Livreur");

                // Créer le profil Livreur
                var livreur = new Livreur
                {
                    IdentityUserId = user.Id,
                    Nom = model.Nom,
                    Telephone = model.Telephone,
                    Email = model.Email
                };

                _context.Livreurs.Add(livreur);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Livreur créé avec succès !";
                return RedirectToAction(nameof(Livreurs));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    // Supprimer un livreur
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLivreur(int id)
    {
        var livreur = await _context.Livreurs
            .Include(l => l.IdentityUser)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (livreur == null)
        {
            return NotFound();
        }

        // Supprimer l'utilisateur Identity
        if (livreur.IdentityUser != null)
        {
            await _userManager.DeleteAsync(livreur.IdentityUser);
        }

        // Supprimer le livreur
        _context.Livreurs.Remove(livreur);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Livreur supprimé avec succès !";
        return RedirectToAction(nameof(Livreurs));
    }

    // === GESTION DES CLIENTS ===
    
    // Liste des clients
    public async Task<IActionResult> Clients()
    {
        var clients = await _context.Clients
            .Include(c => c.IdentityUser)
            .ToListAsync();
        return View(clients);
    }

    // Créer un nouveau client
    public IActionResult CreateClient()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClient(CreateClientViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Créer l'utilisateur Identity
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Ajouter le rôle Client
                await _userManager.AddToRoleAsync(user, "Client");

                // Créer le profil Client
                var client = new Client
                {
                    IdentityUserId = user.Id,
                    Nom = model.Nom,
                    Telephone = model.Telephone,
                    Email = model.Email
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Client créé avec succès !";
                return RedirectToAction(nameof(Clients));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    // Supprimer un client
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await _context.Clients
            .Include(c => c.IdentityUser)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound();
        }

        // Vérifier s'il a des colis
        var hasColis = await _context.Colis.AnyAsync(c => c.ClientId == id);
        if (hasColis)
        {
            TempData["Error"] = "Impossible de supprimer ce client car il a des colis associés.";
            return RedirectToAction(nameof(Clients));
        }

        // Supprimer l'utilisateur Identity
        if (client.IdentityUser != null)
        {
            await _userManager.DeleteAsync(client.IdentityUser);
        }

        // Supprimer le client
        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Client supprimé avec succès !";
        return RedirectToAction(nameof(Clients));
    }

    // === GESTION DES UTILISATEURS ===
    
    // Liste des utilisateurs pour promotion admin
    public async Task<IActionResult> Utilisateurs()
    {
        var users = await _userManager.Users.ToListAsync();
        var userViewModels = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            });
        }

        return View(userViewModels);
    }

    // Promouvoir un utilisateur en admin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromouvoirAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.AddToRoleAsync(user, "Admin");
        if (result.Succeeded)
        {
            TempData["Success"] = $"L'utilisateur {user.Email} a été promu administrateur !";
        }
        else
        {
            TempData["Error"] = "Erreur lors de la promotion en admin.";
        }

        return RedirectToAction(nameof(Utilisateurs));
    }

    // Retirer le rôle admin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetirerAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Empêcher de retirer le rôle admin à soi-même
        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Vous ne pouvez pas retirer votre propre rôle administrateur.";
            return RedirectToAction(nameof(Utilisateurs));
        }

        var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
        if (result.Succeeded)
        {
            TempData["Success"] = $"Le rôle administrateur a été retiré à {user.Email} !";
        }
        else
        {
            TempData["Error"] = "Erreur lors du retrait du rôle admin.";
        }

        return RedirectToAction(nameof(Utilisateurs));
    }

    // Statistiques détaillées
    public async Task<IActionResult> Statistiques()
    {
        var stats = new StatistiquesViewModel
        {
            TotalColis = await _context.Colis.CountAsync(),
            ColisEnCours = await _context.Colis.CountAsync(c => c.Statut == "En cours"),
            ColisLivres = await _context.Colis.CountAsync(c => c.Statut == "Livré"),
            ColisEnAttente = await _context.Colis.CountAsync(c => c.Statut == "En attente"),
            TotalClients = await _context.Clients.CountAsync(),
            TotalLivreurs = await _context.Livreurs.CountAsync(),
            ColisParMois = await _context.Colis
                .Where(c => c.DateEnvoi.HasValue)
                .GroupBy(c => new { c.DateEnvoi!.Value.Year, c.DateEnvoi.Value.Month })
                .Select(g => new { Mois = $"{g.Key.Year}-{g.Key.Month:D2}", Count = g.Count() })
                .OrderBy(x => x.Mois)
                .ToListAsync(),
            TopLivreurs = await _context.Livreurs
                .Include(l => l.IdentityUser)
                .Select(l => new { 
                    Nom = l.Nom, 
                    ColisLivres = _context.Colis.Count(c => c.LivreurId == l.Id && c.Statut == "Livré") 
                })
                .OrderByDescending(x => x.ColisLivres)
                .Take(5)
                .ToListAsync()
        };

        return View(stats);
    }
}

// ViewModels pour l'admin
public class AdminStatsViewModel
{
    public int TotalColis { get; set; }
    public int ColisEnCours { get; set; }
    public int ColisLivres { get; set; }
    public int TotalClients { get; set; }
    public int TotalLivreurs { get; set; }
    public dynamic ColisParStatut { get; set; } = new List<object>();
}

public class CreateLivreurViewModel
{
    [Required(ErrorMessage = "Le nom est requis")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le téléphone est requis")]
    public string Telephone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis")]
    [StringLength(100, ErrorMessage = "Le mot de passe doit contenir au moins {2} caractères", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class CreateClientViewModel
{
    [Required(ErrorMessage = "Le nom est requis")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le téléphone est requis")]
    public string Telephone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis")]
    [StringLength(100, ErrorMessage = "Le mot de passe doit contenir au moins {2} caractères", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();
}

public class StatistiquesViewModel
{
    public int TotalColis { get; set; }
    public int ColisEnCours { get; set; }
    public int ColisLivres { get; set; }
    public int ColisEnAttente { get; set; }
    public int TotalClients { get; set; }
    public int TotalLivreurs { get; set; }
    public dynamic ColisParMois { get; set; } = new List<object>();
    public dynamic TopLivreurs { get; set; } = new List<object>();
} 