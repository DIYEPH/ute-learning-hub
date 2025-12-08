using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Document : SoftDeletableEntity<Guid>, IAggregateRoot, IAuditable
{
    public Guid? SubjectId { get; set; }
    public Guid TypeId { get; set; }
    public string Description { get; set; } = default!;
    public string DocumentName { get; set; } = default!;
    public string NormalizedName { get; set; } = default!;
    public bool IsDownload { get; set; } = true;
    public VisibilityStatus Visibility { get; set; } = VisibilityStatus.Internal;
    public Guid? CoverFileId { get; set; }
    public File? CoverFile { get; set; }

    public Subject? Subject { get; set; }
    public Type Type { get; set; } = default!;
    public ICollection<DocumentTag> DocumentTags { get; set; } = [];
    public ICollection<DocumentFile> DocumentFiles { get; set; } = [];
    public ICollection<DocumentAuthor> DocumentAuthors { get; set; } = [];
    public ICollection<UserDocumentProgress> UserProgresses { get; set; } = [];
    public ICollection<Report> Reports { get; set; } = [];
    public ICollection<DocumentReview> Reviews { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}

