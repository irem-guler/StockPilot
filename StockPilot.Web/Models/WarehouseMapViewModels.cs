namespace StockPilot.Web.Models
{
    public class WarehouseMapPoint
    {
        public int WarehouseId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int TotalStock { get; set; }

        public int ProductCount { get; set; }

        public int CriticalCount { get; set; }
    }

    public class TransferFlow
    {
        public double FromLatitude { get; set; }
        public double FromLongitude { get; set; }
        public double ToLatitude { get; set; }
        public double ToLongitude { get; set; }
        public string FromName { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public int TransferCount { get; set; }
    }

    public class TransferSuggestion
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;

        public string FromWarehouseName { get; set; } = string.Empty;
        public string ToWarehouseName { get; set; } = string.Empty;

        public int SuggestedQuantity { get; set; }
        public int SourceAvailable { get; set; }
        public int TargetCurrent { get; set; }

        public double DistanceKm { get; set; }
        public double DurationMinutes { get; set; }
        public bool IsApproximate { get; set; }

        public double FromLat { get; set; }
        public double FromLng { get; set; }
        public double ToLat { get; set; }
        public double ToLng { get; set; }
    }

    public class WarehouseMapViewModel
    {
        public List<WarehouseMapPoint> Points { get; set; } = new();
        public List<TransferFlow> Flows { get; set; } = new();
        public List<TransferSuggestion> Suggestions { get; set; } = new();
    }
}