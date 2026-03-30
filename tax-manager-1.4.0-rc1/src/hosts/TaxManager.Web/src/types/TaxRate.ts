export interface TaxRate {
  type: Type;
  strategy: Strategy;
  code: string | null;
  value: string;
  valueType: string;
  dependents?: SimpleTaxationDto[];
  startsOn?: string;
  endsOn?: string;
  startsOnHour?: string;
  endsOnHour?: string;
  startsOnTimeZone?: string;
  endsOnTimeZone?: string;
  startDate?: Date;
  endDate?: Date;
}

export interface TaxationTaxRateDto {
  taxRateId: number;
  strategyId: number;
  code?: string | null;
  value: number;
  valueType: string;
  dependentTaxations?: SimpleTaxationDto[];
  startDate?: Date | null; 
  endDate?: Date | null;
  startDateTimeZone?: string;
  endDateTimeZone?: string; 
}


export interface Type {
  id: number,
  name: string
}
  
export interface Strategy {
    id: number,
    name: string
} 

export interface Currency {
  name: string
}

export interface TimeZone {
  id: string,
  info: string
}

export interface SimpleTaxationDto {
    id: number;
    code: string;
}
