import { createContext, useContext, useState, ReactNode } from 'react';

const LoaderContext = createContext({
    loading: false,
    setLoading: (_: boolean) => {},
});

export const LoaderProvider = ({ children }: { children: ReactNode }) => {
    const [loading, setLoading] = useState(false);
    return (
        <LoaderContext.Provider value={{ loading, setLoading }}>
            {children}
        </LoaderContext.Provider>
    );
};

export const useLoader = () => useContext(LoaderContext);
