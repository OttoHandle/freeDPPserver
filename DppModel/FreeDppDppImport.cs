namespace freeDPPapi.DppModel
{
    public class FreeDppDppImport
    {
        // this class is used for Imports of DPP via POST Method
        // it differs from the two output classes for some elements needed at import
        public string digitalProductPassportId { get; set; } = String.Empty;
        public string uniqueProductIdentifier { get; set; } = String.Empty;
        public string granularity { get; set; } = String.Empty;
        public string dppSchemaVersion { get; set; } = String.Empty;
        public string dppStatus { get; set; } = String.Empty;
        public DateTime lastUpdated { get; set; }
        public string economicOperatorId { get; set; } = String.Empty;
        public string facilityId { get; set; } = String.Empty;
        public string hashMD5 { get; set; } = String.Empty;
        public List<string>? contentSpecificationIds { get; set; }
        public List<FreeDppElement>? elements { get; set; }
    }
    public class FreeDppElement
    {
        public string elementId { get; set; } = String.Empty;
        public string objectType { get; set; } = String.Empty;
        public string dictionaryReference { get; set; } = String.Empty;
        public string language { get; set; } = String.Empty;
        public string valueDataType { get; set; } = String.Empty;
        public string value { get; set; } = String.Empty;
        public string resourceTitle { get; set; } = String.Empty;
        public string contentType { get; set; } = String.Empty;
        public string url { get; set; } = String.Empty;
        public List<FreeDppElement>? elements { get; set; }
        public Guid critGuid { get; set; }
        public Guid paramToCritGuid { get; set; }
        public Guid valueGuid { get; set; }
    }

    public class RepoCriteria
    {
        public Guid CritGuid { get; set; }
        public string CritShortName { get; set; } = string.Empty;
        public List<RepoParam> RepoParams { get; set; } = new();
    }

    public class RepoParam
    {
        public Guid ParamToCritGuid { get; set; }
        public Guid ParentParamToCritGuid { get; set; }
        public Guid CritGuid { get; set; }
        public string ParamName { get; set; } = string.Empty;
        public string ParamValueType { get; set; } = string.Empty;
        public Byte IsValueList { get; set; }
        public bool IsMandatory { get; set; }
        public List<RepoParam>? RepoParams { get; set; }
    }
}
