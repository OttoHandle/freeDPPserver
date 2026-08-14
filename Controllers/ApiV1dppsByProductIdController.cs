using freeDPPapi.DppModel;
using Microsoft.AspNetCore.Mvc;
using static freeDPPapi.DppModel.FreeDppDppFull;

namespace freeDPPapi.Controllers;

[ApiController]
[Route("v1/dppsByProductId")]

// contains HTTP methods according EN 18222:
// ReadDppsByProductId (GET)
// further methods
// CreateDPP (POST) - see DppPostController.cs
// UpdateDPPById (PATCH) see ApiV1dppsController.cs
// DeleteDPPById (DELETE) see ApiV1dppsController.cs
public class ApiV1dppsByProductIdController : ControllerBase
{
    private readonly ILogger<PropertyController> _logger;
    private IWebHostEnvironment _environment;
    private apiModel _apiModel;
    private AssistRepoData _assistRepoData;
    private AssistInclude _assistInclude;
    private AssistInclude.gtRequestHeader _glRequestHeader;
    private AssistDppPostData _assistDppPostData;

    public ApiV1dppsByProductIdController(ILogger<PropertyController> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _environment = env;
        _apiModel = new apiModel();
        _apiModel._env = _environment;
        AssistInclude.FuncModelBefuellen(ref _apiModel, _environment);
        _assistRepoData = new(_apiModel);
        _assistInclude = new AssistInclude();
        _glRequestHeader = new AssistInclude.gtRequestHeader();
    }


    // ReadDppsByProductId: /v1/dppsByProductId/{productId} where productId may be urlencoded and follow schemas in EN 18219 5.2
    //                                         {productId} in {dppId}, works similar to ReadDPPbyId, but uses the productId instead of dppId to find the DPP
    //                     note: resolving is nearly similar to ReadDPPbyId, therefore it calls the same dppController.FuncFillDPP --> 
    //[HttpGet(Name = "ReadDppsByProductId")] // see line 11
    [HttpGet(Name = "ReadDPPByProductId")]
    [HttpGet("{dppId}")]
    [HttpGet("{dppId}/{route1}")]
    [HttpGet("{dppId}/{route1}/{route2}")] // if == elements, next
    [HttpGet("{dppId}/{route1}/{route2}/{route3}")] // Route3=elementIdPath
    [HttpGet("{dppId}/{route1}/{route2}/{route3}/{route4}")] // further routes should never be used in ReadDPPbyId, but handled for possible extensions
    [HttpGet("{dppId}/{route1}/{route2}/{route3}/{route4}/{route5}")]
    [HttpGet("{dppId}/{route1}/{route2}/{route3}/{route4}/{route5}/{route6}")]

    public IActionResult Get(string dppId = "", string route1 = "", string route2 = "", string route3 = "", string route4 = "", string route5 = "", string route6 = "", [FromQuery] string? representation = "compressed", [FromQuery] string? language = "")
    // representation "full" for EN 18223 Annex 1, "compressed" as default for EN 18223 5.2 serialisation
    // there should not be a route, but get nonetheless it for error handling
    // language not used in API yet
    {       // https://localhost:7032/v1/dppsByProductId/4003973287696?representation=compressed
            // allows also for use of ID-scheme as productId
            // example: https://localhost:7032/v1/dppsByProductId/01/4003973287696?representation=full
        // populate header section of DPP
        _apiModel.requestHeader = _assistInclude.FuncGetRequestHeader(Request.Headers, _glRequestHeader);
        // define language
        _assistInclude.SetLanguage(_apiModel, language ?? "");
        // evaluate routes and parameters and secure against XSS 
        AssistInclude.FuncFillRouteMime(ref _apiModel, "", dppId, route1, route2, route3, "", "", ""); //dppId in route1
        _apiModel.dppId = dppId;
        // find DPP
        DigitalProductPassport loDPP = new DigitalProductPassport();
        bool lbSuccess = dppController.FuncFillDPP(ref _apiModel, ref loDPP);

        // does not exist in EN18222 for dppsByProductId, only dpps. But works fine
        //                   -> voluntary extension for dppsByProductId, but should be used with care, because it is not defined in EN18222
        if (_apiModel.route2 == "elements" && _apiModel.route3.Length > 0 && loDPP.economicOperatorId.Length > 0)
        { // ReadDataElement:
          // take the part behind the elementPath of ReadDPPbyId
            // examples:
            // https://localhost:7032/v1/dppsByProductId/5012345101095/elements/c0DemoEconomicOperator?representation=full
            // https://localhost:7032/v1/dppsByProductId/5012345101095/elements/c0DemoEconomicOperator%2F_p_d_LEI?representation=full
            // https://localhost:7032/v1/dppsByProductId/01/5012345101095/elements/c0DemoEconomicOperator/_p_d_LEI?representation=full
            var loPart = AssistDppElementPath.FuncGetElementOnPath(ref _apiModel, ref loDPP);
            //if (_apiModel.isCompressed == true )
            //{ // not yet implemented, but should be compressed if representation=compressed
            //    return Ok(FreeDppDppCompressed.FuncCompressDPP(ref _apiModel, ref loPart));
            //} else
            //{
            return Ok(loPart);
            //}
        }

        if (loDPP.economicOperatorId.Length > 0)
        {
            if (_apiModel.isCompressed == true) // default representation == "compressed"
            {
                return Ok(FreeDppDppCompressed.FuncCompressDPP(ref _apiModel, ref loDPP));
            }
            else // representation == "full"
            {
                return Ok(loDPP);
            }
        }
        else
        {
            // 204 Response
            return NoContent();
        }
    }
}
