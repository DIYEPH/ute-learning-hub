using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.File.Queries.GetFile;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FileController(
    IFileStorageService fileStorageService,
    IFileRepository fileRepository,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IMediator mediator,
    IDocumentPageCountService documentPageCountService) : ControllerBase
{
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private static readonly string[] DocumentExtensions = [".pdf"];
    private const long MaxFileSizeBytes = 100 * 1024 * 1024;

    [HttpPost]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [Consumes("multipart/form-data")]
    [EnableRateLimiting("upload")]
    public async Task<ActionResult<FileDto>> Upload(
        IFormFile file,
        [FromQuery] string? category,
        CancellationToken ct)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("Bạn cần đăng nhập để upload file");

        if (file == null || file.Length == 0)
            throw new BadRequestException("Không có file nào được upload");

        var normalizedCategory = category?.Trim();
        var isImageOnlyCategory = string.IsNullOrWhiteSpace(normalizedCategory) ||
                                  normalizedCategory.Equals("AvatarUser", StringComparison.OrdinalIgnoreCase) ||
                                  normalizedCategory.Equals("AvatarConversation", StringComparison.OrdinalIgnoreCase) ||
                                  normalizedCategory.Equals("DocumentCover", StringComparison.OrdinalIgnoreCase) ||
                                  normalizedCategory.Equals("DocumentFileCover", StringComparison.OrdinalIgnoreCase);

        var allowedExtensions = isImageOnlyCategory ? ImageExtensions : [..ImageExtensions, ..DocumentExtensions];
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new BadRequestException(isImageOnlyCategory ? "Chỉ chấp nhận file ảnh." : "Chỉ chấp nhận: jpg, jpeg, png, gif, webp, pdf.");

        if (file.Length > MaxFileSizeBytes)
            throw new BadRequestException("Kích thước file phải nhỏ hơn 100MB");

        var userId = currentUserService.UserId ?? Guid.Empty;
        int? totalPages = null;
        await using var stream = file.OpenReadStream();

        if (file.ContentType == "application/pdf")
        {
            try
            {
                totalPages = await documentPageCountService.GetPageCountAsync(stream, file.ContentType, ct);
                stream.Position = 0;
            }
            catch { totalPages = null; }
        }

        var url = await fileStorageService.UploadFileAsync(stream, file.FileName, file.ContentType, normalizedCategory, ct);

        var entity = new Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            FileUrl = url,
            FileSize = file.Length,
            MimeType = file.ContentType,
            CreatedById = userId,
            CreatedAt = dateTimeProvider.OffsetNow
        };

        fileRepository.Add(entity);
        await fileRepository.UnitOfWork.SaveChangesAsync(ct);

        return Ok(new FileDto
        {
            Id = entity.Id,
            FileSize = entity.FileSize,
            MimeType = entity.MimeType,
            TotalPages = totalPages
        });
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFile(Guid id, CancellationToken ct)
    {
        var query = new GetFileByIdQuery { FileId = id };
        var response = await mediator.Send(query, ct);

        if (response.IsRedirect && !string.IsNullOrEmpty(response.RedirectUrl))
            return Redirect(response.RedirectUrl);

        if (response.Stream == null)
            throw new NotFoundException("Không tìm thấy nội dung file");

        Response.Headers.Append("Content-Disposition", "inline");
        return File(response.Stream, response.MimeType);
    }
}