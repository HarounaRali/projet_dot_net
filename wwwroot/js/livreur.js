// ===== JAVASCRIPT LIVREUR =====

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

// Confirmation pour les actions importantes
function initConfirmations() {
    const confirmButtons = document.querySelectorAll('[onclick*="confirm"]');
    confirmButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            const message = this.getAttribute('data-confirm-message') || 
                           'Êtes-vous sûr de vouloir effectuer cette action ?';
            
            if (!confirm(message)) {
                e.preventDefault();
                return false;
            }
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

// Gestion des demandes de colis
function initColisRequests() {
    const requestButtons = document.querySelectorAll('[data-action="demander-colis"]');
    requestButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            const colisId = this.getAttribute('data-colis-id');
            const message = `Êtes-vous sûr de vouloir demander ce colis ?`;
            
            if (confirm(message)) {
                // Le formulaire sera soumis automatiquement
                return true;
            } else {
                e.preventDefault();
                return false;
            }
        });
    });
}

// Mise à jour du statut en temps réel
function initStatusUpdates() {
    const statusSelects = document.querySelectorAll('select[name="Statut"]');
    statusSelects.forEach(select => {
        select.addEventListener('change', function() {
            const newStatus = this.value;
            const colisId = this.getAttribute('data-colis-id');
            
            // Mettre à jour l'interface visuellement
            updateStatusDisplay(colisId, newStatus);
        });
    });
}

// Mise à jour de l'affichage du statut
function updateStatusDisplay(colisId, status) {
    const statusBadge = document.querySelector(`[data-colis-id="${colisId}"] .status-badge`);
    if (statusBadge) {
        statusBadge.className = `badge status-badge ${getStatusClass(status)}`;
        statusBadge.textContent = status;
    }
}

// Obtenir la classe CSS pour un statut
function getStatusClass(status) {
    const statusMap = {
        'En attente': 'bg-warning',
        'En cours': 'bg-info',
        'Livré': 'bg-success',
        'Annulé': 'bg-danger'
    };
    return statusMap[status] || 'bg-secondary';
}

// Initialisation générale
document.addEventListener('DOMContentLoaded', function() {
    initSubmitButtonAnimation();
    initRealTimeValidation();
    initCardAnimations();
    initButtonAnimations();
    initConfirmations();
    initAutoDismissAlerts();
    initStatusBadgeAnimations();
    initColisRequests();
    initStatusUpdates();
    
    console.log('Livreur JavaScript initialisé');
});

// Fonctions utilitaires
window.LivreurUtils = {
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
    }
}; 