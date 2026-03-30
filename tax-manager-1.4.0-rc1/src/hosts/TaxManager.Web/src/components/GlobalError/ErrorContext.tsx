import React, { createContext, useContext, useState } from 'react';

type ErrorContextType = {
    error: string | string[] | null;
    setError: (msg: string | string[] | null) => void;
};

const ErrorContext = createContext<ErrorContextType | undefined>(undefined);

export const ErrorProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [error, setError] = useState<string | string[] | null>(null);

    return (
        <ErrorContext.Provider value={{ error, setError }}>
            {children}
        </ErrorContext.Provider>
    );
};

export const useError = () => {
    const context = useContext(ErrorContext);
    if (!context) throw new Error('useError must be used within ErrorProvider');
    return context;
};
