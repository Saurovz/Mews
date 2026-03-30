import axiosInstance from '../Interceptors/axiosInterceptor';
import { LegalEnvironment, LegalEnvironmentCreateDto } from '../types/LegalEnvironment';

export const getLegalEnvironments = async (): Promise<LegalEnvironment[]> => {
    const response = await axiosInstance.get('/LegalEnvironment/ListLegalEnvironments');
    return response.data;
};

export const getLegalEnvironmentByCode = async (code: string): Promise<LegalEnvironment> => {
    const response = await axiosInstance.get(`/LegalEnvironment/${code}`);
    return response.data;
};
export const createLegalEnvironment = async (legalEnvironmentCreateDto: LegalEnvironmentCreateDto): Promise<LegalEnvironment> => {
    const response = await axiosInstance.post(`/LegalEnvironment/AddLegalEnvironment`, legalEnvironmentCreateDto);
    return response.data;
};
