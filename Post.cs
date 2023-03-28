public record Post(Guid By, string Content)
{
    public Guid Id { get; set; }
}