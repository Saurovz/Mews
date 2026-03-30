import axiosInstance from '../Interceptors/axiosInterceptor';
import { Taxation, TaxationResultDto } from '../types/Taxation'; 
import { Country } from "../types/Country";
import { Subdivision } from "../types/Subdivision";
import { Currency, Strategy, TimeZone } from '../types/TaxRate';


export const getTaxations = async (): Promise<Taxation[]> => {
  const response = await axiosInstance.get('/Taxation/ListTaxations');
  return response.data;
};

export const getTaxationByCode = async (code : string): Promise<Taxation> => {
  const response = await axiosInstance.get(`/Taxation/${code}`);
  return response.data;
};

export const createTaxation = async (taxationCreateDto: Taxation): Promise<TaxationResultDto> => {
  const response = await axiosInstance.post(`/Taxation/AddTaxation`, taxationCreateDto);
  return response.data;
};

export const getCountries = async (): Promise<Country[]> => {
    const response = await axiosInstance.get('/Country/ListCountries');
    return response.data;
};

export const getSubdivisions = async (countryId: number): Promise<Subdivision[]> => {
    const response = await axiosInstance.get(`/Country/GetSubdivisionsByCountryId/${countryId}`);
    return response.data;
};

export const getTaxRates = async (): Promise<Country[]> => {
  const response = await axiosInstance.get('/Taxation/GetTaxRates');
  return response.data;
};

export const getStrategies = async (): Promise<Strategy[]> => {
  const response = await axiosInstance.get('/Taxation/GetStrategies');
  return response.data;
};

export const getCurrencies = async (): Promise<Currency[]> => {
  const response = await axiosInstance.get('/Taxation/GetCurrencies');
  return response.data;
};

export const getTimeZone = async (): Promise<TimeZone[]> => {
  const response = await axiosInstance.get('/Taxation/GetTimeZones');
  return response.data;
};

export const getTaxationsByCountryId = async (countryId: number): Promise<Taxation[]> => {
  const response = await axiosInstance.get(`/Taxation/GetTaxationsByCountryId/${countryId}`);
  return response.data;
};
