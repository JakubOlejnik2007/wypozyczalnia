namespace Wypozyczalnia;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    private async void WylogujButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//login");

    }
}
