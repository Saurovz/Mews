using System.Runtime.Serialization;
using AutoMapper;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.TimeZones;
using TaxManager.Application.Common;
using TaxManager.Application.Dto;
using TaxManager.Application.Interfaces;
using TaxManager.Domain.Entities;
using TaxManager.Domain.Interfaces;
using TaxManager.Application.Common.Exception;
using TaxManager.Domain.Enums;

namespace TaxManager.Application.Services;

public class TaxationService(IMapper mapper, ITaxationRepository repository,
    ICountryRepository countryRepository,
    ILogger<TaxationService> logger,
    ICacheService redis) : ITaxationService
{
    public async Task<TaxationDto> GetTaxationByCodeAsync(string code)
    {
        var taxation = await repository.GetByCodeAsync(code);
        var entity =  mapper.Map<TaxationDto>(taxation);
        if (entity == null)
        {
            throw new NotFoundException($"Entity with code '{code}' not found.");
        }
        return entity;
    }

    public async Task<IEnumerable<TaxationDto>> GetAllTaxationsAsync()
    {
        // Get the cache key 
        const string cacheKey = CacheKeys.TaxationAll;
        
        // Try get from cache first
        var cachedTaxations = await redis.GetAsync<IEnumerable<TaxationDto>>(cacheKey);
        if (cachedTaxations != null)
        {
            logger.LogInformation("Found taxations from cache");
            return cachedTaxations;
        }
        
        //If not in cache, get from database
        var taxations = await repository.GetAllAsync();
        var entities =  mapper.Map<IEnumerable<TaxationDto>>(taxations).ToList();
        if (entities == null || entities.Count == 0 )
        {
            throw new NotFoundException("No Items found.");
        }
        
        //Cache the result for 20 mins; TTL is an optional parameter
        await redis.SetAsync(cacheKey, entities, TimeSpan.FromMinutes(20));
        
        return entities;
    }
 
    public async Task<SaveValidationResultDto> CreateTaxationAsync(TaxationCreateDto taxationCreateDto)
    {
        // Business rule validation for uniqueness of Taxation Code
        if (await TaxationCodeExistsAsync(taxationCreateDto.Code))
        {
            return new SaveValidationResultDto
                { IsValid = false, Errors = new List<string> { "Taxation Code exists!!" } };
        }
        var country = await countryRepository.GetCountryByIdAsync(taxationCreateDto.CountryId);
        if (country == null)
        {
            logger.LogError($"Invalid country id passed into CreateTaxationAsync. CountryId: '{taxationCreateDto.CountryId}'");
            throw new BusinessRuleException("Country id does not exist!");
        }

        //Map the incoming DTO to Entity for persistence
        var taxation = mapper.Map<Taxation>(taxationCreateDto);

        var validationErrors = new List<string>();
        validationErrors.AddRange(await ValidateSubdivisions(taxation));
        validationErrors.AddRange(await ValidateTaxRates(taxation));

        if (validationErrors.Any())
            return new SaveValidationResultDto { IsValid = false, Errors = validationErrors };
        var createdTaxation = await repository.AddAsync(taxation);
        
        //Invalidate the all-taxation cache and taxation by country id
        await redis.RemoveAsync(CacheKeys.TaxationAll);
        await redis.RemoveAsync(CacheKeys.TaxationByCountryId(taxation.CountryId));
        var result = new SaveValidationResultDto
        {
            IsValid = true,
            Entity = mapper.Map<TaxationDto>(createdTaxation)
        };
        return result;
    }

    public async Task<IEnumerable<TaxRateDto>> GetTaxRatesAsync()
    {
        var cachedTaxRates = await redis.GetAsync<IEnumerable<TaxRateDto>>(CacheKeys.TaxRatesAll);
        if (cachedTaxRates != null)
        {
            logger.LogInformation("Found tax rates from cache");
            return cachedTaxRates;
        }
        var taxRates = mapper.Map<IEnumerable<TaxRateDto>>(await repository.GetTaxRatesAsync()).ToList();
        await redis.SetAsync(CacheKeys.TaxRatesAll, taxRates, TimeSpan.FromDays(30));
        return taxRates;
    }

    public IEnumerable<StrategyDto> GetStrategies()
    {
        var strategies = new List<StrategyDto>();
        foreach (Strategy strategy in Enum.GetValues(typeof(Strategy)))
        {
            strategies.Add(new StrategyDto((int)strategy, GetEnumValue(strategy)));
        }
        return strategies;
    }

    public IEnumerable<CurrencyDto> GetCurrencies()
    {
        var currencies = new List<CurrencyDto>();
        foreach (Currency currency in Enum.GetValues(typeof(Currency)))
        {
            currencies.Add(new CurrencyDto(Enum.GetName(typeof(Currency), currency) ?? string.Empty));
        }
        return currencies;
    }

    public async Task<IEnumerable<TimeZoneDto>> GetTimeZonesAsync()
    {
        var cachedTimeZones = await redis.GetAsync<IEnumerable<TimeZoneDto>>(CacheKeys.TimeZonesAll);
        if (cachedTimeZones != null)
        {
            logger.LogInformation("Found time zones from cache");
            return cachedTimeZones;
        }

        if (TzdbDateTimeZoneSource.Default.ZoneLocations == null
            || TzdbDateTimeZoneSource.Default.ZoneLocations.Count == 0)
        {
            throw new ServiceException("No time zones found.");
        }

        var nodaTimeZones = TzdbDateTimeZoneSource.Default
            .ZoneLocations
            .Where(z => !string.IsNullOrEmpty(z.CountryCode))
            .ToList()
            .OrderBy(t => t.ZoneId);
        
        var timeZones = new List<TimeZoneDto> { new TimeZoneDto("UTC" , "00:00:00") };
        foreach (var timeZone in nodaTimeZones)
        {
            var zone = DateTimeZoneProviders.Tzdb[timeZone.ZoneId];
            var now = SystemClock.Instance.GetCurrentInstant();
            var offset = zone.GetUtcOffset(now);
            timeZones.Add(new TimeZoneDto(timeZone.ZoneId, offset.ToTimeSpan().ToString()));
        }
        await redis.SetAsync(CacheKeys.TimeZonesAll, timeZones, TimeSpan.FromDays(30));
        return timeZones;
    }

    public async Task<IEnumerable<TaxationDto>> GetTaxationsByCountryIdAsync(int countryId)
    {
        string cacheKey = CacheKeys.TaxationByCountryId(countryId);
        var cachedTaxations = await redis.GetAsync<IEnumerable<TaxationDto>>(cacheKey);
        if (cachedTaxations != null)
        {
            logger.LogInformation($"Found taxations for country {countryId} from cache");
            return cachedTaxations;
        }

        //If not in cache, get from database
        var taxations = await repository.GetTaxationsByCountryIdAsync(countryId);
        var entities = mapper.Map<IEnumerable<TaxationDto>>(taxations).ToList();
        if (entities == null || entities.Count == 0 )
        {
            throw new NotFoundException("No Items found.");
        }

        //Cache the result for 20 mins; TTL is an optional parameter
        await redis.SetAsync(cacheKey, entities, TimeSpan.FromMinutes(20));
        return entities;
    }

    #region Private Methods
   
    //Duplicity Checks
    private async Task<bool> TaxationCodeExistsAsync(string taxationCode)
    {
        if (string.IsNullOrWhiteSpace(taxationCode))
        {
            throw new ArgumentException("Taxation code cannot be empty", nameof(taxationCode));
        }

        return await repository.AnyAsync(p => p.Code == taxationCode);
    }

    private async Task<List<string>> ValidateSubdivisions(Taxation taxation)
    {
        var errors = new List<string>();
        foreach (var subdivision in taxation.Subdivisions)
        {
            if (subdivision.Id == 0 &&
                await countryRepository.CheckIfSubdivisionExistsAsync(subdivision.CountryId, subdivision.Name))
            {
                errors.Add("Subdivision name already exists for the same country.");
            }

            if (subdivision.CountryId != taxation.CountryId)
            {
                
                logger.LogError($"Subdivision CountryId in ValidateSubdivisions doesn't match Taxation CountryId. " +
                                $"Taxation CountryId: '{taxation.CountryId}', Subdivision CountryId: '{subdivision.CountryId}'");
                throw new ApplicationException("Subdivision country id does not match Taxation.");
            }
        }

        return errors;
    }

    private async Task<List<string>> ValidateTaxRates(Taxation taxation)
    {
        var taxRates = await GetTaxRatesAsync();
        var taxRateDtos = taxRates.ToList();
        var errors = new List<string>();
        if (!taxation.TaxationTaxRates.Any())
        {
            errors.Add("At least one Tax Rate is required!");
            return errors;
        }

        foreach (var taxRate in taxation.TaxationTaxRates)
        {
            var taxRateObj = taxRateDtos.First(t => t.Id == taxRate.TaxRateId);
            switch (taxRate.Strategy)
            {
                case Strategy.FlatRate
                    when !Enum.IsDefined(typeof(Currency), taxRate.ValueType ?? string.Empty):
                    errors.Add($"Tax Rates of type {GetEnumValue(Strategy.FlatRate)} must have a currency valueType");
                    break;
                case Strategy.RelativeRate or Strategy.RelativeRateWithDependencies
                    when taxRate.ValueType != "%":
                    errors.Add($"Tax Rates of type {GetEnumValue(Strategy.RelativeRate)} and {GetEnumValue(Strategy.RelativeRateWithDependencies)} must have a percentage valueType");
                    break;
            }
            
            //throw exception here because this is not the result of user entry, but of system error
            if ((!string.IsNullOrEmpty(taxRate.StartDateTimeZone) && 
                 GetTimeZonesAsync().Result.All(tz => tz.Id != taxRate.StartDateTimeZone)) 
                || (!string.IsNullOrEmpty(taxRate.EndDateTimeZone) && 
                    GetTimeZonesAsync().Result.All(tz => tz.Id != taxRate.EndDateTimeZone)))
            {
                logger.LogError($"Invalid timezone passed into tax rate validation. Timezones: '{taxRate.StartDateTimeZone}', '{taxRate.EndDateTimeZone}'");
                throw new ServiceException($"Invalid Timezone for '{taxRateObj.Name}'.");
            }

            if (!ValidateDatesAndTimeZonesEarlier(taxRate.StartDate, taxRate.StartDateTimeZone, taxRate.EndDate, taxRate.EndDateTimeZone))
            {
                errors.Add($"Start date with time zone is not earlier than end date for tax rate '{taxRateObj.Name}'.");
            }

            //Only validate dependents if there are dependents
            if (taxRate.DependentTaxations.Count != 0)
            {
                if(taxRate.Strategy != Strategy.RelativeRateWithDependencies)
                {
                    errors.Add($"Dependent tax rates are only supported for the strategy {GetEnumValue(Strategy.RelativeRateWithDependencies)}");
                    return errors;
                }

                var dependentIds = taxRate.DependentTaxations.Select(t => t.ChildTaxationId).ToList();
                if (taxRate.StartDate == null && taxRate.EndDate == null)
                {
                    //Still need to validate that dependents have this tax rate
                    errors.AddRange(await ValidateDependentTaxationTaxRates(taxRate.TaxRateId, taxRateObj.Name,
                        dependentIds, dependentIds));
                }
                else
                {
                    errors.AddRange(await ValidateDependentTaxationDates(taxRate.TaxRateId, taxRateObj.Name,
                        dependentIds, dependentIds, taxRate.StartDate, taxRate.EndDate, taxRate.StartDateTimeZone,
                        taxRate.EndDateTimeZone));
                }
            }

            //Only validate dependees if this is an update call and the tax rate has dates
            if (taxation.Id != Guid.Empty && taxRate.StartDate != null || taxRate.EndDate != null)
            {
                errors.AddRange(await ValidateDependeeTaxationDates(taxRate.TaxationId, taxRate.TaxRateId,
                    taxRateObj.Name, taxRate.StartDate,
                    taxRate.EndDate, taxRate.StartDateTimeZone, taxRate.EndDateTimeZone));
            }
        }

        return errors;
    }

    private string GetEnumValue<T>(T enumValue) where T : Enum
    {
        var type = enumValue.GetType();
        var memInfo = type.GetMember(enumValue.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(EnumMemberAttribute), false);
        return ((EnumMemberAttribute)attributes[0]).Value ?? string.Empty;
    }
    
    private OffsetDateTime GetDateTimeWithCurrentOffset(DateTime date, string timeZone)
    {
        var timezoneProvider = DateTimeZoneProviders.Tzdb[timeZone];
        var now = SystemClock.Instance.GetCurrentInstant();
        return new OffsetDateTime(date.ToLocalDateTime(), timezoneProvider.GetUtcOffset(now));
    }
    
    /// <summary>
    /// Checks to see if date1 is earlier than date2 when considering time zones.
    /// </summary>
    private bool ValidateDatesAndTimeZonesEarlier(DateTime? date1, string? timeZone1, DateTime? date2, string? timeZone2)
    {
        //If either date is null, treat the check as valid
        if (date1 == null || string.IsNullOrEmpty(timeZone1) || date2 == null || string.IsNullOrEmpty(timeZone2))
        {
            return true;
        }

        var date1WithOffSet = GetDateTimeWithCurrentOffset(date1.Value, timeZone1);
        var date2WithOffSet = GetDateTimeWithCurrentOffset(date2.Value, timeZone2);
		//If the date is before or equal
        return OffsetDateTime.Comparer.Instant.Compare(date1WithOffSet, date2WithOffSet) <= 0;

    }

    private async Task<List<string>> ValidateDependentTaxationTaxRates(int taxRateId, string taxRateName,
        List<Guid> dependentTaxationIds, List<Guid> submittedDependentTaxationIds)
    {
        var errors = new List<string>();
        foreach (var dependent in dependentTaxationIds)
        {
            var dependentTaxRate = await repository.GetTaxationTaxRateAndDependentsTaxRates(dependent, taxRateId);
            var dependentTaxation = await repository.GetByIdAsync(dependent);
            if (dependentTaxation == null)
                throw new NotFoundException($"Taxation {dependent} not found");
            if (dependentTaxRate == null)
            {
                errors.Add(
                    $"Tax rate: '{taxRateName}' for Dependent Taxation: '{dependentTaxation.Code}' not found.");
                continue;
            }
            if (dependentTaxRate.DependentTaxations.Count <= 0) continue;
            // Recursively validate child dependencies if any
            var childIds = dependentTaxRate.DependentTaxations.Select(dt => dt.ChildTaxation.Id).ToList();
            //If any submitted dependent is a dependent of a another submitted dependent
            if (childIds.Any(submittedDependentTaxationIds.Contains))
            {
                errors.Add(
                    $"Selected Dependent for Tax rate: '{taxRateName}' is a Dependent of Taxation: '{dependentTaxation.Code}'.");
                return errors;
            }
            errors.AddRange(await ValidateDependentTaxationTaxRates(taxRateId, taxRateName, childIds, submittedDependentTaxationIds));
        }
        return errors;
    }

    private async Task<List<string>> ValidateDependentTaxationDates(int taxRateId, string taxRateName, List<Guid> dependentTaxationIds, List<Guid> submittedDependentTaxationIds,
        DateTime? submittedTaxRateStartDate, DateTime? submittedTaxRateEndDate, string? submittedTaxRateStartDateTimeZone, string? submittedTaxRateEndDateTimeZone)
    {
        var errors = new List<string>();
        foreach (var dependent in dependentTaxationIds)
        {
            var dependentTaxRate = await repository.GetTaxationTaxRateAndDependentsTaxRates(dependent, taxRateId);
            var dependentTaxation = await repository.GetByIdSimplifiedAsync(dependent);
            if (dependentTaxation == null)
                throw new NotFoundException($"Taxation {dependent} not found");
            if (dependentTaxRate == null)
            {
                errors.Add(
                    $"Tax rate: '{taxRateName}' for Dependent Taxation: '{dependentTaxation.Code}' not found.");
                continue;
            }
            
            //The submitted date is the boundary and all dependent dates needs to be within that boundary, the submitted date is a parent to the dependent,
            //  so the dependent start date should be after the submitted start date
            if (!ValidateDatesAndTimeZonesEarlier(submittedTaxRateStartDate, submittedTaxRateStartDateTimeZone, dependentTaxRate.StartDate, dependentTaxRate.StartDateTimeZone))
            {
                errors.Add($"Start date for tax rate '{taxRateName}' is after the dependent taxation: {dependentTaxRate.Taxation.Code}'s '{taxRateName}' start date: " +
                              $"{GetDateTimeWithCurrentOffset(dependentTaxRate.StartDate!.Value, dependentTaxRate.StartDateTimeZone!)}");
            }
            if (!ValidateDatesAndTimeZonesEarlier(dependentTaxRate.EndDate, dependentTaxRate.EndDateTimeZone, submittedTaxRateEndDate, submittedTaxRateEndDateTimeZone))
            {
                errors.Add($"End date for tax rate '{taxRateName}' is before the dependent taxation: {dependentTaxRate.Taxation.Code}'s '{taxRateName}' end date: " +
                              $"{GetDateTimeWithCurrentOffset(dependentTaxRate.EndDate!.Value, dependentTaxRate.EndDateTimeZone!)}");
            }
            //Also compare submitted start date to dependent end date and vice versa for the case where one date is null
            if (!ValidateDatesAndTimeZonesEarlier(submittedTaxRateStartDate, submittedTaxRateStartDateTimeZone, dependentTaxRate.EndDate, dependentTaxRate.EndDateTimeZone))
            {
                errors.Add($"Start date for tax rate '{taxRateName}' is after the dependent taxation: {dependentTaxRate.Taxation.Code}'s '{taxRateName}' end date: " +
                           $"{GetDateTimeWithCurrentOffset(dependentTaxRate.EndDate!.Value, dependentTaxRate.EndDateTimeZone!)}");
            }
            if (!ValidateDatesAndTimeZonesEarlier(dependentTaxRate.StartDate, dependentTaxRate.StartDateTimeZone, submittedTaxRateEndDate, submittedTaxRateEndDateTimeZone))
            {
                errors.Add($"End date for tax rate '{taxRateName}' is before the dependent taxation: {dependentTaxRate.Taxation.Code}'s '{taxRateName}' start date: " +
                           $"{GetDateTimeWithCurrentOffset(dependentTaxRate.StartDate!.Value, dependentTaxRate.StartDateTimeZone!)}");
            }

            if (dependentTaxRate.DependentTaxations.Count <= 0) continue;
            // Recursively validate child dependencies if any
            var childIds = dependentTaxRate.DependentTaxations.Select(dt => dt.ChildTaxation.Id).ToList();
            //If any submitted dependent is a dependent of a another submitted dependent
            if (childIds.Any(submittedDependentTaxationIds.Contains))
            {
                errors.Add(
                    $"Selected Dependent for Tax rate: '{taxRateName}' is a Dependent of Taxation: '{dependentTaxation.Code}'.");
                return errors;
            }
            errors.AddRange(await ValidateDependentTaxationDates(taxRateId, taxRateName, childIds, submittedDependentTaxationIds, submittedTaxRateStartDate,
                submittedTaxRateEndDate, submittedTaxRateStartDateTimeZone, submittedTaxRateEndDateTimeZone));
        }

        return errors;
    }

    private async Task<List<string>> ValidateDependeeTaxationDates(Guid taxationId, int taxRateId, string taxRateName, DateTime? submittedTaxRateStartDate,
        DateTime? submittedTaxRateEndDate, string? submittedTaxRateStartDateTimeZone, string? submittedTaxRateEndDateTimeZone)
    {
        var errors = new List<string>();
        var dependeeTaxRates = await repository.GetTaxationTaxRateAndDependeeTaxRates(taxationId, taxRateId);
        foreach (var dependee in dependeeTaxRates)
        {
            if (dependee == null)
                continue;
            
            //The dependee is the boundary and the submitted date needs to be within that boundary, the dependee is the parent to the submitted date,
            // so the dependee date should be before the submitted start date
            if (!ValidateDatesAndTimeZonesEarlier(dependee.StartDate, dependee.StartDateTimeZone, submittedTaxRateStartDate, submittedTaxRateStartDateTimeZone))
            {
                errors.Add(
                    $"This tax rate is a dependent and the selected start date for tax rate '{taxRateName}' is before the start date of taxation: " +
                    $"{dependee.Taxation.Code}'s '{taxRateName}' start date: {GetDateTimeWithCurrentOffset(dependee.StartDate!.Value, dependee.StartDateTimeZone!)}");
            }
            if (!ValidateDatesAndTimeZonesEarlier(submittedTaxRateEndDate, submittedTaxRateEndDateTimeZone, dependee.EndDate, dependee.EndDateTimeZone))
            {
                errors.Add(
                    $"This tax rate is a dependent and the selected end date for tax rate '{taxRateName}' is after the end date of taxation: " +
                    $"{dependee.Taxation.Code}'s '{taxRateName}' end date: {GetDateTimeWithCurrentOffset(dependee.EndDate!.Value, dependee.EndDateTimeZone!)}");
            }
            //Also compare submitted start date to dependee end date and vice versa for the case where one date is null
            if (!ValidateDatesAndTimeZonesEarlier(dependee.StartDate, dependee.StartDateTimeZone, submittedTaxRateEndDate, submittedTaxRateEndDateTimeZone))
            {
                errors.Add(
                    $"This tax rate is a dependent and the selected end date for tax rate '{taxRateName}' is before the start date of taxation: " +
                    $"{dependee.Taxation.Code}'s '{taxRateName}' start date: {GetDateTimeWithCurrentOffset(dependee.StartDate!.Value, dependee.StartDateTimeZone!)}");
            }
            if (!ValidateDatesAndTimeZonesEarlier(submittedTaxRateStartDate, submittedTaxRateStartDateTimeZone, dependee.EndDate, dependee.EndDateTimeZone))
            {
                errors.Add(
                    $"This tax rate is a dependent and the selected start date for tax rate '{taxRateName}' is before the end date of taxation: " +
                    $"{dependee.Taxation.Code}'s '{taxRateName}' end date: {GetDateTimeWithCurrentOffset(dependee.EndDate!.Value, dependee.EndDateTimeZone!)}");
            }

            errors.AddRange(await ValidateDependeeTaxationDates(dependee.TaxationId, taxRateId, taxRateName, submittedTaxRateStartDate,
                submittedTaxRateEndDate, submittedTaxRateStartDateTimeZone, submittedTaxRateEndDateTimeZone));
        }

        return errors;
    }

    #endregion
}
