import { useEffect } from 'react';
import { setErrorSetter } from '../../Interceptors/axiosInterceptor';
import { useError } from './ErrorContext';

const ErrorInitializer = () => {
    const { setError } = useError();

    useEffect(() => {
        setErrorSetter(setError);
    }, [setError]);

    return null;
};

export default ErrorInitializer;
