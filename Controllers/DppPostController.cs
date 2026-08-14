using freeDPPapi.DppModel;
using Microsoft.AspNetCore.Mvc;

namespace freeDPPapi.Controllers;

/// <summary>
/// implements CreateDPP According EN 18222 Table 16
/// this is part of the API
/// </summary>

[ApiController]
[Route("v1/dpps")]
public class DppPostController : ControllerBase
{
    private readonly ILogger<DppPostController> _logger;
    private IWebHostEnvironment _environment;
    private apiModel _apiModel;
    private AssistDppPostData _assistDppPostData;
    private AssistInclude _assistInclude;
    private AssistInclude.gtRequestHeader _glRequestHeader;

    public DppPostController(ILogger<DppPostController> logger, IWebHostEnvironment env)
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

    [HttpPost]
    public IActionResult CreateDpp([FromBody] FreeDppDppImport dpp)
    {
        // Verify JSON with Dpp Model
        if (dpp != null)
        {
            // Set request header
            _apiModel.requestHeader = _assistInclude.FuncGetRequestHeader(Request.Headers, _glRequestHeader);

            // Set language
            _assistInclude.SetLanguage(_apiModel);

            if (dpp.digitalProductPassportId.Length > 0)
            {
                bool validDpp = _assistDppPostData.VerifyDpp(dpp);

                if (validDpp == true)
                {
                    //string dppId = _assistCreateDpp.CreateDpp(dpp);
                }

                return Ok(validDpp);
            }
        }

        // TODO: Return different messages with different errors
        return StatusCode(400, "ClientErrorBadRequest");
    }
}
