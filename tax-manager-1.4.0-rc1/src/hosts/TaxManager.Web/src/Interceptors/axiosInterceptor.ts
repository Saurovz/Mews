import axios from 'axios';

let setLoadingGlobal: (loading: boolean) => void;
let setErrorGlobal: (error: string | null) => void;
let activeRequests = 0;

export const setLoaderSetter = (setter: (loading: boolean) => void) => {
    setLoadingGlobal = setter;
};

export const setErrorSetter = (setter: (error: string | null) => void) => {
    setErrorGlobal = setter;
};

const axiosInstance = axios.create({
    baseURL: 'http://localhost:5001',
    timeout: 10000,
});

const handleRequestEnd = () => {
    activeRequests = Math.max(activeRequests - 1, 0);
    if (activeRequests === 0) setLoadingGlobal?.(false);
};

// REQUEST INTERCEPTOR
axiosInstance.interceptors.request.use(
    (config) => {
        activeRequests += 1;
        setLoadingGlobal?.(true);
        setErrorGlobal?.(null); // clear old error on new request
        return config;
    },
    (error) => {
        handleRequestEnd();
        return Promise.reject(error);
    }
);

// RESPONSE INTERCEPTOR
axiosInstance.interceptors.response.use(
    (response) => {
        handleRequestEnd();
        return response;
    },
    (error) => {
        handleRequestEnd();
        
        if (!error.response) {
            error.message = 'Unable to connect to the server. Please try again later.';
            setErrorGlobal?.(error.message);
        } else if (error.response.status >= 500) {
            error.message = 'Server error occurred. Please try again later.';
            setErrorGlobal?.(error.message);
        }

        return Promise.reject(error);
    }
);

export default axiosInstance;