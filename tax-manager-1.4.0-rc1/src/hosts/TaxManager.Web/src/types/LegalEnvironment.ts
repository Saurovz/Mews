import { Taxation } from "./Taxation";

export interface LegalEnvironment {
    code: string;
    name: string;
    depositTaxRateMode: number;
    taxations: Taxation[];
}

export interface LegalEnvironmentCreateDto {
    code: string;
    name: string;
    depositTaxRateMode: number;
    taxationIds: number[];
}
  