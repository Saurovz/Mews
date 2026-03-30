import { Subdivision } from './Subdivision';
import { Country } from './Country';
import { TaxationTaxRateDto } from "./TaxRate";

export interface Taxation {
    id?: number;
    code: string;
    countryId: number;
    name: string;
    subdivisions?: Subdivision[];
    country?: Country;
    taxationTaxRates: TaxationTaxRateDto[];
}

export interface TaxationResultDto {
    isValid: boolean;
    errors: string[];
    entity: Taxation;
}
