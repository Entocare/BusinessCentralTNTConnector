﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OData;
using Microsoft.OData.Client;
using MicrosoftBC;          //namespace van de gegenereerde ODataClient, hangt af van configuratie in *.tt file
using Entocare.NAV;

namespace BCConnector
{
    /// <summary>
    /// Connector for a particular Business Central instance, either the basic Microsoft provided API, OR
    /// a do-it-yourself API
    /// It has hardcoded:
    /// - the instance / tenant GUID, necessary for Basic Auth
    /// - the Basic Auth key
    /// - a default company ID to use on each request, until it is replaced using the SetCompany method
    /// - relative URL for (each of the) the do-it-yourself APIs (currently just 1), plus a base URL for BC itself
    /// 
    /// It uses the MS autogenerated OData clients, which looks for an .edmx file in a given location.
    /// (currently: C:\odata\metadata\businesscentral.edmx)
    /// 
    /// It has methods for:
    /// - Getting the list of companies (within our BC instance) and caching it for use in SetCompany
    /// - Setting the company to work with
    /// - Getting a ChangeListener that allows sending of really partial updates and inserts: only the set fields are sent
    ///   (The generated MS Clients default is to send everything, with value null or a default like "0000..." 
    ///   for guids, which MS BC does not like)
    /// </summary>
    public class BusinessCentralConnector
    {
        public enum APIs { MicrosoftBusinessCentralApi, EntocareBusinessCentralAPI };

        //singletons: one for each supported API
        private static BusinessCentralConnector MsCon = null;
        private static BusinessCentralConnector EntCon = null;

        //API BaseURL parts, in order of appearance
        public const string BCBaseURL = "https://api.businesscentral.dynamics.com/v1.0";
        public const string TenantGUID = "02a4d0db-d99f-46f0-b5de-073bf122086b";  //deliberately kept as string, not Guid
        public const string EntocareApiURL = "/Entocare/E/v1.0";

        //authentication
        private const string Username = "WOLTER";
        private const string SandboxPassword = "6Jur7Y7LyC8TsABVSbPpm+h4F78GQhKBxLVuh5xvbTQ=";
        private const string ProductionPassword = "N2Fdti6V7WYL08zBoLPu8QIhcKJPEq16JDAOZRbCoQs=";

        //companies
        //public static readonly Guid DefaultCompanyID = new Guid("8f689ff7-caa1-46ad-93b1-0865ea2873c3");  //Entocare LIVE
        public static readonly Guid DefaultCompanyID = new Guid("07abc4ce-1d83-ea11-a813-000d3aaf935d");    //Test april 2020
        public static readonly Guid DefaultCompanyIDSandbox = new Guid("cba76fc3-216c-ea11-a815-000d3aafcaa0"); //Entocare CV with OnValidate Eng
        public Guid CompanyId { get; private set; }               //Id of current company
        private Dictionary<string, MyCompany> companies = null;   //cache... only filled on demand

        /// <summary>
        /// Are requests being done in context of the above current company?
        /// True by default, after the constructor finishes, and set to true after each of the methods in this connector that involves companies.
        /// Should be false for requests that involve the company entityset itself, and maybe some other rare cases.
        /// </summary>
        public bool InCompanyContext { get; set; }

        public readonly bool Debug;
        public readonly APIs api;   //which of the apis does this object support?
        private Uri BaseURL;

        //contexts: one for each API that we support, only one of them will have a value, in any instance of this class
        public readonly MicrosoftBC.NAV Mctx = null;              //short name: its public and will be used often
        public readonly Entocare.NAV.NAV Ectx = null;

        //Current company, in two versions: only one of the two will have a value
        private MicrosoftBC.CompanySingle _MCompanySingle;
        public MicrosoftBC.CompanySingle MCompanySingle
        {
            get { if (_MCompanySingle != null) return _MCompanySingle; else throw new InvalidOperationException("CompanySingle: you chose the wrong subtype."); }
            private set { _MCompanySingle = value; }
        }

        private Entocare.NAV.CompanySingle _ECompanySingle;
        public Entocare.NAV.CompanySingle ECompanySingle
        {
            get { if (_ECompanySingle != null) return _ECompanySingle; else throw new InvalidOperationException("CompanySingle: you chose the wrong subtype."); }
            private set { _ECompanySingle = value; }
        }

        //Factory methods for each of the API's
        public static BusinessCentralConnector GetMicrosoftBCConnector(bool Debug = false)
        {
            if (MsCon == null)
            {
                MsCon = new BusinessCentralConnector(APIs.MicrosoftBusinessCentralApi, Debug);
            }
            return MsCon;
        }
        public static BusinessCentralConnector GetEntocareBCConnector(bool Debug = false)
        {
            if (EntCon == null)
            {
                EntCon = new BusinessCentralConnector(APIs.EntocareBusinessCentralAPI, Debug);
            }
            return EntCon;
        }

        /// <summary>
        /// Constructor: set up the connector with a base Url, authentication, and a default company within the base
        /// </summary>
        /// <param name="Debug">if true: connect to Sandbox instead of production</param>
        private BusinessCentralConnector(APIs api, bool Debug = false)
        {
            this.Debug = Debug;
            this.api = api;
            string password = Debug ? SandboxPassword : ProductionPassword;
            string basicauthStr = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Username}:{password}"));

            if (new Uri(BCBaseURL).Scheme != Uri.UriSchemeHttps)
            {
                throw new InvalidOperationException("Business Central requires https while we are using Basic Auth.");
            }
            string BaseURL0 = BCBaseURL + "/" + TenantGUID + 
                (Debug ? "/Sandbox" : "") +
                (api == APIs.MicrosoftBusinessCentralApi? "/api/beta" : "/api");
            switch (api)
            {
                case APIs.MicrosoftBusinessCentralApi:
                    BaseURL = new Uri(BaseURL0);        //the current company id will be added dynamically!
                    Mctx = new MicrosoftBC.NAV(BaseURL);
                    ContextAddEventHandlers(Mctx, basicauthStr);
                    Mctx.MergeOption = MergeOption.OverwriteChanges;
                    break;
                case APIs.EntocareBusinessCentralAPI:
                    BaseURL = new Uri(BaseURL0 + EntocareApiURL);
                    Ectx = new Entocare.NAV.NAV(BaseURL);
                    ContextAddEventHandlers(Ectx, basicauthStr);
                    Ectx.MergeOption = MergeOption.OverwriteChanges;
                    break;
            }

            SetCompany2(Debug ? DefaultCompanyIDSandbox : DefaultCompanyID); 
            InCompanyContext = true;
        }

        /// <summary>
        /// Choose a company to work with in following requests, from the list of available companies in the BC instance.
        /// </summary>
        /// <param name="name">Name of the company (may differ from displayName)</param>
        public async Task SetCompany(string name)
        {
            InCompanyContext = false;
            if (companies == null)
            {
                companies = await GetCompanies();
            }
            MyCompany comp;
            if (!companies.TryGetValue(name, out comp) )
            {
                throw new InvalidOperationException("Not a valid Company Name for this Business Central instance: " + name + "(Sandbox=" + Debug + ")");
            }
            SetCompany2(comp.Id);
            InCompanyContext = true;
        }

        /// <summary>
        /// Get a Dictionary of companies from the cache if possible, optionally refresh the cache.
        /// Dictionary is keyed by Name.
        /// </summary>
        /// <param name="refresh">Force a refresh?</param>
        /// <returns>the Dictionary</returns>
        public async Task<Dictionary<string, MyCompany>> GetCompanies(bool refresh = false)
        {
            if (companies == null | refresh)
            {
                InCompanyContext = false;     //we need to turn it off for the refresh
                Dictionary<string, MyCompany> comps = new Dictionary<string, MyCompany>();
                try
                {
                    switch (api)
                    {
                        case APIs.MicrosoftBusinessCentralApi:
                            var comps1 = await Mctx.Companies.GetAllPagesAsync();
                            foreach (MicrosoftBC.Company Mcomp in comps1) { AddToMyCompanyDictionary(comps, Mcomp); }
                            break;
                        case APIs.EntocareBusinessCentralAPI:
                            var comps2 = await Ectx.Companies.GetAllPagesAsync();
                            foreach (Entocare.NAV.Company Ecomp in comps2) { AddToMyCompanyDictionary(comps, null, Ecomp); }
                            break;
                    }
                    companies = comps;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + ex.InnerException);
                }
            }
            InCompanyContext = true;   //we promised to always turn it back on after doing something with companies.
            return companies;
        }

        /// <summary>
        /// Add one Company (a type from one of the OData clients) to a Dictionary of MyCompany objects.
        /// Only one of the "Comp" parameters should be non-null.
        /// </summary>
        /// <param name="companies">the Dictionary to add to</param>
        /// <param name="MComp">a Company to add, from the Microsoft BC API Client</param>
        /// <param name="EComp">a Company to add, from the Entocare BC API Client</param>
        private void AddToMyCompanyDictionary(Dictionary<string, MyCompany> companies, MicrosoftBC.Company MComp = null, Entocare.NAV.Company EComp = null)
        {
            //Cast the received Company "subtype" to the MyCompany "supertype"... sort of
            MyCompany comp = null;
            if (MComp != null)      comp = new MyCompany(MComp);
            else if (EComp != null) comp = new MyCompany(EComp);
            else throw new InvalidOperationException("Error building up the Company Dictionary: one of the two Company subtypes should be supplied.");

            //Add it to the Dictionary
            if (!companies.ContainsKey(comp.Name))
            {
                companies.Add(comp.Name, comp);
            }
            else
            {
                throw new InvalidOperationException("Company name " + comp.Name + " is not unique within this BC instance (Sandbox=" + Debug + ")");
            }
        }

        /// <summary>
        /// Wrapper for the various "Company" classes that we have to deal with: one for each BC-API! (currently: 2)
        /// It tries to emulate a superclass... sort of.
        /// </summary>
        public class MyCompany
        {
            //Constructors take care that one and only one of the following has a value
            private MicrosoftBC.Company _MCompany = null;
            private Entocare.NAV.Company _ECompany = null;

            public MyCompany(MicrosoftBC.Company MCompany)
            {
                this._MCompany = MCompany;
            }
            public MyCompany(Entocare.NAV.Company ECompany)
            {
                this._ECompany = ECompany;
            }

            //Getters for the wrapped "subtype" instances, you got to know which one you want
            public MicrosoftBC.Company MCompany { get { if (_MCompany != null) return _MCompany; else throw new InvalidOperationException("MyCompany: sorry, you asked for the wrong one."); } }
            public Entocare.NAV.Company ECompany { get { if (_ECompany != null) return _ECompany; else throw new InvalidOperationException("MyCompany: sorry, you asked for the wrong one."); } }

            //Friendly getters for the primitive properties
            public Guid Id
            {
                get
                {
                    if (MCompany != null) return _MCompany.Id;
                    else if (ECompany != null) return _ECompany.Id;
                    else throw new InvalidOperationException("MyCompany: constructors should guarantee that one of the subtype instances has a value.");
                }
            }
            public string SystemVersion
            {
                get
                {
                    if (MCompany != null) return _MCompany.SystemVersion;
                    else if (ECompany != null) return _ECompany.SystemVersion;
                    else throw new InvalidOperationException("MyCompany: constructors should guarantee that one of the subtype instances has a value.");
                }
            }
            public string Name
            {
                get
                {
                    if (MCompany != null) return _MCompany.Name;
                    else if (ECompany != null) return _ECompany.Name;
                    else throw new InvalidOperationException("MyCompany: constructors should guarantee that one of the subtype instances has a value.");
                }
            }
            public string DisplayName
            {
                get
                {
                    if (MCompany != null) return _MCompany.DisplayName;
                    else if (ECompany != null) return _ECompany.DisplayName;
                    else throw new InvalidOperationException("MyCompany: constructors should guarantee that one of the subtype instances has a value.");
                }
            }
            public string BusinessProfileId
            {
                get
                {
                    if (MCompany != null) return _MCompany.BusinessProfileId;
                    else if (ECompany != null) return _ECompany.BusinessProfileId;
                    else throw new InvalidOperationException("MyCompany: constructors should guarantee that one of the subtype instances has a value.");
                }
            }
        }

        /// <summary>
        /// Wrap an object in a ChangeListener to send only changed properties to Business Central.
        /// Usage:
        /// 1 - Connect the empty object to other objects using AddRelatedObject BEFORE wrapping it here
        /// 2 - Set object properties AFTER wrapping
        /// 3 - Call SaveChanges on the created object (instead of on the context)
        /// You can add a second, third... object and call SaveChanges only once.
        /// </summary>
        /// <typeparam name="T">Type of object(s) to wrap</typeparam>
        /// <param name="obj">The first object to wrap</param>
        /// <returns>the ChangeListener</returns>
        public ChangeListener<T> CreateChangeListener<T>(T obj)
        {
            return new ChangeListener<T>(obj, this);
        }

        /// <summary>
        /// Logically group two tricks to enable sending of only the set properties:
        /// - DataServiceCollection
        /// - SaveChanges with "PostOnlySetProperties" option
        /// </summary>
        /// <typeparam name="T">Type of object(s) to wrap</typeparam>
        public class ChangeListener<T>
        {
            //private BusinessCentralConnector owner;
            private DataServiceContext ctx;
            private DataServiceCollection<T> collection;

            public ChangeListener(BusinessCentralConnector owner)
            {
                switch (owner.api)
                {
                    case APIs.MicrosoftBusinessCentralApi:
                        this.ctx = owner.Mctx;
                        break;
                    case APIs.EntocareBusinessCentralAPI:
                        this.ctx = owner.Ectx;
                        break;
                }
                this.collection = new DataServiceCollection<T>(ctx);
            }

            public ChangeListener(T obj, BusinessCentralConnector owner) : this(owner)
            {
                collection.Add(obj);
            }

            public void Add(T obj)
            {
                collection.Add(obj);
            }

            public async Task<DataServiceResponse> SaveChangesAsync()
            {
                return await ctx.SaveChangesAsync(SaveChangesOptions.PostOnlySetProperties);
            }
        }

        /// <summary>
        /// Extension to the constructor of this class: wire up one of the supported contexts with needed event handlers
        /// </summary>
        /// <param name="ctx">One of the supported context singletons</param>
        private void ContextAddEventHandlers(DataServiceContext ctx, string basicauthStr)
        {
            ctx.AddAndUpdateResponsePreference = DataServiceResponsePreference.NoContent;
            ctx.BuildingRequest += new EventHandler<BuildingRequestEventArgs>(delegate (object sender, BuildingRequestEventArgs e)
            {
                if (InCompanyContext)
                {
                    this.IncludeCompanyInUrl(e);
                }
            });
            ctx.SendingRequest2 += new EventHandler<SendingRequest2EventArgs>(delegate (object sender, SendingRequest2EventArgs e)
            {
                e.RequestMessage.SetHeader("Authorization", basicauthStr);
            });
        }

        /// <summary>
        /// Takes care that the 2 cached representations of the current company are in sync
        /// </summary>
        /// <param name="id">ID of the chosen company</param>
        private void SetCompany2(Guid id)
        {
            CompanyId = id;  //representation 1: the id

            //representation 2: one of the API specific CompanySingle variables
            switch (api)
            {
                case APIs.MicrosoftBusinessCentralApi:
                    MCompanySingle = Mctx.Companies.ByKey(id);    //InCompanyContext must be false for such a query
                    break;
                case APIs.EntocareBusinessCentralAPI:
                    ECompanySingle = Ectx.Companies.ByKey(id); 
                    break;
            }
        }

        //Trick to make OData "Add" operations work in the context of the chosen company. None of the generated OData "add" methods have these
        //context aware URLs... But then we might just as well follow the BC advice to do almost all requests in the context of a specific
        //company even though it is not technically necessary. For add operations it clearly IS !!!

        /// <summary>
        /// Extend the BaseURL dynamically to include the current company.
        /// </summary>
        /// <param name="e">OData properties for the request that is about to be built: you can change them now.</param>
        private void IncludeCompanyInUrl(BuildingRequestEventArgs e)
        {
            string url = e.RequestUri.AbsoluteUri;
            if (InCompanyContext && !url.Contains("/companies(")) 
            {
                string leftpart = BaseURL.AbsoluteUri;
                string companiesclause = "/companies(" + CompanyId + ")";
                string rightpart = url.Substring(leftpart.Length);
                url = leftpart + companiesclause + rightpart;
                e.RequestUri = new Uri(url);
            }
        }
    }
}
