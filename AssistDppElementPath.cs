using static freeDPPapi.DppModel.FreeDppDppFull;

namespace freeDPPapi
{
    /// <summary>
    /// methods to find a part of an DPP by ElementIdPath 
    /// </summary>
    public class AssistDppElementPath
    {
        public static object FuncGetElementOnPath(ref apiModel _apiModel, ref DigitalProductPassport loDPP)
        {
            string lsPath = Uri.UnescapeDataString(_apiModel.route3.Replace("\\", "/")); // falls mit \ getrennt
            // connect further parts of the path if they are set in routes instead of route3
            if (_apiModel.route4.Length > 0)  lsPath = lsPath + (lsPath.EndsWith("/")?"": "/") + Uri.UnescapeDataString(_apiModel.route4.Replace("\\", "/"));
            if (_apiModel.route5.Length > 0) lsPath = lsPath + (lsPath.EndsWith("/") ? "" : "/") + Uri.UnescapeDataString(_apiModel.route5.Replace("\\", "/"));
            if (_apiModel.route6.Length > 0) lsPath = lsPath + (lsPath.EndsWith("/") ? "" : "/") + Uri.UnescapeDataString(_apiModel.route6.Replace("\\", "/"));
            if (_apiModel.route7.Length > 0) lsPath = lsPath + (lsPath.EndsWith("/") ? "" : "/") + Uri.UnescapeDataString(_apiModel.route7.Replace("\\", "/"));

            string[] laPath = lsPath.Split("/");
            if (laPath.Length > 0) {
                DataElementCollection loObjFinal = null;
                DataElementCollection loObject = FuncGetFromPathFirstDataElementCollection(ref _apiModel, ref loDPP, laPath[0]);
                DataElementCollectionWithValues loObjectTemp = null;
              
                DataElementCollectionWithValues loObjValFinal = null;
                if (loObject != null)
                {
                    loObjFinal = loObject;
                }
                for (int i = 1; i < laPath.Length; i++)
                {
                    {
                        // any above the last is a dataElementCollection
                        // foreach DataelementCollection dc in loDPP
                        // from second level, loObjectTemp as collectionWithValues is used
                        loObjectTemp = FuncGetFromPathAnyDataElementCollection(ref _apiModel, loObject, loObjectTemp, laPath[i]);
                        if (loObjectTemp!=null)
                        {
                            loObjValFinal = loObjectTemp;
                        }
                    }
                }
                if (loObjValFinal==null)
                {
                    return loObjFinal;
                }
                else
                {
                    return loObjValFinal;
                }
            } else {
                return null;
            }
        }
        public static DataElementCollection FuncGetFromPathFirstDataElementCollection(ref apiModel _apiModel, ref DigitalProductPassport loDPP, string lsElementPathPart = "")
        {
            DataElementCollection loDC = new();
            // find with first part of path the DataelementCollection
            foreach (DataElementCollection dc in loDPP.elements)
            {
                if (dc.elementId == lsElementPathPart)
                {
                    return dc;
                }
            }
            return null;
        }
        public static DataElementCollectionWithValues FuncGetFromPathAnyDataElementCollection(ref apiModel _apiModel, DataElementCollection loDPPcoll, DataElementCollectionWithValues loDPPcollVal=null, string lsElementPathPart = "")
        {
            // find with first part of path the DataelementCollection
            if (loDPPcollVal == null)
            {
                foreach (DataElementCollectionWithValues dc in loDPPcoll.elements)
                {
                    if (dc.elementId == lsElementPathPart)
                    {
                        return dc;
                    }
                }
            } else {
                foreach (DataElementCollectionWithValues dc in loDPPcollVal.elements)
                {
                    if (dc.elementId == lsElementPathPart)
                    {
                        return dc;
                    }
                }
            }
            return null;
        }
    }

}
