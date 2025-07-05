using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviLivraison.Data;
using SuiviLivraison.Models;
using Microsoft.AspNetCore.Identity;

namespace SuiviLivraison.Controllers
{
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

        public async Task<IActionResult> Dashboard()
        {
            var totalColis = await _context.Colis.CountAsync();
            var colisLivres = await _context.Colis.CountAsync(c => c.Statut == "Livré");
            
            var stats = new AdminStatsViewModel
            {
                TotalColis = totalColis,
                ColisEnAttente = await _context.Colis.CountAsync(c => c.Statut == "En attente"),
                ColisEnCours = await _context.Colis.CountAsync(c => c.Statut == "En cours"),
                ColisLivres = colisLivres,
                TotalClients = await _context.Clients.CountAsync(),
                TotalLivreurs = await _context.Livreurs.CountAsync(),
                TauxLivraison = totalColis > 0 ? (decimal)colisLivres / totalColis * 100 : 0,
                ColisAnnules = await _context.Colis.CountAsync(c => c.Statut == "Annulé")
            };

            // Ajouter les statistiques par statut
            var statuts = await _context.Colis
                .GroupBy(c => c.Statut)
                .Select(g => new StatutCount { Statut = g.Key, Count = g.Count() })
                .ToListAsync();
            stats.ColisParStatut = statuts;

            return View(stats);
        }

        public async Task<IActionResult> Utilisateurs()
        {
            var users = await _context.Users
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    Nom = u.UserName ?? "",
                    Telephone = u.PhoneNumber ?? "",
                    DateCreation = DateTime.UtcNow, // Placeholder
                    IsActive = true
                })
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Clients()
        {
            var clients = await _context.Clients
                .Include(c => c.Colis)
                .Include(c => c.IdentityUser)
                .ToListAsync();
            return View(clients);
        }

        public async Task<IActionResult> Livreurs()
        {
            var livreurs = await _context.Livreurs
                .Include(l => l.IdentityUser)
                .ToListAsync();
            return View(livreurs);
        }

        public async Task<IActionResult> Statistiques()
        {
            var totalColis = await _context.Colis.CountAsync();
            var colisLivres = await _context.Colis.CountAsync(c => c.Statut == "Livré");
            
            var stats = new StatistiquesViewModel
            {
                TotalColis = totalColis,
                ColisEnAttente = await _context.Colis.CountAsync(c => c.Statut == "En attente"),
                ColisEnCours = await _context.Colis.CountAsync(c => c.Statut == "En cours"),
                ColisLivres = colisLivres,
                ColisAnnules = await _context.Colis.CountAsync(c => c.Statut == "Annulé"),
                TotalClients = await _context.Clients.CountAsync(),
                TotalLivreurs = await _context.Livreurs.CountAsync(),
                TauxLivraison = totalColis > 0 ? (decimal)colisLivres / totalColis * 100 : 0,
                ColisRecents = await _context.Colis.OrderByDescending(c => c.DateEnvoi).Take(10).ToListAsync()
            };

            // Statistiques par mois
            var colisParMois = await _context.Colis
                .Where(c => c.DateEnvoi.HasValue)
                .GroupBy(c => new { c.DateEnvoi!.Value.Year, c.DateEnvoi!.Value.Month })
                .Select(g => new { Mois = $"{g.Key.Year}-{g.Key.Month:D2}", Count = g.Count() })
                .OrderBy(x => x.Mois)
                .ToListAsync();

            foreach (var item in colisParMois)
            {
                stats.ColisParMois[item.Mois] = item.Count;
            }

            // Statistiques par statut
            var statuts = await _context.Colis
                .GroupBy(c => c.Statut)
                .Select(g => new { Statut = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var statut in statuts)
            {
                stats.ColisParStatut[statut.Statut] = statut.Count;
            }

            return View(stats);
        }

        [HttpGet]
        public IActionResult CreateClient()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClient(CreateClientViewModel model)
        {
            // Supprimer les erreurs de validation pour ConfirmPassword
            ModelState.Remove("ConfirmPassword");
            
            if (ModelState.IsValid)
            {
                // Créer d'abord l'utilisateur Identity
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.Telephone,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Ajouter le rôle Client
                    await _userManager.AddToRoleAsync(user, "Client");

                    // Créer le client avec l'IdentityUserId
                    var client = new Client
                    {
                        Nom = model.Nom,
                        Email = model.Email,
                        Telephone = model.Telephone,
                        IdentityUserId = user.Id
                    };

                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Client créé avec succès !";
                    return RedirectToAction(nameof(Clients));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateLivreur()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLivreur(CreateLivreurViewModel model)
        {
            // Supprimer les erreurs de validation pour ConfirmPassword
            ModelState.Remove("ConfirmPassword");
            
            if (ModelState.IsValid)
            {
                // Créer d'abord l'utilisateur Identity
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.Telephone,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Ajouter le rôle Livreur
                    await _userManager.AddToRoleAsync(user, "Livreur");

                    // Créer le livreur avec l'IdentityUserId
                    var livreur = new Livreur
                    {
                        Nom = model.Nom,
                        Email = model.Email,
                        Telephone = model.Telephone,
                        Statut = "Actif",
                        IdentityUserId = user.Id
                    };

                    _context.Livreurs.Add(livreur);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Livreur créé avec succès !";
                    return RedirectToAction(nameof(Livreurs));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        public async Task<IActionResult> ClientDetails(int id)
        {
            var client = await _context.Clients
                .Include(c => c.IdentityUser)
                .Include(c => c.Colis)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients
                .Include(c => c.IdentityUser)
                .Include(c => c.Colis)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                TempData["Error"] = "Client introuvable.";
                return RedirectToAction(nameof(Clients));
            }

            try
            {
                // Supprimer d'abord les colis associés
                if (client.Colis.Any())
                {
                    _context.Colis.RemoveRange(client.Colis);
                }

                // Supprimer le client
                _context.Clients.Remove(client);

                // Supprimer l'utilisateur Identity associé
                if (client.IdentityUser != null)
                {
                    await _userManager.DeleteAsync(client.IdentityUser);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Client '{client.Nom}' supprimé avec succès.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la suppression : {ex.Message}";
            }

            return RedirectToAction(nameof(Clients));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLivreur(int id)
        {
            var livreur = await _context.Livreurs
                .Include(l => l.IdentityUser)
                .Include(l => l.ColisLivres)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (livreur == null)
            {
                TempData["Error"] = "Livreur introuvable.";
                return RedirectToAction(nameof(Livreurs));
            }

            try
            {
                // Retirer l'assignation des colis
                if (livreur.ColisLivres.Any())
                {
                    foreach (var colis in livreur.ColisLivres)
                    {
                        colis.LivreurId = null;
                        colis.Statut = "En attente";
                    }
                }

                // Supprimer le livreur
                _context.Livreurs.Remove(livreur);

                // Supprimer l'utilisateur Identity associé
                if (livreur.IdentityUser != null)
                {
                    await _userManager.DeleteAsync(livreur.IdentityUser);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Livreur '{livreur.Nom}' supprimé avec succès.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la suppression : {ex.Message}";
            }

            return RedirectToAction(nameof(Livreurs));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLivreurStatus(int id)
        {
            var livreur = await _context.Livreurs
                .Include(l => l.IdentityUser)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (livreur == null)
            {
                TempData["Error"] = "Livreur introuvable.";
                return RedirectToAction(nameof(Livreurs));
            }

            try
            {
                // Basculer le statut
                livreur.Statut = livreur.Statut == "Actif" ? "Inactif" : "Actif";
                
                // Si on désactive le livreur, retirer l'assignation des colis en cours
                if (livreur.Statut == "Inactif")
                {
                    var colisEnCours = await _context.Colis
                        .Where(c => c.LivreurId == livreur.Id && c.Statut == "En cours")
                        .ToListAsync();
                    
                    foreach (var colis in colisEnCours)
                    {
                        colis.LivreurId = null;
                        colis.Statut = "En attente";
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Statut du livreur '{livreur.Nom}' modifié : {livreur.Statut}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la modification : {ex.Message}";
            }

            return RedirectToAction(nameof(Livreurs));
        }
    }
} 