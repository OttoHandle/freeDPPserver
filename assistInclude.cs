using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Security.Cryptography;
using System.Text;


namespace freeDPPapi
{
    /// <summary>
    /// helper class with different supporting methods
    /// </summary>
    public class AssistInclude
    {
        public Microsoft.AspNetCore.Http.IHttpContextAccessor HttpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor();
        // [inject] needs services.AddHttpContextAccessor();

        public static IConfigurationRoot dppConfig { get; set; }

        public string gsClientIPadress { get; set; }
        public gtRequestHeader glRequestHeader { get; set; }

        public struct gtRequestHeader
        {
            public string ClientIPadress { get; set; } //
            public string Host { get; set; }
            public string Authorization { get; set; }
            public string AcceptEncoding { get; set; }
            public string AcceptLanguage { get; set; }
            public string Accept { get; set; }
            public string Connection { get; set; }
            public string ContentLength { get; set; }
            public string ContentType { get; set; }
            public string Referer { get; set; }
            public string UserAgent { get; set; }
            public string Origin { get; set; }
            public string Method { get; set; }
            public string Path { get; set; } // template/var1/var2...
            public string FullPath { get; set; } // https://www.domain.de/template/...

            public string cookieConsent { get; set; }
            public string serverName { get; set; }
            public int serverPort { get; set; }
            public bool isHttps { get; set; }

            //public Microsoft.Extensions.Primitives.StringValues QueryString {get; set; }
            public IQueryCollection QueryString { get; set; }

            public IFormCollection Form { get; set; }
            public bool lbFormHashAllowed { get; set; }
            public bool lbFormHashAllIdAllowed { get; set; }
            public bool lbFormHasXSS { get; set; }
            public IFormFileCollection FormFiles { get; set; }
            public IRequestCookieCollection Cookies { get; set; }

            public string[] laFormKeys { get; set; }


        }

        /***************************************************
            get Info from appsettings.json
        ***************************************************/
        public static void FuncCreateConfigurationBuilder(IWebHostEnvironment _env)
        {
            if (AssistInclude.dppConfig == null)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(_env.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                dppConfig = builder.Build();
            }
        }
        public static string FuncGetAppsetting(IWebHostEnvironment _env, string lsKey)
        {
            FuncCreateConfigurationBuilder(_env); // initialize dppConfig if null,
                                                  // note: may come from appsettings.developement.json if not active, but then the values are not available in production
            string lsResult = dppConfig.GetValue<string>(lsKey);
            return lsResult;
        }

        public static void FuncModelBefuellen(ref apiModel myModel, IWebHostEnvironment myEnv)
        {
            myModel.SQLserverIP = AssistInclude.FuncGetAppsetting(myEnv, "Global:SQLserverIP"); 
            myModel.SQLconnection = AssistInclude.FuncGetAppsetting(myEnv, "Global:SQLconnection");
            myModel.SQLconnectionReadonly = AssistInclude.FuncGetAppsetting(myEnv, "Global:SQLconnectionReadonly"); 
            myModel.objConnection = AssistInclude.FuncGetSqlConnection(ref myModel, false);
            myModel.objConnectionReadonly = AssistInclude.FuncGetSqlConnection(ref myModel, true);

            string lsCommandTimeout = AssistInclude.FuncGetAppsetting(myEnv, "Global:SQLcommandTimeout");
            int liCommandTimeout = AssistText.FuncMakeInteger(lsCommandTimeout);
            if (liCommandTimeout > 0) myModel.SqlCommandTimeout = liCommandTimeout;

            myModel.Code = AssistInclude.FuncGetAppsetting(myEnv, "Global:code");
            myModel.freeDPPcode = AssistInclude.FuncGetAppsetting(myEnv, "Global:freeDPPcode");
            myModel.lsfreeDPPdb = AssistText.FuncQuerywert(AssistInclude.FuncGetAppsetting(myEnv, "Global:DppDB"), "dpp");
            myModel.lsLogDB = AssistText.FuncQuerywert(AssistInclude.FuncGetAppsetting(myEnv, "Global:logDB"), myModel.lsfreeDPPdb);
            string lsLogBadBot = AssistText.FuncQuerywert(AssistInclude.FuncGetAppsetting(myEnv, "Global:logbadbot"), "false").ToLower();
            myModel.lbLogBadBot = (lsLogBadBot == "true") ? true : false; //oh240626
            myModel.lsPimDB = AssistText.FuncQuerywert(AssistInclude.FuncGetAppsetting(myEnv, "Global:pimDB"), myModel.lsfreeDPPdb);
            myModel.lsMandantDB = AssistText.FuncQuerywert(AssistInclude.FuncGetAppsetting(myEnv, "Global:mandantDB"), myModel.lsfreeDPPdb);
            myModel.dictUri = AssistText.FuncQuerywert(AssistInclude.FuncGetAppsetting(myEnv, "Global:dictUri"), "https://www.freedpp.eu/dict/");

            myModel.mailRelay.Mailserver = AssistText.FuncQuerywert(AssistInclude.FuncGetAppsetting(myEnv, "Mailrelay:Mailserver"), "");
            myModel.mailRelay.Mailuser = AssistText.FuncQuerywert(AssistInclude.FuncGetAppsetting(myEnv, "Mailrelay:Mailuser"), "");
            myModel.mailRelay.Mailpassword = AssistText.FuncQuerywert(AssistInclude.FuncGetAppsetting(myEnv, "Mailrelay:Mailpassword"), "");
            myModel.mailRelay.Mailfrom = AssistText.FuncQuerywert(AssistInclude.FuncGetAppsetting(myEnv, "Mailrelay:Mailfrom"), "");
        }
        /***************************************************
            create sqlConnection
        ***************************************************/
        public static System.Data.SqlClient.SqlConnection FuncGetSqlConnection(ref apiModel J, bool lbReadonly)
        {
            string sSQLserverIP = J.SQLserverIP;
            string sSQLconnection = J.SQLconnection;
            if (lbReadonly) { sSQLconnection = J.SQLconnectionReadonly; }

            SqlConnection objConnection = new SqlConnection("server=" + sSQLserverIP + ";" + sSQLconnection);
            //? hier? objConnection.Open();
            // here try to open database or switch to alternative server
            return objConnection;
        }

        public static int FuncExecuteSQLcommand(ref  apiModel J, string lsSQLquery, string param1="", string param2="", string param3="", string param4 = "", string param5 = "", string param6 = "", string param7="", bool lbReadonly=false)
        {
            // note: System.Data.SqlClient creates far less dependencies than Microsoft.Data.SqlClient
            //       therefore stay on system.data.SqlClient
            // executes SQL query and max. 7 Parameters and responds number of rows affected (int)
            // propose to add error handling

            SqlConnection objConnection = J.objConnection;
            if (lbReadonly == true)
            { 
                objConnection = J.objConnectionReadonly;
            }
            if (objConnection.State == ConnectionState.Closed)
            {
                // open connection initially. Do not close and reopen at any request to avoid connection overflow
                objConnection.Open();
            }
            int liRecordsAffected = 0;
            SqlCommand objCommand = new SqlCommand(lsSQLquery, objConnection);
            objCommand.CommandTimeout = J.SqlCommandTimeout;
            if (lsSQLquery.IndexOf("@param") > 0)
            {
                // param1 bis param7
                string[] laParamList = { param1, param2, param3, param4, param5, param6, param7 };
                for (int lij = 1; lij <= 7; lij++)
                {
                    if (lsSQLquery.IndexOf("@param" + lij.ToString().Trim()) > 0)
                    {
                        objCommand.Parameters.AddWithValue("@param" + lij.ToString().Trim(), AssistText.FuncQuerywert(laParamList[lij - 1], ""));
                    }
                }
            }
            liRecordsAffected = objCommand.ExecuteNonQuery();
            return liRecordsAffected;
        }
        public static DataTable FuncGetSQLtable(ref apiModel J, string lsSQLquery, string param1 = "", string param2 = "", string param3 = "", string param4 = "", string param5 = "", string param6 = "", string param7 = "", bool lbReadonly = false)
        {
            // note: System.Data.SqlClient creates far less dependencies than Microsoft.Data.SqlClient
            //       therefore stay on system.data.SqlClient
            // creates a result table from SQL query and max. 7 Parameters
            // propose to add error handling
            SqlConnection objConnection = J.objConnection;
            if (lbReadonly == true)
            {
                objConnection = J.objConnectionReadonly;
            }
            if (objConnection.State == ConnectionState.Closed)
            {
                // open connection initially. Do not close and reopen at any request to avoid connection overflow
                objConnection.Open();
            }
            System.Data.DataTable loDataTable = new System.Data.DataTable();
            loDataTable.CaseSensitive = false; // column-Names non case sensitive https://docs.microsoft.com/en-us/dotnet/api/system.data.datatable.casesensitive?redirectedfrom=MSDN&view=net-6.0#System_Data_DataTable_CaseSensitive
            SqlDataAdapter adapter = new();
            adapter.SelectCommand = new SqlCommand(lsSQLquery, objConnection);
            adapter.SelectCommand.CommandTimeout = J.SqlCommandTimeout;
            if (lsSQLquery.IndexOf("@param") > 0)
            {
                // param1 bis param7
                string[] laParamList = { param1, param2, param3, param4, param5, param6, param7 };
                for (int lij = 1; lij <= 7; lij++)
                {
                    if (lsSQLquery.IndexOf("@param" + lij.ToString().Trim()) > 0)
                    { 
                        adapter.SelectCommand.Parameters.AddWithValue("@param" + lij.ToString().Trim(), AssistText.FuncQuerywert(laParamList[lij - 1], ""));
                    }
                }
            }
            try
            {
                adapter.Fill(loDataTable);
            } catch(Exception ex) {
                string lsError = ex.ToString + " ... " + lsSQLquery;
            }
            if (loDataTable.Columns.Count==0)
            {
                loDataTable.Columns.Add("error", typeof(String));
                loDataTable.Rows.Add("error");
            }
            return loDataTable;
        }
        public static string FuncCreateCookieGuid() 
        {
            string lsCookieGuid = "00000000-0000-0000-0000-000000000000";
            lsCookieGuid = Guid.NewGuid().ToString().Trim();
            return lsCookieGuid;
        }


        public gtRequestHeader FuncGetRequestHeader(IHeaderDictionary dict,  gtRequestHeader llRequestHeader)
        {
            //return llRequestHeader;
            llRequestHeader.QueryString = HttpContextAccessor.HttpContext.Request.Query;  // HttpContextAccessor ganz oben instanzieren

            llRequestHeader.Cookies = HttpContextAccessor.HttpContext.Request.Cookies;
            llRequestHeader.ClientIPadress = HttpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            llRequestHeader.Method = HttpContextAccessor.HttpContext.Request.Method;
            llRequestHeader.Host = HttpContextAccessor.HttpContext.Request.Host.ToString().Trim();
            llRequestHeader.Path = HttpContextAccessor.HttpContext.Request.Path.ToString().Trim();
            llRequestHeader.isHttps = HttpContextAccessor.HttpContext.Request.IsHttps;
            //  llRequestHeader.Authorization = HttpContextAccessor.HttpContext.Request.Headers.Authorization;

            if (llRequestHeader.ClientIPadress == "::1")
            {
                llRequestHeader.ClientIPadress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[Dns.GetHostEntry(Dns.GetHostName()).AddressList.Length - 1].ToString();
            }
            IEnumerable<string> leClientIPAdress = new string[] { llRequestHeader.ClientIPadress.ToString() };
            llRequestHeader.ClientIPadress = leClientIPAdress.FirstOrDefault().ToString();
            gsClientIPadress = llRequestHeader.ClientIPadress;

            // Initialising, if not all Keys submitted
            llRequestHeader.AcceptEncoding = "";
            llRequestHeader.AcceptLanguage = "";
            llRequestHeader.Accept = ""; //text/html
            llRequestHeader.Connection = "";
            llRequestHeader.ContentLength = "";
            llRequestHeader.ContentType = "";
            llRequestHeader.Host = "";
            llRequestHeader.Referer = "";
            llRequestHeader.UserAgent = "";
            llRequestHeader.Origin = "";

            foreach (Microsoft.Extensions.Primitives.StringValues keys in dict.Keys)
            {
                //string str = "Key : " + keys + ":  Values : " + dict[keys];
                switch (keys.ToString().ToLower().Trim())
                {
                    case "accept-encoding":
                        llRequestHeader.AcceptEncoding = dict[keys];
                        break;
                    case "accept-language":
                        llRequestHeader.AcceptLanguage = dict[keys];
                        break;
                    case "accept":
                        llRequestHeader.Accept = dict[keys];
                        break;
                    case "connection":
                        llRequestHeader.Connection = dict[keys];
                        break;
                    case "content-length":
                        llRequestHeader.ContentLength = dict[keys];
                        break;
                    case "content-type":
                        llRequestHeader.ContentType = dict[keys];
                        break;
                    case "host":
                        llRequestHeader.Host = dict[keys];
                        break;
                    case "referer":
                        llRequestHeader.Referer = dict[keys];
                        break;
                    case "user-agent":
                        llRequestHeader.UserAgent = dict[keys];
                        break;
                    case "origin":
                        llRequestHeader.Origin = dict[keys];
                        break;
                }
            }
            //Console.WriteLine(str);
            llRequestHeader.cookieConsent = AssistText.FuncQuerywert(llRequestHeader.Cookies["c"], ""); // llRequestHeader.cookieConsent
            llRequestHeader.serverName = AssistText.FuncQuerywert(llRequestHeader.serverName, "");
            llRequestHeader.serverPort = Convert.ToInt32(AssistText.FuncQuerywert(llRequestHeader.serverPort.ToString(), "80"));
            llRequestHeader.Referer = AssistText.FuncQuerywert(llRequestHeader.Referer, "");

            llRequestHeader.FullPath = (llRequestHeader.isHttps) ? "https://" : "http://";
            llRequestHeader.FullPath += llRequestHeader.Host.Trim();
            llRequestHeader.Path = llRequestHeader.Path;
            llRequestHeader.FullPath += llRequestHeader.Path;



            // llRequestHeader.QueryString = lsvQueryString;
            return llRequestHeader;
        }

        public static string GetAbsoluteUri(IHttpContextAccessor myContext, string lsPart = "all")
        {
            UriBuilder uriBuilder = new UriBuilder();
            string lsHost = myContext.HttpContext.Request.Host.ToString();
            lsHost = lsHost.Replace("[", "").Replace("]", "");
            uriBuilder.Scheme = myContext.HttpContext.Request.Scheme.ToString();
            uriBuilder.Host = lsHost;
            // because of localhost while debugging {[localhost:4444] is not a correct URI -> Exception
            // not solveable yet
            uriBuilder.Path = myContext.HttpContext.Request.Path.ToString();
            uriBuilder.Query = myContext.HttpContext.Request.QueryString.ToString();
            string lsResult = "";
            switch (lsPart.ToLower().Trim())
            {
                case "host":
                    lsResult = lsHost;
                    break;
                case "scheme":
                    lsResult = myContext.HttpContext.Request.Scheme.ToString();
                    break;
                case "path":
                    lsResult = myContext.HttpContext.Request.Path.ToString();
                    break;
                case "querystring":
                    lsResult = myContext.HttpContext.Request.QueryString.ToString();
                    break;
                default: // all
                    try
                    {
                        lsResult = uriBuilder.Uri.ToString();
                    }
                    catch
                    {
                        lsResult = myContext.HttpContext.Request.Scheme.ToString() + "://" +
                            myContext.HttpContext.Request.Host.ToString().Replace("[", "").Replace("]", "") +
                            myContext.HttpContext.Request.Path.ToString() +
                            myContext.HttpContext.Request.QueryString.ToString();
                    }
                    break;
            }
            return lsResult;
        }

        public static void FuncFillRouteMime (ref apiModel J, string lsFormat="", string lsRoute1="", string lsRoute2 = "", string lsRoute3 = "", string lsRoute4 = "", string lsRoute5 = "", string lsRoute6 = "", string lsRoute7 = "")
        {
            // creates usable parameters without XSS from the original request

            J.format = AssistText.Func_Remove_XSS_from_route(lsFormat);
            J.route1 = AssistText.Func_Remove_XSS_from_route(lsRoute1);
            J.route2 = AssistText.Func_Remove_XSS_from_route(lsRoute2);
            J.route3 = AssistText.Func_Remove_XSS_from_route(lsRoute3);
            J.route4 = AssistText.Func_Remove_XSS_from_route(lsRoute4);
            J.route5 = AssistText.Func_Remove_XSS_from_route(lsRoute5);
            J.route6 = AssistText.Func_Remove_XSS_from_route(lsRoute6);
            J.route7 = AssistText.Func_Remove_XSS_from_route(lsRoute7);

            // URL encoding
            // 		Uri.UnescapeDataString("https%3A%2F%2Flocalhost%3A7032%2Fv1%2Fdpps%2F%3F.sd%3Dmeinefehler")	"https://localhost:7032/v1/dpps/?.sd=meinefehler"	string
            //      Uri.EscapeDataString("https://localhost:7032/v1/dpps/?.sd=meinefehler")	"https%3A%2F%2Flocalhost%3A7032%2Fv1%2Fdpps%2F%3F.sd%3Dmeinefehler"	string

            if (true)
            {  //oh260804 wenn dann andersrum, oder? wir wollen die unescaped variante
               // J.format = Uri.EscapeDataString(J.format);
                J.route1 = Uri.UnescapeDataString(J.route1);
                J.route2 = Uri.UnescapeDataString(J.route2);
                J.route3 = Uri.UnescapeDataString(J.route3);
                J.route4 = Uri.UnescapeDataString(J.route4);
                J.route5 = Uri.UnescapeDataString(J.route5);
                J.route6 = Uri.UnescapeDataString(J.route6);
                J.route7 = Uri.UnescapeDataString(J.route7);
            }
            //oh260704 Frage --> Timon hier auch die ?query=parameter prüfen und übernehmen?
            //                   any queryparameter in key-value store, without XSS
            var queryParams = J.requestHeader.QueryString;
            foreach (var param in queryParams)
            {
                J.gdQueryParam.Add(AssistText.Func_Remove_XSS_from_route(param.Key), AssistText.Func_Remove_XSS_from_route(param.Value));
                if (param.Key.ToString().ToLower()== "representation" && param.Value.ToString().ToLower() == "full")
                {
                    J.isCompressed = false; // API requires param ?representation= full for complete JSON response as EN 18223 Annex 1; default="compressed" as EN 18223 5.2
                }
            }

            // request.accept = application / json->json
            // might be text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8 -> also HTML, therefore proof html last
            if (J.requestHeader.Accept.ToLower().Contains("application/xml")) J.mimeType = "xml";
            if (J.requestHeader.Accept.ToLower().Contains("application/json")) J.mimeType = "json";
            if (J.requestHeader.Accept.ToLower().Contains("text/csv")) J.mimeType = "csv";
            if (J.requestHeader.Accept.ToLower().Contains("text/plain")) J.mimeType = "csv"; // txt?
            if (J.requestHeader.Accept.ToLower().Contains("application/javascript")) J.mimeType = "js";
            if (J.requestHeader.Accept.ToLower().Contains("text/css")) J.mimeType = "css";
            if (J.requestHeader.Accept.ToLower().Contains("text/html")) J.mimeType = "html";
        }
        public static string FuncEncryptSHA256(string text, string key)
        {
            // change according to your needs, an UTF8Encoding
            // could be more suitable in certain situations
            ASCIIEncoding asciiencoding = new ASCIIEncoding();
            UTF8Encoding utf8encoding = new UTF8Encoding();

            Byte[] textBytes = utf8encoding.GetBytes(text);
            Byte[] keyBytes = utf8encoding.GetBytes(key);

            Byte[] hashBytes;

            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
        public static string FuncEncryptPassword(string text)
        {
            if (text.Length > 0)
            {
                var md5source = text.Substring(0, 1) + text.Length.ToString().PadLeft(8, '0') + text;
                var md5 = System.Security.Cryptography.MD5.Create();
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(md5source));
                return hash.Aggregate("", (p, n) => p + n.ToString("x2"));
            }
            else
            {
                return "";
            }
        }
        public static string FuncEncodeBase64(string text)
        {
            var Bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(Bytes);
        }
        public static string FuncDecodeBase64(string text)
        {
            var Bytes = System.Convert.FromBase64String(text);
            return Encoding.UTF8.GetString(Bytes);
        }

        public static void funcWaitMilliseconds(int liMilliseconds = 1)
        {
            DateTime dt = DateTime.Now;
            while (TimeSpan.FromMilliseconds(liMilliseconds) > DateTime.Now - dt)
            {
                // dont use, needs additional ressources
                // Task.Delay(liMilliseconds+1); // wait() to set Timeout in case of wrong or manipulated form data
            }
        }
        public void FuncSetContextHeader(ref apiModel jT, string lsMimeType, string lsOutput = "",
           bool lbDoCaching = false, string lsStoreCookie = "", int cssCacheTimeoutSeconds = 297) // "text/html"; - Führt zu 406...
        {
            // HttpContextAccessor.HttpContext.Response.ContentType = lsMimeType; 
            HttpContextAccessor.HttpContext.Response.Clear(); //   Current.Response.Clear();
            //besser nicht, wenn der nicht passt dann bricht der browser ab und wir brauchens eh nur für HEAD
            //HttpContextAccessor.HttpContext.Response.Headers.Add("Content-Length", lsOutput.Length.ToString()+5); +0 ist zu kurz, +5 bricht aber ab ist vermutlich zu lang
            HttpContextAccessor.HttpContext.Response.Headers.Add("Content-Type", lsMimeType);
            if (lsMimeType.StartsWith("text/html"))
            {  // possible: allow, deny, sameorigin
                string lsXFRAME = AssistText.FuncQuerywert(AssistInclude.FuncGetAppsetting(jT._env, "Global:XFRAME"), "sameorigin");
                // HttpContextAccessor.HttpContext.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN"); // nicht in andere Webs als iframe einbinden lassen sonst DENY
                if (lsXFRAME.ToLower() != "allow") HttpContextAccessor.HttpContext.Response.Headers.Add("X-Frame-Options", lsXFRAME);       // könnte ein Problem mit youtube machen -> dann ins DICT
            }
            HttpContextAccessor.HttpContext.Response.Headers.Add("X-XSS-Protection", " 1; mode=block");

            // x-powered-by jubacon in iis http-header einstellen https://stackoverflow.com/questions/45882715/how-to-remove-x-powered-by-header-in-net-core-2-0
            // test für authentification header - bringt keinen Vorteil                   Google Analytics ausschalten: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
            // HttpContextAccessor.HttpContext.Response.Headers.Add("Bearer", "Token");                                 https://de.wikipedia.org/wiki/Liste_der_HTTP-Headerfelder -> Referrer-Policy

            // für dpp ist cache gar nicht gut
            HttpContextAccessor.HttpContext.Response.Headers.Add("Cache-Control", "public, max-age=0");


            //oh260704 do we need really Cookie and content length?
            if (lsStoreCookie != "")
            {
                CookieOptions option = new CookieOptions();
                option.HttpOnly = true; // not accessable for client-side script
                option.IsEssential = true; // Kennzeichnung für essential cookies that do not need consent
                option.SameSite = SameSiteMode.Strict;
                // how to make cookie non-persistent? https://codeasp.net/blogs/asp-net/6235/persistent-and-non-persistent-cookies-in-asp-net
                // just leave timeout free
                // not: option.Expires = DateTime.Now.AddMinutes(24*60);
                // nur wenn site https ist
                // option.Secure = true; 

                HttpContextAccessor.HttpContext.Response.Cookies.Append("j", lsStoreCookie, option); // das muss hier stehen sonst wirds nicht übertragen
            }
            //if (lsMimeType.Contains("application/json") == true)
            //{ solve 5MB Problem no chance...
            //    //  HttpContextAccessor.HttpContext.Response.Headers.Add("Content-Length", "50000000");
            //    //  HttpContextAccessor.HttpContext.Response.ContentLength = lsOutput.Length;
            //    byte[] bytes = Encoding.Default.GetBytes(lsOutput);
            //    string result = System.Text.Encoding.UTF8.GetString(bytes);
            //    oh240126
            //    statt HttpContextAccessor.HttpContext.Response.WriteAsync(lsOutput);// als utf-8
            //    wegen 5MB Hürde https://github.com/dotnet/aspnetcore/issues/45154

            var pipeWriter = HttpContextAccessor.HttpContext.Response.BodyWriter;
            byte[] bytes = Encoding.Default.GetBytes(lsOutput);
            pipeWriter.WriteAsync(bytes);
            // jubaAssist.assistText.FuncWriteTextfile(lsOutput, "c:\\jubacon\\161.txt");
        }
        public void FuncSetContextHeader404(int liErrorcode = 404, string lsTargetUrl = "") // "text/html" creates 406 ERROR, handled seperately...
        {
            HttpContextAccessor.HttpContext.Response.StatusCode = liErrorcode;
            if (lsTargetUrl.Length > 0) HttpContextAccessor.HttpContext.Response.Redirect(lsTargetUrl); // instead of using IIS definitions in _appConfiguration["App:MissingTenantUrl"] + "/Error?statusCode=404"
        }
        public void FuncContextRedirect(int liStatuscode = 302, string lsTargetUrl = "") // use 302 for temporary moved instead of 301 for permanent moved
        {
            HttpContextAccessor.HttpContext.Response.StatusCode = liStatuscode;
            if (lsTargetUrl.Length > 0) HttpContextAccessor.HttpContext.Response.Redirect(lsTargetUrl);// _appConfiguration["App:MissingTenantUrl"] + "/Error?statusCode=404"
        }


        /// <summary>
        /// defines languages applicable and sets current language used
        /// required language can be set via HTTP Accept-Language or query-param ?language=...
        /// </summary>
        /// <param name="apiModel"></param>
        /// <param name="requestedLanguage"></param>
        /// <returns>language code if available, else default langugae en-GB</returns>
        public string SetLanguage(apiModel apiModel, string requestedLanguage = "")
        {
            apiModel.defaultSprache = "en-GB";
            string defaultLanguageShort = "en";

            // Verfügbare Sprachen
            HashSet<string> supportedLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "bg-BG", // Bulgarian - source: https://style-guide.europa.eu/de/content/-/isg/topic?identifier=annex-a8-language-codes
                "cs-CZ", // Czech     - maybe changed due to european implementation acts
                "da-DK", // Danish
                "de-DE", // German 
                "de-AT", // German - Austria
                "de-CH", // German - Switzerland
                "el-GR", // Greek 
                "en-GB", // English 
                "en-US", // English - US
                "es-ES", // Spanish
                "et-EE", // Estonian
                "fi-FI", // Finnish
                "fr-FR", // French
                "ga-IE", // Irish 
                "hr-HR", // Croatian
                "hu-HU", // Hungarian
                "it-IT", // Italian
                "lt-LT", // Lithuanian
                "lv-LV", // Latvian
                "mt-MT", // Maltese
                "nl-NL", // Dutch
                "pl-PL", // Polish
                "pt-PT", // Portuguese
                "ro-RO", // Romanian
                "sk-SK", // Slovak 
                "sl-SI", // Slovenian 
                "sv-SE", // Swedish

                "tr-TR", // Turkey - source: https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes
                "no-NO",  // Norway
                "is-IS"  // Iceland

            };

            // check for requested language 
            string language;
            
            if(requestedLanguage.Length > 0)
            {
                language = requestedLanguage;
            }
            else
            {
                language = apiModel.requestHeader.AcceptLanguage.Split(',').FirstOrDefault() ?? apiModel.defaultSprache;
            }

            // cut region code to create short language code 2 digit
            language = language.Split('-').FirstOrDefault() ?? defaultLanguageShort;

            // add available language to apiModel for current session
            language = supportedLanguages.FirstOrDefault(l => l.StartsWith(language + "-", StringComparison.OrdinalIgnoreCase)) ?? apiModel.defaultSprache;

            apiModel.Sprache = language;

            return apiModel.Sprache;
        }
    }
}
