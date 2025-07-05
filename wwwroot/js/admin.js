// ===== JAVASCRIPT ADMIN =====

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

// Confirmation pour les actions destructives
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

// Initialisation des tooltips Bootstrap
function initTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Animation des badges
function initBadgeAnimations() {
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

// Animation des icônes
function initIconAnimations() {
    const icons = document.querySelectorAll('.fa-3x');
    icons.forEach(icon => {
        icon.addEventListener('mouseenter', function() {
            this.style.color = '#0056b3';
        });
        
        icon.addEventListener('mouseleave', function() {
            this.style.color = '';
        });
    });
}

// Initialisation générale
document.addEventListener('DOMContentLoaded', function() {
    initSubmitButtonAnimation();
    initRealTimeValidation();
    initCardAnimations();
    initButtonAnimations();
    initConfirmations();
    initTooltips();
    initBadgeAnimations();
    initAutoDismissAlerts();
    initIconAnimations();
    
    console.log('Admin JavaScript initialisé');
});

// Fonctions utilitaires
window.AdminUtils = {
    // Afficher une notification toast
    showToast: function(message, type = 'info') {
        const toastContainer = document.getElementById('toast-container') || createToastContainer();
        const toast = createToast(message, type);
        toastContainer.appendChild(toast);
        
        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();
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
    
    // Animer un élément
    animateElement: function(element, animation) {
        element.classList.add(animation);
        setTimeout(() => {
            element.classList.remove(animation);
        }, 1000);
    }
};

// Créer un conteneur de toasts
function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toast-container';
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    container.style.zIndex = '9999';
    document.body.appendChild(container);
    return container;
}

// Créer un toast
function createToast(message, type) {
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;
    
    return toast;
} 