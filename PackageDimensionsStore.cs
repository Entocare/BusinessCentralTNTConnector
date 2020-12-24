using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessCentralTNTConnector
{
    /// <summary>
    /// Holds the defined Package Dimensions, as they are used in the TNT connector
    /// 
    /// The same Package dimensions should be used in Courier Price calculations.
    /// May be the information hardcoded here has to come from a database or from BC sooner or later
    /// </summary>
    public static class PackageDimensionsStore
    {
        /// <summary>
        /// Defined package dimensions, each having a code (like: "S", "M", "L", "XL")
        /// </summary>
        public static readonly Dictionary<string, PackageDimensions> Dict = new Dictionary<string, PackageDimensions>()
        {
            {"S",  new PackageDimensions(0.3, 0.2, 0.2, 0.3)},
            {"M",  new PackageDimensions(0.4, 0.3, 0.2, 0.6)},
            {"L",  new PackageDimensions(0.4, 0.3, 0.3, 0.9)},
            {"XL",  new PackageDimensions(0.6, 0.4, 0.35, 1.8)},
            {"XXL", new PackageDimensions(0.6, 0.4, 0.4, 2.1) }
        };

        /// <summary>
        /// length, widhth, height of a package in meters
        /// weight in kilograms
        /// </summary>
        public class PackageDimensions
        {
            public PackageDimensions(double length, double width, double height, double weight)
            {
                this.length = length;
                this.width = width;
                this.height = height;
                this.weight = weight;
                this.volume = length * width * height;  //calculated
            }
            public double length { get; private set; }
            public double width { get; private set; }
            public double height { get; private set; }
            public double weight { get; private set; }
            public double volume { get; private set; }
        }
    }
}
