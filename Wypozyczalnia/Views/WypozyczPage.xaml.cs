
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace Wypozyczalnia.Views;

public partial class WypozyczPage : ContentPage
{
    private class Sprzet
    {
        public int id { get; set; }
        public string nazwa { get; set; }

        public string NazwaZKodem => $"{id} - {nazwa}";

        public Sprzet(int Id, string Nazwa)
        {
            this.id = Id;
            this.nazwa = Nazwa;
        }
    }

    private class Wypozyczenie
    {
        public int id { get; set; }
        public int idSprzetu { get; set; }
        public string osoba { get; set; }
        public DateTime data { get; set; }
        public string notatka { get; set; }
        public bool Aktywne { get; set; } = true;

        public Wypozyczenie(int id, int idSprzetu, string osoba, string notatka)
        {
            Id = id;
            this.idSprzetu = idSprzetu;
            this.osoba = osoba;
            this.notatka = notatka;
            data = DateTime.Now;
        }
    }

    private readonly List<Sprzet> sprzety = new()
    {
        new Sprzet(1, "Laptop"),
        new Sprzet(2, "Projektor"),
        new Sprzet(3, "Aparat"),
        new Sprzet(4, "Mikrofon")
    };

    private readonly List<Wypozyczenie> historia = new();

    private int nastepneId = 1;

    private List<Sprzet> DostepneSprzety()
    {
        return sprzety
            .Where(s => !historia.Any(w =>
                w.idSprzetu == s.id && w.Aktywne))
            .ToList();
    }

    public WypozyczPage()
    {
        InitializeComponent();

        WypozyczButton.Clicked += WypozyczButton_Clicked;
        WyczyscButton.Clicked += WyczyscButton_Clicked;

        DostepneCollection.SelectionChanged +=
            DostepneCollection_SelectionChanged;

        OdswiezListe();
    }

    private void OdswiezListe()
    {
        var dostepne = DostepneSprzety();

        SprzetPicker.ItemsSource = dostepne;
        DostepneCollection.ItemsSource = dostepne;
    }

    private void DostepneCollection_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Sprzet wybrany)
        {
            SprzetPicker.SelectedItem = wybrany;
        }

        DostepneCollection.SelectedItem = null;
    }

    private void WypozyczButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (SprzetPicker.SelectedItem is not Sprzet wybranySprzet)
            {
                PokazKomunikat("Wybierz sprzêt do wypo¿yczenia.", true);
                return;
            }

            string osoba = OsobaEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(osoba))
            {
                PokazKomunikat(
                    "Podaj imiê i nazwisko osoby wypo¿yczaj¹cej.", true);
                return;
            }

            if (historia.Any(w =>
                w.idSprzetu == wybranySprzet.id && w.Aktywne))
            {
                PokazKomunikat("Ten sprzêt jest ju¿ wypo¿yczony.", true);
                OdswiezListe();
                return;
            }

            string notatka = NotatkaEditor.Text?.Trim() ?? "";

            var wypozyczenie = new Wypozyczenie(
                nastepneId++,
                wybranySprzet.id,
                osoba,
                notatka
            );

            historia.Add(wypozyczenie);

            PokazKomunikat(
                $"Pomyœlnie wypo¿yczono {wybranySprzet.nazwa}. " +
                $"ID wypo¿yczenia: {wypozyczenie.Id}",
                false
            );

            SprzetPicker.SelectedItem = null;
            OsobaEntry.Text = "";
            NotatkaEditor.Text = "";

            OdswiezListe();
        }
        catch (Exception ex)
        {
            PokazKomunikat(ex.Message, true);
        }
    }

    private void WyczyscButton_Clicked(object sender, EventArgs e)
    {
        SprzetPicker.SelectedItem = null;
        OsobaEntry.Text = "";
        NotatkaEditor.Text = "";

        KomunikatLabel.Text = "";
        KomunikatLabel.IsVisible = false;

        DostepneCollection.SelectedItem = null;
    }

    private void PokazKomunikat(string tekst, bool blad)
    {
        KomunikatLabel.Text = tekst;
        KomunikatLabel.TextColor = blad
            ? Colors.Red
            : Colors.Green;

        KomunikatLabel.IsVisible = true;
    }
}
