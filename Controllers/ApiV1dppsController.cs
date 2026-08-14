using freeDPPapi.DppModel;
using Microsoft.AspNetCore.Mvc;
using static freeDPPapi.DppModel.FreeDppDppFull;

namespace freeDPPapi.Controllers;

[ApiController]
[Route("v1/dpps")]

// contains HTTP methods according EN 18222:
// ReadDPPById (GET)
// CreateDPP (POST)  - see DppPostController.cs
// UpdateDPPById (PATCH) - not yet finalized
// DeleteDPPById (DELETE) - not yet finalized
public class ApiV1dppsController : ControllerBase
{
    private readonly ILogger<PropertyController> _logger;
    private IWebHostEnvironment _environment;
    private apiModel _apiModel;
    private AssistRepoData _assistRepoData;
    private AssistInclude _assistInclude;
    private AssistInclude.gtRequestHeader _glRequestHeader;
    private AssistDppPostData _assistDppPostData;

    public ApiV1dppsController(ILogger<PropertyController> logger, IWebHostEnvironment env)
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


    // ReadDPPbyId path: /v1/dpps/{dppId} where dppId may be an urlencoded Link to the DPP as on QR-Code according EN 18222
    // ReadDPPByProductId: /v1/dppsByProductId/{productId} where productId may be urlencoded and follow schemas in EN 18219 5.2
    //                                         {productId} in {dppId}, works similar to ReadDPPbyId, but uses the productId instead of dppId to find the DPP
    //                     see ApiV1dppsByProductIdController.cs
    //                     note: resolving is nearly similar to ReadDPPbyId, therefore it calls the same dppController.FuncFillDPP --> 

    [HttpGet(Name = "ReadDPPbyId")]
    [HttpGet("{dppId}")]
    [HttpGet("{dppId}/{route1}")]
    [HttpGet("{dppId}/{route1}/{route2}")] // if == elements, next
    [HttpGet("{dppId}/{route1}/{route2}/{route3}")] // Route3=elementIdPath
    [HttpGet("{dppId}/{route1}/{route2}/{route3}/{route4}")] // further routes should never be used in ReadDPPbyId, but handled for possible extensions
    [HttpGet("{dppId}/{route1}/{route2}/{route3}/{route4}/{route5}")]
    [HttpGet("{dppId}/{route1}/{route2}/{route3}/{route4}/{route5}/{route6}")]
    public IActionResult Get(string dppId = "", string route1 = "", string route2 = "", string route3 = "", string route4 = "", string route5 = "", string route6 = "", [FromQuery] string? representation = "compressed", [FromQuery] string? language = "") 
        // representation "full" for Annex 1, "compressed" as default for EN 18223 5.2 serialisation
        // there should not be a route, but get nonetheless it for error handling
        // language not used in API yet
    {       // https://localhost:7032/v1/dpps/4003973287696?representation=compressed
        // Header befüllen
        _apiModel.requestHeader = _assistInclude.FuncGetRequestHeader(Request.Headers, _glRequestHeader);
        // Sprache einstellen
        _assistInclude.SetLanguage(_apiModel, language ?? "");
        // Routen und Parameter befüllen und von XSS befreien
        AssistInclude.FuncFillRouteMime(ref _apiModel, "", dppId, route1, route2, route3, "", "", ""); //dppId in route1
        _apiModel.dppId = dppId;
        // DPP suchen suchen
        DigitalProductPassport loDPP = new DigitalProductPassport();
        bool lbSuccess = dppController.FuncFillDPP(ref _apiModel, ref loDPP);

        if (_apiModel.route2=="elements" && _apiModel.route3.Length>0 && loDPP.economicOperatorId.Length > 0)
        { // ReadDataElement: take the part behind the elementPath of ReadDPPbyId
            // https://localhost:7032/v1/dpps/5012345101095/elements/c0DemoEconomicOperator?representation=full
            // https://localhost:7032/v1/dpps/5012345101095/elements/c0DemoEconomicOperator%2F_p_d_LEI?representation=full
            // https://localhost:7032/v1/dpps/5012345101095/elements/c0DemoEconomicOperator/_p_d_LEI?representation=full
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
                return Ok(FreeDppDppCompressed.FuncCompressDPP(ref _apiModel,ref loDPP));
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



    // POST done by DppPostController.cs


    // UpdateDPPById path: /v1/dpps/{dppId}
    [HttpPatch(Name = "UpdateDPPbyId")]
    [HttpPatch("{dppId}")]
    [HttpPatch("{dppId}/{route1}")]
    [HttpPatch("{dppId}/{route1}/{route2}")]
    [HttpPatch("{dppId}/{route1}/{route2}/{route3}")]
    [HttpPatch("{dppId}/{route1}/{route2}/{route3}/{route4}")]
    [HttpPatch("{dppId}/{route1}/{route2}/{route3}/{route4}/{route5}")]
    [HttpPatch("{dppId}/{route1}/{route2}/{route3}/{route4}/{route5}/{route6}")]
    [HttpPatch("{dppId}/{route1}/{route2}/{route3}/{route4}/{route5}/{route6}/{route7}")]


    public IActionResult Patch(string dppId, [FromBody] FreeDppDppImport dpp, [FromQuery] string? representation = "compressed", [FromQuery] string? language = "")
    // representation "full" for Annex 1, "compressed" as default for EN 18223 5.2 serialisation
    // there should not be a route, but get nonetheless it for error handling
    // language not used in API yet
    // same as DppPostController c IActionResult CreateDpp([FromBody] FreeDppDpp dpp)
    // note: needs HTTP Content-Type = application/json and content as json-code in body of Request
    {
        // check JSON body, if not set, return BadRequest
        if (dpp == null)
        {
            return BadRequest("ClientErrorBadRequest");
        }

        _apiModel.requestHeader = _assistInclude.FuncGetRequestHeader(Request.Headers, _glRequestHeader);

        // define language for response, if not set, default is "en-GB"
        _assistInclude.SetLanguage(_apiModel, language ?? "");

        // Routen befüllen
        AssistInclude.FuncFillRouteMime(ref _apiModel, "", dppId, "", "", "", "", "", "");


        // not yet finalized
        return StatusCode(501, "ServerNotImplemented");
        // find DPP properties - code not reachable yet, but should be used for update sequence in database
        FreeDppProperty freeDppProperty = new();
        freeDppProperty = _assistRepoData.GetProperty(_apiModel.route1);

        if (freeDppProperty.Id > 0)
        {
            return Ok(freeDppProperty);
        }
        else
        {
            // 204 Response
            return NoContent();
        }
    }


    // DeleteDPPbyId path: /v1/dpps/{dppId} where dppId may be an urlencoded Link to the DPP as on QR-Code according EN 18222
    [HttpDelete(Name = "DeleteDPPbyId")]
    [HttpDelete("{dppId}")]
    [HttpDelete("{dppId}/{route1}")]
    [HttpDelete("{dppId}/{route1}/{route2}")]
    [HttpDelete("{dppId}/{route1}/{route2}/{route3}")]
    [HttpDelete("{dppId}/{route1}/{route2}/{route3}/{route4}")]
    [HttpDelete("{dppId}/{route1}/{route2}/{route3}/{route4}/{route5}")]
    [HttpDelete("{dppId}/{route1}/{route2}/{route3}/{route4}/{route5}/{route6}")]
    [HttpDelete("{dppId}/{route1}/{route2}/{route3}/{route4}/{route5}/{route6}/{route7}")]
    public IActionResult Delete(string dppId = "", [FromQuery] string? language = "") //, [FromQuery] string? representation = "compressed"
    // representation "full" for Annex 1, "compressed" as default for EN 18223 5.2 serialisation
    // there should not be a route, but get nonetheless it for error handling
    // representation not used here, because delete does not return a DPP, but only a status code
    // language not used in API yet
    {
        // Header befüllen
        _apiModel.requestHeader = _assistInclude.FuncGetRequestHeader(Request.Headers, _glRequestHeader);

        // Sprache einstellen
        _assistInclude.SetLanguage(_apiModel, language ?? "");

        // Routen befüllen
        AssistInclude.FuncFillRouteMime(ref _apiModel, "", dppId, "", "", "", "", "", "");

        // DPP suchen suchen
        FreeDppProperty freeDppProperty = new();
        freeDppProperty = _assistRepoData.GetProperty(_apiModel.route1);

        if (freeDppProperty.Id > 0)
        {
            // not yet finalized
            // delete sequence in database missing
            return Ok(dppId);
        }
        else
        {
            // 204 Response
            return NoContent();
        }
    }
}
