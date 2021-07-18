using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessCentralTNTConnector
{
    /// <summary>
    /// This class knows which countries are in the EU.
    /// </summary>
    public static class EUCountries
    {
        /// <summary>
        /// Set of Counties in the EU
        /// </summary>
        public static readonly HashSet<string> EUCountries0 = new HashSet<string>()
            { "AT", "BE", "DE", "DK", "ES", "FI", "FR", "IT", "LU", "NL", "PL", "PT", "SE", "SK"};

        /// <summary>
        /// Check whether a given 2 letter CountryCode is in the EU
        /// </summary>
        public static bool inEU(string CountryCode)
        {
            return EUCountries0.Contains(CountryCode);
        }
    }
}
