using TaxManager.Application.Dto;

namespace TaxManager.Application.Interfaces;

public interface ILegalEnvironmentService
{
    Task<LegalEnvironmentDto> GetLegalEnvironmentByCodeAsync(string code);
    Task<IEnumerable<LegalEnvironmentDto>> GetAllLegalEnvironmentsAsync();
    Task<LegalEnvironmentDto> CreateLegalEnvironmentAsync(LegalEnvironmentCreateDto legalEnvironmentCreateDto);
}
