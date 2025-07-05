using Microsoft.EntityFrameworkCore;
using SuiviLivraison.Data;
using Microsoft.AspNetCore.Identity;
using SuiviLivraison.Services;
using SuiviLivraison.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection");

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Ajout d'Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Enregistrement des services personnalisés
builder.Services.AddHttpClient();
builder.Services.AddScoped<IColisAssignmentService, ColisAssignmentService>();
builder.Services.AddScoped<IMapService, OpenStreetMapService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IBillingService, BillingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Désactivation de la redirection HTTPS en développement
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles(); // ← nécessaire pour wwwroot

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Création automatique des rôles et admin initial au démarrage
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Créer les rôles s'ils n'existent pas
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    if (!await roleManager.RoleExistsAsync("Client"))
    {
        await roleManager.CreateAsync(new IdentityRole("Client"));
    }
    if (!await roleManager.RoleExistsAsync("Livreur"))
    {
        await roleManager.CreateAsync(new IdentityRole("Livreur"));
    }

    // Créer l'admin initial s'il n'existe pas
    var adminEmail = "admin@suivilivraison.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // Créer un tarif par défaut s'il n'existe pas
    if (!context.Tarifs.Any())
    {
        var tarifDefaut = new Tarif
        {
            Nom = "Tarif Standard",
            Description = "Tarif de base pour les livraisons",
            PrixBase = 10.00m,
            PrixParKm = 0.50m,
            PrixUrgence = 5.00m,
            EstActif = true
        };
        context.Tarifs.Add(tarifDefaut);
        await context.SaveChangesAsync();
    }

    // Créer quelques FAQ par défaut
    if (!context.FAQs.Any())
    {
        var faqs = new List<FAQ>
        {
            new FAQ
            {
                Question = "Comment créer un colis ?",
                Reponse = "Connectez-vous à votre compte client, puis cliquez sur 'Nouveau Colis' et remplissez les informations requises.",
                Categorie = "Livraison",
                OrdreAffichage = 1,
                EstActive = true
            },
            new FAQ
            {
                Question = "Combien de temps dure une livraison ?",
                Reponse = "Le temps de livraison varie selon la distance et la disponibilité des livreurs. Une estimation vous sera fournie lors de la création du colis.",
                Categorie = "Livraison",
                OrdreAffichage = 2,
                EstActive = true
            },
            new FAQ
            {
                Question = "Comment suivre mon colis ?",
                Reponse = "Vous pouvez suivre votre colis en temps réel depuis votre espace client dans la section 'Mes Colis'.",
                Categorie = "Suivi",
                OrdreAffichage = 1,
                EstActive = true
            },
            new FAQ
            {
                Question = "Comment contacter le support ?",
                Reponse = "Vous pouvez créer un ticket de support depuis votre espace client ou nous contacter directement via le formulaire de contact.",
                Categorie = "Support",
                OrdreAffichage = 1,
                EstActive = true
            }
        };
        context.FAQs.AddRange(faqs);
        await context.SaveChangesAsync();
    }
}

app.Run();
