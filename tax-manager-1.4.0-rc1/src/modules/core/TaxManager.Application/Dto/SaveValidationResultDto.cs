namespace TaxManager.Application.Dto;

public record SaveValidationResultDto
{
    public bool IsValid { get; set; }
    public IEnumerable<string> Errors { get; set; }
    public object Entity { get; set; }
}
