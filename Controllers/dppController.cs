using Microsoft.AspNetCore.Mvc;
using static freeDPPapi.DppModel.FreeDppDppFull;

namespace freeDPPapi.Controllers;

/// <summary>
/// contains reponse on QR-Code Link to a DPP
/// either as JSON (on HTTP-Accept application/json) - also possible with ?contentType=json
/// or as HTML file containing a JS that requests the JSON and renders for human readable view
/// This is NOT the API according EN 18222 - see ApiV1dppsController for API
/// </summary>


[ApiController]
[Route("/")]
public class dppController : ControllerBase
{

    public AssistInclude myAssist = new AssistInclude();

    private IWebHostEnvironment Environment;

    public dppController(IWebHostEnvironment env) // results from startup.cs: services.AddSingleton<IWebHostEnvironment>(env);  // env is injected by DI
    {
        Environment = env;
        // may use services.AddHttpContextAccessor(); myObject.HttpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor();
    }

    private readonly ILogger<dppController> _logger;

    //public dppController(ILogger<dppController> logger)
    //{
    //    _logger = logger;
    //}

    [HttpGet(Name = "Getroot")]
    [HttpGet("{format}")] // this is not a conflict to API, since routes are used differently there
    [HttpGet("{format}/{route1}")]
    [HttpGet("{format}/{route1}/{route2}")]
    [HttpGet("{format}/{route1}/{route2}/{route3}")]
    [HttpGet("{format}/{route1}/{route2}/{route3}/{route4}")]
    [HttpGet("{format}/{route1}/{route2}/{route3}/{route4}/{route5}")]
    [HttpGet("{format}/{route1}/{route2}/{route3}/{route4}/{route5}/{route6}")]
    [HttpGet("{format}/{route1}/{route2}/{route3}/{route4}/{route5}/{route6}/{route7}")]


    //oh260716 void instead of IActionresult 
    // to avoid problems with FuncSetContextHeader <-> StatusCode cannot be set because the response has already
        public void Get(string format = "", string route1 = " ", string route2 = "", string route3 = "", string route4 = "", string route5 = "",
            string route6 = "", string route7 = "", [FromQuery] string? contentType = "", [FromQuery] string? language = "")   //IEnumerable<DppTestData> Get()
                                                      // Full Example https://example.com/01/09524000059109/22/2A/10/ABC123/21/12345XYZ?11=251121
                                                      //                                 /01/GTIN/22/VARIANT/10/BATCH/21/SERIALNR?11=date
    {
        apiModel J = new apiModel();
        J._env = Environment;
        AssistInclude.FuncModelBefuellen(ref J, Environment); // replaces:
                                                              // J.Code = AssistInclude.funcGetAppsetting(Environment, "Global:dppDB"); 
        
        // outside API, adressed by QR-Code Link, response is not compressed but full, while in API default is isCompressed=true
        J.isCompressed = false; 

        AssistInclude.gtRequestHeader glRequestHeader = new AssistInclude.gtRequestHeader();
        J.requestHeader = myAssist.FuncGetRequestHeader(Request.Headers, glRequestHeader); // needs to be called initially in any controller, to get the request header information into the model J

        // set language from query parameter, if provided, otherwise from HTTP header Accept-Language, otherwise default 
        myAssist.SetLanguage(J, language ?? "");

        AssistInclude.FuncFillRouteMime(ref J, format, route1, route2, route3, route4, route5, route6, route7);

        // creates a DigitalProductPassport object and fills it with data from the database, based on the request parameters in J if JSON is requested,
        // otherwise returns HTML with JS that requests the JSON and renders it for human readable view
        if (J.mimeType == "json" || contentType == "json")
        {
            DigitalProductPassport loDPP = new DigitalProductPassport();
            bool lbSuccess= dppController.FuncFillDPP(ref J, ref loDPP);
            string jsonobject = System.Text.Json.JsonSerializer.Serialize(loDPP);
            myAssist.FuncSetContextHeader(ref J, "application/json; charset=utf-8", jsonobject.ToString(), true, "");
        }
        else if (J.mimeType=="html")
        {
            string html= "<!DOCTYPE html><html lang=\"de-de\"><head><title>freeDPP service</title>"+
                "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0,  maximum-scale=1, user-scalable=yes\">" +
                "<meta name = \"generator\" content = \"freeDPP\"><link rel=\"stylesheet\" href=\"/style.css\">" +
                "<script>var lsDPPsource=\""+J.requestHeader.FullPath+"\"; gs_language='" + J.Sprache + "'</script>" +
                "<script src=\"/script.js\" defer></script></head>" +
                "<body></body></html>";
            myAssist.FuncSetContextHeader(ref J, "text/html; charset=utf-8", html, true, "");
        } else
        {
           //file from wwwroot
        }
         return;
    }
    public static bool FuncFillDPP(ref apiModel J, ref DigitalProductPassport loDPP)
    {
        bool lbResult = false;
        bool lbDPPidentifierInLink=AssistDPPdata.FuncFindDppIdentifier(ref J); // find DPP according request
        if (lbDPPidentifierInLink)
        {
            lbResult = AssistDPPdata.FuncGetDppHeader(ref J, ref loDPP);
            if (lbResult) {
                lbResult = AssistDPPdata.FuncGetDppCriteria(ref J, ref loDPP);
                    }; // read ESPR-Annex-1 Criteria
        }
        return lbResult;
    }
}
