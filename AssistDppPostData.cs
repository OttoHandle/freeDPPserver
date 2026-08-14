using freeDPPapi.DppModel;
using System.Data;
using System.Data.SqlClient;


namespace freeDPPapi
{
    /// <summary>
    /// helper class with methods to create a DPP
    /// used by DppPostController for EN 18222 API CreateDPP
    /// </summary>
    public class AssistDppPostData
    {
        private apiModel _apiModel;
        private string _connectionString;

        public AssistDppPostData(apiModel apiModel)
        {
            _apiModel = apiModel;
            _connectionString = "Server=" + _apiModel.SQLserverIP + ";" + _apiModel.SQLconnection;
        }

        private Guid GetEconomicOperator(SqlConnection conn)
        {
            string query = "SELECT TOP 1 eoGUID FROM " + _apiModel.lsfreeDPPdb + ".dbo.dppEconomicOperator WHERE dppCode = @dppCode";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@dppCode", SqlDbType.Char).Value = _apiModel.freeDPPcode;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader.GetGuid(0);
                    }
                }

            }

            return Guid.Empty;
        }

        // TODO: sector needs to be searched with the spezification from a standard in the dpp header and not with a parameter
        private Guid GetSectorGuid(List<FreeDppElement> freeDppElements, SqlConnection conn)
        {// to be corrected - currently the sector is loaded via the parameter _p_d_SectorName in the productInformation element, but it should be loaded via the specification ID
            FreeDppElement? productInformationElement = freeDppElements.FirstOrDefault(e => e.elementId == "c0ProductInformation");

            if (productInformationElement != null)
            {
                if (productInformationElement.elements != null)
                {
                    FreeDppElement? sectorNameElement = productInformationElement.elements.FirstOrDefault(e => e.elementId == "_p_d_SectorName");

                    if (sectorNameElement != null)
                    {
                        string query = "SELECT TOP 1 sectorGUID FROM " + _apiModel.lsfreeDPPdb + ".dbo.dppRepoSector WHERE sectorName = @sectorName";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {

                            cmd.Parameters.Add("@sectorName", SqlDbType.NVarChar).Value = sectorNameElement.value;

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    return reader.GetGuid(0);
                                }
                            }

                        }
                    }
                }

            }

            return Guid.Empty;
        }

        private List<RepoCriteria> GetRepository(Guid sectorGuid, SqlConnection conn)
        {
            List<RepoCriteria> repoCriterias = new();

            string query = @"SELECT
                p2c.p2cGUID, p2c.collGUID, p2c.critGUID,
                c.critShortName,
                p.paramName, p.paramValueType, p.isValueList,
                IIF(p2s.isMandatory = 1, p2s.isMandatory, p2c.isMandatory) AS isMandatory
                FROM " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoParam2Crit p2c
                JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoCriteria c ON p2c.critGUID = c.critGUID
                JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoParam p ON p2c.paramGUID = p.paramGUID
                LEFT JOIN " + _apiModel.lsfreeDPPdb + @".dbo.dppRepoParam2Sector p2s ON p2c.p2cGUID = p2s.p2cGUID AND p2s.sectorGUID = @sectorGuid
                WHERE (p2s.id IS NOT NULL OR p2c.[global] = 1)";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@sectorGuid", SqlDbType.UniqueIdentifier).Value = sectorGuid;

            Dictionary<Guid, RepoCriteria> repoCriteriaDict = new();
            Dictionary<Guid, RepoParam> repoParamDict = new();

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                // Save criteria in dictionary
                Guid critGUID = reader.GetGuid(reader.GetOrdinal("critGUID"));

                if (!repoCriteriaDict.TryGetValue(critGUID, out var repoCriteria))
                {
                    repoCriteria = new RepoCriteria
                    {
                        CritGuid = critGUID,
                        CritShortName = reader.GetString(reader.GetOrdinal("critShortName")),
                        RepoParams = new List<RepoParam>()
                    };

                    repoCriteriaDict.Add(critGUID, repoCriteria);
                }

                // Save parameter in dictionary
                Guid paramToCritGuid = reader.GetGuid(reader.GetOrdinal("p2cGUID"));

                RepoParam repoParam = new RepoParam
                {
                    ParamToCritGuid = paramToCritGuid,
                    ParentParamToCritGuid = reader.GetGuid(reader.GetOrdinal("collGUID")),
                    CritGuid = reader.GetGuid(reader.GetOrdinal("critGUID")),
                    ParamName = reader.GetString(reader.GetOrdinal("paramName")),
                    ParamValueType = reader.GetString(reader.GetOrdinal("paramValueType")),
                    IsValueList = reader.GetByte(reader.GetOrdinal("isValueList")),
                    IsMandatory = reader.GetBoolean(reader.GetOrdinal("isMandatory"))
                };

                repoParamDict.Add(paramToCritGuid, repoParam);
            }

            // Assign parameter to a parameter or a criteria
            foreach (RepoParam repoParam in repoParamDict.Values)
            {
                if (repoParam.ParentParamToCritGuid != Guid.Empty)
                {
                    if (repoParamDict.TryGetValue(repoParam.ParentParamToCritGuid, out RepoParam? parentRepoParam))
                    {
                        if (parentRepoParam.RepoParams == null)
                        {
                            parentRepoParam.RepoParams = new();
                        }

                        parentRepoParam.RepoParams.Add(repoParam);
                    }
                }
                else
                {
                    if (repoCriteriaDict.TryGetValue(repoParam.CritGuid, out RepoCriteria? repoCriteria))
                    {
                        repoCriteria.RepoParams.Add(repoParam);
                    }
                }
            }

            repoCriterias = repoCriteriaDict.Values.ToList();

            return repoCriterias;
        }

        public bool VerifyDpp(FreeDppDppImport dpp)
        {
            if (VerifyDppHeader(dpp) == true)
            {
                SqlConnection conn = new SqlConnection(_connectionString);

                // Ensures the connection gets closed
                try
                {
                    conn.Open();

                    Guid eoGuid = GetEconomicOperator(conn);

                    if (eoGuid != Guid.Empty)
                    {
                        if (dpp.elements != null)
                        {
                            Guid sectorGuid = GetSectorGuid(dpp.elements, conn);

                            if (sectorGuid != Guid.Empty)
                            {
                                // Load repository
                                List<RepoCriteria> repoCriterias = GetRepository(sectorGuid, conn);

                                foreach (RepoCriteria repoCriteria in repoCriterias)
                                {
                                    // TODO: Order criteria after clause

                                    // Look for criteria in first layer
                                    FreeDppElement? dppCriteria = dpp.elements.FirstOrDefault(e => e.elementId == repoCriteria.CritShortName);

                                    // Only valid if there is no mandatory parameter inside the non existend criteria
                                    if (dppCriteria == null)
                                    {
                                        if (ContainsMandatoryParameter(repoCriteria.RepoParams) == true)
                                        {
                                            return false;
                                        }
                                    }

                                    if (dppCriteria != null)
                                    {
                                        // Check if criteria was already validated
                                        if (dppCriteria.critGuid == Guid.Empty)
                                        {
                                            dppCriteria.critGuid = repoCriteria.CritGuid;

                                            // TODO: Recursion which is called in the DateElementCollection probably starts here
                                            foreach (RepoParam repoParam in repoCriteria.RepoParams)
                                            {
                                                FreeDppElement? dppParam = dppCriteria.elements.FirstOrDefault(e => e.elementId == repoParam.ParamName);

                                                if (dppParam == null)
                                                {
                                                    if (repoParam.IsMandatory == true)
                                                    {
                                                        return false;
                                                    }

                                                    // Verify for any mandatory paramter in data collections
                                                    if (repoParam.IsMandatory == false && repoParam.RepoParams != null)
                                                    {
                                                        if (ContainsMandatoryParameter(repoParam.RepoParams) == true)
                                                        {
                                                            return false;
                                                        }
                                                    }
                                                }

                                                if (dppParam != null)
                                                {
                                                    // If the parameter was not validated yet
                                                    if (dppParam.paramToCritGuid == Guid.Empty)
                                                    {
                                                        // TODO: Validate parameter and the values after object type schemas
                                                        if (repoParam.IsValueList == 0 && repoParam.ParamValueType != "xsd:multiValueList")
                                                        {
                                                            if (repoParam.ParamValueType == "xsd:dataCollection")
                                                            {
                                                                // DataElementCollection
                                                                // TODO: Verify order of collections

                                                            }
                                                            else if (repoParam.ParamValueType == "xsd:relatedResource")
                                                            {

                                                            }
                                                            else
                                                            {
                                                                // SingleValuedDataElement

                                                            }

                                                        }
                                                        else
                                                        {
                                                            // MultiValuedDataElement or SingleValuedDataElement with value from list
                                                        }

                                                        // Mark the parameter so that it is not validated again
                                                        dppParam.paramToCritGuid = repoParam.ParamToCritGuid;
                                                    }

                                                }
                                            }
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool VerifyDppHeader(FreeDppDppImport dpp)
        {
            // TODO: Verify Dpp Header
            // not yet implemented - just a placeholder for the moment
            return true;
        }

        private bool ContainsMandatoryParameter(List<RepoParam> repoParams)
        {
            foreach (RepoParam repoParam in repoParams)
            {
                if (repoParam.IsMandatory == true)
                {
                    return true;
                }

                if (repoParam.RepoParams != null)
                {
                    if (ContainsMandatoryParameter(repoParam.RepoParams) == true)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // TODO: Insert DPP Method is not finished yet
        // Need to executed after the verification is valid
        public string CreateDpp(FreeDppDppImport dpp)
        {
            string dppId = String.Empty;
            // not yet implemented - just a placeholder for the moment
            SqlConnection conn = new SqlConnection(_connectionString);

            // Ensures that the connection is closed
            try
            {
                conn.Open();

                Guid eoGuid = GetEconomicOperator(conn);

                if (eoGuid != Guid.Empty)
                {
                    if (dpp.elements != null)
                    {
                        Guid sectorGuid = GetSectorGuid(dpp.elements, conn);

                        if (sectorGuid != Guid.Empty)
                        {
                            string query = "INSERT INTO " + _apiModel.lsfreeDPPdb + ".dbo.dpp (jubaCode, dppCode, eoGUID, facilityGUID, sectorGUID, productID, dppIdentifier, dppSchemaVersion, dppStatus, dppGranularity) OUTPUT INSERTED.dppGUID VALUES (@jubaCode, @dppCode, @eoGUID, @facilityGUID, @sectorGUID, @productID, @dppIdentifier, @dppSchemaVersion, @dppStatus, @dppGranularity)";

                            Guid dppGuid = Guid.Empty;

                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                // TODO: Some elements are inserted with the wrong value
                                cmd.Parameters.Add("@jubaCode", SqlDbType.Char).Value = _apiModel.Code;
                                cmd.Parameters.Add("@dppCode", SqlDbType.Char).Value = _apiModel.freeDPPcode;
                                cmd.Parameters.Add("@eoGUID", SqlDbType.UniqueIdentifier).Value = eoGuid;
                                cmd.Parameters.Add("@facilityGUID", SqlDbType.UniqueIdentifier).Value = new Guid("00000000-0000-0000-0000-000000000000");
                                cmd.Parameters.Add("@sectorGUID", SqlDbType.UniqueIdentifier).Value = sectorGuid;
                                cmd.Parameters.Add("@productID", SqlDbType.VarChar).Value = dpp.uniqueProductIdentifier;
                                //cmd.Parameters.Add("@lotNumber", SqlDbType.VarChar).Value = dpp.lotNumber;
                                //cmd.Parameters.Add("@serialNumber", SqlDbType.VarChar).Value = dpp.serialNumber;
                                cmd.Parameters.Add("@dppIdentifier", SqlDbType.VarChar).Value = dpp.digitalProductPassportId;
                                cmd.Parameters.Add("@dppSchemaVersion", SqlDbType.VarChar).Value = dpp.dppSchemaVersion;
                                cmd.Parameters.Add("@dppStatus", SqlDbType.VarChar).Value = dpp.dppStatus;
                                cmd.Parameters.Add("@dppGranularity", SqlDbType.Char).Value = dpp.granularity;
                                //cmd.Parameters.Add("@registryID", SqlDbType.VarChar).Value = dpp.registryId;
                                //cmd.Parameters.Add("@isArchive", SqlDbType.Bit).Value = dpp.isArchive;
                                //cmd.Parameters.Add("@zolltarifnum", SqlDbType.VarChar).Value = dpp.zolltarifNum;
                                //cmd.Parameters.Add("@prodName", SqlDbType.NVarChar).Value = dpp.prodName;
                                //cmd.Parameters.Add("@compCode", SqlDbType.Char).Value = dpp.compCode;
                                //cmd.Parameters.Add("@prodID", SqlDbType.Int).Value = dpp.prodID;
                                //cmd.Parameters.Add("@inheritsFrom", SqlDbType.UniqueIdentifier).Value = dpp.inheritsFrom;

                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        dppGuid = reader.GetGuid(0);
                                        dppId = dpp.digitalProductPassportId;
                                    }
                                }
                            }

                            if (dppGuid != Guid.Empty)
                            {
                                // Load repository
                                List<RepoCriteria> repoCriterias = GetRepository(sectorGuid, conn);

                                foreach (RepoCriteria repoCriteria in repoCriterias)
                                {
                                    // Search criteria in the first layer of the dpp elements
                                    FreeDppElement? dppCriteria = dpp.elements.FirstOrDefault(e => e.elementId == repoCriteria.CritShortName);

                                    // Create missing criteria for mandatory parameters
                                    // This is not needed because the dpp cannot be inserted if mandatory paramters are missing
                                    if (dppCriteria == null)
                                    {
                                        dppCriteria = new FreeDppElement
                                        {
                                            elementId = repoCriteria.CritShortName,
                                            elements = new()
                                        };
                                    }

                                    // TODO: Recursion which is called in the DateElementCollection probably starts here
                                    foreach (RepoParam repoParam in repoCriteria.RepoParams)
                                    {
                                        FreeDppElement? dppParam = dppCriteria.elements.FirstOrDefault(e => e.elementId == repoParam.ParamName);

                                        // Create empty element if the mandatory parameter doesn't exists
                                        // This is not needed because the dpp cannot be inserted if mandatory paramters are missing
                                        if (dppParam == null && repoParam.IsMandatory == true)
                                        {
                                            dppParam = new FreeDppElement
                                            {
                                                elementId = repoParam.ParamName,
                                                value = "null"
                                            };

                                            dppCriteria.elements.Add(dppParam);
                                        }

                                        if (dppParam != null)
                                        {
                                            // If the parameter was not created yet
                                            if (dppParam.paramToCritGuid == Guid.Empty)
                                            {
                                                if (repoParam.IsValueList == 0 && repoParam.ParamValueType != "xsd:multiValueList")
                                                {
                                                    if (repoParam.ParamValueType == "xsd:dataCollection")
                                                    {
                                                        // DataElementCollection
                                                        
                                                    }
                                                    else if (repoParam.ParamValueType == "xsd:relatedResource")
                                                    {

                                                    }
                                                    else
                                                    {
                                                        // SingleValuedDataElement
                                                        InsertSingleValueDataElement(conn, dppGuid, repoParam.ParamToCritGuid, dppParam.value, Guid.Empty);
                                                    }

                                                }
                                                else
                                                {
                                                    // MultiValuedDataElement or SingleValuedDataElement with value from list
                                                }


                                                // Mark the parameter so that it is not inserted again
                                                dppParam.paramToCritGuid = repoParam.ParamToCritGuid;
                                            }

                                        }
                                    }
                                }
                            }
                        }

                    }

                }
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return dppId;
        }


        private Guid InsertSingleValueDataElement(SqlConnection conn, Guid dppGuid, Guid paramToCritGuid, string value, Guid parentValueGuid)
        {   // not yet implemented - just a placeholder for the moment
            string query = "INSERT INTO " + _apiModel.lsfreeDPPdb + ".dbo.dppParamValue (dppGUID, paramValue, p2cGUID, zuValueGUID) OUTPUT INSERTED.valueGUID VALUES (@dppGUID, @paramValue, @p2cGUID, @zuValueGUID)";

            Guid valueGuid = Guid.Empty;

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@dppGUID", SqlDbType.UniqueIdentifier).Value = dppGuid;
                cmd.Parameters.Add("@paramValue", SqlDbType.Char).Value = value;
                cmd.Parameters.Add("@p2cGUID", SqlDbType.UniqueIdentifier).Value = paramToCritGuid;
                cmd.Parameters.Add("@zuValueGUID", SqlDbType.UniqueIdentifier).Value = parentValueGuid;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        valueGuid = reader.GetGuid(0);
                    }
                }
            }

            return valueGuid;
        }
    }
}