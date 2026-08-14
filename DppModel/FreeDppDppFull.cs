using System.Text.Json.Serialization;

namespace freeDPPapi.DppModel
{
    public class FreeDppDppFull
    {
        /// <summary>
        /// DPP serialised in full serialisation described in EN 18223 Annex 1. 
        /// Note: for API request serialisation compressed, described in EN 18223 5.2 is default according EN 18222
        ///       compressed serialisation usese model in FreeDppDppCompressed.
        ///       
        /// The digital product passport is modelled as a collection that contains zero-to-many DataElementCollections and zero-to-many DataElements
        /// </summary>
        /// 

        public class DigitalProductPassport
        {
            public string digitalProductPassportId { get; set; } = "";
            public string uniqueProductIdentifier { get; set; } = "";

            public string granularity { get; set; } = "Model"; //batch, item
            public string dppSchemaVersion { get; set; } = "0.1";
            public string dppStatus { get; set; } = "Active";
            public DateTime lastUpdated { get; set; } = DateTime.Now;
            public string economicOperatorId { get; set; } = ""; // Guid.NewGuid();
            public string facilityId { get; set; } = ""; // Guid.NewGuid();
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string hashMD5 { get; set; } // not yet, to be handled when EN 18239 published; but must not be deleted because JS code needs it
            public List<String> contentSpecificationIds { get; set; } = new List<String>();

            public List<DataElementCollection> elements { get; set; } = new List<DataElementCollection>();
            // at least one
            // note: different in compressed serialisation
        }

        /// <summary>
        /// Collection of DataElements included in the digital product passport entity. It contains one-to-many DataElements.
        /// </summary>
        public class DataElementCollectionWithValues
        {
            public String elementId { get; set; } = string.Empty; 
            public string objectType { get; set; } = "DataElementCollection";
            public string dictionaryReference { get; set; } = string.Empty;
            public string valueDataType { get; set; } = "xsd:float"; // string.Empty; 
            // xsd:float, xsd:string, xsd:boolean, xsd:integer; nearly all of https://www.w3.org/TR/xmlschema-2/#builtin- datatypes


            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // dont show elements that are not filled in final JSON
            public object? value { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public List<DataElementCollectionWithValues>? elements { get; set; }
            // Note: compressed serialisation differs by individual named elements. See freeDppDppCompressed
            // the elements are dataElementCollections that may also include elements or dataElementCollections
        }
        public class DataElementCollection // in Root - Criteria - no elements given
        {  //oh260705 - check if DataElementCollectionWithValues is sufficient and this can be deleted
            public String elementId { get; set; } = string.Empty; 
            public string objectType { get; set; } = "DataElementCollection";
            public string dictionaryReference { get; set; } = string.Empty;
            public List<DataElementCollectionWithValues> elements { get; set; } = new List<DataElementCollectionWithValues>();
        }

        /// <summary>
        /// Represents a value associated to a MultivaluedDataElement instance
        /// </summary>
        public class ValueElement
        {
            public Guid elementId { get; set; } = Guid.NewGuid();
            public string dictionaryReference { get; set; } = "";
            public string objectType { get; set; } = "SingleValuedDataElement";// laut annex 1 
            public string value { get; set; } = string.Empty;
            public string valueDataType { get; set; } = string.Empty;
            // public string Language { get; set; } = string.Empty;
            // public string Unit { get; set; } = string.Empty;
        }
    }
}
