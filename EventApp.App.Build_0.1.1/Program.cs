// uitzoeken
using Microsoft.Extensions.FileProviders;
using EventApp.Web.Repositories;

// uitzoeken, wat is een builder en waarom slaat die hem op in een variabele
var builder = WebApplication.CreateBuilder(args);

// Use wwwroot/uploads for uploaded images - works on any machine
var uploadsPath = Path.Combine(builder.Environment.WebRootPath, "uploads");

// wat is razor pages en waarom?
builder.Services.AddRazorPages();

// Connection string opslag, maar zoek uit wat precies elke line zegt
builder.Services.AddScoped<IEventRepository>(ProviderAliasAttribute =>
{
    var connectionString = builder.Configuration.GetConnectionString("db-eventapp")
        ?? throw new InvalidOperationException("Missing connection string: db-eventapp");

    return new SqlEventRepository(connectionString);
});

// weer een builder
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Create uploads directory if it doesn't exist
Directory.CreateDirectory(uploadsPath);

// app methods, wat doen deze?
app.UseHttpsRedirection();
app.UseStaticFiles(); // Serves wwwroot by default (includes wwwroot/images and wwwroot/uploads)

app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

// API endpoint voor de bookmarks pagina.
// De browser stuurt een komma-gescheiden lijst van event ID's mee als query parameter,
// en de server geeft de bijbehorende event data terug als JSON.
// Voorbeeld: GET /api/bookmarked-events?ids=3,7,12
app.MapGet("/api/bookmarked-events", async (string? ids, IEventRepository eventRepository) =>
{
    // Geen ID's meegestuurd? Geef een lege lijst terug.
    if (string.IsNullOrWhiteSpace(ids))
        return Results.Ok(Array.Empty<object>());

    // Verwerk de komma-gescheiden string naar een lijst van geldige integers.
    // Ongeldige waarden (zoals letters) worden overgeslagen met TryParse.
    var idList = ids
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => int.TryParse(s.Trim(), out var n) ? n : -1)
        .Where(n => n > 0)
        .ToList();

    if (idList.Count == 0)
        return Results.Ok(Array.Empty<object>());

    var events = await eventRepository.GetEventsByIdsAsync(idList);
    return Results.Ok(events);
});

app.Run();

// Om de index te voorzien van event cards, loopt de logica als volgt per cs file:
// /Repositories/IEventRepository legt de daadwerkelijke connectie met de database
// /Repositories/SqlEventRepository bepaalt of die data wel klopt qua format (int, string, etc) en
// slaat dit op als een EventCard object waarvan je de contructer hier vindt: /Models/EventCards.cs
// Die objecten worden doorgegeven aan Index.cshtml.cs en weergegeven op index.cshtml