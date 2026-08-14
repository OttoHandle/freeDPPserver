using System.Globalization;
using System.Text.Json.Nodes;

namespace freeDPPapi.DppModel
{
    public class FreeDppDppCompressed
    {

        // compressed serialisation of a DPP
        // as described in EN 18223 5.2
        // default for API Response, or if requuested with ?representation=compressed
        // if requested with ?representation=full -> model freeDppDppFull is used

        // uses DigitalProductPassport 
        //      EnumDppStatus
        //      EnumDppGranularity
        // from FreeDppDppFull
        // because they should be identical

        // maybe not necessary since covered by freeDppDppFull -> CompressedDataElementCollectionWithValues
        public static JsonObject FuncCompressDPP(ref apiModel J, ref FreeDppDppFull.DigitalProductPassport loDppFull)
        {
            // Build a fully dynamic JsonObject from the full DPP model
            // create dynamic root object
            var root = new JsonObject();

            // add simple common properties
            root["digitalProductPassportId"] = loDppFull.digitalProductPassportId;
            root["uniqueProductIdentifier"] = loDppFull.uniqueProductIdentifier;
            root["granularity"] = loDppFull.granularity;
            root["dppSchemaVersion"] = loDppFull.dppSchemaVersion;
            root["dppStatus"] = loDppFull.dppStatus;
            root["lastUpdated"] = JsonValue.Create(loDppFull.lastUpdated.ToString("o"));
            root["economicOperatorId"] = loDppFull.economicOperatorId;
            root["facilityId"] = loDppFull.facilityId;

            // contentSpecificationIds as array
            var csArr = new JsonArray();
            foreach (var cs in loDppFull.contentSpecificationIds ?? Enumerable.Empty<string>())
                csArr.Add(cs);
            root["contentSpecificationIds"] = csArr;

            // convert each top-level DataElementCollection into a dynamic property
            if (loDppFull.elements != null)
            {
                foreach (var dec in loDppFull.elements)
                {
                    if (string.IsNullOrEmpty(dec.elementId)) continue;
                    root[dec.elementId] = ConvertDataElementCollectionToNode(dec);
                }
            }
            return root;
        }

        // Convert the FreeDppDppFull data element types into a dynamic JsonNode structure
        private static JsonNode? ConvertDataElementCollectionToNode(FreeDppDppFull.DataElementCollection dec)
        {
            var jo = new JsonObject();
            if (dec.elements == null) return jo;

            foreach (var child in dec.elements)
            {
                if (string.IsNullOrEmpty(child.elementId)) continue;

                // if no grandchildren -> single primitive
                if (child.elements == null || child.elements.Count == 0)
                {
                    jo[child.elementId] = GetElementValue(child.value, child.valueDataType);
                    
                    continue;
                }

                // if grandchildren are all simple leaves with elementIds -> nested object
                bool allGrandAreLeafWithIds = child.elements.All(e => !string.IsNullOrEmpty(e.elementId) && (e.elements == null || e.elements.Count == 0));
                if (allGrandAreLeafWithIds)
                {
                    var nestedObj = new JsonObject();
                    foreach (var gv in child.elements)
                        nestedObj[gv.elementId] = GetElementValue(gv.value, gv.valueDataType);
                    jo[child.elementId] = nestedObj;
                    continue;
                }

                // if grandchildren are items that themselves have elements -> array of objects
                bool itemsAreObjects = child.elements.All(e => e.elements != null && e.elements.Count > 0);
                if (itemsAreObjects)
                {
                    var arr = new JsonArray();
                    foreach (var item in child.elements)
                    {
                        var itemObj = new JsonObject();
                        if (item.elements != null)
                        {
                            foreach (var gv in item.elements)
                                itemObj[gv.elementId] = ConvertChildToNode(gv);
                        }
                        arr.Add(itemObj);
                    }
                    jo[child.elementId] = arr;
                    continue;
                }

                // fallback: nested object with converted children
                var fallback = new JsonObject();
                foreach (var gv in child.elements)
                    fallback[gv.elementId] = ConvertChildToNode(gv);
                jo[child.elementId] = fallback;
            }

            return jo;
        }

        private static JsonNode? ConvertChildToNode(FreeDppDppFull.DataElementCollectionWithValues child)
        {
            if (child.elements == null || child.elements.Count == 0)
                return GetElementValue(child.value, child.valueDataType);

            var obj = new JsonObject();
            foreach (var c in child.elements)
                obj[c.elementId] = ConvertChildToNode(c);
            return obj;
        }

        private static object? ParsePrimitive(string? value, string? valueDataType)
        {
            if (value == null) return null;
            if (string.IsNullOrEmpty(valueDataType)) return value;

            var t = valueDataType.ToLowerInvariant();
            if (t.Contains("boolean") || t.Contains("bool"))
            {
                if (bool.TryParse(value, out var b)) return b;
            }
            if (t.Contains("int") || t.Contains("integer"))
            {
                if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var i)) return i;
            }
            if (t.Contains("float") || t.Contains("double") || t.Contains("decimal"))
            {
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
            }
            // default fallback to string
            return value;
        }

        /// <summary>
        /// Returns JsonValue Object from the value of a SingleValuedDataElement or a MultiValuedDataElement
        /// SingleValuedDataElement is one value
        /// MultiValuedDataElement is a array of values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        private static JsonValue GetElementValue(object value, string dataType)
        {
            JsonValue elementValue;

            // Value can also be a list of values from a MultiValuedDataElement
            if (value is string s)
            {
                elementValue = JsonValue.Create(ParsePrimitive(s, dataType));
            }
            else if (value is List<FreeDppDppFull.DataElementCollectionWithValues> list)
            {
                List<object> multiValuedDataElementValues = new();

                foreach (FreeDppDppFull.DataElementCollectionWithValues param in list)
                {
                    multiValuedDataElementValues.Add(JsonValue.Create(ParsePrimitive(param.value.ToString(), dataType)));
                }

                //jo[child.elementId] = JsonValue.Create(ParsePrimitive("test", child.valueDataType));
                elementValue = JsonValue.Create(multiValuedDataElementValues);
            }
            else
            {
                elementValue = JsonValue.Create(string.Empty);
            }

            return elementValue;
        }
    }
}
