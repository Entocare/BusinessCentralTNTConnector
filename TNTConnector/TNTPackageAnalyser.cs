using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Threading.Tasks;
using BusinessCentralTNTConnector;

namespace BusinessCentralTNTConnector.TNTConnector
{
    /// <summary>
    /// Given an Entocare PackageCode like "M" or "M+M" or "M+S" this class will analyse it and prepare the following summary 
    /// information about the packages:
    /// - total item count
    /// - total weight
    /// - total volume
    /// The iterator returns KeyValuePairs having a Simple Packgecode (like M) as a key and the number of times it occured as value
    /// 
    /// PackageCodes like "M+M" are split in the plus "+" sign.
    /// The meaning of the elementary symbols (like "S", "M", "XL") is then looked up in the Package Dimensions Dictionary that gives
    /// for each symbol: length, widhth, height, volume (calculated) and weight. 
    /// </summary>
    public class TNTPackageAnalyser : IEnumerable<TNTPackageAnalyser.PackageMultiple>
    {
        /// <summary>
        /// Defined package dimensions, each having a code (like: "S", "M", "L", "XL")
        /// Filled at initialisation.
        /// </summary>
        private readonly Dictionary<string, PackageDimensionsStore.PackageDimensions> PackageDimensionsDict;

        //The packages for a single consignment, each with its multiplicity, to be iterated in the IEnumerator
        private Dictionary<string, PackageMultiple> PackageMultiples;

        //Document that the package nodes are destined for
        MyXMLDocument mydoc;

        //Summary information available after a call to MakePackages
        private bool OutsideEU;
        private double InvoiceValue;
        public double ItemInvoiceValue { get; private set; }
        public int TotalItems { get; private set; }
        public double TotalWeight { get; private set; }
        public double TotalVolume { get; private set; }

        /// <summary>
        /// Constructor: loads configuration information
        /// </summary>
        public TNTPackageAnalyser()
        {
            PackageDimensionsDict = PackageDimensionsStore.Dict;
        }

        /// <summary>
        /// Analyse the package code for a single consignment. Calculate totals for the consignment based on that.
        /// Prepare for iterating the xml package elements one by one.
        /// </summary>
        /// <param name="PackageCode"></param>
        /// <param name="OutsideEU"></param>
        /// <param name="InvoiceValue"></param>
        public void MakePackages(string PackageCode, bool OutsideEU, double InvoiceValue, MyXMLDocument mydoc)
        {
            //start fresh
            PackageMultiples = new Dictionary<string, PackageMultiple>();
            this.OutsideEU = OutsideEU;
            this.InvoiceValue = InvoiceValue;
            this.mydoc = mydoc;
            this.TotalItems = 0;
            this.TotalWeight = 0;
            this.TotalVolume = 0;

            //analyse package code
            string[] symbols = PackageCode.Split('+');
            foreach (string s in symbols)
            {
                PackageDimensionsStore.PackageDimensions pak;
                if (!PackageDimensionsDict.TryGetValue(s, out pak) )
                {
                    throw new InvalidOperationException("TNTPackageAnalyser: symbol " + s + ", as occuring in " + PackageCode + ", not found in Package Dimensions Dictionary.");
                }
                if (!PackageMultiples.ContainsKey(s))
                {
                    PackageMultiples.Add(s, new PackageMultiple(s, 1, pak) );
                }
                else
                {
                    PackageMultiples[s].AddOne();
                }
                TotalItems++;
                TotalWeight += pak.weight;
                TotalVolume += pak.volume;
            }
            ItemInvoiceValue = InvoiceValue / TotalItems;
        }

        public IEnumerator<PackageMultiple> GetEnumerator()
        {
            foreach (var m in PackageMultiples)
            {
                yield return m.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public class PackageMultiple
        {
            public string Code { get; private set; }
            public int HowMany { get; private set; }
            public PackageDimensionsStore.PackageDimensions Package { get; private set; }

            public PackageMultiple(string Code, int HowMany, PackageDimensionsStore.PackageDimensions Package)
            {
                this.Code = Code;
                this.HowMany = HowMany;
                this.Package = Package;
            }
            public void AddOne()
            {
                HowMany++;
            }
        }
    }
}
