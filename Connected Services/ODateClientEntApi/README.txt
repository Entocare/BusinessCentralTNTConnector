ECon: ODataClientEntApi.cs

Na genereren van de ODataClientEntApi.cs waren er 10 build fouten in die file, alle gelijk.
Die heb ik opgelost door 10 casts van een IDictionary naar een Dictionary middels:
    (System.Collections.Generic.Dictionary<string,object>)

..te plaatsen voor het rood onderstreekpte "_keys".

Dit moet je dus telkens herhalen als je de ".tt" template opnieuw runt, of de "OData Connected Service" generator upgraden
naar een versie die het probleem niet heeft (onduidelijk hoe dat moet).

Oeps.. nog meer narigheid: onderaan: dit codeblok heb ik toegevoegd aan de partial class ShippingPostalAddress
direct na de constructor, dus aan de gegenereerde code. Want in de aparte partial class zie je 't niet in de Designer.


BCCon: ODataClient1.cs

Deze file is nog gemaakt met de oude "OData ClientCode Generator" die voor VS 2019 niet meer werkt.
BCCon is nu niet in gebruik, maar de centrale connector klasse houdt wel rekening met een mogelijk toekomstig gebruik.
Er lopen dus wel referenties naar deze file. Als je hem weghaalt heb je build fouten.
Alvorens hem werkelijk in gebruik te nemen moet je hem upgraden naar de nieuwe versie van de MS BC API en daarbij
moet je onvermijdelijk ook upgraden naar een nieuwe versie code generator met een andere naam een iets andere filosofie.
Maar het komt goed, het is voor de ECon ook gelukt.
De gegenereerde klassen en hun methoden zijn wel dezelfde. De Namespace kun je kiezen in de settings en dus gelijk houden.


-- bijlage: zelfgeschreven code toegevoegd in file met gegenereerde code

        //Calculated properties added by Wolter 02-05-2021-- 

        public enum SumStatusses { Open, Released, ReadyForShip, Shipped, FullyShipped, Invoiced, Unknown }

        public bool HasLinesAndHasThemPicked
        {
            get
            {
                return ((bool)this.HasLines && !(bool)this.HasLinesUnpicked);
            }
        }

        public SumStatusses SumStatus
        {
            get
            {
                if (this.Status == "Open")
                    return SumStatusses.Open;
                else if (this.Invoice != null && (bool)this.Invoice)
                    return SumStatusses.Invoiced;
                else if (this.CompletelyShipped != null && (bool)this.CompletelyShipped)
                    return SumStatusses.FullyShipped;
                else if (this.HasLinesAndHasThemPicked)
                    return SumStatusses.ReadyForShip;
                else if (this.Shipped != null && (bool)this.Shipped)
                    return SumStatusses.Shipped;
                else if (this.Status == "Released")
                    return SumStatusses.Released;
                else
                    return SumStatusses.Unknown;
            }
        }

        //End of self written code block -------------------