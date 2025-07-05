// ===== JAVASCRIPT CLIENT =====

// Animation du bouton de soumission pour les formulaires
function initSubmitButtonAnimation() {
    const forms = document.querySelectorAll('form[id$="Form"]');
    forms.forEach(form => {
        form.addEventListener('submit', function() {
            const submitBtn = form.querySelector('#submitBtn');
            if (submitBtn) {
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Traitement en cours...';
                submitBtn.disabled = true;
            }
        });
    });
}

// Validation en temps réel pour les formulaires
function initRealTimeValidation() {
    const inputs = document.querySelectorAll('.form-control, .form-select');
    inputs.forEach(input => {
        input.addEventListener('input', function() {
            validateField(this);
        });
        
        // Pour les selects, écouter aussi le changement
        if (input.tagName === 'SELECT') {
            input.addEventListener('change', function() {
                validateField(this);
            });
        }
    });
}

// Validation d'un champ
function validateField(field) {
    if (field.checkValidity()) {
        field.classList.remove('is-invalid');
        field.classList.add('is-valid');
    } else {
        field.classList.remove('is-valid');
        field.classList.add('is-invalid');
    }
}

// Animation des cartes au survol
function initCardAnimations() {
    const cards = document.querySelectorAll('.card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-4px)';
            this.style.boxShadow = '0 8px 25px rgba(0,0,0,0.12)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
            this.style.boxShadow = '';
        });
    });
}

// Animation des boutons au survol
function initButtonAnimations() {
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-2px)';
            this.style.boxShadow = '0 5px 15px rgba(0,0,0,0.12)';
        });
        
        button.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
            this.style.boxShadow = '';
        });
    });
}

// Gestion des alertes auto-fermantes
function initAutoDismissAlerts() {
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000); // Ferme après 5 secondes
    });
}

// Animation des badges de statut
function initStatusBadgeAnimations() {
    const badges = document.querySelectorAll('.badge');
    badges.forEach(badge => {
        badge.addEventListener('mouseenter', function() {
            this.style.transform = 'scale(1.1)';
        });
        
        badge.addEventListener('mouseleave', function() {
            this.style.transform = 'scale(1)';
        });
    });
}

// Gestion des notifications
function initNotifications() {
    const notificationItems = document.querySelectorAll('.notification-item');
    notificationItems.forEach(item => {
        item.addEventListener('click', function() {
            const notificationId = this.getAttribute('data-notification-id');
            if (notificationId) {
                markNotificationAsRead(notificationId);
            }
        });
    });
}

// Marquer une notification comme lue
function markNotificationAsRead(notificationId) {
    fetch(`/Colis/MarkNotificationAsRead/${notificationId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
        }
    })
    .then(response => {
        if (response.ok) {
            const notificationItem = document.querySelector(`[data-notification-id="${notificationId}"]`);
            if (notificationItem) {
                notificationItem.classList.remove('notification-unread');
                notificationItem.classList.add('notification-read');
            }
        }
    })
    .catch(error => {
        console.error('Erreur lors du marquage de la notification:', error);
    });
}

// Gestion de la carte pour les colis
function initMapFeatures() {
    const mapContainers = document.querySelectorAll('.map-container');
    mapContainers.forEach(container => {
        const colisId = container.getAttribute('data-colis-id');
        if (colisId) {
            loadColisMap(container, colisId);
        }
    });
}

// Charger la carte pour un colis
function loadColisMap(container, colisId) {
    // Ici vous pouvez intégrer une API de carte comme Leaflet ou Google Maps
    // Pour l'instant, on affiche juste un placeholder
    container.innerHTML = `
        <div class="d-flex align-items-center justify-content-center h-100 bg-light">
            <div class="text-center">
                <i class="fas fa-map fa-3x text-muted mb-3"></i>
                <p class="text-muted">Carte de suivi du colis #${colisId}</p>
            </div>
        </div>
    `;
}

// Gestion des coordonnées GPS
function initGPSFeatures() {
    const gpsButtons = document.querySelectorAll('[data-action="get-gps"]');
    gpsButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const colisId = this.getAttribute('data-colis-id');
            getCurrentLocation(colisId);
        });
    });
}

// Obtenir la position GPS actuelle
function getCurrentLocation(colisId) {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
            function(position) {
                const lat = position.coords.latitude;
                const lng = position.coords.longitude;
                
                // Mettre à jour les champs de coordonnées
                const latInput = document.querySelector(`[data-colis-id="${colisId}"] input[name="Latitude"]`);
                const lngInput = document.querySelector(`[data-colis-id="${colisId}"] input[name="Longitude"]`);
                
                if (latInput) latInput.value = lat;
                if (lngInput) lngInput.value = lng;
                
                // Afficher une notification
                ClientUtils.showNotification('Position GPS obtenue avec succès !', 'success');
            },
            function(error) {
                console.error('Erreur GPS:', error);
                ClientUtils.showNotification('Impossible d\'obtenir la position GPS', 'warning');
            }
        );
    } else {
        ClientUtils.showNotification('La géolocalisation n\'est pas supportée par votre navigateur', 'warning');
    }
}

// Initialisation générale
document.addEventListener('DOMContentLoaded', function() {
    initSubmitButtonAnimation();
    initRealTimeValidation();
    initCardAnimations();
    initButtonAnimations();
    initAutoDismissAlerts();
    initStatusBadgeAnimations();
    initNotifications();
    initMapFeatures();
    initGPSFeatures();
    
    console.log('Client JavaScript initialisé');
});

// Fonctions utilitaires
window.ClientUtils = {
    // Afficher une notification
    showNotification: function(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(notification);
        
        // Auto-fermer après 3 secondes
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 3000);
    },
    
    // Valider un formulaire
    validateForm: function(formId) {
        const form = document.getElementById(formId);
        if (!form) return false;
        
        const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
        let isValid = true;
        
        inputs.forEach(input => {
            if (!input.checkValidity()) {
                input.classList.add('is-invalid');
                isValid = false;
            } else {
                input.classList.remove('is-invalid');
                input.classList.add('is-valid');
            }
        });
        
        return isValid;
    },
    
    // Formater les coordonnées
    formatCoordinates: function(lat, lng) {
        if (!lat || !lng) return 'Non défini';
        return `${parseFloat(lat).toFixed(6)}, ${parseFloat(lng).toFixed(6)}`;
    },
    
    // Calculer la distance entre deux points
    calculateDistance: function(lat1, lon1, lat2, lon2) {
        const R = 6371; // Rayon de la Terre en km
        const dLat = (lat2 - lat1) * Math.PI / 180;
        const dLon = (lon2 - lon1) * Math.PI / 180;
        const a = Math.sin(dLat/2) * Math.sin(dLat/2) +
                  Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
                  Math.sin(dLon/2) * Math.sin(dLon/2);
        const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
        return R * c;
    },
    
    // Formater une date
    formatDate: function(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('fr-FR', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }
}; 