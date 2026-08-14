using System.Data.SqlClient;

namespace freeDPPapi;
public class apiModel
{
    public IWebHostEnvironment _env { get; set; }
    public string Code { get; set; }
    public string freeDPPcode { get; set; }
    public string Mandant { get; set; }
    public string dppId { get; set; }=string.Empty;
    public string dppGUID { get; set; } = string.Empty;
    public string dictUri { get; set; } = "https://www.freedpp.eu/dict/"; 
    public int redirectStatus { get; set; } = 0;
    public string redirectString { get; set; } = "";
    public string Sprache { get; set; }
    public string defaultSprache { get; set; }

    public SqlConnection objConnection = new SqlConnection();
    public SqlConnection objConnectionReadonly = new SqlConnection();
    public SqlConnection objConnectionAlternate = new SqlConnection(); //oh260704 check if alternate connection is needed for REST calls to other DPPs
    public int SqlCommandTimeout { get; set; } = 7; //oh260804 limit on 7 seconds runtime

    public string SQLconnection { get; set; } = "";
    public string SQLconnectionReadonly { get; set; } = "";

    public string SQLserverIP { get; set; } = "";
    public string lsfreeDPPdb { get; set; }
    public string lsLogDB { get; set; }
    public bool lbLogBadBot { get; set; } = false;
    public string lsMandantDB { get; set; }
    public string lsPimDB { get; set; }

    public string lsTablePraefix { get; set; }

    public string debugSenderMethod { get; set; } // for error handling, where does it come from?

    public int rekursionZaehler { get; set; }
    public int rekursionMax { get; set; }
    public int httpStatus { get; set; }

    public string Method { get; set; }

    public int userAgentId { get; set; }
    public int AcceptLanguageId { get; set; }

    public bool isFileNotFound { get; set; }
    public bool isSSLrequired { get; set; }
    public string cookieGuid { get; set; }
    public string tokenGuid { get; set; }
    public bool isNewCookie { get; set; }
    public string isActualLoginSuccessful { get; set; }
    public int testlevel { get; set; }
    public string testAusgabe { get; set; }
    public Mailrelay mailRelay = new();
    public string format { get; set; } = "";
    public string route1 { get; set; } = "";
    public string route2 { get; set; } = "";
    public string route3 { get; set; } = "";
    public string route4 { get; set; } = "";
    public string route5 { get; set; } = "";
    public string route6 { get; set; } = "";
    public string route7 { get; set; } = "";

    public Dictionary<string, string> gdQueryParam = new();

    //oh260704 ?Query=Parameter catched in key-value-pairs and checked for XSS
    // needs AssistInclude.FuncFillRouteMime(ref _apiModel, ...route1...);

    public string requestModelID { get; set; } = "";
    public string requestBatchID { get; set; } = "";
    public string requestItemID { get; set; } = "";
    public string requestVariant { get; set; } = "";
    public Guid actualDppGuid { get; set; } = new Guid("00000000-0000-0000-0000-000000000000");

    public DateTime requestDateTime { get; set; } = DateTime.Now;
    public string mimeType { get; set; } = "html";
    public bool isCompressed { get; set; } = true; // default: compressed=true for API according EN 18222

   public AssistInclude.gtRequestHeader requestHeader { get; set; }

}
public class Mailrelay
{
    public string Mailserver { get; set; }
    public string Mailuser { get; set; }
    public string Mailpassword { get; set; }
    public string Mailfrom { get; set; }
}



  



