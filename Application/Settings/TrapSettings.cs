using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Settings
{
    public class TrapSettings
    {
        /// </summary>
        public double ConnectivityThresholdHours { get; set; } = 2.0;

        /// <summary>
        /// Minimum allowed threshold in hours (prevents overly strict detection).
        /// </summary>
        public double MinimumThresholdHours { get; set; } = 0.5;

        /// <summary>
        /// Maximum allowed threshold in hours (prevents overly lenient detection).
        /// </summary>
        public double MaximumThresholdHours { get; set; } = 6.0;

        /// <summary>
        /// Multiplier applied to average interval to calculate adaptive threshold.
        /// </summary>
        public double AdaptiveMultiplier { get; set; } = 1.5;

        /// <summary>
        /// Grace period (in hours) for a new/just‑reset trap to be considered connected
        /// even if it has no activity records yet.
        /// </summary>
        public double NewTrapGracePeriodHours { get; set; } = 0.5;
    }
}
