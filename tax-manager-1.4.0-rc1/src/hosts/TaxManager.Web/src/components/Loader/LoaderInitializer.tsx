import { useEffect } from 'react';
import { useLoader } from './loaderContext';
import { setLoaderSetter } from '../../Interceptors/axiosInterceptor';

const LoaderInitializer = () => {
  const { setLoading } = useLoader();

  useEffect(() => {
    setLoaderSetter(setLoading);
  }, [setLoading]);

  return null;
};

export default LoaderInitializer;
