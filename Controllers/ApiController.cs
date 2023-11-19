using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Workout.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApiController : ControllerBase
{
    protected IActionResult Problem(List<Error> errors)
    {
        Serilog.Log.Error("Error: {errors}", errors);
        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            ModelStateDictionary modelStateDictionary = new();

            foreach (Error error in errors)
            {
                modelStateDictionary.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(modelStateDictionary);
        }

        if (errors.Any(e => e.Type == ErrorType.Unexpected))
        {
            return Problem();
        }

        Error firstError = errors[0];

        int statusCode = firstError.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(statusCode: statusCode, title: firstError.Description);
    }
}