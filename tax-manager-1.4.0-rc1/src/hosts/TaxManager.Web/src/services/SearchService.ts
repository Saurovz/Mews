import axiosInstance from "../Interceptors/axiosInterceptor";

export const globalSearch = async (code: string, name: string) => {
    const response = await axiosInstance.get("/Search/GetSearchResults", {
      params: {
        code,
        name,
      },
    });
    return response.data;
  };