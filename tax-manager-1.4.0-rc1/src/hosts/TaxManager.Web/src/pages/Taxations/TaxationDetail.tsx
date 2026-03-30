import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Taxation } from '../../types/Taxation';
import { getStrategies, getTaxationByCode, getTaxRates } from '../../services/TaxationService';
import { TaxRateColumns } from '../../constants/TaxRatesData';
import { Strategy, TaxRate, Type } from '../../types/TaxRate';

const TaxationDetail = () => {
    const { code } = useParams<{ code: string }>();
    const [tax, setTax] = useState<Taxation | null>(null);
    const [typeOptions, setTypeOptions] = useState<Type[]>([]);
    const [strategyOptions, setStrategyOptions] = useState<Strategy[]>([]);
    const [taxRates, setTaxRates] = useState<TaxRate[]>([]);
    const navigate = useNavigate();

    useEffect(() => {
        const fetchTypeAndStrategyData = async () => {
            try {
                const [types, strategies] = await Promise.all([
                    getTaxRates(),
                    getStrategies()
                ]);
                setTypeOptions(types);
                setStrategyOptions(strategies);
            } catch (error) {
                console.error('Error loading tax rate types and strategy:', error);
            }
        };
        fetchTypeAndStrategyData();
    }, []);

    useEffect(() => {
        if (code) {
            getTaxationByCode(code)
                .then((data) => {
                    setTax(data);
                })
                .catch((err) => {
                    console.error('Failed to load taxation details:', err);
                });
        }
    }, [code]);

    useEffect(() => {
        if (tax) {
            const mappedRates = mapTaxRates();
            setTaxRates(mappedRates);
        } else {
            setTaxRates([]);
        }
    }, [tax]);

    const handleBack = () => {
        navigate('/taxations');
    };

    const handleEdit = () => {
        navigate(`/taxations/${code}/edit`);
    };

    const getType = (id: number): Type => {
        const found = typeOptions.find(t => t.id === id);
        return {
            id,
            name: found?.name ?? 'Unknown',
        };
    };

    const getStrategy = (id: number): Strategy => {
        const found = strategyOptions.find(s => s.id === id);
        return {
            id,
            name: found?.name ?? 'Unknown',
        };
    };

    const taxRateTypeList = typeOptions.map(t => ({
        id: t.id,
        name: t.name,
    }));

    const mapTaxRates = (): TaxRate[] => {
        if (!tax?.taxationTaxRates) return [];

        return tax.taxationTaxRates.map((x) => ({
            type: getType(x.taxRateId),
            strategy: getStrategy(x.strategyId),
            code: x.code ?? null,
            value: `${x.value.toString()} ${x.valueType}`,
            valueType: x.valueType,
            dependents: x.dependentTaxations ?? [],
            startsOn: x.startDate
                ? `${new Date(x.startDate).toLocaleDateString('en-US', {
                    year: 'numeric',
                    month: '2-digit',
                    day: '2-digit',
                }).replace(/\//g, '-')}` +
                ` ${new Date(x.startDate).toLocaleTimeString('en-US', {
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: false,
                })} ` +
                ` ${x.startDateTimeZone ?? ''}`.trim()
                : '',
            endsOn: x.endDate
                ? `${new Date(x.endDate).toLocaleDateString('en-US', {
                    year: 'numeric',
                    month: '2-digit',
                    day: '2-digit',
                }).replace(/\//g, '-')}` +
                ` ${new Date(x.endDate).toLocaleTimeString('en-US', {
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: false,
                })} ` +
                ` ${x.endDateTimeZone ?? ''}`.trim()
                : '',
        }));
    };

    return (
        <div className="p-4">
            <div className="flex justify-between items-center mb-4">
                <h1 className="text-2xl font-semibold">Taxation Details</h1>
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
                        <span className="w-32 font-bold">Code:</span>
                        <span className="text-lg">{tax?.code}</span>
                    </div>
                    <div className="flex">
                        <span className="w-32 font-bold">Name:</span>
                        <span className="text-lg">{tax?.name}</span>
                    </div>
                    <div className="flex">
                        <span className="w-32 font-bold">Country:</span>
                        <span className="text-lg">{tax?.country?.name}</span>
                    </div>
                    <div className="flex items-start">
                        <span className="w-32 font-bold">Subdivision:</span>
                        <div className="flex flex-wrap gap-2">
                            {tax?.subdivisions?.length ? (
                                tax.subdivisions.map((sub, index) => (
                                    <span
                                        key={index}
                                        className="bg-gray-200 text-gray-800 px-3 py-1 rounded-full text-sm"
                                    >
                                        {sub.name}
                                    </span>
                                ))
                            ) : (
                                <span className="text-sm text-gray-500"></span>
                            )}
                        </div>
                    </div>
                    <div className="flex items-start gap-4 mt-6">
                        <span className="w-32 font-bold">Tax Rates:</span>
                        <table className="w-full text-sm border border-collapse border-gray-300">
                            <thead className="bg-gray-100">
                                <tr>
                                    {TaxRateColumns.map((column) => (
                                        <th key={column.key} className="px-4 py-2 border">
                                            {column.label}
                                        </th>
                                    ))}
                                </tr>
                            </thead>
                            <tbody>
                                {taxRateTypeList.map(({ id, name }) => {
                                    const tr = taxRates.find((t) => t.type.id === id);

                                    return (
                                        <tr key={id} className="border-t">
                                            <td className="px-4 py-2 border">{name}</td>
                                            <td className="px-4 py-2 border">{tr?.strategy?.name}</td>
                                            <td className="px-4 py-2 border">{tr?.code}</td>
                                            <td className="px-4 py-2 border">{tr?.value}</td>
                                            <td className="px-4 py-2 border align-top">
                                                {tr?.dependents?.length ? (
                                                    tr.dependents.map((d, idx) => (
                                                        <div key={idx}>
                                                            <a
                                                                href={`/taxations/taxation-detail/${d.code}`}
                                                                target="_blank"
                                                                rel="noopener noreferrer"
                                                                className="hover:underline"
                                                            >
                                                                {d.code}
                                                                <img
                                                                    src="/images/link-open.svg"
                                                                    alt="external link"
                                                                    className="w-4 h-2 inline-block"
                                                                />
                                                            </a>
                                                        </div>
                                                    ))
                                                ) : null}
                                            </td>

                                            <td className="px-4 py-2 border">{tr?.startsOn}</td>
                                            <td className="px-4 py-2 border">{tr?.endsOn}</td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default TaxationDetail;
