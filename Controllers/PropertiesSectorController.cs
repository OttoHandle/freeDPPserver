using freeDPPapi.DppModel;
using Microsoft.AspNetCore.Mvc;

namespace freeDPPapi.Controllers;

/// <summary>
/// responses temporary dictionary endpoint - part sectors
/// used by script.js in html representation
/// will be reworked when final standards on dictionary referencing exist
/// </summary>

[ApiController]
[Route("properties_sector")]
public class PropertiesSectorController : ControllerBase
{
    private readonly ILogger<PropertiesSectorController> _logger;
    private IWebHostEnvironment _environment;
    private apiModel _apiModel;
    private AssistRepoData _assistRepoData;
    private AssistInclude _assistInclude;
    private AssistInclude.gtRequestHeader _glRequestHeader;

    public PropertiesSectorController(ILogger<PropertiesSectorController> logger, IWebHostEnvironment env)
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
    [HttpGet("{sectorName}")]
    public IActionResult Get(string sectorName = "", [FromQuery] string? language = "")
    {
        // Set header
        _apiModel.requestHeader = _assistInclude.FuncGetRequestHeader(Request.Headers, _glRequestHeader);

        // Set language
        _assistInclude.SetLanguage(_apiModel, language ?? "");

        // Set routes
        AssistInclude.FuncFillRouteMime(ref _apiModel, "", sectorName, "", "", "", "", "", "");

        // Search properties
        List<FreeDppProperty> freeDppProperties = new();
        freeDppProperties = _assistRepoData.GetPropertiesFromSector(_apiModel.route1);

        if (freeDppProperties.Count() > 0)
        {
            return Ok(freeDppProperties);
        }
        else
        {
            // 204 Response
            return NoContent();
        }

    }
}
