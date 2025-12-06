using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Account.Commands.UpdateProfile;
using UteLearningHub.Application.Features.File.Queries.GetFile;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FileController : ControllerBase
{
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private static readonly string[] DocumentExtensions = [".pdf"];
    private const long MaxFileSizeBytes = 100 * 1024 * 1024;

    private readonly IFileStorageService _fileStorageService;
    private readonly IFileRepository _fileRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;

    public FileController(
        IFileStorageService fileStorageService,
        IFileRepository fileRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IUserService userService,
        IMediator mediator)
    {
        _fileStorageService = fileStorageService;
        _fileRepository = fileRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _userService = userService;
        _mediator = mediator;
    }

    [HttpPost]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileDto>> Upload(
        IFormFile file,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var normalizedCategory = category?.Trim();
        var isImageOnlyCategory = string.IsNullOrWhiteSpace(normalizedCategory) ||
                                  normalizedCategory.Equals("AvatarUser", StringComparison.OrdinalIgnoreCase) ||
                                  normalizedCategory.Equals("AvatarConversation", StringComparison.OrdinalIgnoreCase) ||
                                  normalizedCategory.Equals("DocumentCover", StringComparison.OrdinalIgnoreCase) ||
                                  normalizedCategory.Equals("DocumentFileCover", StringComparison.OrdinalIgnoreCase);

        var allowedExtensions = isImageOnlyCategory
            ? ImageExtensions
            : ImageExtensions.Concat(DocumentExtensions).ToArray();

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            var message = isImageOnlyCategory
                ? "Invalid file type. Only image files are allowed."
                : "Invalid file type. Allowed types: jpg, jpeg, png, gif, webp, pdf.";
            return BadRequest(message);
        }

        if (file.Length > MaxFileSizeBytes)
            return BadRequest("File size must be less than 100MB");

        var userId = _currentUserService.UserId ?? Guid.Empty;

        await using var stream = file.OpenReadStream();
        var url = await _fileStorageService.UploadFileAsync(
            stream,
            file.FileName,
            file.ContentType,
            cancellationToken);

        var entity = new UteLearningHub.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            FileUrl = url,
            FileSize = file.Length,
            MimeType = file.ContentType,
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        await _fileRepository.AddAsync(entity, cancellationToken);
        await _fileRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new FileDto
        {
            Id = entity.Id,
            FileSize = entity.FileSize,
            MimeType = entity.MimeType
        };

        return Ok(dto);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFile(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetFileQuery { FileId = id };
            var response = await _mediator.Send(query, cancellationToken);

            // Set Content-Disposition to inline to display in browser
            Response.Headers.Append("Content-Disposition", "inline");
            
            return File(response.FileStream, response.MimeType);
        }
        catch (NotFoundException)
        {
            return NotFound("File not found");
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}


