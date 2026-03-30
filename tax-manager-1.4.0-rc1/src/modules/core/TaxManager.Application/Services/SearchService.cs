using AutoMapper;
using Microsoft.Extensions.Logging;
using TaxManager.Application.Common;
using TaxManager.Application.Dto;
using TaxManager.Application.Interfaces;
using TaxManager.Application.Common.Exception;
using TaxManager.Domain.Entities;
using TaxManager.Domain.Interfaces;

namespace TaxManager.Application.Services;

public class SearchService(ITaxationService taxationService,
    ILegalEnvironmentService legalEnvironmentService) : ISearchService
{
    public async Task<SearchDto> Search(string code = "", string name = "")
    {
        code = code.ToUpper();
        name = name.ToUpper();
        
        //TBD : Instead of getting all Taxations(context.taxations) and then filtering,
        //      an IQueryable<T> implementation in a SearchRepository would've been more appropriate to boost performance  
        var taxations = await taxationService.GetAllTaxationsAsync();
        var legalEnvironments = await legalEnvironmentService.GetAllLegalEnvironmentsAsync();

        if (!string.IsNullOrEmpty(code) || !string.IsNullOrEmpty(name))
        {
            if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(name))
            {
                // Both code and name are provided ? AND logic
                taxations = taxations.Where(t =>
                    t.Code.Contains(code) && t.Name.ToUpper().Contains(name));
                legalEnvironments = legalEnvironments.Where(l =>
                    l.Code.Contains(code) && l.Name.ToUpper().Contains(name));
            }
            else if (!string.IsNullOrEmpty(code))
            {
                // Only code provided
                taxations = taxations.Where(t => t.Code.Contains(code));
                legalEnvironments = legalEnvironments.Where(l => l.Code.Contains(code));
            }
            else if (!string.IsNullOrEmpty(name))
            {
                // Only name provided
                taxations = taxations.Where(t => t.Name.ToUpper().Contains(name));
                legalEnvironments = legalEnvironments.Where(l => l.Name.ToUpper().Contains(name));
            }
        }

        if (!taxations.Any() && !legalEnvironments.Any())
        {
            throw new NotFoundException("No results found.");
        }

        var searchResult = new SearchDto(taxations, legalEnvironments);
       
        return searchResult;
    }
}
