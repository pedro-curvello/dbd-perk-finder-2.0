using DbdPerkApi;

var builder = WebApplication.CreateBuilder(args);

// Habilita o CORS para permitir que o frontend JavaScript acesse a API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddHttpClient(); // Necessário para chamar a API da IA

var app = builder.Build();
app.UseCors("AllowAll");

// Base de dados em memória
var perks = new List<Perk>
{
    new Perk { Id = 1, NomePt = "Sprint Final", NomeEn = "Sprint Burst", Tipo = "Sobrevivente", Personagem = "Meg Thomas", Descricao = "Ao correr, arranque a 150% da velocidade por 3s." },
    new Perk { Id = 2, NomePt = "Auto-Cura", NomeEn = "Self-Care", Tipo = "Sobrevivente", Personagem = "Claudette Morel", Descricao = "Permite se curar sem kit médico a 35% da velocidade." },
    new Perk { Id = 3, NomePt = "Vocação de Enfermeira", NomeEn = "A Nurse's Calling", Tipo = "Assassino", Personagem = "A Enfermeira", Descricao = "Revela auras de sobreviventes se curando dentro de 28m." }
};

// Endpoint 1: Obter todas ou filtrar por nome (PT ou EN)
app.MapGet("/api/perks", (string? query) =>
{
    if (string.IsNullOrWhiteSpace(query))
        return Results.Ok(perks);

    var filtered = perks.Where(p =>
        p.NomePt.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        p.NomeEn.Contains(query, StringComparison.OrdinalIgnoreCase)
    ).ToList();

    return Results.Ok(filtered);
});

// Endpoint 2: Integração com IA para criar builds recomendadas
app.MapPost("/api/ia/build-recommendation", async (IHttpClientFactory clientFactory, HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var promptUsuario = await reader.ReadToEndAsync();

    // Substitua pela sua chave de API do Google Gemini
    var apiKey = "SUA_GEMINI_API_KEY_AQUI";
    var client = clientFactory.CreateClient();

    var requestBody = new
    {
        contents = new[]
        {
            new
            {
                parts = new[]
                {
                    new { text = $"Você é um especialista no jogo Dead by Daylight. O usuário pediu: '{promptUsuario}'. Recomende uma build de 4 perks explicativa e estratégica em português." }
                }
            }
        }
    };

    var response = await client.PostAsJsonAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}", requestBody);

    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var textoIa = result.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
        return Results.Ok(new { resposta = textoIa });
    }

    return Results.Problem("Erro ao consultar a IA.");
});

app.Run();
