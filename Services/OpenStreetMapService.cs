using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SuiviLivraison.Services
{
    public interface IMapService
    {
        Task<TimeSpan?> GetTravelTimeAsync(double originLat, double originLng, double destLat, double destLng);
        Task<string?> GetAddressFromCoordinatesAsync(double lat, double lng);
        Task<DateTime> EstimateDeliveryTimeAsync(double originLat, double originLng, double destLat, double destLng);
    }

    public class OpenStreetMapService : IMapService
    {
        private readonly HttpClient _httpClient;

        public OpenStreetMapService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TimeSpan?> GetTravelTimeAsync(double originLat, double originLng, double destLat, double destLng)
        {
            try
            {
                // Utiliser l'API OSRM (Open Source Routing Machine) pour calculer le temps de trajet
                var url = $"https://router.project-osrm.org/route/v1/driving/{originLng},{originLat};{destLng},{destLat}?overview=false";
                
                var response = await _httpClient.GetStringAsync(url);
                var jsonDoc = JsonDocument.Parse(response);
                var root = jsonDoc.RootElement;

                if (root.GetProperty("code").GetString() == "Ok")
                {
                    var routes = root.GetProperty("routes");
                    if (routes.GetArrayLength() > 0)
                    {
                        var duration = routes[0].GetProperty("duration").GetDouble();
                        return TimeSpan.FromSeconds(duration);
                    }
                }

                return null;
            }
            catch (Exception)
            {
                // En cas d'erreur, calculer une estimation basée sur la distance
                return EstimateTimeFromDistance(originLat, originLng, destLat, destLng);
            }
        }

        public async Task<string?> GetAddressFromCoordinatesAsync(double lat, double lng)
        {
            try
            {
                // Utiliser Nominatim (OpenStreetMap) pour la géocodification inverse
                var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={lat}&lon={lng}&zoom=18&addressdetails=1";
                
                // Ajouter un User-Agent pour respecter les conditions d'utilisation
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "SuiviLivraison/1.0");
                
                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                
                var jsonDoc = JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("display_name", out var displayName))
                {
                    return displayName.GetString();
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<DateTime> EstimateDeliveryTimeAsync(double originLat, double originLng, double destLat, double destLng)
        {
            var travelTime = await GetTravelTimeAsync(originLat, originLng, destLat, destLng);
            
            if (travelTime.HasValue)
            {
                // Ajouter 15 minutes pour le temps de préparation et de livraison
                var totalTime = travelTime.Value.Add(TimeSpan.FromMinutes(15));
                return DateTime.UtcNow.Add(totalTime);
            }
            
            // Si on ne peut pas calculer le temps, estimer 30 minutes
            return DateTime.UtcNow.AddMinutes(30);
        }

        private TimeSpan EstimateTimeFromDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Calcul de distance approximative (formule de Haversine)
            const double R = 6371; // Rayon de la Terre en kilomètres
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c;

            // Estimation : 30 km/h en ville (vitesse moyenne)
            var estimatedTimeHours = distance / 30.0;
            return TimeSpan.FromHours(estimatedTimeHours);
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
} 