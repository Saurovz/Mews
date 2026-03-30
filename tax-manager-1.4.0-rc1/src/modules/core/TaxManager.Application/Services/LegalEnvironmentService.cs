using AutoMapper;
using TaxManager.Application.Dto;
using TaxManager.Application.Interfaces;
using TaxManager.Application.Common.Exception;
using TaxManager.Domain.Entities;
using TaxManager.Domain.Interfaces;

namespace TaxManager.Application.Services;

public class LegalEnvironmentService(IMapper mapper, 
    ILegalEnvironmentRepository repository, ITaxationRepository taxationRepository) : ILegalEnvironmentService
{
    public async Task<LegalEnvironmentDto> GetLegalEnvironmentByCodeAsync(string code)
    {
        var legalEnvironment = await repository.GetByCodeAsync(code);
        var entity =  mapper.Map<LegalEnvironmentDto>(legalEnvironment);
        if (entity == null)
        {
            throw new NotFoundException($"Entity with code '{code}' not found.");
        }
        return entity;
    }

    public async Task<IEnumerable<LegalEnvironmentDto>> GetAllLegalEnvironmentsAsync()
    {
        var legalEnvironments = await repository.GetAllAsync();
        var entities =  mapper.Map<IEnumerable<LegalEnvironmentDto>>(legalEnvironments);
        if (entities == null || entities.Count() == 0 )
        {
            throw new NotFoundException("No Items found.");
        }
        return entities;
    }
    public async Task<LegalEnvironmentDto> CreateLegalEnvironmentAsync(LegalEnvironmentCreateDto legalEnvironmentCreateDto)
    {
        // Business rule validation for uniqueness of Legal Environment Code
        if (await LegalEnvironmentCodeExistsAsync(legalEnvironmentCreateDto.Code))
        {
            throw new BusinessRuleException("Legal Environment Code exists!!");
        }
        
        //Map the incoming DTO to Entity for persistence
        var legalEnvironment = mapper.Map<LegalEnvironment>(legalEnvironmentCreateDto);
        foreach (var id in legalEnvironmentCreateDto.TaxationIds)
        {
            legalEnvironment.Taxations.Add(await taxationRepository.GetByIdAsync(id) 
                                           ?? throw new NotFoundException($"Taxation id {id} not found!"));
        }
        
        var createdLegalEnvironment = await repository.AddAsync(legalEnvironment);
        return mapper.Map<LegalEnvironmentDto>(createdLegalEnvironment);
    }
    
    #region Private Methods
   
    //Duplicity Checks
    private async Task<bool> LegalEnvironmentCodeExistsAsync(string legalEnvironmentCode)
    {
        if (string.IsNullOrWhiteSpace(legalEnvironmentCode))
        {
            throw new ArgumentException("Legal Environment code cannot be empty", nameof(legalEnvironmentCode));
        }

        return await repository.AnyAsync(p => p.Code == legalEnvironmentCode);
    }
    #endregion
}
