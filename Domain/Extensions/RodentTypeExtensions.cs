using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Extensions
{
    public static class RodentTypeExtensions
    {
        // Dictionary mapping enum values to Arabic display names
        private static readonly Dictionary<RodentType, string> ArabicNames = new()
        {
            { RodentType.NormalRat, "فار منزلى" },
            { RodentType.ClimbingRat, "جرذ متسلق" },
            { RodentType.NorwegianRat, "جرذ نرويجى" },
            { RodentType.Unknown, "غير معروف" }
        };

        /// <summary>
        /// Returns the Arabic display name for the rodent type.
        /// </summary>
        /// <param name="type">The rodent type enum value.</param>
        /// <returns>Arabic name (e.g., "فار منزلى" for NormalRat).</returns>
        public static string GetDisplayName(this RodentType type)
        {
            return ArabicNames.TryGetValue(type, out var name) ? name : type.ToString();
        }

        /// <summary>
        /// Tries to parse an Arabic name back to a RodentType.
        /// </summary>
        /// <param name="arabicName">The Arabic name (e.g., "فار منزلى").</param>
        /// <returns>The RodentType enum value, or null if not found.</returns>
        public static RodentType? ParseArabicName(string arabicName)
        {
            foreach (var pair in ArabicNames)
            {
                if (pair.Value == arabicName)
                    return pair.Key;
            }
            return null;
        }

        /// <summary>
        /// Tries to parse an English, Arabic, or enum name back to a RodentType.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <returns>The RodentType, or null if not parsed.</returns>
        public static RodentType? ParseFromString(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            if (Enum.TryParse<RodentType>(value, true, out var type))
                return type;

            var normalized = value.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "house mouse":
                case "normalrat":
                case "normal rat":
                case "فار منزلى":
                case "فأر منزلي":
                case "جرذ منزلي":
                    return RodentType.NormalRat;

                case "climbing rat":
                case "climbingrat":
                case "جرذ متسلق":
                    return RodentType.ClimbingRat;

                case "norwegian rat":
                case "norwegianrat":
                case "جرذ نرويجى":
                case "جرذ نرويجي":
                    return RodentType.NorwegianRat;

                case "unknown":
                case "غير معروف":
                    return RodentType.Unknown;
            }

            return ParseArabicName(value);
        }

        /// <summary>
        /// Returns all Arabic display names (for dropdowns, filters, etc.).
        /// </summary>
        public static IReadOnlyCollection<string> GetAllDisplayNames()
        {
            return ArabicNames.Values.ToList().AsReadOnly();
        }

        /// <summary>
        /// Returns a list of all rodent types with their Arabic names.
        /// </summary>
        public static IEnumerable<(RodentType Type, string ArabicName)> GetAllTypesWithNames()
        {
            foreach (var pair in ArabicNames)
            {
                yield return (pair.Key, pair.Value);
            }
        }
    }
}
