using System.Data;
using static freeDPPapi.DppModel.FreeDppDppFull;

namespace freeDPPapi
{
    public class AssistDPPdata
    {
        public static bool FuncFindDppIdentifier(ref apiModel J)
        {
            // based on route and/or query find out what are model, batch, itemID and date for DPP request.
            // enlarge for different ID-schemes later 
            // according EN 18219
            // Type 1: full Example https://example.com/01/09524000059109/22/2A/10/ABC123/21/12345XYZ?11=251121
            //                                 /01/GTIN/22/VARIANT/10/BATCH/21/SERIALNR?11=date
            // Test Example:        https://example.com/01/4003973287696
            bool lbFound = FuncFindDppIdType1(ref J);
            // Type 2: find on dppID, what could be the full link
            if (!lbFound) lbFound=FuncFindDppIdType2(ref J); // ...and so on
            if (lbFound || J.dppGUID.Length<1) // e.g. Type2 already fills dppGUID itself partly, if ddpID fits with database
            {
                // search for Guid if at least ModelID given
                // select the one dppGuid that fits best
                string lsQuery = "with temp as (";
                lsQuery += "select 100 as reihung, dppGuid, productid, lotNumber, serialNumber from " + J.lsfreeDPPdb + ".dbo.view_dpp_header where jubaCode=@param1 and dppCode=@param2 and productId=@param3 ";
                if (J.requestBatchID.Length > 0) lsQuery += "union select 70 as reihung, dppGuid, productid, lotNumber, serialNumber from " + J.lsfreeDPPdb + ".dbo.view_dpp_header where jubaCode=@param1 and dppCode=@param2 and productId=@param3 and lotNumber=@param4 union ";
                if (J.requestItemID.Length > 0) lsQuery += "union select 50 as reihung, dppGuid, productid, lotNumber, serialNumber from " + J.lsfreeDPPdb + ".dbo.view_dpp_header where jubaCode=@param1 and dppCode=@param2 and productId=@param4 and serialNumber=@param5 union ";
                if (J.dppId.Length>0)
                {
                    lsQuery += "union select 10 as reihung, dppGuid, productid, lotNumber, serialNumber from " + J.lsfreeDPPdb + ".dbo.view_dpp_header where jubaCode=@param1 and dppCode=@param2 and dppIdentifier=@param6 ";
                }
                if (J.actualDppGuid != new Guid("00000000-0000-0000-0000-000000000000"))
                {
                    lsQuery += "union select 10 as reihung, dppGuid, productid, lotNumber, serialNumber from " + J.lsfreeDPPdb + ".dbo.view_dpp_header where jubaCode=@param1 and dppCode=@param2 and dppGuid=@param7 ";
                }
                lsQuery += " ) select top 1 dppGuid, productid, lotNumber, serialNumber from temp order by reihung desc";
                DataTable dtDPP = new DataTable();
                dtDPP = AssistInclude.FuncGetSQLtable(ref J, lsQuery, J.Code, J.freeDPPcode, J.requestModelID, J.requestBatchID, J.requestItemID, Uri.UnescapeDataString(J.dppId), J.actualDppGuid.ToString()); // unescaping escaped link as dppId
                // eg. https://insulation.freedpp.eu/01/5901644642562 -> https%3A%2F%2Finsulation.freedpp.eu%2F01%2F5901644642562
                //                                                    --> https://localhost:7032/v1/dpps/https%3A%2F%2Finsulation.freedpp.eu%2F01%2F5901644642562?representation=full
                if (dtDPP.Columns.Contains("dppguid") && dtDPP.Rows.Count >= 1) // bei Fehler gäbs nur Column "error"
                {   // only use first one, since this is the one where select fits best
                    lbFound = true;
                    J.actualDppGuid = new Guid(AssistText.FuncQuerywert(dtDPP.Rows[0]["dppguid"].ToString(), ""));
                    J.dppGUID = AssistText.FuncQuerywert(dtDPP.Rows[0]["dppguid"].ToString(), ""); // double, in case it is used as string somewhere, because can be checked on "";
                    // and potentially missing fields filling with the ones from the dpp found
                    if (dtDPP.Columns.Contains("productid") && J.requestModelID.Length<1) J.requestModelID= AssistText.FuncQuerywert(dtDPP.Rows[0]["productid"].ToString(), "");
                    if (dtDPP.Columns.Contains("lotNumber") && J.requestBatchID.Length < 1) J.requestBatchID = AssistText.FuncQuerywert(dtDPP.Rows[0]["lotNumber"].ToString(), "");
                    if (dtDPP.Columns.Contains("serialNumber") && J.requestItemID.Length < 1) J.requestItemID = AssistText.FuncQuerywert(dtDPP.Rows[0]["serialNumber"].ToString(), "");
                }
                else
                {
                    lbFound = false;
                }
            }
            return lbFound;
        }
        public static bool FuncFindDppIdType1(ref apiModel J)
        {
            // Type 1: full Example https://example.com/01/09524000059109/22/2A/10/ABC123/21/12345XYZ?11=251121
            //                                /01/GTIN13/22    /VARIANT/10/BATCH/21/SERIALNR?11=date
            //                             format/route1/route2/route3 /....
            bool lbFound = false;
            // test if at least GTIN13 part existing:
            // note: to be proved if it could happen that order of elements in route differ
            //       this case, resolution on part 01 / 22 / 10 / 21 would be necessary. 
            if (J.format.Length > 0 && J.route1.Length > 0)
            {
                if (AssistText.FuncMakeInteger(J.format) == 1 && AssistText.FuncIsGTIN13(J.route1) == true)
                {
                    J.requestModelID = J.route1;
                    lbFound = true;
                    // further variant, batch and serial dependend on modelID, therefore here not after if-clause
                    if (AssistText.FuncMakeInteger(J.route2) == 22 && J.route3.Length >= 1)
                    {
                        J.requestVariant = J.route3;
                    }
                    if (AssistText.FuncMakeInteger(J.route4) == 10 && J.route5.Length >= 1)
                    {
                        J.requestBatchID = J.route5;
                    }
                    if (AssistText.FuncMakeInteger(J.route6) == 21 && J.route7.Length >= 1)
                    {
                        J.requestItemID = J.route7; // synonym serialNr
                    }
                    if (J.requestHeader.QueryString.ContainsKey("11"))
                    {
                        // ?11=date
                        string lsDate = J.requestHeader.QueryString["11"].ToString();
                        if (AssistText.FuncIsDateTime(lsDate) == true || AssistText.FuncIsDateOnly(lsDate) == true) 
                        {
                            DateTime ldDate;
                            if (DateTime.TryParse(lsDate, out ldDate))
                            {
                                J.requestDateTime = ldDate;
                            }
                        }

                    }
                }

            }
            return lbFound;
        }
        public static bool FuncFindDppIdType2(ref apiModel J)
        {
            // Type 2: find via DPP-ID, what also could be a whole link in Route1:
            //                             https://localhost:7032/v1/dpps/5012345101095?representation=full
            //                          or https://localhost:7032/v1/dpps/https%3A%2F%2Flocalhost%3A7032%2F01%2FGTIN13%2F22%20%20%20%20%2FVARIANT%2F10%2FBATCH%2F21%2FSERIALNR%3F11%3Ddate?representation=full
            //                          what could be deserialised to any other type
            //
            bool lbFound = false;
            // first: find with DPP-ID
            // select the one dppGuid that fits best
            string lsQuery =  "select top 1 dppGuid from " + J.lsfreeDPPdb + ".dbo.view_dpp_header where jubaCode=@param1 and dppCode=@param2 and  dppIdentifier=@param6";
            lsQuery+=" order by serialNumber desc, lotNumber desc"; // take first with a serialNumber if given, or first with a logNumber if given, or first with productId in resultset
            DataTable dtDPP = new DataTable();
            dtDPP = AssistInclude.FuncGetSQLtable(ref J, lsQuery, J.Code, J.freeDPPcode, J.requestModelID, J.requestBatchID, J.requestItemID, J.dppId);
            if (dtDPP.Columns.Contains("dppguid") && dtDPP.Rows.Count >= 1) // bei Fehler gäbs nur Column "error"
            {   // only use first one, since this is the one where select fits best
                lbFound = true;
                J.actualDppGuid = new Guid(AssistText.FuncQuerywert(dtDPP.Rows[0]["dppguid"].ToString(), ""));
                J.dppGUID = AssistText.FuncQuerywert(dtDPP.Rows[0]["dppguid"].ToString()); // double, in case it is used as string somewhere, because can be checked on "";
            }
            else
            {
                lbFound = false;
            }
            return lbFound;
        }


        public static bool FuncGetDppHeader(ref apiModel J, ref DigitalProductPassport myDPP)
        {
            bool lbDPPfound = false;
            if (myDPP == null) { return false; } //fehlerbehandlung falls myDPP nicht existiert
            else if (J.requestModelID.Length == 0 && J.actualDppGuid==new Guid("00000000-0000-0000-0000-000000000000")) { return false; } // fehlerbehandlung falls keine Model-Nr da - die muss es immer geben
            else
            {
                string lsSQLquery = "select top 1 * from " + J.lsfreeDPPdb + ".dbo.view_dpp_header where jubaCode=@param1 and dppCode=@param2 and dppGuid=@param3";
                
                DataTable dtDPP = new DataTable();
                dtDPP = AssistInclude.FuncGetSQLtable(ref J, lsSQLquery, J.Code, J.freeDPPcode, J.actualDppGuid.ToString()); //J.requestModelID, J.requestBatchID, J.requestItemID);
                // fehlt noch: Variantenbehandlung, Datum
                if (dtDPP.Columns.Contains("dppguid") && dtDPP.Rows.Count >= 1) // bei Fehler gäbs nur Column "error"
                {   // only use first one, since this is the one where select fits best
                    lbDPPfound = true;
                    J.dppGUID = AssistText.FuncQuerywert(dtDPP.Rows[0]["dppguid"].ToString(), "");
                    if (dtDPP.Columns.Contains("dppidentifier")) myDPP.digitalProductPassportId = AssistText.FuncQuerywert(dtDPP.Rows[0]["dppidentifier"].ToString(), "");
                    if (dtDPP.Columns.Contains("granularity")) myDPP.granularity = AssistText.FuncQuerywert(dtDPP.Rows[0]["granularity"].ToString(), "");
                    if (dtDPP.Columns.Contains("productid")) myDPP.uniqueProductIdentifier = AssistText.FuncQuerywert(dtDPP.Rows[0]["productid"].ToString(), "");
                    if (dtDPP.Columns.Contains("dppSchemaVersion")) myDPP.dppSchemaVersion = AssistText.FuncQuerywert(dtDPP.Rows[0]["dppSchemaVersion"].ToString(), "");
                    if (dtDPP.Columns.Contains("dppStatus")) myDPP.dppStatus = AssistText.FuncQuerywert(dtDPP.Rows[0]["dppStatus"].ToString(), "");
                    if (dtDPP.Columns.Contains("eogln")) myDPP.economicOperatorId = AssistText.FuncQuerywert(dtDPP.Rows[0]["eogln"].ToString(), "");
                    if (dtDPP.Columns.Contains("f_eogln")) myDPP.facilityId = AssistText.FuncQuerywert(dtDPP.Rows[0]["f_eogln"].ToString(), "");
                    DateTime ldTestDateTime;
                    if (dtDPP.Columns.Contains("lastupdated"))
                    {
                        if (DateTime.TryParse(AssistText.FuncQuerywert(dtDPP.Rows[0]["lastupdated"].ToString(), ""), out ldTestDateTime))
                        {
                            myDPP.lastUpdated = ldTestDateTime;
                        }
                    }
                    // contentSpecificationIds gesondert abfragen
                    if (dtDPP.Columns.Contains("sectorGUID"))
                    {
                        string lsSectorGuid = AssistText.FuncQuerywert(dtDPP.Rows[0]["sectorGUID"].ToString(), "");
                        lsSQLquery = @"select rs.*, s.* from " + J.lsfreeDPPdb + ".dbo.dppRepoStandard2Sector as rs join " + J.lsfreeDPPdb + ".dbo.dppRepoStandard as s on " +
                            "s.standardGUID=rs.standardGUID where rs.sectorGUID=@param1 and rs.ismandatory=1";
                        DataTable dt = new DataTable();
                        dt = AssistInclude.FuncGetSQLtable(ref J, lsSQLquery, lsSectorGuid);
                       // contentSpecificationIds csi = new contentSpecificationIds();

                        if (dt.Rows.Count > 0) {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (dt.Columns.Contains("standardFullReference"))
                                {
                                   // csi.SpecificationId.Add(AssistText.FuncQuerywert(dtDPP.Rows[i]["standardFullReference"].ToString(), "")); //???
                                    myDPP.contentSpecificationIds.Add(AssistText.FuncQuerywert(dt.Rows[i]["standardFullReference"].ToString(), ""));
                                }
                            }
                        } else
                        {
                            myDPP.contentSpecificationIds.Add("errorContentSpecIdMissing");
                        }
                    } //contentSpecificationIds
                }
            }
            return lbDPPfound;
        }
        public static bool FuncGetDppCriteria(ref apiModel J, ref DigitalProductPassport myDPP)
        {
            bool lbCritFound = false;
            if (myDPP == null) { return false; } // error handling if myDPP does not exist
            else if (J.requestModelID.Length == 0 && J.dppGUID.Length<1) { return false; } // error handling if model number missing - needs always to exist
            else
            {   // take only criteria according ESPR Annex 1 that are filled
                string lsSQLquery = @"WITH temp AS (
                                          select distinct
                                          pv.dppguid, pc.critguid as pc_critguid, r.*
                                          from " + J.lsfreeDPPdb + @".dbo.dppParamValue as pv
                                          join " + J.lsfreeDPPdb + @".dbo.dppRepoParam2Crit as pc on pc.p2cGUID=pv.p2cGUID
                                          join " + J.lsfreeDPPdb + @".dbo.dppRepoCriteria as r on r.critGUID=pc.critGUID
                                          where pv.dppGUID=@param1

                                          UNION select distinct
                                          pv.dppguid, pc.critguid as pc_critguid, r.*
                                          from " + J.lsfreeDPPdb + @".dbo.dppParamValueList as pv
                                          join " + J.lsfreeDPPdb + @".dbo.dppRepoParam2Crit as pc on pc.p2cGUID=pv.p2cGUID
                                          join " + J.lsfreeDPPdb + @".dbo.dppRepoCriteria as r on r.critGUID=pc.critGUID
                                          where pv.dppGUID=@param1
                                      )
                                      SELECT DISTINCT * FROM temp
                                      order by clause";
                DataTable dtCrit = new DataTable();
                dtCrit = AssistInclude.FuncGetSQLtable(ref J, lsSQLquery, J.dppGUID);
                // missing: variant handling, date
                if (dtCrit.Columns.Contains("dppguid") && dtCrit.Columns.Contains("critName") && dtCrit.Rows.Count >= 1) {  // in case of error, only column "error" results
                    lbCritFound = true;
                    string lsCritGuid = "";
                    for (int i = 0; i < dtCrit.Rows.Count; i++) {
                        // create collections as "elements" for any criteria
                        lsCritGuid = "";
                        DataElementCollection loCrit = new DataElementCollection();
                        if (dtCrit.Columns.Contains("critShortName")) loCrit.elementId = AssistText.FuncQuerywert(dtCrit.Rows[i]["critShortName"].ToString(), "");
                        if (dtCrit.Columns.Contains("critShortName")) loCrit.dictionaryReference = J.dictUri+ AssistText.FuncQuerywert(dtCrit.Rows[i]["critShortName"].ToString(), "");
                        if (dtCrit.Columns.Contains("critGUID")) lsCritGuid=AssistText.FuncQuerywert(dtCrit.Rows[i]["critGUID"].ToString(), "");
                        // and read parameters of this criteria
                        // no nestec collections yet, therefore lsCollGUID="00000000-0000-0000-0000-000000000000"
                        bool lbCollectionsFound = AssistDPPdata.FuncGetDppCollections(ref J, ref myDPP, ref loCrit, J.dppGUID, lsCritGuid, "00000000-0000-0000-0000-000000000000");
                        bool lbParamFound = AssistDPPdata.FuncGetDppParam(ref J, ref myDPP, ref loCrit, J.dppGUID, lsCritGuid);
                        myDPP.elements.Add(loCrit);
                    }
                } else
                {
                    // elements must not be empty, in case fill one with "error"
                    DataElementCollection loCrit = new DataElementCollection();
                    loCrit.elementId = "cErrorCritMissing";
                    loCrit.dictionaryReference = J.dictUri + "cErrorCritMissing";
                    myDPP.elements.Add(loCrit); 
                }
            }
            return lbCritFound;
        }

        public static bool FuncGetDppCollections(ref apiModel J, ref DigitalProductPassport myDPP, ref DataElementCollection loColl, string lsDppGUID = "", string lsCritGuid = "", string lsCollGUID = "00000000-0000-0000-0000-000000000000")
        { // read sub-collections in a criteria and then recursively read below and then search for the parameters via FuncGetDppParam,
          // without ValueLists and without nested DataCollections for now
            bool lbCollectionFound = false;
            if (myDPP == null) { return false; } //fehlerbehandlung falls myDPP nicht existiert
            else if (J.requestModelID.Length == 0 && J.dppGUID.Length < 1) { return false; } // fehlerbehandlung falls keine Model-Nr da - die muss es immer geben
            else
            {   // only take criteria according ESPR Annex 1 that are filled
                string lsSQLquery = @"select distinct p.paramguid, 
                                      pv.dppguid, pc.critguid as pc_critguid, r.*, pv.valueGUID, pv.paramValue, p.paramName, p.paramValueType, p.isValueList  
                                      from " + J.lsfreeDPPdb + ".dbo.dppParamValue as pv join " + J.lsfreeDPPdb + ".dbo.dppRepoParam2Crit " +
                                      " as pc on pc.p2cGUID=pv.p2cGUID /* nicht mit paramGUID joinen */" +
                                      "join " + J.lsfreeDPPdb + ".dbo.dppRepoCriteria as r on r.critGUID=pc.critGUID " +
                                      "join " + J.lsfreeDPPdb + ".dbo.dppRepoParam as p on p.paramGUID = pc.paramGUID " +
                                      "where pv.dppGUID=@param1 and pc.critguid=@param2 and pv.zuValueGUID=@param3 and p.ParamValueType='xsd:dataCollection' order by clause";
                DataTable dtCollection = new DataTable();
                dtCollection = AssistInclude.FuncGetSQLtable(ref J, lsSQLquery, J.dppGUID, lsCritGuid, lsCollGUID);
                // missing: variant handling, date
                if (dtCollection.Columns.Contains("paramguid") && dtCollection.Columns.Contains("paramName") && dtCollection.Rows.Count >= 1)
                {  // in case of error, only column "error" results
                    lbCollectionFound = true;
                    string lsNewCollGUID = ""; 
                    for (int i = 0; i < dtCollection.Rows.Count; i++)
                    {
                        // create collections as "elements" for any criteria, give all params and read the parameters of the criteria
                        // from crit, call recursively FuncGetDppCollections for nested collections, then call FuncGetDppParam for parameters of this collection

                        lsNewCollGUID = "";
                        DataElementCollectionWithValues loCollection = new DataElementCollectionWithValues();
                        if (dtCollection.Columns.Contains("paramName")) loCollection.elementId = AssistText.FuncQuerywert(dtCollection.Rows[i]["paramName"].ToString(), "");
                        if (dtCollection.Columns.Contains("paramName")) loCollection.dictionaryReference = J.dictUri + AssistText.FuncQuerywert(dtCollection.Rows[i]["paramName"].ToString(), "");
                        loCollection.objectType = "DataElementCollection";
                        if (dtCollection.Columns.Contains("ValueGUID")) lsNewCollGUID= AssistText.FuncQuerywert(dtCollection.Rows[i]["ValueGUID"].ToString(), "");
                        // MultiValueDataElement not yet implemented here

                        // not necessary: if (dtCollection.Columns.Contains("paramValueType")) loCollection.valueDataType = AssistText.FuncQuerywert(dtCollection.Rows[i]["paramValueType"].ToString(), "");
                        // not necessary: if (dtCollection.Columns.Contains("paramValue")) loCollection.value = AssistText.FuncQuerywert(dtCollection.Rows[i]["paramValue"].ToString(), "");

                        // dtCollection.Rows[i]["valueGuid"]
                        // Valuelists yet missing  - dtCollection.Rows[i]["isValueList"]==1
                        // bool lbValuelistFound = AssistDPPdata.FuncGetDppParamValueList(ref J, ref myDPP, ref loCrit, J.dppGUID);
                        if (AssistText.Func_String_IsGuid(lsNewCollGUID))
                        {
                            bool lbCollectionsFound = AssistDPPdata.FuncGetDppCollections(ref J, ref myDPP, ref loColl, J.dppGUID, lsCritGuid, lsNewCollGUID);
                        }

                        bool lbParamFound = AssistDPPdata.FuncGetDppParam(ref J, ref myDPP, ref loColl, J.dppGUID, lsCritGuid, lsNewCollGUID, loCollection);

                        loColl.elements.Add(loCollection);
                    }
                }
                else
                {
                    // elements must not be empty, in case fill one with "error"
                    DataElementCollectionWithValues loCollection = new DataElementCollectionWithValues();
                    loCollection.elementId = "cErrorCollMissing";
                    loCollection.dictionaryReference = J.dictUri + "cErrorCollMissing";
                    // loColl.elements.Add(loCollection); - no, no Error Colls in result required
                }
            }
            return lbCollectionFound;
        }


        public static bool FuncGetDppParam(ref apiModel J, ref DigitalProductPassport myDPP, ref DataElementCollection loCrit, string lsDppGUID = "", string lsCritGuid="", string lsCollGUID= "00000000-0000-0000-0000-000000000000", DataElementCollectionWithValues loColl = null!)
        { // read parameter in a criteria or below, for now without ValueLists and without nested DataCollections
            bool lbParameterFound = false;
            if (myDPP == null) { return false; } //fehlerbehandlung falls myDPP nicht existiert
            else if (J.requestModelID.Length == 0 && J.dppGUID.Length < 1) { return false; } // error handling if model-number and Guid missing - always required
            else
            {   // only take criteria according ESPR Annex 1 that are filled
                // there are multiple values per parameter - for now solved with grouping, but records should actually be cleaned up
                // ToDo: check if value is a ValueList and then read the ValueList instead of the value
                // ToDo: RelatedResource missing yet
                string lsSQLquery = @"WITH temp AS (
	                                      SELECT
	                                      *, RANK() OVER (PARTITION BY dppGUID, p2cGUID, zuValueGUID ORDER BY listOrder DESC) AS reihung
	                                      FROM " + J.lsfreeDPPdb + @".dbo.dppParamValue
	                                      WHERE dppGUID=@param1 and zuValueGuid=@param3
                                      ), temp2 AS (
	                                      SELECT
	                                      *, RANK() OVER (PARTITION BY dppGUID, p2cGUID, zuValueGUID ORDER BY id DESC) AS reihung
	                                      FROM " + J.lsfreeDPPdb + @".dbo.dppParamValueList
	                                      WHERE dppGUID=@param1 and zuValueGuid=@param3
                                      )
                                      select distinct
                                      p.paramguid,
                                      pv.dppguid, pc.critguid as pc_critguid, pv.p2cGUID, pv.zuValueGUID,
                                      r.id, r.critGUID, r.clause, r.critName, r.critDescription, r.critShortName, r.createdTime,
                                      pv.valueGUID, ISNULL(t.TransName, pv.paramValue) AS paramValue, p.paramName, p.paramValueType, p.isValueList  
                                      from temp as pv
                                      join " + J.lsfreeDPPdb + @".dbo.dppRepoParam2Crit as pc on pc.p2cGUID=pv.p2cGUID and pc.critguid=@param2
                                      join " + J.lsfreeDPPdb + @".dbo.dppRepoCriteria as r on r.critGUID=pc.critGUID
                                      join " + J.lsfreeDPPdb + @".dbo.dppRepoParam as p on p.paramGUID = pc.paramGUID and p.ParamValueType<>'xsd:dataCollection'
                                      LEFT JOIN " + J.lsfreeDPPdb + @".dbo.dppRepoTrans t ON pv.valueGUID = t.sourceGUID AND t.sourceTable = 'dppParamValue' AND t.langcode = @param4
                                      WHERE pv.reihung = 1

                                      --ValueList indirect
                                      UNION select distinct
                                      p.paramguid,
                                      pv.dppguid, pc.critguid as pc_critguid, pv.p2cGUID, pv.zuValueGUID,
                                      r.id, r.critGUID, r.clause, r.critName, r.critDescription, r.critShortName, r.createdTime,
                                      pv.valueGUID, ISNULL(t.TransName, vl.valueListName) AS paramValue, p.paramName, p.paramValueType, p.isValueList
                                      from temp2 as pv
                                      join " + J.lsfreeDPPdb + @".dbo.dppRepoParam2Crit as pc on pc.p2cGUID=pv.p2cGUID and pc.critguid=@param2
                                      join " + J.lsfreeDPPdb + @".dbo.dppRepoCriteria as r on r.critGUID=pc.critGUID
                                      join " + J.lsfreeDPPdb + @".dbo.dppRepoParam as p on p.paramGUID = pc.paramGUID and p.ParamValueType <> 'xsd:dataCollection'
                                      join " + J.lsfreeDPPdb + @".dbo.dppRepoValueList as vl on pv.paramValueGuid = vl.valueListGUID
                                      LEFT JOIN " + J.lsfreeDPPdb + @".dbo.dppRepoTrans t ON pv.paramValueGuid = t.sourceGUID AND t.sourceTable = 'dppRepoValueList' AND t.langcode = @param4
                                      WHERE pv.reihung = 1

                                      order by clause";
                DataTable dtParameter = new DataTable();
                dtParameter = AssistInclude.FuncGetSQLtable(ref J, lsSQLquery, J.dppGUID, lsCritGuid, lsCollGUID, J.Sprache);
                // yet missing: variant handling, date
                if (dtParameter.Columns.Contains("paramguid") && dtParameter.Columns.Contains("paramName") && dtParameter.Rows.Count >= 1)
                {  // in case off error, only column "error" results
                    lbParameterFound = true;
                    for (int i = 0; i < dtParameter.Rows.Count; i++)
                    {
                        // create collections as "elements" and insert all params per criteria
                        // yet without ValueLists and without nested DataCollections

                        DataElementCollectionWithValues loParameter = new DataElementCollectionWithValues();
                        if (dtParameter.Columns.Contains("paramName")) loParameter.elementId = AssistText.FuncQuerywert(dtParameter.Rows[i]["paramName"].ToString(), "");
                        if (dtParameter.Columns.Contains("paramName")) loParameter.dictionaryReference = J.dictUri + AssistText.FuncQuerywert(dtParameter.Rows[i]["paramName"].ToString(), "");
                        if (dtParameter.Columns.Contains("paramValueType")) loParameter.valueDataType = AssistText.FuncQuerywert(dtParameter.Rows[i]["paramValueType"].ToString(), "");

                        if (dtParameter.Rows[i].Field<byte>("isValueList") < 2)
                        {
                            loParameter.objectType = "SingleValuedDataElement";

                            if (dtParameter.Columns.Contains("paramValue")) loParameter.value = AssistText.FuncQuerywert(dtParameter.Rows[i]["paramValue"].ToString(), "");
                        }
                        else
                        {
                            loParameter.objectType = "MultiValuedDataElement";
                            //loParameter.value = null;

                            string lsSQLqueryMultiValueList = @"SELECT
                                vl.valueListName
                                from " + J.lsfreeDPPdb + @".dbo.dppParamValueList v
                                join " + J.lsfreeDPPdb + @".dbo.dppRepoParam2Crit as pc on pc.p2cGUID = v.p2cGUID
                                join " + J.lsfreeDPPdb + @".dbo.dppRepoCriteria as r on r.critGUID = pc.critGUID
                                join " + J.lsfreeDPPdb + @".dbo.dppRepoParam as p on p.paramGUID = pc.paramGUID and p.ParamValueType <> 'xsd:dataCollection' AND p.isValueList = 2
                                join " + J.lsfreeDPPdb + @".dbo.dppRepoValueList as vl on v.paramValueGuid = vl.valueListGUID
                                LEFT JOIN " + J.lsfreeDPPdb + @".dbo.dppRepoTrans t ON v.paramValueGuid = t.sourceGUID AND t.sourceTable = 'dppRepoValueList' AND t.langcode = @param4
                                WHERE v.dppGUID = @param1 AND v.p2cGUID = @param2 AND v.zuValueGuid = @param3
                            ";

                            DataTable dtValueList = new DataTable();
                            dtValueList = AssistInclude.FuncGetSQLtable(ref J, lsSQLqueryMultiValueList, J.dppGUID, dtParameter.Rows[i].Field<Guid>("p2cGUID").ToString(), dtParameter.Rows[i].Field<Guid>("zuValueGUID").ToString(), J.Sprache);

                            List<DataElementCollectionWithValues>? multiValuedDataElements = new();

                            //loParameter.elements = new();

                            for (int j = 0; j < dtValueList.Rows.Count; j++)
                            {
                                DataElementCollectionWithValues multiValuedDataElement = new();
                                multiValuedDataElement.elementId = loParameter.elementId + (j + 1).ToString();
                                multiValuedDataElement.objectType = "SingleValuedDataElement";
                                multiValuedDataElement.dictionaryReference = loParameter.dictionaryReference;
                                multiValuedDataElement.valueDataType = loParameter.valueDataType;
                                multiValuedDataElement.value = dtValueList.Rows[j].Field<string>("valueListName") ?? string.Empty;

                                //loParameter.elements.Add(multiValuedDataElement);
                                multiValuedDataElements.Add(multiValuedDataElement);
                            }

                            loParameter.value = multiValuedDataElements;
                        }

                        // dtParameter.Rows[i]["valueGuid"]
                        // Valuelists fehlen noch - dtParameter.Rows[i]["isValueList"]==1
                        // bool lbValuelistFound = AssistDPPdata.FuncGetDppParamValueList(ref J, ref myDPP, ref loCrit, J.dppGUID);

                        // either add Crit or Coll
                        if(loColl == null)
                        {
                            // Crit
                            loCrit.elements.Add(loParameter);
                        }
                        else
                        {
                            // Coll
                            if(loColl.elements == null)
                            {
                                loColl.elements = new();
                            }
                            
                            loColl.elements.Add(loParameter);
                        }
                        
                    }
                }
                else
                {
                    // elements must not be empty, in case fill one with "error"
                    DataElementCollectionWithValues loParameter = new DataElementCollectionWithValues();
                    loParameter.elementId = "pErrorParamMissing";
                    loParameter.dictionaryReference = J.dictUri + "pErrorParamMissing";
                    //  loCrit.elements.Add(loParameter); - nein, wir wollen keine Error Params in der Ausgabe
                }
            }
            return lbParameterFound;
        }
    }
}

