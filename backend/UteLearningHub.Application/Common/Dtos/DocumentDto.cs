using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos
{
    public record DocumentDto
    {
        public Guid Id { get; init; }
        public string DocumentName { get; init; } = default!;
        public string Description { get; init; } = default!;
        public bool IsDownload { get; init; }
        public VisibilityStatus Visibility { get; init; }
        public ReviewStatus ReviewStatus { get; init; }
        public SubjectDto? Subject { get; init; }
        public TypeDto Type { get; init; } = default!;
        public IList<TagDto> Tags { get; init; } = [];
        public IList<AuthorDto> Authors { get; init; } = [];
        public Guid? ThumbnailFileId { get; init; }
        public int FileCount { get; init; }
        public int CommentCount { get; init; }
        public int UsefulCount { get; init; }
        public int NotUsefulCount { get; init; }
        public Guid CreatedById { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }

    public record SubjectDto
    {
        public Guid Id { get; init; }
        public string SubjectName { get; init; } = default!;
        public string SubjectCode { get; init; } = default!;
        public IList<MajorDto> Majors { get; init; } = [];
    }

    public record TypeDto
    {
        public Guid Id { get; init; }
        public string TypeName { get; init; } = default!;
    }

    public record TagDto
    {
        public Guid Id { get; init; }
        public string TagName { get; init; } = default!;
    }

    public record AuthorDto
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = default!;
    }
}
