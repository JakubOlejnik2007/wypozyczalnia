using MySql.Data.MySqlClient;

namespace Wypozyczalnia.Views;

public partial class SprzetPage : ContentPage
{
    public SprzetPage()
    {
        InitializeComponent();
        WyswietlSprzet();
    }

    private async void WyswietlSprzet()
    {
        try
        {
            using var conn = new MySqlConnection(Config.ConnectionString);
            await conn.OpenAsync();
            string sql = @"SELECT kod, nazwa, kategoria_id, opis, czy_wypozyczone FROM sprzet";

            var SprzetList = new List<dynamic>();

            using var cmd = new MySqlCommand(sql, conn);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    string kod = reader.GetString(0);
                    string nazwa = reader.GetString(1);
                    int kategoriaId = reader.GetInt32(2);
                    string opis = reader.GetString(3);
                    bool czyWypozyczone = reader.GetBoolean(4);
                    
                    SprzetList.Add(new { kod, nazwa, kategoria = kategoriaId.ToString(), opis });
                }
                SprzetCollection.ItemsSource = SprzetList;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Błąd połączenia",
                ex.Message,
                "OK");
        }
    }
}
