using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using TaxManager.Application.Common.Exception;
using TaxManager.Application.Dto;
using TaxManager.Application.Interfaces;

namespace TaxManager.Features.Search;

[ApiController]
[Route("[controller]")]
public class SearchController : ControllerBase
{
    internal const string ActivitySourceName = "TaxManager.Api.Search";
    private readonly ISearchService _searchService;
    
    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("GetSearchResults")]
    [OpenApiOperation("Endpoint for the search operation",
        "This fetches searchs and legal environments by code or name.")]
    public async Task<ActionResult<SearchDto>> Search(string code = "", string name = "")
    {
        if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name))
        {
            throw new NotFoundException("Search code and name are empty.");
        }

        var searchResult = await _searchService.Search(code, name);
        return Ok(searchResult);
    }
}
