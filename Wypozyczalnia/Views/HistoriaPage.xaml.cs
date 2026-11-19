using MySql.Data.MySqlClient;

namespace Wypozyczalnia.Views;

public partial class HistoriaPage : ContentPage
{

    public HistoriaPage()
    {
        InitializeComponent();
    }

    private async void HistoriaCollection_Loaded(object sender, EventArgs e)
    {
        try
        {
            using var conn = new MySqlConnection(Config.ConnectionString);

            await conn.OpenAsync();

            string sql =
                @"SELECT wypozyczenia.id, sprzet.nazwa, osoba, status, data_wypozyczenia, data_zwrotu, notatka
                      FROM wypozyczenia join sprzet on sprzet.id = wypozyczenia.sprzet_id";

            var HistoriaList = new List<dynamic>();

            using var cmd = new MySqlCommand(sql, conn);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int id = reader.GetInt32(0);
                    string nazwa = reader.GetString(1);
                    string osoba = reader.GetString(2);
                    string status = reader.GetString(3);
                    DateTime data_wypozyczenia = reader.GetDateTime(4);
                    DateTime? data_zwrotu = reader.IsDBNull(5)
                        ? null
                        : reader.GetDateTime(5);

                    string notatka = reader.IsDBNull(6)
                        ? ""
                        : reader.GetString(6);

                    HistoriaList.Add(new { id, nazwa, osoba, status, data_wypozyczenia, data_zwrotu, notatka });
                }
                HistoriaCollection.ItemsSource = HistoriaList;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "B³¹d po³¹czenia",
                ex.Message,
                "OK");
        }
    }

}
