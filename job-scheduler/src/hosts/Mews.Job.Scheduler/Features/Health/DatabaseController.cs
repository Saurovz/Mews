using System.Data;
using System.Net;
using Mews.Job.Scheduler.Core.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Features.Health;

[ApiController]
[Route("/db")]
public sealed class DatabaseController : ControllerBase
{
    private readonly JobSchedulerDbContext _context;

    public DatabaseController(JobSchedulerDbContext context)
    {
        _context = context;
    }

    [HttpGet("health")]
    [ProducesResponseType(200)]
    public IActionResult GetInfo()
    {
        try
        {
            _context.Database.OpenConnection();

            if (_context.Database.GetDbConnection().State == ConnectionState.Open)
            {
                return Ok(value: "Connection successful.");
            }
            else
            {
                var response = new { Message = "Failed to establish a connection to the database." };
                return StatusCode(
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    value: response
                );
            }
        }
        catch (Exception ex)
        {
            var errorResponse = new
            {
                Message = "An error occurred while connecting to the database.",
                ExceptionType = ex.GetType().Name,
                ExceptionMessage = ex.Message
            };
            return StatusCode(
                statusCode: (int)HttpStatusCode.InternalServerError,
                value: errorResponse
            );
        }
    }
}
