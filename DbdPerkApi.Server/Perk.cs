namespace DbdPerkApi;

public class Perk
{
    public int Id { get; set; }
    public string NomePt { get; set; } = string.Empty;
    public string NomeEn { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // "Sobrevivente" ou "Assassino"
    public string Personagem { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}