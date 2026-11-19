using System.Data;
using MySql.Data.MySqlClient;
using Wypozyczalnia.Models;

namespace Wypozyczalnia.Views;

public partial class WypozyczPage : ContentPage {
    private List<SprzetItem> _sprzet = new();

    public WypozyczPage() {
        InitializeComponent();

        WypozyczButton.Clicked += WypozyczButton_Clicked;
        WyczyscButton.Clicked += WyczyscButton_Clicked;

        DostepneCollection.SelectionChanged +=
            DostepneCollection_SelectionChanged;
    }

    protected override async void OnAppearing() {
        base.OnAppearing();
        WyczyscFormularz();
        await ZaladujSprzetAsync();
    }

    private async Task ZaladujSprzetAsync() {
        try {
            _sprzet.Clear();

            using var conn =
                new MySqlConnection(Config.ConnectionString);

            await conn.OpenAsync();

            string sql =
                @"SELECT id, kod, nazwa
                  FROM sprzet
                  WHERE czy_wypozyczone = 0
                  ORDER BY nazwa";

            using var cmd =
                new MySqlCommand(sql, conn);

            using var reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync()) {
                _sprzet.Add(new SprzetItem {
                    Id = reader.GetInt32("id"),
                    Kod = reader.GetString("kod"),
                    Nazwa = reader.GetString("nazwa")
                });
            }

            SprzetPicker.ItemsSource = null;
            SprzetPicker.ItemsSource = _sprzet;

            DostepneCollection.ItemsSource = null;
            DostepneCollection.ItemsSource = _sprzet;
        } catch (Exception ex) {
            await DisplayAlert(
                "B³¹d",
                ex.Message,
                "OK");
        }
    }

    private void DostepneCollection_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e) {
        var wybrany =
            e.CurrentSelection.FirstOrDefault()
            as SprzetItem;

        if (wybrany != null) {
            SprzetPicker.SelectedItem = wybrany;
        }
    }

    private async void WypozyczButton_Clicked(
        object sender,
        EventArgs e) {
        var sprzet =
            SprzetPicker.SelectedItem as SprzetItem;

        string osoba =
            OsobaEntry.Text?.Trim() ?? "";

        string notatka =
            NotatkaEditor.Text?.Trim();

        if (sprzet == null) {
            await DisplayAlert(
                "B³¹d",
                "Wybierz sprzêt.",
                "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(osoba)) {
            await DisplayAlert(
                "B³¹d",
                "Podaj osobê wypo¿yczaj¹c¹.",
                "OK");
            return;
        }

        try {
            using var conn =
                new MySqlConnection(Config.ConnectionString);

            await conn.OpenAsync();

            using var transaction =
                await conn.BeginTransactionAsync();

            try {
                // sprawdzenie czy sprzêt nadal jest dostêpny
                string sqlSprawdz =
                    @"SELECT czy_wypozyczone
                      FROM sprzet
                      WHERE id = @id";

                using var cmdSprawdz =
                    new MySqlCommand(
                        sqlSprawdz,
                        conn,
                        (MySqlTransaction)transaction);

                cmdSprawdz.Parameters.AddWithValue(
                    "@id",
                    sprzet.Id);

                bool czyWypozyczone =
                    Convert.ToBoolean(
                        await cmdSprawdz.ExecuteScalarAsync());

                if (czyWypozyczone) {
                    await transaction.RollbackAsync();

                    await DisplayAlert(
                        "Brak dostêpnoœci",
                        "Wybrany sprzêt zosta³ w³aœnie wypo¿yczony przez innego operatora.",
                        "OK");

                    await ZaladujSprzetAsync();

                    return;
                }

                string sqlWypozyczenie =
                    @"INSERT INTO wypozyczenia
                    (
                        sprzet_id,
                        osoba,
                        operator_id,
                        notatka
                    )
                    VALUES
                    (
                        @sprzet,
                        @osoba,
                        @operator,
                        @notatka
                    )";

                using var cmd1 =
                    new MySqlCommand(
                        sqlWypozyczenie,
                        conn,
                        (MySqlTransaction)transaction);

                cmd1.Parameters.AddWithValue(
                    "@sprzet",
                    sprzet.Id);

                cmd1.Parameters.AddWithValue(
                    "@osoba",
                    osoba);

                cmd1.Parameters.AddWithValue(
                    "@operator",
                    Session.OperatorId);

                cmd1.Parameters.AddWithValue(
                    "@notatka",
                    string.IsNullOrWhiteSpace(notatka)
                        ? DBNull.Value
                        : notatka);

                await cmd1.ExecuteNonQueryAsync();

                string sqlAktualizacja =
                    @"UPDATE sprzet
                      SET czy_wypozyczone = 1
                      WHERE id = @id";

                using var cmd2 =
                    new MySqlCommand(
                        sqlAktualizacja,
                        conn,
                        (MySqlTransaction)transaction);

                cmd2.Parameters.AddWithValue(
                    "@id",
                    sprzet.Id);

                await cmd2.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                KomunikatLabel.Text =
                    "Pomyœlnie wypo¿yczono sprzêt.";

                KomunikatLabel.TextColor =
                    Colors.Green;

                KomunikatLabel.IsVisible = true;

                await ZaladujSprzetAsync();
            } catch {
                await transaction.RollbackAsync();
                throw;
            }
        } catch (Exception ex) {
            KomunikatLabel.Text =
                ex.Message;

            KomunikatLabel.TextColor =
                Colors.Red;

            KomunikatLabel.IsVisible = true;
        }
    }

    private void WyczyscButton_Clicked(
        object sender,
        EventArgs e) {
        WyczyscFormularz();
    }

    private void WyczyscFormularz() {
        SprzetPicker.SelectedItem = null;
        DostepneCollection.SelectedItem = null;
        OsobaEntry.Text = "";
        NotatkaEditor.Text = "";
    }
}