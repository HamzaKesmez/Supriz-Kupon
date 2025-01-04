using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using System.Linq;
using SQLite;

namespace DenemeKupon
{
    public partial class Harita1 : ContentPage
    {
        private Microsoft.Maui.Controls.Maps.Map map;
        private List<Kupon> kupons;
        private KuponVeritabani kuponVeritabani;

        public Harita1()
        {
            InitializeComponent();
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "kuponlar.db3");
            kuponVeritabani = new KuponVeritabani(dbPath);
            kupons = new List<Kupon>(); // Kuponlar veritabanýndan yüklenecek
            SetupMap();
            LoadKuponsAsync(); // Veritabanýndan kuponlarý yükle
            StartLocationMonitoring();
        }

        private void SetupMap()
        {
            map = new Microsoft.Maui.Controls.Maps.Map
            {
                IsShowingUser = true
            };

            map.MapClicked += OnMapClicked;
            Content = map;
        }

        private async void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            if (MainPage.UserRole != "admin") return; // Yalnýzca admin kupon ekleyebilir

            var clickedPosition = e.Location;

            string kuponAdi = await DisplayPromptAsync("Kupon Ekle", "Kupon adýný giriniz:");
            if (string.IsNullOrWhiteSpace(kuponAdi)) return;

            var newKupon = new Kupon
            {
                Ad = kuponAdi,
                Enlem = clickedPosition.Latitude,
                Boylam = clickedPosition.Longitude
            };
            kupons.Add(newKupon);

            // Veritabanýna kaydet
            await kuponVeritabani.SaveKuponAsync(newKupon);

            var pin = new Pin
            {
                Label = newKupon.Ad,
                Location = new Location(newKupon.Enlem, newKupon.Boylam)
            };
            map.Pins.Add(pin);

            await DisplayAlert("Baþarýlý", $"{newKupon.Ad} kuponu eklendi!", "Tamam");
        }

        private async void LoadKuponsAsync()
        {
            var savedKupons = await kuponVeritabani.GetKuponsAsync();
            foreach (var kupon in savedKupons)
            {
                kupons.Add(kupon);

                var pin = new Pin
                {
                    Label = kupon.Ad,
                    Location = new Location(kupon.Enlem, kupon.Boylam)
                };
                map.Pins.Add(pin);
            }
        }

        private async void StartLocationMonitoring()
        {
            if (MainPage.UserRole == "admin") return; // Admin için konum takibi yapýlmaz

            while (true)
            {
                try
                {
                    var location = await Geolocation.GetLastKnownLocationAsync();
                    if (location == null)
                    {
                        var request = new GeolocationRequest(GeolocationAccuracy.Best);
                        location = await Geolocation.GetLocationAsync(request);
                    }

                    if (location != null)
                    {
                        CheckKuponProximity(location);
                    }
                }
                catch
                {
                    // Konum alýnamazsa bir þey yapma
                }

                await Task.Delay(5000); // 5 saniyede bir konumu kontrol et
            }
        }

        private async void CheckKuponProximity(Location currentLocation)
        {
            foreach (var kupon in kupons)
            {
                var kuponLocation = new Location(kupon.Enlem, kupon.Boylam);
                var distance = Location.CalculateDistance(currentLocation, kuponLocation, DistanceUnits.Kilometers);

                if (distance <= 1) // 1 km mesafede
                {
                    await DisplayAlert("Tebrikler!", $"{kupon.Ad} Kuponu kazandýnýz!", "Tamam");
                    kupons.Remove(kupon); // Kuponu listeden çýkar
                    await kuponVeritabani.DeleteKuponAsync(kupon); // Veritabanýndan sil
                    break;
                }
            }
        }

        public class Kupon
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Ad { get; set; }
            public double Enlem { get; set; }
            public double Boylam { get; set; }
        }
    }
}
