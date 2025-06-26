# Guide d'Administration - SuiviLivraison

## 🚀 Accès Administrateur

### Compte Admin Initial
- **Email :** admin@suivilivraison.com
- **Mot de passe :** Admin123!

⚠️ **Important :** Changez ce mot de passe après votre première connexion !

## 📊 Fonctionnalités Administrateur

### 1. Dashboard Principal
- **Accès :** `/Admin/Dashboard`
- **Fonctionnalités :**
  - Vue d'ensemble des statistiques
  - Cartes avec les chiffres clés
  - Actions rapides pour créer livreurs/clients
  - Graphique des colis par statut

### 2. Gestion des Livreurs
- **Accès :** `/Admin/Livreurs`
- **Fonctionnalités :**
  - Liste de tous les livreurs
  - Créer un nouveau livreur
  - Supprimer un livreur
  - Voir le statut des comptes

### 3. Gestion des Clients
- **Accès :** `/Admin/Clients`
- **Fonctionnalités :**
  - Liste de tous les clients
  - Créer un nouveau client
  - Supprimer un client (si pas de colis associés)
  - Voir le statut des comptes

### 4. Gestion des Utilisateurs
- **Accès :** `/Admin/Utilisateurs`
- **Fonctionnalités :**
  - Voir tous les utilisateurs et leurs rôles
  - Promouvoir un utilisateur en admin
  - Retirer le rôle admin (sauf à soi-même)

### 5. Statistiques Détaillées
- **Accès :** `/Admin/Statistiques`
- **Fonctionnalités :**
  - Graphiques des colis par statut
  - Évolution des colis par mois
  - Top 5 des livreurs les plus performants
  - Taux de livraison et colis en attente

## 🔧 Création d'Utilisateurs

### Créer un Livreur
1. Aller dans "Administration" → "Gérer les livreurs"
2. Cliquer sur "Nouveau Livreur"
3. Remplir le formulaire :
   - Nom complet
   - Email (servira de nom d'utilisateur)
   - Téléphone
   - Mot de passe
4. Le compte sera créé automatiquement avec le rôle "Livreur"

### Créer un Client
1. Aller dans "Administration" → "Gérer les clients"
2. Cliquer sur "Nouveau Client"
3. Remplir le formulaire (même structure que livreur)
4. Le compte sera créé automatiquement avec le rôle "Client"

## 🔐 Gestion des Rôles

### Rôles Disponibles
- **Admin :** Accès complet à toutes les fonctionnalités
- **Livreur :** Gestion des colis à livrer
- **Client :** Suivi de ses propres colis

### Promouvoir un Utilisateur en Admin
1. Aller dans "Administration" → "Gérer les utilisateurs"
2. Trouver l'utilisateur dans la liste
3. Cliquer sur "Promouvoir Admin"
4. Confirmer l'action

⚠️ **Sécurité :** Soyez prudent avec l'attribution du rôle admin !

## 📈 Statistiques et Rapports

### Métriques Disponibles
- **Total des colis** par statut
- **Nombre de clients** et **livreurs**
- **Taux de livraison** (colis livrés / total)
- **Performance des livreurs** (top 5)
- **Évolution mensuelle** des colis

### Graphiques
- **Camembert :** Répartition des colis par statut
- **Ligne :** Évolution des colis par mois
- **Barres :** Performance des livreurs

## 🛡️ Sécurité

### Bonnes Pratiques
1. **Changez le mot de passe admin initial**
2. **Limitez le nombre d'administrateurs**
3. **Surveillez les promotions en admin**
4. **Vérifiez régulièrement les statistiques**
5. **Sauvegardez régulièrement les données**

### Actions Sécurisées
- Impossible de supprimer un client avec des colis
- Impossible de retirer son propre rôle admin
- Confirmation requise pour les actions critiques
- Logs des actions importantes

## 🚨 Dépannage

### Problèmes Courants

**Un utilisateur ne peut pas se connecter :**
- Vérifier que le compte existe
- Vérifier que l'email est confirmé
- Réinitialiser le mot de passe si nécessaire

**Un livreur ne voit pas ses colis :**
- Vérifier qu'il a le rôle "Livreur"
- Vérifier qu'il est assigné à des colis

**Les statistiques ne s'affichent pas :**
- Vérifier la connexion à la base de données
- Vérifier qu'il y a des données

## 📞 Support

Pour toute question ou problème :
1. Vérifiez d'abord ce guide
2. Consultez les logs de l'application
3. Contactez l'équipe de développement

---

**Version :** 1.0  
**Dernière mise à jour :** Janvier 2025 