import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { LegalEnvironment } from '../../types/LegalEnvironment';
import { getLegalEnvironments } from '../../services/LegalEnvironmentService';


const LegalEnvironmentList = () => {
    const [legalEnvironmentData, setLegalEnvironmentData] = useState<LegalEnvironment[]>([]);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const fetchLegalEnvironmentdata = async () => {
        try {
            const data = await getLegalEnvironments();
            setLegalEnvironmentData(data);
        } catch (err: any) {
            setError(err.message || 'Something went wrong');
        }
    };

    useEffect(() => {
        fetchLegalEnvironmentdata();
    }, []);

    const handleRowClick = (code: string) => {
        navigate(`/legal-environments/legal-environment-detail/${code}`);  
    };

    return (
        <div className="p-4">
           <div className="flex justify-between items-center mb-4">
                <h1 className="text-2xl font-semibold">Legal Environments</h1>
                <button
                    onClick={() => navigate('/legal-environments/createNew')}
                    className="bg-blue-600 text-white px-2 py-1 rounded hover:bg-blue-700 transition"
                >
                    + Create New
                </button>
            </div>
            <div className="max-h-[450px] overflow-y-auto border border-gray-300 rounded">
                <table className="min-w-full table-auto border-collapse">
                    <thead className="sticky top-0 z-20 bg-gray-100">
                    <tr className="border-b border-gray-300">
                        <th className="px-4 py-2 border-r border-gray-300 text-left">Code</th>
                        <th className="px-4 py-2 text-left">Name</th>
                    </tr>
                    </thead>
                    <tbody>
                        {legalEnvironmentData.length > 0 ? (
                            legalEnvironmentData.map((le) => (
                                <tr
                                    key={le.code}
                                    className="cursor-pointer hover:bg-gray-100 border-b border-gray-200"
                                    onClick={() => handleRowClick(le.code)} // 

                                >
                                    <td className="px-4 py-2 border px-4 py-2 border-r border-gray-200 break-words">{le.code}</td>

                                    <td className="px-4 py-2">{le.name}</td>
                                </tr>
                            ))
                        ) : (
                            <tr>
                                <td colSpan={3} className="text-center py-4">No results found.</td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default  LegalEnvironmentList;
