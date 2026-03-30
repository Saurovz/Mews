import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Taxation } from '../../types/Taxation';
import { getTaxations } from '../../services/TaxationService';

const TaxationList = () => {
    const [taxData, setTaxData] = useState<Taxation[]>([]);
    const navigate = useNavigate();

     const fetchTaxations = async () => {
        try {
            const data = await getTaxations();
            setTaxData(data);
        } catch (err: any) {
            console.error(err.message || 'Something went wrong');
        }
    };

    useEffect(() => {
        fetchTaxations();
    }, []);

    const handleRowClick = (code: string) => {
        navigate(`/taxations/taxation-detail/${code}`);  
    };

    return (
        <div className="p-4">
            <div className="flex justify-between items-center mb-4">
                <h1 className="text-2xl font-semibold">Taxations</h1>
                <button
                    onClick={() => navigate('/taxations/createNew')}
                    className="bg-blue-600 text-white px-2 py-1 rounded hover:bg-blue-700 transition"
                >
                    + Create New
                </button>
            </div>
            <div className="max-h-[450px] overflow-y-auto border border-gray-300 rounded">
                <table className="min-w-full table-auto border-collapse">
                    <thead  className="sticky top-0 z-20 bg-gray-100">
                    <tr className="border-b border-gray-300">
                        <th className="px-4 py-2 border-r border-gray-300 text-left">Code</th>
                        <th className="px-4 py-2 border-r border-gray-300 text-left">Country</th>
                        <th className="px-4 py-2 text-left">Name</th>
                    </tr>
                    </thead>
                    <tbody>
                    {taxData.length > 0 ? (
                        taxData.map((tax) => (
                            <tr
                                key={tax.code}
                                className="cursor-pointer hover:bg-gray-100 border-b border-gray-200"
                                onClick={() => handleRowClick(tax.code)}
                            >
                                <td className="px-4 py-2 border-r border-gray-200 break-words">{tax.code}</td>
                                <td className="px-4 py-2 border-r border-gray-200">{tax.country?.name}</td>
                                <td className="px-4 py-2">{tax.name}</td>
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

export default TaxationList;
