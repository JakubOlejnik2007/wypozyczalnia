namespace Wypozyczalnia.Models;

public class SprzetItem
{
    public int Id { get; set; }

    public string Kod { get; set; } = "";

    public string Nazwa { get; set; } = "";

    public string NazwaZKodem => $"{Kod} - {Nazwa}";
}