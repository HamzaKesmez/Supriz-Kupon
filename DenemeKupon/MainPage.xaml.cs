using System;
using Microsoft.Maui.Controls;

namespace DenemeKupon
{
    public partial class MainPage : ContentPage
    {
        public static string UserRole { get; private set; } // Kullanıcı rolünü tutar

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text?.Trim();
            string password = PasswordEntry.Text?.Trim();

            if (username == "admin" && password == "1234")
            {
                UserRole = "admin";
                await Navigation.PushAsync(new Harita1());
            }
            else if (username == "hamza" && password == "1234")
            {
                UserRole = "customer";
                await Navigation.PushAsync(new Harita1());
            }
            else
            {
                ErrorLabel.Text = "Hatalı kullanıcı adı veya şifre!";
                ErrorLabel.IsVisible = true;
            }
        }
    }
}
