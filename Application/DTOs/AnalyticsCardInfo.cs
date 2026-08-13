using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs
{
    public class AnalyticsCardInfo
    {
        public string Title { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string BackgroundClass { get; set; } = string.Empty;
        public string IconColorClass { get; set; } = string.Empty;
        public string TextColorClass { get; set; } = string.Empty;
        public string IconBgClass { get; set; } = string.Empty;
        public string? TrendIcon { get; set; }

        public AnalyticsCardInfo(
            string title,
            int value,
            string icon,
            string bgClass,
            string iconBgClass,
            string insetClass,
            string? textColorClass = null,
            string? trendIcon = null)
        {
            Title = title;
            Value = value;
            Icon = icon;
            BackgroundClass = bgClass;
            IconBgClass = iconBgClass;
            TextColorClass = textColorClass ?? "text-dark";
            TrendIcon = trendIcon;
            // insetClass is used for internal styling if needed
        }
    }
}
