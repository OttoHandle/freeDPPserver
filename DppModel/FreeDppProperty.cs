namespace freeDPPapi.DppModel
{
    public class FreeDppProperty
    {
        public int Id { get; set; }
        public Guid ParamGUID { get; set; }
        public string WrittenName { get; set; } = string.Empty;
        public string ParamName { get; set; } = string.Empty;
        public string ParamDescription {  get; set; } = string.Empty;
        public string ParamValueType {  get; set; } = string.Empty;
        public byte IsValueList { get; set; }
        public FreeDppStandard? TestStandard { get; set; }
        public FreeDppStandard? Standard { get; set; }
        public decimal Bandwidth { get; set; }
        public DateTime CreatedTime {  get; set; }
        public bool IsUsuallyMandatory { get; set; }
        public bool IsUsuallyDynamic { get; set; }
        public FreeDppUnit? Unit { get; set; }
        public DateTimeOffset ActiveUntil { get; set; }
        public List<FreeDppValueListElement>? ValueList { get; set; }
    }
    public class FreeDppValueListElement
    {
        public int Id { get; set; }
        public Guid ValueListGUID { get; set; }
        public string ValueListName { get; set; } = string.Empty;
        public string ValueListValueType { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public FreeDppUnit? ValueListUnit { get; set; }
    }
    public class FreeDppUnit
    {
        public int Id { get; set; }
        public Guid UnitGUID { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string UnitDescription { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
    }
    public class FreeDppStandard
    {
        public int Id { get; set; }
        public Guid StandardGUID { get; set; }
        public string StandardName { get; set; } = string.Empty;
        public string StandardDescription { get; set; } = string.Empty;
        public string StandardReference { get; set; } = string.Empty;
        public string StandardFullReference { get; set; } = string.Empty;
        public DateTimeOffset StandardValidFrom { get; set; }
        public string StandardType { get; set; } = string.Empty;
        public string StandardUri { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public FreeDppStandard? ReplacesStandard { get; set; }
    }
}
