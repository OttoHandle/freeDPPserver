using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace freeDPPapi.Controllers;

//oh260810 - not yet comletely implemented - just a test controller for dpp validation

[ApiController]
[Route("dppValidate")]
public class DppValidateController : ControllerBase
{
    private readonly ILogger<DppValidateController> _logger;
    private IWebHostEnvironment _environment;
    private apiModel _apiModel;
    private AssistDppPostData _assistDppPostData;
    private AssistInclude _assistInclude;
    private AssistInclude.gtRequestHeader _glRequestHeader;
    private readonly string _schemaPfad = Path.Combine("Schemas", "dpp-schema.json");
    private readonly string _schemaPfadFull = Path.Combine("Schemas", "dpp-schema.json");
    public DppValidateController(ILogger<DppValidateController> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _environment = env;
        _apiModel = new apiModel();
        _apiModel._env = _environment;
        AssistInclude.FuncModelBefuellen(ref _apiModel, _environment);
        _assistDppPostData = new(_apiModel);
        _assistInclude = new AssistInclude();
        _glRequestHeader = new AssistInclude.gtRequestHeader();
    }

    [HttpPost("create")]
    public IActionResult CreateObject([FromBody] JsonObject jsonObject)
    {
        // Set request header
        _apiModel.requestHeader = _assistInclude.FuncGetRequestHeader(Request.Headers, _glRequestHeader);

        // Set language
        _assistInclude.SetLanguage(_apiModel);

        // Verify dpp
        if (jsonObject == null)
        {
            return BadRequest("no data received");
        } else
        {
            // not yet implemented:
            // check if jsonObject is valid according to the DPP schema
            // and insert DPP in database if valid, otherwise return BadRequest with error message

            //bool validDpp = _assistDppPostData.VerifyDpp(FreeDppDppImport jsonObject);
            //if (!validDpp)
            //{
            //    return BadRequest("Invalid DPP");
            //}
        }

        return Ok(jsonObject);

    }
}

