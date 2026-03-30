using AutoMapper;
using TaxManager.Application.Dto;
using TaxManager.Domain.Entities;

namespace TaxManager.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        //<Source,Destination>
        //mapping from DTO to Entity 
        CreateMap<TaxationDto, Taxation>();
        CreateMap<TaxationCreateDto, Taxation>();
        CreateMap<TaxationTaxRateDto, TaxationTaxRate>()
            .ForMember(d => d.Strategy, o => o.MapFrom(s => s.StrategyId));
        CreateMap<TaxRateDto, TaxRate>();
        CreateMap<SimpleTaxationDto, DependentTaxation>()
            .ForMember(d => d.ChildTaxationId, o => o.MapFrom(s => s.Id));
        CreateMap<CountryDto, Country>();
        CreateMap<SubdivisionDto, Subdivision>();
        CreateMap<LegalEnvironmentCreateDto, LegalEnvironment>()
            .ForMember(d => d.Taxations, opt => opt.Ignore());
        //mapping from Entity to DTO
        CreateMap<Subdivision, SubdivisionDto>();
        CreateMap<TaxRate, TaxRateDto>();
        CreateMap<TaxationTaxRate, TaxationTaxRateDto>()
            .ForCtorParam("StrategyId", o => o.MapFrom(s => s.Strategy));
        CreateMap<Taxation, TaxationDto>()
            .ForMember(d => d.Country, opt => opt.MapFrom(s => s.Country))
            .ForMember(d => d.Subdivisions, opt => opt.MapFrom(s => s.Subdivisions));
        CreateMap<DependentTaxation, SimpleTaxationDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.ChildTaxationId))
            .ForMember(d => d.Code,opt => opt.MapFrom(s => s.ChildTaxation.Code));
        CreateMap<Country, CountryDto>();
        CreateMap<LegalEnvironment, LegalEnvironmentDto>();
        CreateMap<TaxRate, TaxRateDto>();
    }
}
