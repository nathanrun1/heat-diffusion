using System.Drawing;
using UnityEngine;

namespace Blizzard.Temperature
{
    public struct TemperatureCell
    {
        /// <summary>
        /// The current temperature of the cell
        /// </summary>
        public float temperature;
        /// <summary>
        /// How much heat is produced by this cell per second
        /// </summary>
        public float heatSource;
        /// <summary>
        /// How much this cell insulates heat (range from 0 -> 1)
        /// </summary>
        public float insulation;

        public static int GetSize()
        {
            return sizeof(float) * 3;
        }
    }
}

