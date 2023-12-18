using CourtPiece.WebApi.Grains;
using Microsoft.AspNetCore.Mvc;
using Orleans.Concurrency;
using static AuthService;
namespace CourtPiece.WebApi.Controller.v2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
public class AuthenticationController : ControllerBase
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IGrainFactory grainFactory, ILogger<AuthenticationController> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
    }


    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid payload");
            var grain = this._grainFactory.GetGrain<IAuthenticationGrain>(0);
            var (status, message) = await grain.Login(new Immutable<LoginModel>(model));
            if (status == 0)
                return BadRequest(message);
            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    [Route("registeration")]
    public async Task<IActionResult> Register(RegistrationModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid payload");

            var grain = this._grainFactory.GetGrain<IAuthenticationGrain>(0);

            var (status, message) = await grain.Registration(new Immutable<RegistrationModel>(model));
            if (status == 0)
            {
                return BadRequest(message);
            }
            return CreatedAtAction(nameof(Register), model);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}

