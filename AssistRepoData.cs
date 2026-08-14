using freeDPPapi.DppModel;
using System.Data;

namespace freeDPPapi
{
    /// <summary>
    /// helper class for creating properties in temporary repositor
    /// used by propertyControler.cs and propertySectorController.cs
    /// to be reworked completely when final standards on dictionary referencing exist
    /// </summary>
    public class AssistRepoData
    {
        private apiModel _apiModel;

        public AssistRepoData(apiModel apiModel)
        {
            _apiModel = apiModel;
        }

        /// <summary>
        /// Returns one property
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public FreeDppProperty GetProperty(string paramName)
        {
            string sqlQueryProperty = @"SELECT
                p.id, p.paramGUID, p.paramName, ISNULL(t.TransDescription, p.paramDescription) AS paramDescription, p.paramValueType, p.isValueList, p.teststandardGUID, p.standardGUID, p.bandwidth, p.createdTime, p.isUsuallyMandatory, p.isUsuallyDynamic, p.unitGUID, ISNULL(t.TransName, p.writtenName) AS writtenName, p.activeUntil,

                s.id AS s_id, s.standardGUID AS s_standardGUID, ISNULL(t2.TransName, s.standardName) AS s_standardName, ISNULL(t2.TransDescription, s.standardDescription) AS s_standardDescription, s.standardReference AS s_standardReference, s.standardFullReference AS s_standardFullReference, s.standardValidFrom AS s_standardValidFrom, s.standardtype AS s_standardtype, s.standardUri AS s_standardUri, s.createdTime AS s_createdTime, s.replacesStandardGuid AS s_replacesStandardGuid,

                s2.id AS s2_id, s2.standardGUID AS s2_standardGUID, ISNULL(t3.TransName, s2.standardName) AS s2_standardName, ISNULL(t3.TransDescription, s2.standardDescription) AS s2_standardDescription, s2.standardReference AS s2_standardReference, s2.standardFullReference AS s2_standardFullReference, s2.standardValidFrom AS s2_standardValidFrom, s2.standardtype AS s2_standardtype, s2.standardUri AS s2_standardUri, s2.createdTime AS s2_createdTime, s2.replacesStandardGuid AS s2_replacesStandardGuid,

                s3.id AS s3_id, s3.standardGUID AS s3_standardGUID, ISNULL(t4.TransName, s3.standardName) AS s3_standardName, ISNULL(t4.TransDescription, s3.standardDescription) AS s3_standardDescription, s3.standardReference AS s3_standardReference, s3.standardFullReference AS s3_standardFullReference, s3.standardValidFrom AS s3_standardValidFrom, s3.standardtype AS s3_standardtype, s3.standardUri AS s3_standardUri, s3.createdTime AS s3_createdTime, s3.replacesStandardGuid AS s3_replacesStandardGuid,

                s4.id AS s4_id, s4.standardGUID AS s4_standardGUID, ISNULL(t5.TransName, s4.standardName) AS s4_standardName, ISNULL(t5.TransDescription, s4.standardDescription) AS s4_standardDescription, s4.standardReference AS s4_standardReference, s4.standardFullReference AS s4_standardFullReference, s4.standardValidFrom AS s4_standardValidFrom, s4.standardtype AS s4_standardtype, s4.standardUri AS s4_standardUri, s4.createdTime AS s4_createdTime, s4.replacesStandardGuid AS s4_replacesStandardGuid,

                u.id AS u_id, u.unitGUID AS u_unitGUID, ISNULL(t6.TransName, u.unitName) AS u_unitName, u.unit AS u_unit, ISNULL(t6.TransDescription, u.unitDescription) AS u_unitDescription, u.createdTime AS u_createdTime,

                vl.id AS vl_id, vl.valueListGUID AS vl_valueListGUID, ISNULL(t7.TransName, vl.valueListName) AS vl_valueListName, vl.valueListValueType AS vl_valueListValueType, vl.createdTime AS vl_createdTime, vl.valueListUnit AS vl_valueListUnit,

                u2.id AS u2_id, u2.unitGUID AS u2_unitGUID, ISNULL(t8.TransName, u2.unitName) AS u2_unitName, u2.unit AS u2_unit, ISNULL(t8.TransDescription, u2.unitDescription) AS u2_unitDescription, u2.createdTime AS u2_createdTime

                FROM " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoParam p
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoStandard s ON s.standardGUID = p.teststandardGUID
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoStandard s2 ON s2.standardGUID = p.standardGUID
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoStandard s3 ON s3.standardGUID = s.replacesStandardGuid
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoStandard s4 ON s4.standardGUID = s2.replacesStandardGuid
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoUnit u ON u.unitGUID = p.unitGUID
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoValue2Param v2p ON v2p.paramGUID = p.paramGUID
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoValueList vl ON vl.valueListGUID = v2p.valueGUID
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoUnit u2 ON u2.unitGUID = vl.valueListUnit
                --Translations
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t ON p.paramGUID = t.sourceGUID AND t.sourceTable = 'dppRepoParam' AND t.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t2 ON s.standardGUID = t2.sourceGUID AND t2.sourceTable = 'dppRepoStandard' AND t2.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t3 ON s2.standardGUID = t3.sourceGUID AND t3.sourceTable = 'dppRepoStandard' AND t3.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t4 ON s3.standardGUID = t4.sourceGUID AND t4.sourceTable = 'dppRepoStandard' AND t4.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t5 ON s4.standardGUID = t5.sourceGUID AND t5.sourceTable = 'dppRepoStandard' AND t5.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t6 ON u.unitGUID = t6.sourceGUID AND t6.sourceTable = 'dppRepoUnit' AND t6.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t7 ON vl.valueListGUID = t7.sourceGUID AND t7.sourceTable = 'dppRepoValueList' AND t7.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t8 ON u2.unitGUID = t8.sourceGUID AND t8.sourceTable = 'dppRepoUnit' AND t8.langcode = @param2
                WHERE p.paramName = @param1";

            DataTable dtFreeDppProperties = new DataTable();
            dtFreeDppProperties = AssistInclude.FuncGetSQLtable(ref _apiModel, sqlQueryProperty, paramName, _apiModel.Sprache);

            FreeDppProperty? freeDppProperty = GetPropertiesFromDataTable(dtFreeDppProperties).FirstOrDefault();

            if(freeDppProperty is null)
            {
                // Search for criteria if no parameter is found
                string sqlQueryCriterias = @"SELECT
                c.id, c.critGUID, c.clause, ISNULL(IIF(LEN(t.TransName) > 0, t.TransName, c.critName), c.critName) AS critName, ISNULL(IIF(LEN(t.TransDescription) > 0, t.TransDescription, c.critDescription), c.critDescription) AS critDescription, c.critShortName, c.createdTime
                FROM " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoCriteria c
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t ON c.critGUID = t.sourceGUID AND t.sourceTable = 'dppRepoCriteria' AND t.langcode = @param2
                WHERE c.critShortName = @param1";

                DataTable dtFreeDppCriterias = new DataTable();
                dtFreeDppCriterias = AssistInclude.FuncGetSQLtable(ref _apiModel, sqlQueryCriterias, paramName, _apiModel.Sprache);
                freeDppProperty = GetCriteriaFromDataTable(dtFreeDppCriterias).FirstOrDefault() ?? new FreeDppProperty();
            }

            return freeDppProperty;
        }

        /// <summary>
        /// Returns all properties from a sector
        /// </summary>
        /// <param name="sectorName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<FreeDppProperty> GetPropertiesFromSector(string sectorName)
        {
            // Get all paremeter from sector and all global parameter
            // TODO: Value List exclude from sector and p2c is not implementet yet
            string sqlQueryProperties = @"
                SELECT DISTINCT
                p.id, p.paramGUID, p.paramName, ISNULL(t.TransDescription, p.paramDescription) AS paramDescription, p.paramValueType, p.isValueList, p.teststandardGUID, p.standardGUID, p.bandwidth, p.createdTime, p.isUsuallyMandatory, p.isUsuallyDynamic, p.unitGUID, ISNULL(t.TransName, p.writtenName) AS writtenName, p.activeUntil,

                s.id AS s_id, s.standardGUID AS s_standardGUID, ISNULL(t2.TransName, s.standardName) AS s_standardName, ISNULL(t2.TransDescription, s.standardDescription) AS s_standardDescription, s.standardReference AS s_standardReference, s.standardFullReference AS s_standardFullReference, s.standardValidFrom AS s_standardValidFrom, s.standardtype AS s_standardtype, s.standardUri AS s_standardUri, s.createdTime AS s_createdTime, s.replacesStandardGuid AS s_replacesStandardGuid,

                s2.id AS s2_id, s2.standardGUID AS s2_standardGUID, ISNULL(t3.TransName, s2.standardName) AS s2_standardName, ISNULL(t3.TransDescription, s2.standardDescription) AS s2_standardDescription, s2.standardReference AS s2_standardReference, s2.standardFullReference AS s2_standardFullReference, s2.standardValidFrom AS s2_standardValidFrom, s2.standardtype AS s2_standardtype, s2.standardUri AS s2_standardUri, s2.createdTime AS s2_createdTime, s2.replacesStandardGuid AS s2_replacesStandardGuid,

                s3.id AS s3_id, s3.standardGUID AS s3_standardGUID, ISNULL(t4.TransName, s3.standardName) AS s3_standardName, ISNULL(t4.TransDescription, s3.standardDescription) AS s3_standardDescription, s3.standardReference AS s3_standardReference, s3.standardFullReference AS s3_standardFullReference, s3.standardValidFrom AS s3_standardValidFrom, s3.standardtype AS s3_standardtype, s3.standardUri AS s3_standardUri, s3.createdTime AS s3_createdTime, s3.replacesStandardGuid AS s3_replacesStandardGuid,

                s4.id AS s4_id, s4.standardGUID AS s4_standardGUID, ISNULL(t5.TransName, s4.standardName) AS s4_standardName, ISNULL(t5.TransDescription, s4.standardDescription) AS s4_standardDescription, s4.standardReference AS s4_standardReference, s4.standardFullReference AS s4_standardFullReference, s4.standardValidFrom AS s4_standardValidFrom, s4.standardtype AS s4_standardtype, s4.standardUri AS s4_standardUri, s4.createdTime AS s4_createdTime, s4.replacesStandardGuid AS s4_replacesStandardGuid,

                u.id AS u_id, u.unitGUID AS u_unitGUID, ISNULL(t6.TransName, u.unitName) AS u_unitName, u.unit AS u_unit, ISNULL(t6.TransDescription, u.unitDescription) AS u_unitDescription, u.createdTime AS u_createdTime,

                vl.id AS vl_id, vl.valueListGUID AS vl_valueListGUID, ISNULL(t7.TransName, vl.valueListName) AS vl_valueListName, vl.valueListValueType AS vl_valueListValueType, vl.createdTime AS vl_createdTime, vl.valueListUnit AS vl_valueListUnit,

                u2.id AS u2_id, u2.unitGUID AS u2_unitGUID, ISNULL(t8.TransName, u2.unitName) AS u2_unitName, u2.unit AS u2_unit, ISNULL(t8.TransDescription, u2.unitDescription) AS u2_unitDescription, u2.createdTime AS u2_createdTime

                FROM " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoParam p
                JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoParam2Crit p2c ON p2c.paramGUID = p.paramGUID
                JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoCriteria c ON c.critGUID = p2c.critGUID
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoStandard s ON s.standardGUID = p.teststandardGUID
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoStandard s2 ON s2.standardGUID = p.standardGUID
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoStandard s3 ON s3.standardGUID = s.replacesStandardGuid
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoStandard s4 ON s4.standardGUID = s2.replacesStandardGuid
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoUnit u ON u.unitGUID = p.unitGUID
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoValue2Param v2p ON v2p.paramGUID = p.paramGUID
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoValueList vl ON vl.valueListGUID = v2p.valueGUID
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoUnit u2 ON u2.unitGUID = vl.valueListUnit
                --Translations
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t ON p.paramGUID = t.sourceGUID AND t.sourceTable = 'dppRepoParam' AND t.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t2 ON s.standardGUID = t2.sourceGUID AND t2.sourceTable = 'dppRepoStandard' AND t2.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t3 ON s2.standardGUID = t3.sourceGUID AND t3.sourceTable = 'dppRepoStandard' AND t3.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t4 ON s3.standardGUID = t4.sourceGUID AND t4.sourceTable = 'dppRepoStandard' AND t4.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t5 ON s4.standardGUID = t5.sourceGUID AND t5.sourceTable = 'dppRepoStandard' AND t5.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t6 ON u.unitGUID = t6.sourceGUID AND t6.sourceTable = 'dppRepoUnit' AND t6.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t7 ON vl.valueListGUID = t7.sourceGUID AND t7.sourceTable = 'dppRepoValueList' AND t7.langcode = @param2
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t8 ON u2.unitGUID = t8.sourceGUID AND t8.sourceTable = 'dppRepoUnit' AND t8.langcode = @param2
                WHERE (
	                p2c.p2cGUID IN (
		                SELECT
		                p2s.p2cGUID
		                FROM " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoParam2Sector p2s
		                JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoSector se ON se.sectorGUID = p2s.sectorGUID AND se.sectorName = @param1
	                )
	                OR (
                        p2c.[global] = 1
                        --Only show results if the sector exists
                        AND EXISTS (SELECT 1 FROM " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoSector WHERE sectorName = @param1)
                    )
                )
                ORDER BY p.paramName
            ";

            DataTable dtFreeDppProperties = new DataTable();
            dtFreeDppProperties = AssistInclude.FuncGetSQLtable(ref _apiModel, sqlQueryProperties, sectorName, _apiModel.Sprache);
            List<FreeDppProperty> freeDppProperties = GetPropertiesFromDataTable(dtFreeDppProperties);

            // Load criteria as parameter
            string sqlQueryCriterias = @"SELECT DISTINCT
                c.id, c.critGUID, c.clause, ISNULL(IIF(LEN(t.TransName) > 0, t.TransName, c.critName), c.critName) AS critName, ISNULL(IIF(LEN(t.TransDescription) > 0, t.TransDescription, c.critDescription), c.critDescription) AS critDescription, c.critShortName, c.createdTime
                FROM " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoParam p
                JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoParam2Crit p2c ON p2c.paramGUID = p.paramGUID
                JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoCriteria c ON c.critGUID = p2c.critGUID
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoTrans t ON c.critGUID = t.sourceGUID AND t.sourceTable = 'dppRepoCriteria' AND t.langcode = @param2
                WHERE (
	                p2c.p2cGUID IN (
		                SELECT
		                p2s.p2cGUID
		                FROM " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoParam2Sector p2s
		                JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoSector se ON se.sectorGUID = p2s.sectorGUID AND se.sectorName = @param1
	                )
	                OR (
                        p2c.[global] = 1
                        --Es sollen nur Datensätze kommen, wenn der eingegebene Sektor auch existiert
                        AND EXISTS (SELECT 1 FROM " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoSector WHERE sectorName = @param1)
                    )
                )
                ORDER BY c.critShortName";

            DataTable dtFreeDppCriterias = new DataTable();
            dtFreeDppCriterias = AssistInclude.FuncGetSQLtable(ref _apiModel, sqlQueryCriterias, sectorName, _apiModel.Sprache);
            List<FreeDppProperty> freeDppCriterias = GetCriteriaFromDataTable(dtFreeDppCriterias);
            freeDppProperties.AddRange(freeDppCriterias);

            return freeDppProperties;
        }

        /// <summary>
        /// Maps DataTable to a list of FreeDppProperties
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private List<FreeDppProperty> GetPropertiesFromDataTable(DataTable dataTable)
        {
            List<FreeDppProperty> freeDppProperties = dataTable.AsEnumerable()
            .GroupBy(row => row.Field<int>("id"))
            .Select(group =>
            {
                var first = group.First();

                return new FreeDppProperty
                {
                    Id = first.Field<int>("id"),
                    ParamGUID = first.Field<Guid>("paramGUID"),
                    WrittenName = first.Field<string>("writtenName") ?? string.Empty,
                    ParamName = first.Field<string>("paramName") ?? string.Empty,
                    ParamDescription = first.Field<string>("paramDescription") ?? string.Empty,
                    ParamValueType = first.Field<string>("paramValueType") ?? string.Empty,
                    IsValueList = first.Field<byte>("isValueList"),
                    Bandwidth = first.Field<decimal>("bandwidth"),
                    CreatedTime = first.Field<DateTime>("createdTime"),
                    IsUsuallyMandatory = first.Field<bool>("isUsuallyMandatory"),
                    IsUsuallyDynamic = first.Field<bool>("isUsuallyDynamic"),

                    Unit = first.Field<int?>("u_id") is not null
                    ? new FreeDppUnit
                    {
                        Id = first.Field<int>("u_id"),
                        UnitGUID = first.Field<Guid>("u_unitGUID"),
                        UnitName = first.Field<string>("u_unitName") ?? string.Empty,
                        Unit = first.Field<string>("u_unit") ?? string.Empty,
                        UnitDescription = first.Field<string>("u_unitDescription") ?? string.Empty,
                        CreatedTime = first.Field<DateTime>("u_createdTime")
                    }
                    : null,

                    ActiveUntil = first.Field<DateTimeOffset>("activeUntil"),

                    TestStandard = first.Field<int?>("s_id") is not null
                    ? new FreeDppStandard
                    {
                        Id = first.Field<int>("s_id"),
                        StandardGUID = first.Field<Guid>("s_standardGUID"),
                        StandardName = first.Field<string>("s_standardName") ?? string.Empty,
                        StandardDescription = first.Field<string>("s_standardDescription") ?? string.Empty,
                        StandardReference = first.Field<string>("s_standardReference") ?? string.Empty,
                        StandardFullReference = first.Field<string>("s_standardFullReference") ?? string.Empty,
                        StandardValidFrom = first.Field<DateTimeOffset>("s_standardValidFrom"),
                        StandardType = first.Field<string>("s_standardtype") ?? string.Empty,
                        StandardUri = first.Field<string>("s_standardUri") ?? string.Empty,
                        CreatedTime = first.Field<DateTime>("s_createdTime"),

                        ReplacesStandard = first.Field<int?>("s3_id") is not null
                        ? new FreeDppStandard
                        {
                            Id = first.Field<int>("s3_id"),
                            StandardGUID = first.Field<Guid>("s3_standardGUID"),
                            StandardName = first.Field<string>("s3_standardName") ?? string.Empty,
                            StandardDescription = first.Field<string>("s3_standardDescription") ?? string.Empty,
                            StandardReference = first.Field<string>("s3_standardReference") ?? string.Empty,
                            StandardFullReference = first.Field<string>("s3_standardFullReference") ?? string.Empty,
                            StandardValidFrom = first.Field<DateTimeOffset>("s3_standardValidFrom"),
                            StandardType = first.Field<string>("s3_standardtype") ?? string.Empty,
                            StandardUri = first.Field<string>("s3_standardUri") ?? string.Empty,
                            CreatedTime = first.Field<DateTime>("s3_createdTime")
                        }
                        : null
                    }
                    : null,

                    Standard = first.Field<int?>("s2_id") is not null
                    ? new FreeDppStandard
                    {
                        Id = first.Field<int>("s2_id"),
                        StandardGUID = first.Field<Guid>("s2_standardGUID"),
                        StandardName = first.Field<string>("s2_standardName") ?? string.Empty,
                        StandardDescription = first.Field<string>("s2_standardDescription") ?? string.Empty,
                        StandardReference = first.Field<string>("s2_standardReference") ?? string.Empty,
                        StandardFullReference = first.Field<string>("s2_standardFullReference") ?? string.Empty,
                        StandardValidFrom = first.Field<DateTimeOffset>("s2_standardValidFrom"),
                        StandardType = first.Field<string>("s2_standardtype") ?? string.Empty,
                        StandardUri = first.Field<string>("s2_standardUri") ?? string.Empty,
                        CreatedTime = first.Field<DateTime>("s2_createdTime"),

                        ReplacesStandard = first.Field<int?>("s4_id") is not null
                        ? new FreeDppStandard
                        {
                            Id = first.Field<int>("s4_id"),
                            StandardGUID = first.Field<Guid>("s4_standardGUID"),
                            StandardName = first.Field<string>("s4_standardName") ?? string.Empty,
                            StandardDescription = first.Field<string>("s4_standardDescription") ?? string.Empty,
                            StandardReference = first.Field<string>("s4_standardReference") ?? string.Empty,
                            StandardFullReference = first.Field<string>("s4_standardFullReference") ?? string.Empty,
                            StandardValidFrom = first.Field<DateTimeOffset>("s4_standardValidFrom"),
                            StandardType = first.Field<string>("s4_standardtype") ?? string.Empty,
                            StandardUri = first.Field<string>("s4_standardUri") ?? string.Empty,
                            CreatedTime = first.Field<DateTime>("s4_createdTime")
                        }
                        : null
                    }
                    : null,

                    ValueList = group
                    .Where(row => row.Field<int?>("vl_id") != null)
                    .Select(row => new FreeDppValueListElement
                    {
                        Id = row.Field<int>("vl_id"),
                        ValueListGUID = row.Field<Guid>("vl_valueListGUID"),
                        ValueListName = row.Field<string>("vl_valueListName") ?? string.Empty,
                        ValueListValueType = row.Field<string>("vl_valueListValueType") ?? string.Empty,
                        CreatedTime = row.Field<DateTime>("vl_createdTime"),

                        ValueListUnit = row.Field<int?>("u2_id") is not null
                        ? new FreeDppUnit
                        {
                            Id = row.Field<int>("u2_id"),
                            UnitGUID = row.Field<Guid>("u2_unitGUID"),
                            UnitName = row.Field<string>("u2_unitName") ?? string.Empty,
                            Unit = row.Field<string>("u2_unit") ?? string.Empty,
                            UnitDescription = row.Field<string>("u2_unitDescription") ?? string.Empty,
                            CreatedTime = row.Field<DateTime>("u2_createdTime")
                        }
                        : null
                    }).ToList()
                };
            }).ToList();

            return freeDppProperties;
        }

        /// <summary>
        /// Returs a list of criterias as properties from a datatable
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private List<FreeDppProperty> GetCriteriaFromDataTable(DataTable dataTable)
        {

            List<FreeDppProperty> freeDppCriterias = dataTable.AsEnumerable()
                .Select(row => new FreeDppProperty
                {
                    Id = row.Field<int>("id"),
                    ParamGUID = row.Field<Guid>("critGUID"),
                    WrittenName = row.Field<string>("critName") ?? string.Empty,
                    ParamName = row.Field<string>("critShortName") ?? string.Empty,
                    ParamDescription = row.Field<string>("critDescription") ?? string.Empty,
                    CreatedTime = row.Field<DateTime>("createdTime")
                })
                .ToList();

            return freeDppCriterias;
        }
    }
}

