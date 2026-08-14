using freeDPPapi.DppModel;
using Microsoft.AspNetCore.Mvc;

namespace freeDPPapi.Controllers;
/// <summary>
/// responses temporary dictionary endpoint - part parameters and criteria
/// used by script.js in html representation
/// will be reworked when final standards on dictionary referencing exist
/// </summary>
[ApiController]
[Route("property")]
public class PropertyController : ControllerBase
{
    private readonly ILogger<PropertyController> _logger;
    private IWebHostEnvironment _environment;
    private apiModel _apiModel;
    private AssistRepoData _assistRepoData;
    private AssistInclude _assistInclude;
    private AssistInclude.gtRequestHeader _glRequestHeader;

    public PropertyController(ILogger<PropertyController> logger, IWebHostEnvironment env)
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

    [HttpGet]
    [HttpGet("{paramName}")]
    public IActionResult Get(string paramName = "", [FromQuery] string? language = "")
    {
        // Set header
        _apiModel.requestHeader = _assistInclude.FuncGetRequestHeader(Request.Headers, _glRequestHeader);

        // Set language
        _assistInclude.SetLanguage(_apiModel, language ?? "");

        // Set routes
        AssistInclude.FuncFillRouteMime(ref _apiModel, "", paramName, "", "", "", "", "", "");

        // Search property
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
}
