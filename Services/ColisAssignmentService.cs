using Microsoft.EntityFrameworkCore;
using SuiviLivraison.Data;
using SuiviLivraison.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SuiviLivraison.Services
{
    public interface IColisAssignmentService
    {
        Task AssignColisToNearestLivreurAsync(Colis colis);
        Task<Livreur?> FindNearestAvailableLivreurAsync(double colisLatitude, double colisLongitude);
        double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
    }

    public class ColisAssignmentService : IColisAssignmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapService _mapService;

        public ColisAssignmentService(ApplicationDbContext context, IMapService mapService)
        {
            _context = context;
            _mapService = mapService;
        }

        public async Task AssignColisToNearestLivreurAsync(Colis colis)
        {
            var nearestLivreur = await FindNearestAvailableLivreurAsync(colis.Latitude, colis.Longitude);
            
            if (nearestLivreur != null)
            {
                colis.LivreurId = nearestLivreur.Id;
                colis.Statut = "Assigné";
                
                // Calculer l'heure de livraison estimée avec OpenStreetMap
                colis.HeureLivraisonEstimee = await _mapService.EstimateDeliveryTimeAsync(
                    nearestLivreur.Latitude, nearestLivreur.Longitude, 
                    colis.Latitude, colis.Longitude);
                
                // Obtenir l'adresse de livraison
                colis.AdresseLivraison = await _mapService.GetAddressFromCoordinatesAsync(
                    colis.Latitude, colis.Longitude);
                
                // Créer une notification pour le livreur
                var notificationLivreur = new Notification
                {
                    ColisId = colis.Id,
                    Message = $"📦 Nouveau colis assigné : {colis.Description}. Livraison estimée : {DateTimeHelper.FormatLocalTime(colis.HeureLivraisonEstimee)}",
                    DateEnvoi = DateTimeHelper.GetCurrentUtcTime()
                };
                
                // Créer une notification pour le client
                var notificationClient = new Notification
                {
                    ColisId = colis.Id,
                    Message = $"✅ Votre colis '{colis.Description}' a été assigné au livreur {nearestLivreur.Nom} (tél: {nearestLivreur.Telephone}). Livraison estimée : {DateTimeHelper.FormatLocalTime(colis.HeureLivraisonEstimee)}",
                    DateEnvoi = DateTimeHelper.GetCurrentUtcTime()
                };
                
                _context.Notifications.Add(notificationLivreur);
                _context.Notifications.Add(notificationClient);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Livreur?> FindNearestAvailableLivreurAsync(double colisLatitude, double colisLongitude)
        {
            var availableLivreurs = await _context.Livreurs
                .Where(l => l.Statut == "Disponible" || l.Statut == "En livraison")
                .ToListAsync();

            if (!availableLivreurs.Any())
                return null;

            var nearestLivreur = availableLivreurs
                .OrderBy(l => CalculateDistance(colisLatitude, colisLongitude, l.Latitude, l.Longitude))
                .FirstOrDefault();

            return nearestLivreur;
        }

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Rayon de la Terre en kilomètres
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
} 