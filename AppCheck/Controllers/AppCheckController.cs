using AppCheck.Helper.Attributes;
using AppCheck.Settings.Model.ResponseModel;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AppCheck.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppCheckController : ControllerBase
    {
        private readonly ILogger<AppCheckController> _logger;

        public AppCheckController(ILogger<AppCheckController> logger)
        {
            _logger = logger;
        }

        [Route("GetDataWithAppCheck")]
        [HttpGet]
        [FirebaseAppCheck]
        public IActionResult GetDataWithAppCheck()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                _logger.LogInformation($"Environment URL: {HttpContext.Request.GetDisplayUrl()}");
                response.Success = true;
                response.Message = "Authorized access!";
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpGet]
        [Route("GetDataWithoutAppCheck")]
        [ExcludeCustomHeader]
        public IActionResult GetDataWithoutAppCheck()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                _logger.LogInformation($"Environment URL: {HttpContext.Request.GetDisplayUrl()}");
                response.Success = true;
                response.Message = "Hello World!";
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
        }
    }
}