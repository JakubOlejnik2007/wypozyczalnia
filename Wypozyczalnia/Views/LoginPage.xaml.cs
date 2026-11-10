using MySql.Data.MySqlClient;

namespace Wypozyczalnia.Views;

public partial class LoginPage : ContentPage
{
    private const string ConnectionString =
       "server=mysql.mikr.us;database=;uid=;pwd=;";

    public LoginPage()
    {
        InitializeComponent();
    }

    private async void ZalogujButton_Clicked(object sender, EventArgs e) {
        string login = LoginEntry.Text?.Trim();
        string haslo = HasloEntry.Text;

        if (string.IsNullOrEmpty(login) ||
            string.IsNullOrEmpty(haslo)) {
            await DisplayAlert("B³¹d",
                "Wpisz login i has³o.",
                "OK");
            return;
        }

        try {
            using var conn = new MySqlConnection(ConnectionString);

            await conn.OpenAsync();

            string sql =
                @"SELECT COUNT(*)
                  FROM operatorzy
                  WHERE login=@login
                  AND haslo_hash=@haslo";

            using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@haslo", haslo);

            int wynik = Convert.ToInt32(
                await cmd.ExecuteScalarAsync());

            if (wynik > 0) {
                await DisplayAlert("OK", "Logowanie poprawne", "OK");
            } else {
                BladLabel.IsVisible = true;
            }
        } catch (Exception ex) {
            await DisplayAlert(
                "B³¹d po³¹czenia",
                ex.Message,
                "OK");
        }
    }
}
