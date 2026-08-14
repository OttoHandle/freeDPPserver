using System.Data;
using System.Text;


namespace freeDPPapi
{
    /// <summary>
    ///  contains a couple of helper methods used by different controllers
    /// </summary>
    public class AssistText
    {
               
        public static string Func_Remove_XSS_from_route(string route = "")
        {
            //XSS: wegen Leerzeichen das durch urlencode zu "+" wird vorher umschreiben:
            route = route.Replace(" ", "%20");
            // XSS Cross Site Scripting abfangen
            route = route.Replace("<script", ".script", StringComparison.OrdinalIgnoreCase).Replace("script>", "script.", StringComparison.OrdinalIgnoreCase).Replace("%3Cscript", ".script", StringComparison.OrdinalIgnoreCase).Trim();
            route = route.Replace("%2520", " "); // reconvert spaces, no danger for XSS
            return route;
        }
        public static bool Func_String_IsGuid(string value)
        {
            Guid output;
            return Guid.TryParse(value, out output);
        }
        public static string FuncRight(string value, int length, bool emptyIfValueShorter = true)
        { //  wenn length>value.length dann leer zurück
            value = AssistText.FuncQuerywert(value, "");
            if (length <= value.Length)
            {
                return value.Substring(Math.Max(0, value.Length - length), length); // catch lenght>value.length 
            }
            else if (value.Length <= 0)
            {
                return "";
            }
            else
            { 
                if (emptyIfValueShorter == true)
                {
                    return "";
                }
                else
                {
                    return value;
                }
            }
        }

        public static string FuncLeft(string value, int length)
        {
            value = AssistText.FuncQuerywert(value, "");
            if (value.Length <= 0)
            {
                return "";
            }
            else
            {
                return value.Substring(0, Math.Max(Math.Min(value.Length, length), 0)); // no check for length>value.length, because substring will catch it and return the whole string
            }
        }
        public static string FuncMid(string value, int start, int length)
        {
            string lsReturn = "";
            if (start < 0 || length + start > value.Length)
            {
                lsReturn = "error";
            }
            else
            {
                lsReturn = value.Substring(start, length);
            }
            return lsReturn;
        }

        public static string FuncQuerywert(string lsValue, string lsDefault = "")
        {
            if (String.IsNullOrEmpty(lsValue) == true)
            {
                return lsDefault;
            }
            else { return lsValue; }

        }
        public static int FuncCountTextpart(string lsTextpart = "", string lsCompleteText = "", bool lbCaseSensitive = false)
        {
            int liCount = 0;
            if (lsTextpart.Trim().Length != 0 || lsCompleteText.Trim().Length != 0)
            {
                lsTextpart = lsTextpart.Trim();
                if (lbCaseSensitive == false)
                {
                    lsTextpart = lsTextpart.ToLower();
                    lsCompleteText = lsCompleteText.ToLower();
                }
                string[] lsTemp = lsCompleteText.Split(lsTextpart);
                liCount = lsTemp.Length - 1;
            }
            return liCount;
        }

        public static string FuncSplitStringOnBlankAt(string lsString = "", int liMaxLength = 60, bool lbGetSecondPart = false)
        {
            // splits text after max-length at last blank, if more than 2/3 of max-length is left
            lsString = lsString.Trim();
            string lsResult = AssistText.FuncLeft(lsString, liMaxLength);
            if (lsResult.Contains(" "))
            {
                if (lsResult.LastIndexOf(" ") > liMaxLength / 3 * 2 || lsString.Length - lsResult.LastIndexOf(" ") < liMaxLength)
                {
                    lsResult = AssistText.FuncLeft(lsResult, lsResult.LastIndexOf(" "));
                }
            }
            if (lbGetSecondPart == true) lsResult = AssistText.FuncLeft(AssistText.FuncRight(lsString, (lsString.Length > lsResult.Length) ? lsString.Length - lsResult.Length : 0), 30).Trim();
            return lsResult;
        }

        public static bool FuncIsNumeric(string text)
        {
            text = AssistText.FuncQuerywert(text, "");
            return double.TryParse(text, out _);
        }
        public static bool FuncIsInteger(string text)
        {
            return Int32.TryParse(text, out _);
        }
        public static bool FuncIsDateOnly(string text)
        {
            return DateOnly.TryParse(text, out _);
        }
        public static bool FuncIsDateTime(string text)
        {
            return DateTime.TryParse(text, out _);
        }
        public static int FuncMakeInteger(string text)
        {
            text = FuncQuerywert(text, "");
            int liReturn = 0;

            if (Int32.TryParse(text, out _)) { liReturn = Convert.ToInt32(text); }
            else if (Decimal.TryParse(text.Replace(".", ","), out _))
            {
                if ((double)Convert.ToDecimal(text.Replace(".", ",")) <= Math.Pow(2, 32) / 2)
                {
                    liReturn = (int)Convert.ToDecimal(text.Replace(".", ","));
                }
                else
                {
                    liReturn = 0; // int must not be > int32 
                }
            }
            else
            {
                if (FuncQuerywert(text, "").ToString().ToLower().Trim() == "true")
                {
                    liReturn = 1;
                }
                else if (FuncQuerywert(text, "").ToString().ToLower().Trim() == "false")
                {
                    liReturn = 0;
                }
            }
            return liReturn;
        }
        public static bool FuncIsStringEqual(string ls1, string ls2, bool CaseSensitive)
        {
            bool result = false;
            if (String.Compare(ls1, ls2, comparisonType: StringComparison.OrdinalIgnoreCase) == 0 && CaseSensitive == false) result = true;
            if (String.Compare(ls1, ls2, comparisonType: StringComparison.Ordinal) == 0) result = true;
            return result;
        }

        public static string FuncMakeBit(bool lbBit)
        {
            bool? nullAbleBool = lbBit;
            // creates "0"/"1" from true/false for SQL
            if (!nullAbleBool.HasValue)
            {
                return "";
            }
            else
            {
                return (lbBit ? "1" : "0");
            }
        }
        public static decimal FuncMakeDecimal(string text)
        {
            if (AssistText.FuncIsNumeric(text) && (text.Contains(".") == true && text.Contains(",") == false))
            {
                text = text.Replace(".", ",");
                // 80.000->80,000 because otherwise in SQL-Datarow not displayed correctly otherwise 80000
                // but how to handle with other server localizations than German?
            }
            decimal lnReturn = 0;
            if (Decimal.TryParse(text, out _)) { lnReturn = Convert.ToDecimal(text); }
            else
            {
                if (FuncQuerywert(text, "").ToString().ToLower().Trim() == "true")
                {
                    lnReturn = 1;
                }
                else if (FuncQuerywert(text, "").ToString().ToLower().Trim() == "false")
                {
                    lnReturn = 0;
                }
            }
            return lnReturn;
        }
        public static string FuncMakeDateStringWithoutTime(string text)
        { // creates date-string from data value, but without time
            string lsReturn = text;
            DateOnly ldDate; DateTime ldDateTime;
            if (AssistText.FuncIsDateOnly(text))
            {
                DateOnly.TryParse(text, out ldDate);
                lsReturn = ldDate.ToString();
            }
            else if (AssistText.FuncIsDateTime(text))
            {
                DateTime.TryParse(text, out ldDateTime);
                lsReturn = AssistText.FuncLeft(ldDateTime.ToString(), 10);
            }
            return lsReturn;
        }
        public static List<string> FuncMakeList(string text, string Trennzeichen = "|")
        {
            text = AssistText.FuncQuerywert(text, "");
            List<string> listReturn = new List<string>();
            string[] laList = text.Split(Trennzeichen);
            if (laList.Length > 0)
            {
                for (int lii = 0; lii < laList.Length; lii++)
                {
                    listReturn.Add(laList[lii].ToString());
                }

            }
            return listReturn;
        }


        public static object FuncIif(bool expression, object truePart, object falsePart)
        { return expression ? truePart : falsePart; }


        public static bool FuncIsMailadress(string emailaddress)
        {
            if (AssistText.FuncQuerywert(emailaddress, "").Length < 1) return false;
            try // see Stackoverflow 5342375
            {
                System.Net.Mail.MailAddress m = new System.Net.Mail.MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public string FuncToEncodedString(Stream stream, Encoding enc = null)
        {
            enc ??= Encoding.UTF8; // corresponds to enc = enc ?? Encoding.UTF8;

            byte[] bytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(bytes, 0, (int)stream.Length);
            string data = enc.GetString(bytes);

            return enc.GetString(bytes);
        }
        public static string FuncUnicode2Ascii(string lsTextvariable = "")
        {
            // https://learn.microsoft.com/de-de/dotnet/standard/base-types/character-encoding
            // https://stackoverflow.com/questions/2460206/how-to-convert-from-unicode-to-ascii
            // Convert Unicode to Bytes
            byte[] uni = Encoding.Unicode.GetBytes(lsTextvariable);
            // convert UTF-8 to Bytes
            //uni = Encoding.UTF8.GetBytes(lsTextvariable);
            // Convert to ASCII
            string lsAscii = Encoding.ASCII.GetString(uni).Trim();
            return lsAscii.Replace("&amp;#", "&#"); // because of BMECAT
        }
        public static string FuncUTF8_2Ascii(string lsTextvariable = "")
        {
            // https://learn.microsoft.com/de-de/dotnet/standard/base-types/character-encoding
            // https://stackoverflow.com/questions/2460206/how-to-convert-from-unicode-to-ascii
            // Convert Unicode to Bytes
            // byte[] uni = Encoding.Unicode.GetBytes(lsTextvariable);
            // convert UTF-8 to Bytes
            byte[] uni = Encoding.UTF8.GetBytes(lsTextvariable);
            // Convert to ASCII
            string lsAscii = Encoding.ASCII.GetString(uni).Trim();
            return lsAscii.Replace("&amp;#", "&#"); // wegen BMECAT
        }
        public static string FuncUTF32_2Ascii(string lsTextvariable = "")
        {
            // Convert Unicode to Bytes
            //byte[] uni = Encoding.Unicode.GetBytes(lsTextvariable);
            // convert UTF-8 to Bytes
            byte[] uni = Encoding.UTF32.GetBytes(lsTextvariable);
            // Convert to ASCII
            string lsAscii = Encoding.ASCII.GetString(uni).Trim();
            return lsAscii.Replace("&amp;#", "&#"); // because of BMECAT
        }
        public static bool FuncFirstIsLarger(string firsttext, string secondtext)
        {
            bool lbFirstIsLarger = false;
            // 		String.Compare("o", "n", comparisonType: StringComparison.OrdinalIgnoreCase)	1	- larger than 0 if first is larger than second
            if (String.Compare(firsttext, secondtext, comparisonType: StringComparison.OrdinalIgnoreCase) > 0) lbFirstIsLarger = true;
            return lbFirstIsLarger;
        }

        public static bool FuncIsGTIN13(string lsText="")
        {
            bool lbResult = false;
            if (FuncIsNumeric(lsText))
            {
                if (lsText.Length==13)
                {
                    // further evaluation according GS1 Rules to be added here!
                    lbResult= true;
                }
            }
            return lbResult;
        }

        public static Dictionary<string, string> FuncMakeDictionaryFromDB(ref apiModel myModel, string lsQuery)
        {
             DataTable loDataTable = new();
            loDataTable = AssistInclude.FuncGetSQLtable(ref myModel, lsQuery);
            Dictionary<string, string> loDict = new();
            for (int i = 0; i < loDataTable.Rows.Count; i++)
            {
                loDict.Add(AssistText.FuncQuerywert(loDataTable.Rows[i][0].ToString().Trim(), ""), AssistText.FuncQuerywert(loDataTable.Rows[i][1].ToString().Trim(), ""));
            }
            loDataTable = null;
            return loDict;
        }

    }
}
