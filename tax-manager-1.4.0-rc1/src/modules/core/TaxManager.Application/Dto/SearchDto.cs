namespace TaxManager.Application.Dto;

public class SearchDto
{
    public SearchDto(IEnumerable<TaxationDto> taxations, IEnumerable<LegalEnvironmentDto> legalEnvironments)
    {
        Taxations = taxations;
        LegalEnvironments = legalEnvironments;
    }

    public IEnumerable<TaxationDto> Taxations { get; set; }
    public IEnumerable<LegalEnvironmentDto> LegalEnvironments { get; set; }
}
