using TaxManager.Application.Dto;

namespace TaxManager.Application.Interfaces;

public interface ISearchService
{
    Task<SearchDto> Search(string code, string name);
}
