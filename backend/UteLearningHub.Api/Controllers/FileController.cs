using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.File.Queries.GetFile;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Application.Services.Document;
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
    private readonly IDocumentPageCountService _documentPageCountService;

    public FileController(
        IFileStorageService fileStorageService,
        IFileRepository fileRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IUserService userService,
        IMediator mediator,
        IDocumentPageCountService documentPageCountService)
    {
        _fileStorageService = fileStorageService;
        _fileRepository = fileRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _userService = userService;
        _mediator = mediator;
        _documentPageCountService = documentPageCountService;
    }

    [HttpPost]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [Consumes("multipart/form-data")]
    [EnableRateLimiting("upload")]
    public async Task<ActionResult<FileDto>> Upload(
        IFormFile file,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("Bạn cần đăng nhập để upload file");

        if (file == null || file.Length == 0)
            throw new BadRequestException("Không có file nào được upload");

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
                ? "Loại file không hợp lệ. Chỉ chấp nhận file ảnh."
                : "Loại file không hợp lệ. Các loại được phép: jpg, jpeg, png, gif, webp, pdf.";
            throw new BadRequestException(message);
        }

        if (file.Length > MaxFileSizeBytes)
            throw new BadRequestException("Kích thước file phải nhỏ hơn 100MB");

        var userId = _currentUserService.UserId ?? Guid.Empty;

        // Đếm số trang từ stream gốc trước khi upload S3 (chỉ với PDF)
        int? totalPages = null;
        await using var stream = file.OpenReadStream();
        
        if (file.ContentType == "application/pdf")
        {
            try
            {
                totalPages = await _documentPageCountService.GetPageCountAsync(stream, file.ContentType, cancellationToken);
                stream.Position = 0; 
            }
            catch
            {
                totalPages = null;
            }
        }

        var url = await _fileStorageService.UploadFileAsync(
            stream,
            file.FileName,
            file.ContentType,
            normalizedCategory,
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

        _fileRepository.Add(entity);
        await _fileRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new FileDto
        {
            Id = entity.Id,
            FileSize = entity.FileSize,
            MimeType = entity.MimeType,
            TotalPages = totalPages
        };

        return Ok(dto);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFile(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetFileByIdQuery { FileId = id };
        var response = await _mediator.Send(query, cancellationToken);

        if (response.IsRedirect && !string.IsNullOrEmpty(response.RedirectUrl))
            return Redirect(response.RedirectUrl);
            
        if (response.Stream == null)
            throw new NotFoundException("Không tìm thấy nội dung file");

        Response.Headers.Append("Content-Disposition", "inline");
        return File(response.Stream, response.MimeType);
    }
}


