using Microsoft.EntityFrameworkCore;
using SuiviLivraison.Data;
using SuiviLivraison.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SuiviLivraison.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(string message, string type, int? colisId = null, string? destinataireId = null, string? actionUrl = null, string? icone = null, int priorite = 1);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, bool nonLuesSeulement = false, int limit = 50);
        Task MarquerCommeLueAsync(int notificationId);
        Task MarquerToutesCommeLuesAsync(string userId);
        Task<int> GetNombreNotificationsNonLuesAsync(string userId);
        Task SupprimerNotificationAsync(int notificationId);
        
        // Méthodes en anglais pour compatibilité
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task<List<Notification>> GetRecentNotificationsAsync(string userId, int limit = 10);
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(int notificationId, string userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> CreateNotificationAsync(string message, string type, int? colisId = null, string? destinataireId = null, string? actionUrl = null, string? icone = null, int priorite = 1)
        {
            var notification = new Notification
            {
                Message = message,
                Type = type,
                ColisId = colisId,
                DestinataireId = destinataireId,
                ActionUrl = actionUrl,
                Icone = icone,
                Priorite = priorite,
                DateEnvoi = DateTimeHelper.GetCurrentUtcTime(),
                EstLue = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, bool nonLuesSeulement = false, int limit = 50)
        {
            var baseQuery = _context.Notifications
                .Where(n => n.DestinataireId == userId || n.DestinataireId == null);

            if (nonLuesSeulement)
            {
                baseQuery = baseQuery.Where(n => !n.EstLue);
            }

            return await baseQuery
                .Include(n => n.Colis)
                .OrderByDescending(n => n.DateEnvoi)
                .Take(limit)
                .ToListAsync();
        }

        public async Task MarquerCommeLueAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.EstLue = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarquerToutesCommeLuesAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.DestinataireId == userId && !n.EstLue)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.EstLue = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetNombreNotificationsNonLuesAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.DestinataireId == userId && !n.EstLue);
        }

        public async Task SupprimerNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        // Implémentation des méthodes en anglais
        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            return await GetNombreNotificationsNonLuesAsync(userId);
        }

        public async Task<List<Notification>> GetRecentNotificationsAsync(string userId, int limit = 10)
        {
            return await GetUserNotificationsAsync(userId, false, limit);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.DestinataireId == userId);
            
            if (notification != null)
            {
                notification.EstLue = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await MarquerToutesCommeLuesAsync(userId);
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.DestinataireId == userId);
            
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
} 