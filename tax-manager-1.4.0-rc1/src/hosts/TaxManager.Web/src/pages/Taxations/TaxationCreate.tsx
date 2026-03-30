import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import Select from 'react-select';

import { createTaxation, getCountries, getCurrencies, getStrategies, getSubdivisions, getTaxationsByCountryId, getTaxRates, getTimeZone } from '../../services/TaxationService';
import { Country } from '../../types/Country';
import { Subdivision } from '../../types/Subdivision';
import { TaxRateColumns, RowError } from '../../constants/TaxRatesData';
import { Strategy, TaxationTaxRateDto, TaxRate, Type } from '../../types/TaxRate';
import TaxRatesTable from '../../components/Table/TaxRatesTable';
import { Taxation } from '../../types/Taxation';
import { useError } from '../../components/GlobalError/ErrorContext';

const TaxationCreate = () => {
    const navigate = useNavigate();
    const { setError } = useError();

    const [code, setCode] = useState('');
    const [name, setName] = useState('');
    const [countryId, setCountryId] = useState(0);
    const [countryList, setCountryList] = useState<Country[]>([]);
    const [subdivisions, setSubdivisions] = useState<Subdivision[]>([]);
    const [filteredSubdivisions, setFilteredSubdivisions] = useState<Subdivision[]>([]);
    const [selectedSubdivisions, setSelectedSubdivisions] = useState<Subdivision[]>([]);
    const [subdivisionInput, setSubdivisionInput] = useState('');
    const [tableRows, setTableRows] = useState<TaxRate[]>([]);
    const [tableErrorMessage, setTableErrorMessage] = useState('');
    const [typeOptions, setTypeOptions] = useState<Type[]>([]);
    const [timeZones, setTimeZones] = useState<string[]>([]);
    const [strategyOptions, setStrategyOptions] = useState<Strategy[]>([]);
    const [currencyOptions, setCurrencyOptions] = useState<string[]>([]);
    const [dependentOptions, setDependentOptions] = useState<Taxation[]>([]);
    const countryOptions = countryList.map(c => ({ value: c.id, label: c.name }));

    const [errors, setErrors] = useState<{
        code: string;
        name: string;
        countryCode?: string;
        tableRows: RowError[];
    }>({
        code: '',
        name: '',
        countryCode: '',
        tableRows: [],
    });

    useEffect(() => {
        const fetchAllInitialData = async () => {
            try {
                const [types, strategies, currencies, countries, zones] = await Promise.all([
                    getTaxRates(),
                    getStrategies(),
                    getCurrencies(),
                    getCountries(),
                    getTimeZone(),
                ]);
                setTypeOptions(types);
                setStrategyOptions(strategies);
                setCurrencyOptions(currencies.map(c => c.name));
                setCountryList(countries);
                setTimeZones(zones.map(z => z.id));
            } catch (error) {
                console.error('Error loading initial data:', error);
            }
        };
        fetchAllInitialData();
    }, []);

    useEffect(() => {
        if (typeOptions.length > 0) {
            const rows: TaxRate[] = typeOptions.map((type) => ({
                type: { id: type.id, name: type.name },
                strategy: { id: 0, name: '' },
                code: '',
                value: '',
                valueType: '',
                dependents: [],
                startsOn: '',
                endsOn: ''
            }));
            setTableRows(rows);
        }
    }, [typeOptions]);

    useEffect(() => {
        if (countryId) {
            getSubdivisions(countryId)
                .then(data => {
                    setSubdivisions(data);
                    setFilteredSubdivisions(data);
                    setSelectedSubdivisions([]);
                    setSubdivisionInput('');
                })
                .catch(error => {
                    if (error.response?.status === 404) {
                        // Handle 404 - no subdivisions for this country
                        setSubdivisions([]);
                        setFilteredSubdivisions([]);
                        setSelectedSubdivisions([]);
                        setSubdivisionInput('');
                    } else {
                        // Log or handle other errors
                        console.error(error);
                    }
                });
            getTaxationsByCountryId(countryId)
                .then(data => {
                    setDependentOptions(data);
                })
                .catch(error => {
                    setDependentOptions([]);
                    console.error(error);
                });
        } else {
            setSubdivisions([]);
            setFilteredSubdivisions([]);
            setSelectedSubdivisions([]);
        }
    }, [countryId]);

    const validateCode = (value: string) => {
        const codeRegex = /^[a-zA-Z0-9][A-Za-z0-9-]*$/;
        if (!value) return 'Code is required.';
        if (!codeRegex.test(value))
            return 'Only alphanumeric characters and hyphens allowed, and cannot begin with a hyphen.';
        return '';
    };

    const validateName = (value: string) => {
        const nameRegex = /^[A-Za-z0-9- ]*$/;
        if (!value) return 'Name is required.';
        if (!nameRegex.test(value))
            return 'Name can only contain letters, numbers, spaces, and hyphens.';
        return '';
    };

    const validateCountry = (value: number) => (value == 0 ? 'Country is required.' : '');

    const validateTableRow = (row: TaxRate, rowIndex: number): RowError[] => {
        const rowErrors: RowError[] = [];

        if ((row.strategy.name !== '')) {
            if (!row.value) {
                rowErrors.push({
                    rowIndex,
                    field: 'value',
                    message: 'Value is required.',
                });
            }
            if (row.value) {
                const [amount = '', currency = ''] = (row.value?.split(' ') || []);
                if (Number(amount) <= 0) {
                    rowErrors.push({
                        rowIndex,
                        field: 'value',
                        message: 'Value should be greater than zero.',
                    });
                }
                if (amount && !currency && row.strategy.name === 'Flat Rate') {
                    rowErrors.push({
                        rowIndex,
                        field: 'value',
                        message: 'Currency is required if value is entered.',
                    });
                }
            }
        }

        if (row.startsOn && row.endsOn) {
            const start = new Date(row.startsOn);
            const end = new Date(row.endsOn);
            if (start >= end) {
                rowErrors.push({
                    rowIndex,
                    field: 'startsOn',
                    message: 'Start date must be before end date.',
                });
            }
        }

        return rowErrors;
    };

    const handleCodeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        setCode(value);
        setErrors(prev => ({ ...prev, code: validateCode(value) }));
    };

    const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        setName(value);
        setErrors(prev => ({ ...prev, name: validateName(value) }));
    };

    const handleCountryChange = (selectedOption: { value: number; label: string } | null) => {
        const value = selectedOption?.value ?? 0;
        setCountryId(value);
        setErrors(prev => ({ ...prev, countryCode: validateCountry(value) }));

        // Reset dependent fields in each row
        setTableRows(prevRows =>
            prevRows.map(row => ({
                ...row,
                dependents: []
            }))
        );
        setDependentOptions([]);
    };

    const handleSubdivisionInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const v = e.target.value;
        setSubdivisionInput(v);
        setFilteredSubdivisions(
            subdivisions.filter(s => s.name.toLowerCase().includes(v.toLowerCase()))
        );
    };

    const handleSubdivisionAdd = (name: string) => {
        const trimmed = name.trim();
        if (!trimmed) return;
        if (selectedSubdivisions.some(s => s.name.toLowerCase() === trimmed.toLowerCase()))
            return;
        const existing = subdivisions.find(s => s.name.toLowerCase() === trimmed.toLowerCase());
        const toAdd = existing ?? { id: 0, name: trimmed, countryId };
        setSelectedSubdivisions(prev => [...prev, toAdd]);
        setSubdivisionInput('');
        setFilteredSubdivisions(subdivisions);
    };

    const handleSubdivisionRemove = (name: string) => {
        setSelectedSubdivisions(prev => prev.filter(s => s.name !== name));
    };

    const handleTableRowChange = (updatedRows: TaxRate[]) => {
        setTableRows(updatedRows);
        const newRowErrors: RowError[] = updatedRows
            .flatMap((row, index) => validateTableRow(row, index));
        setErrors(prev => ({ ...prev, tableRows: newRowErrors }));
    };

    const mapTaxRateValues = (filledRows: TaxRate[]) => {
        try {
            const processedRows = filledRows.map((row) => {
                const updatedRow = { ...row };

                if (row.startsOn && row.startsOnHour) {
                    const combined = `${row.startsOn}T${row.startsOnHour.padStart(2, '0')}:00:00Z`;
                    updatedRow.startDate = new Date(combined);
                }

                if (row.endsOn && row.endsOnHour) {
                    const combined = `${row.endsOn}T${row.endsOnHour.padStart(2, '0')}:00:00Z`;
                    updatedRow.endDate = new Date(combined);
                }

                if (row.value) {
                    updatedRow.value = row.value.split(' ')[0];
                    updatedRow.valueType = row.value.split(' ')[1] || '%';
                }
                return updatedRow;
            });

            const completeRows: TaxationTaxRateDto[] = processedRows.map(row => ({
                taxRateId: row.type.id,
                strategyId: row.strategy.id,
                code: row.code || null,
                value: Number(row.value),
                valueType: row.valueType,
                startDate: row.startDate,
                endDate: row.endDate,
                startDateTimeZone: row.startsOnTimeZone,
                endDateTimeZone: row.endsOnTimeZone,
                dependentTaxations: row.dependents?.map(dep => ({
                    id: dep.id,
                    code: dep.code
                })) || []
            }));

            return completeRows;
        } catch (error) {
            console.error('Error mapping tax rate values:', error);
            return [];
        }
    };

    const scrollToFirstError = useCallback(() => {
        requestAnimationFrame(() => {
            const firstErrorElement = document.querySelector('.validation-error');
            if (firstErrorElement) {
                firstErrorElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        });
    }, []);

    const handleSave = async () => {
        const codeError = validateCode(code);
        const nameError = validateName(name);
        const countryError = validateCountry(countryId);
        const rowErrors: RowError[] = tableRows.flatMap((row, idx) => validateTableRow(row, idx));
        setTableErrorMessage("");
        const filledRows = tableRows.filter(row => row.strategy.id != 0)

        setErrors({ code: codeError, name: nameError, countryCode: countryError, tableRows: rowErrors });

        const hasGeneralErrors = codeError || nameError || countryError;
        const hasRowErrors = rowErrors.length > 0;
        const hasNoFilledRows = filledRows.length === 0;

        if (hasNoFilledRows) {
            setTableErrorMessage("Please enter at least one tax rate row.");
            scrollToFirstError();
            return;
        }

        if (hasGeneralErrors || hasRowErrors) {
            if (hasRowErrors) {
                setTableErrorMessage("Please fix the errors in the tax rates table.");
            }
            scrollToFirstError();
            return;
        }

        const mappedRows = mapTaxRateValues(filledRows);
        if (mappedRows.length === 0 && filledRows.length > 0) {
            toast.error('Failed to process tax rate data due to an internal error.');
            return;
        }

        if (!codeError && !nameError && !countryError) {
            try {
                const newTax: Taxation = {
                    code: code.toUpperCase(),
                    countryId,
                    name,
                    subdivisions: selectedSubdivisions,
                    taxationTaxRates: mappedRows,
                };

                const result = await createTaxation(newTax);

                if (result.isValid) {
                    toast.success('Taxation created successfully!');
                    const createdTax = result.entity as Taxation;
                    navigate(`/taxations/taxation-detail/${createdTax.code}`);
                }
                else {
                    setError(result.errors);
                }

            } catch (error) {
                console.error(error);
            }
        }
    }

    return (
        <div className="p-4">
            <div className="flex justify-between items-center mb-4">
                <h1 className="text-2xl font-semibold">New Taxation</h1>
            </div>
            <div className="p-6 border shadow-lg bg-white mx-auto min-h-[200px] transition-all duration-300 ease-in-out">
                <div className="grid grid-cols-[auto_1fr_auto_1fr] gap-y-6 gap-x-4">
                    {/* Code */}
                    <label className="text-sm font-medium self-center">Code:</label>
                    <div>
                        <input
                            type="text"
                            className={`w-64 border rounded-lg p-2 uppercase ${errors.code ? 'border-red-500' : 'border-gray-300'}`}
                            value={code}
                            onChange={handleCodeChange}
                            placeholder="Enter Code"
                            maxLength={40}
                        />
                        {errors.code && <p className="text-red-500 text-xs mt-1 validation-error">{errors.code}</p>}
                    </div>

                    {/* Name */}
                    <label className="text-sm font-medium self-center">Name:</label>
                    <div>
                        <input
                            type="text"
                            className={`w-64 border rounded-lg p-2 ${errors.name ? 'border-red-500' : 'border-gray-300'}`}
                            value={name}
                            onChange={handleNameChange}
                            placeholder="Enter Name"
                            maxLength={255}
                        />
                        {errors.name && <p className="text-red-500 text-xs mt-1 validation-error">{errors.name}</p>}
                    </div>

                    {/* Country */}
                    <label className="text-sm font-medium self-center">Country:</label>
                    <div className="inline-block mt-2" style={{ minWidth: '16rem', maxWidth: 'max-content' }}>
                        <Select
                            options={countryOptions}
                            value={countryOptions.find(o => o.value === countryId) || null}
                            onChange={handleCountryChange}
                            placeholder="Select"
                            isClearable
                            styles={{
                                container: (base) => ({
                                    ...base,
                                    width: 'auto',
                                    minWidth: '16rem',
                                }),
                                control: (base) => ({
                                    ...base,
                                    borderColor: errors.countryCode ? '#f87171' : '#d1d5db',
                                    boxShadow: 'none',
                                    padding: '0.125rem 0.5rem',
                                    borderRadius: '0.5rem',
                                    width: 'auto',
                                    minWidth: '16rem',
                                    maxWidth: '100%',
                                }),
                                valueContainer: (base) => ({
                                    ...base,
                                    padding: 0,
                                }),
                                input: (base) => ({
                                    ...base,
                                    margin: 0,
                                    padding: 0,
                                }),
                            }}
                        />
                        {errors.countryCode && (
                            <p className="text-red-500 text-xs mt-1 validation-error">{errors.countryCode}</p>
                        )}
                    </div>

                    {/* Subdivision */}
                    <label className="text-sm font-medium self-center pt-2">Subdivision:</label>

                    <div className="relative">
                        {/* Single-line tags + input container */}
                        <div className="w-full max-w-4xl border rounded-lg px-2 py-1.5 flex flex-wrap items-center gap-1 min-h-[2.5rem] mt-2">
                            {selectedSubdivisions.map((sub, idx) => (
                                <div
                                    key={idx}
                                    className="inline-flex items-center bg-gray-100 text-xs rounded px-2 py-1 h-[1.5rem]">
                                    <span className="mr-2">{sub.name}</span>
                                    <button
                                        onClick={() => handleSubdivisionRemove(sub.name)}
                                        className="px-2 py-1 text-black text-sm bg-transparent hover:border-transparent focus:outline-none cursor-pointer">
                                        x
                                    </button>
                                </div>
                            ))}

                            {/* Text input for new tags */}
                            <input
                                type="text"
                                className="h-8 text-sm outline-none border-none px-2 min-w-[60px] max-w-full shrink"
                                style={{ flex: '1 0 120px'}}
                                value={subdivisionInput}
                                onChange={handleSubdivisionInputChange}
                                maxLength={50}
                                onKeyDown={e => {
                                    if (e.key === 'Enter') {
                                        e.preventDefault();
                                        handleSubdivisionAdd(subdivisionInput);
                                    }
                                }}
                            />
                        </div>

                        {/* "Add ..." box below */}
                        {subdivisionInput &&
                            !subdivisions.some(
                                s => s.name.toLowerCase() === subdivisionInput.trim().toLowerCase()
                            ) &&
                            !selectedSubdivisions.some(
                                s => s.name.toLowerCase() === subdivisionInput.trim().toLowerCase()
                            ) && (
                                <input
                                    type="text"
                                    readOnly
                                    className="w-full max-w-xl border rounded-sm p-2 mt-1 text-gray-600 text-sm cursor-pointer h-8"
                                    value={`Add "${subdivisionInput.trim()}"`}
                                    onClick={() => handleSubdivisionAdd(subdivisionInput)}
                                />
                            )}

                        {/* Suggestions dropdown */}
                        {subdivisionInput && filteredSubdivisions.length > 0 && (
                            <div className="absolute w-full max-w-xl bg-white border rounded-b-sm shadow mt-[-1px] z-10 max-h-40 overflow-auto text-sm">
                                {filteredSubdivisions.map((sub, i) => (
                                    <div
                                        key={i}
                                        className="px-3 py-1 hover:bg-gray-100 cursor-pointer"
                                        onClick={() => handleSubdivisionAdd(sub.name)}
                                    >
                                        {sub.name}
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                    {/* Tax Rates */}
                    <div className="col-span-4 flex flex-col">
                        <label className="text-sm font-medium mb-2">Tax Rates:</label>
                         {tableErrorMessage && (
                            <div className="text-red-500 text-xs validation-error">
                                {tableErrorMessage}
                            </div>
                        )}
                        <div className="w-full overflow-x-auto">
                            <TaxRatesTable
                                columns={TaxRateColumns}
                                data={tableRows}
                                onChange={handleTableRowChange}
                                rowErrors={errors.tableRows}
                                dependentOptions={dependentOptions}
                                strategyOptions={strategyOptions}
                                currencyOptions={currencyOptions}
                                typeOptions={typeOptions}
                                timeZoneOptions={timeZones}
                            />
                        </div>
                    </div>
                </div>

                {/* Buttons */}
                <div className="flex justify-end gap-4 mt-10">
                    <button
                        onClick={handleSave}
                        className="w-32 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                    >
                        Save
                    </button>
                    <button
                        onClick={() => {
                            setCode('');
                            setName('');
                            setCountryId(0);
                            setTableRows([]);
                            setErrors({ code: '', name: '', countryCode: '', tableRows: [] });
                            navigate('/taxations');
                        }}
                        className="w-32 px-4 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-600"
                    >
                        Cancel
                    </button>
                </div>
            </div>
        </div>
    );
};

export default TaxationCreate;
