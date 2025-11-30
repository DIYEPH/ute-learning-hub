using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Faculty.Commands.CreateFaculty;
using UteLearningHub.Application.Features.Faculty.Commands.DeleteFaculty;
using UteLearningHub.Application.Features.Faculty.Commands.UpdateFaculty;
using UteLearningHub.Application.Features.Faculty.Queries.GetFacultyById;
using UteLearningHub.Application.Features.Faculty.Queries.GetFaculties;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FacultyController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;

    public FacultyController(IMediator mediator, IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _environment = environment;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<FacultyDto>>> GetFaculties([FromQuery] GetFacultiesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FacultyDetailDto>> GetFacultyById(Guid id)
    {
        var query = new GetFacultyByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FacultyDetailDto>> CreateFaculty([FromBody] CreateFacultyCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FacultyDetailDto>> UpdateFaculty(Guid id, [FromBody] UpdateFacultyCommand command)
    {
        command = command with { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFaculty(Guid id)
    {
        var command = new DeleteFacultyCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("upload-logo")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<string>> UploadLogo(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Invalid file type. Only images are allowed.");
        }

        // Validate file size (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest("File size must be less than 5MB");
        }

        try
        {
            // Create directory if it doesn't exist
            var uploadDir = Path.Combine(_environment.WebRootPath, "images", "faculties");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the URL path
            var logoUrl = $"/images/faculties/{fileName}";
            return Ok(new { url = logoUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error uploading file: {ex.Message}");
        }
    }
}
