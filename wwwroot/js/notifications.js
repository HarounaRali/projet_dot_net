class NotificationManager {
    constructor() {
        this.updateInterval = 30000; // 30 secondes
        this.notificationBadge = null;
        this.notificationDropdown = null;
        this.init();
    }

    init() {
        this.notificationBadge = document.querySelector('.notification-badge');
        this.notificationDropdown = document.querySelector('.notification-dropdown');
        
        if (this.notificationBadge || this.notificationDropdown) {
            this.startPolling();
            this.setupEventListeners();
        }
    }

    startPolling() {
        // Mise à jour initiale
        this.updateNotificationCount();
        
        // Mise à jour périodique
        setInterval(() => {
            this.updateNotificationCount();
        }, this.updateInterval);
    }

    async updateNotificationCount() {
        try {
            const response = await fetch('/api/notification/unread-count');
            if (response.ok) {
                const data = await response.json();
                this.updateBadge(data.count);
            }
        } catch (error) {
            console.error('Erreur lors de la mise à jour des notifications:', error);
        }
    }

    updateBadge(count) {
        if (this.notificationBadge) {
            if (count > 0) {
                this.notificationBadge.textContent = count;
                this.notificationBadge.style.display = 'inline';
                
                // Animation de pulsation pour les nouvelles notifications
                if (count > parseInt(this.notificationBadge.textContent || '0')) {
                    this.notificationBadge.classList.add('pulse');
                    setTimeout(() => {
                        this.notificationBadge.classList.remove('pulse');
                    }, 2000);
                }
            } else {
                this.notificationBadge.style.display = 'none';
            }
        }
    }

    async loadRecentNotifications() {
        try {
            const response = await fetch('/api/notification/recent?limit=5');
            if (response.ok) {
                const notifications = await response.json();
                this.renderNotifications(notifications);
            }
        } catch (error) {
            console.error('Erreur lors du chargement des notifications:', error);
        }
    }

    renderNotifications(notifications) {
        if (!this.notificationDropdown) return;

        const container = this.notificationDropdown.querySelector('.dropdown-menu');
        if (!container) return;

        if (notifications.length === 0) {
            container.innerHTML = '<div class="dropdown-item text-muted">Aucune notification</div>';
            return;
        }

        container.innerHTML = notifications.map(notification => `
            <div class="dropdown-item notification-item ${notification.estLue ? '' : 'unread'}" 
                 data-notification-id="${notification.id}">
                <div class="d-flex align-items-start">
                    <div class="notification-icon me-2">
                        <i class="fas ${this.getNotificationIcon(notification.type)} ${this.getNotificationColor(notification.type)}"></i>
                    </div>
                    <div class="flex-grow-1">
                        <div class="notification-message small">${notification.message}</div>
                        <div class="notification-time text-muted" style="font-size: 0.75rem;">
                            ${this.formatTime(notification.dateEnvoi)}
                        </div>
                    </div>
                    ${!notification.estLue ? '<div class="notification-dot"></div>' : ''}
                </div>
            </div>
        `).join('');

        // Ajouter le lien "Voir toutes"
        container.innerHTML += `
            <div class="dropdown-divider"></div>
            <a class="dropdown-item text-center" href="/Colis/Notifications">
                <small>Voir toutes les notifications</small>
            </a>
        `;
    }

    getNotificationIcon(type) {
        const icons = {
            'ColisCree': 'fa-box',
            'ColisEnCours': 'fa-truck',
            'ColisLivre': 'fa-check-circle',
            'ColisRetarde': 'fa-exclamation-triangle',
            'LivreurAssigne': 'fa-user-check',
            'Support': 'fa-headset',
            'Facturation': 'fa-credit-card'
        };
        return icons[type] || 'fa-bell';
    }

    getNotificationColor(type) {
        const colors = {
            'ColisCree': 'text-primary',
            'ColisEnCours': 'text-info',
            'ColisLivre': 'text-success',
            'ColisRetarde': 'text-warning',
            'LivreurAssigne': 'text-success',
            'Support': 'text-secondary',
            'Facturation': 'text-danger'
        };
        return colors[type] || 'text-muted';
    }

    formatTime(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'À l\'instant';
        if (diffMins < 60) return `Il y a ${diffMins} min`;
        if (diffHours < 24) return `Il y a ${diffHours}h`;
        if (diffDays < 7) return `Il y a ${diffDays}j`;
        
        return date.toLocaleDateString('fr-FR');
    }

    async markAsRead(notificationId) {
        try {
            const response = await fetch(`/api/notification/mark-read/${notificationId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            
            if (response.ok) {
                // Mettre à jour l'interface
                const notificationItem = document.querySelector(`[data-notification-id="${notificationId}"]`);
                if (notificationItem) {
                    notificationItem.classList.remove('unread');
                    const dot = notificationItem.querySelector('.notification-dot');
                    if (dot) dot.remove();
                }
                
                // Mettre à jour le compteur
                this.updateNotificationCount();
            }
        } catch (error) {
            console.error('Erreur lors du marquage comme lu:', error);
        }
    }

    setupEventListeners() {
        // Gestionnaire pour l'ouverture du dropdown
        const notificationToggle = document.querySelector('.notification-toggle');
        if (notificationToggle) {
            notificationToggle.addEventListener('click', () => {
                this.loadRecentNotifications();
            });
        }

        // Gestionnaire pour marquer comme lu
        document.addEventListener('click', (e) => {
            if (e.target.closest('.notification-item')) {
                const notificationId = e.target.closest('.notification-item').dataset.notificationId;
                if (notificationId) {
                    this.markAsRead(notificationId);
                }
            }
        });
    }
}

// Initialisation quand le DOM est chargé
document.addEventListener('DOMContentLoaded', () => {
    new NotificationManager();
});

// Styles CSS pour les animations
const style = document.createElement('style');
style.textContent = `
    .notification-badge.pulse {
        animation: pulse 1s infinite;
    }
    
    @keyframes pulse {
        0% { transform: scale(1); }
        50% { transform: scale(1.2); }
        100% { transform: scale(1); }
    }
    
    .notification-item.unread {
        background-color: rgba(0, 123, 255, 0.1);
    }
    
    .notification-dot {
        width: 8px;
        height: 8px;
        background-color: #007bff;
        border-radius: 50%;
        margin-left: auto;
    }
    
    .notification-item:hover {
        background-color: rgba(0, 0, 0, 0.05);
    }
    
    .notification-icon {
        width: 20px;
        text-align: center;
    }
`;
document.head.appendChild(style); 