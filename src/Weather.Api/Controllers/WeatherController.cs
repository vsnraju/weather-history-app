using Microsoft.AspNetCore.Mvc;
using Weather.Application.Weather;

namespace Weather.Api.Controllers;

[ApiController]
[Route("api/weather")]
public sealed class WeatherController(IWeatherReportService weatherReportService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WeatherEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<WeatherEntryDto>>> Get(CancellationToken cancellationToken)
    {
        var result = await weatherReportService.GetWeatherAsync(cancellationToken);
        if (result.IsFailure)
        {
            return Problem(
                title: "Weather data could not be loaded.",
                detail: string.Join(" ", result.Errors.Select(error => error.Message)),
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(result.Value);
    }
}
