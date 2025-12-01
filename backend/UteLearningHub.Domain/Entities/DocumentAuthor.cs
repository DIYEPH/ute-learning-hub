namespace UteLearningHub.Domain.Entities;

public class DocumentAuthor
{
    public Guid DocumentId { get; set; }
    public Guid AuthorId { get; set; }
    public string? Role { get; set; }

    public Document Document { get; set; } = default!;
    public Author Author { get; set; } = default!;
}


