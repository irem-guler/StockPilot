using System.Text.Json;

namespace StockPilot.Web.Services
{
    public class DistanceResult
    {
        public double DistanceKm { get; set; }
        public double DurationMinutes { get; set; }
        public bool IsApproximate { get; set; }
    }

    public class DistanceService
    {
        private readonly HttpClient _httpClient;

        public DistanceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DistanceResult> GetDistanceAsync(
            double fromLat, double fromLng,
            double toLat, double toLng)
        {
            // Önce OSRM ile gerçek yol mesafesi dene
            try
            {
                var url =
                    $"https://router.project-osrm.org/route/v1/driving/" +
                    $"{fromLng.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                    $"{fromLat.ToString(System.Globalization.CultureInfo.InvariantCulture)};" +
                    $"{toLng.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                    $"{toLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                    $"?overview=false";

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(4));
                var response = await _httpClient.GetStringAsync(url, cts.Token);

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (root.GetProperty("code").GetString() == "Ok")
                {
                    var route = root.GetProperty("routes")[0];
                    var distanceMeters = route.GetProperty("distance").GetDouble();
                    var durationSeconds = route.GetProperty("duration").GetDouble();

                    return new DistanceResult
                    {
                        DistanceKm = Math.Round(distanceMeters / 1000, 1),
                        DurationMinutes = Math.Round(durationSeconds / 60, 0),
                        IsApproximate = false
                    };
                }
            }
            catch
            {
                // OSRM erişilemedi; kuş uçuşuna düşülecek
            }

            // Yedek: Haversine (kuş uçuşu) mesafe
            var haversineKm = CalculateHaversine(fromLat, fromLng, toLat, toLng);

            return new DistanceResult
            {
                DistanceKm = Math.Round(haversineKm, 1),
                DurationMinutes = 0,
                IsApproximate = true
            };
        }

        private double CalculateHaversine(
            double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}