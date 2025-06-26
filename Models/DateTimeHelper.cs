using System;

namespace SuiviLivraison.Models
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// Obtient la date et heure actuelles en UTC
        /// </summary>
        public static DateTime GetCurrentUtcTime()
        {
            return DateTime.UtcNow;
        }

        /// <summary>
        /// Convertit une date UTC en heure locale pour l'affichage
        /// </summary>
        public static string FormatLocalTime(DateTime? utcDateTime, string format = "dd/MM/yyyy HH:mm")
        {
            if (!utcDateTime.HasValue)
                return "Non définie";
            
            return utcDateTime.Value.ToLocalTime().ToString(format);
        }

        /// <summary>
        /// Formate les coordonnées GPS avec 4 décimales
        /// </summary>
        public static string FormatCoordinates(double latitude, double longitude)
        {
            return $"{latitude:F4}, {longitude:F4}";
        }
    }
} 