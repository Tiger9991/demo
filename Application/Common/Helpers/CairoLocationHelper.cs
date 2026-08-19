using System;

namespace Application.Common.Helpers
{
    public static class CairoLocationHelper
    {
        // Cairo Central Coordinates
        private const double BaseLatitude = 30.0444;
        private const double BaseLongitude = 31.2357;

        private static readonly Random _random = new();

        /// <summary>
        /// Generates a randomized coordinate within Greater Cairo.
        /// Each time this is called, it returns a different point.
        /// </summary>
        public static (double Latitude, double Longitude) GenerateRandomCairoCoordinate()
        {
            // Random offset between -0.05 and +0.05 degrees (~5.5 km radius)
            double latOffset = (_random.NextDouble() * 0.10) - 0.05;
            double lngOffset = (_random.NextDouble() * 0.10) - 0.05;

            double latitude = Math.Round(BaseLatitude + latOffset, 6);
            double longitude = Math.Round(BaseLongitude + lngOffset, 6);

            return (latitude, longitude);
        }

        /// <summary>
        /// Generates distributed coordinates across Cairo districts (Tagamoa, Heliopolis, Nasr City, Maadi, Zamalek, etc.)
        /// with a unique angular offset per trap so markers do not overlap.
        /// </summary>
        public static (double Latitude, double Longitude) GenerateDistributedCairoCoordinate(string? groupStr, string? trapStr)
        {
            int group = int.TryParse(groupStr, out var g) ? g : _random.Next(0, 10);
            int number = int.TryParse(trapStr, out var n) ? n : _random.Next(1, 20);

            // Cairo District Centers
            double groupLat = group switch
            {
                0 => 30.0074, // New Cairo / Tagamoa
                1 => 30.1026, // Heliopolis
                2 => 30.0566, // Nasr City
                3 => 29.9602, // Maadi
                4 => 30.0609, // Zamalek
                5 => 29.9853, // Giza / Pyramids
                6 => 30.0877, // Shoubra
                7 => 30.0207, // Mokattam
                8 => 30.0614, // Rehab City
                _ => 30.0444  // Cairo Downtown
            };

            double groupLng = group switch
            {
                0 => 31.4913,
                1 => 31.3326,
                2 => 31.3438,
                3 => 31.2569,
                4 => 31.2197,
                5 => 31.1386,
                6 => 31.2461,
                7 => 31.2882,
                8 => 31.4922,
                _ => 31.2357
            };

            // Circular spread angle and radius around the district center with a slight random jitter
            double angle = (number * 36) * Math.PI / 180.0;
            double radius = 0.005 + (_random.NextDouble() * 0.005);

            double lat = Math.Round(groupLat + (radius * Math.Sin(angle)), 6);
            double lng = Math.Round(groupLng + (radius * Math.Cos(angle)), 6);

            return (lat, lng);
        }
    }
}
