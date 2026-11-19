using MySql.Data.MySqlClient;

namespace Wypozyczalnia.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void ZalogujButton_Clicked(object sender, EventArgs e) {
        string login = LoginEntry.Text?.Trim();
        string haslo = HasloEntry.Text;

        if (string.IsNullOrEmpty(login) ||
            string.IsNullOrEmpty(haslo)) {
            await DisplayAlert("Błąd",
                "Wpisz login i hasło.",
                "OK");
            return;
        }

        try {
            using var conn = new MySqlConnection(Config.ConnectionString);

            await conn.OpenAsync();

            string sql =
                @"SELECT id
                  FROM operatorzy
                  WHERE login=@login
                  AND haslo_hash=@haslo";

            using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@haslo", haslo);

            int wynik = Convert.ToInt32(
                await cmd.ExecuteScalarAsync());

            if (wynik > 0) {
                LoginEntry.Text = "";
                HasloEntry.Text = "";
                Session.OperatorId = wynik;
                await Shell.Current.GoToAsync("//wypozycz");
            } else {
                BladLabel.IsVisible = true;
            }
        } catch (Exception ex) {
            await DisplayAlert(
                "Błąd połączenia",
                ex.Message,
                "OK");
        }
    }
}
