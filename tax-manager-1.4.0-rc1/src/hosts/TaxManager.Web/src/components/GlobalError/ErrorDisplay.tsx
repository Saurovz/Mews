import React from 'react';
import { useError } from './ErrorContext';

const ErrorDisplay: React.FC = () => {
    const { error, setError } = useError();

    if (!error) return null;

    const errorList = Array.isArray(error) ? error : [error];

    return (
        <div className="bg-red-100 text-red-800 px-4 py-2 rounded-md text-center font-semibold">
            {errorList && errorList.length == 1 && error}
            <button
                onClick={() => setError(null)}
                className="float-right text-red-600 hover:text-red-800 focus:outline-none text-xs p-1"
                aria-label="Close"
            >
                ✕
            </button>
            {errorList && errorList.length > 1 && (
                <ul className="list-disc pl-48 space-y-1 text-justify">
                    {errorList.map((err, idx) => (
                        <li key={idx} className="leading-tight">{err}</li>
                    ))}
                </ul>
            )}
        </div>
    );
};

export default ErrorDisplay;
