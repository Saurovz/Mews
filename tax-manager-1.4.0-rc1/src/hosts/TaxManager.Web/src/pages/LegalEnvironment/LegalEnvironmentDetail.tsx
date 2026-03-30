import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';

import { LegalEnvironment } from '../../types/LegalEnvironment';
import { depositTaxrateModeLabels } from "../../constants/DepositTaxrateMode";
import { getLegalEnvironmentByCode } from '../../services/LegalEnvironmentService';


const LegalEnvironmentDetail = () => {
    const { code } = useParams<{ code: string }>();
    const [legalEnv, setLegalEnv] = useState<LegalEnvironment | null>(null);
    const navigate = useNavigate();

    useEffect(() => {
        if (code) {
            getLegalEnvironmentByCode(code)
                .then((data) => {
                    setLegalEnv(data);
                })
                .catch((err) => {
                    console.error('Failed to load legal environment details:', err);
                });
        }
    }, [code]);

    const handleBack = () => {
        navigate('/legal-environments');
    };

    const handleEdit = () => {
        navigate(`/legal-environments/${code}/edit`);
    };

    return (
        <div className="p-4">
            <div className="flex justify-between items-center mb-4">
                <h1 className="text-2xl font-semibold">Legal Environment Details</h1>
            </div>
            <div className="p-6 border shadow-lg bg-white mx-auto">
                <div className="flex justify-between items-center mb-6">
                    <button
                        onClick={handleBack}
                        className="px-4 py-1 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
                    >
                        ← Back to List
                    </button>
                    <button
                        onClick={handleEdit}
                        className="px-4 py-1 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
                    >
                        Edit
                    </button>
                </div>

                <div className="space-y-4 text-gray-800">
                    <div className="flex">
                        <span className="w-48 font-bold">Code:</span>
                        <span className="text-lg">{legalEnv?.code}</span>
                    </div>
                    <div className="flex">
                        <span className="w-48 font-bold">Name:</span>
                        <span className="text-lg">{legalEnv?.name}</span>
                    </div>
                    <div className="flex">
                        <span className="w-48 font-bold whitespace-nowrap">Deposit Tax Rate Mode:</span>
                        <span className="text-lg">{(legalEnv && depositTaxrateModeLabels[legalEnv.depositTaxRateMode]) ?? ''}</span>
                    </div>
                    <div className="flex">
                        <span className="w-48 font-bold">Taxations:</span>
                        {legalEnv?.taxations?.length ? (
                            <div className="max-h-64 overflow-y-auto border rounded shadow">
                                <table className="w-full border-collapse">
                                    <thead className="bg-gray-100 sticky top-0 z-10">
                                        <tr>
                                            <th className="text-left border px-2 py-1">Code</th>
                                            <th className="text-left border px-2 py-1">Name</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {legalEnv.taxations.map((t) => (
                                            <tr key={t.id} className="border-t">
                                                <td className="border px-2 py-1">
                                                    <a
                                                        href={`/taxations/taxation-detail/${t.code}`}
                                                        target="_blank"
                                                        rel="noopener noreferrer"
                                                        className="hover:underline"
                                                    >
                                                        {t.code}
                                                        <img
                                                            src="/images/link-open.svg"
                                                            alt="external link"
                                                            className="w-4 h-2 inline-block"
                                                        />
                                                    </a>
                                                </td>
                                                <td className="border px-2 py-1">{t.name}</td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        ) : (
                            <p className="text-gray-500">No taxations found.</p>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default LegalEnvironmentDetail;
