import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import Select, { SingleValue } from 'react-select';

import { createLegalEnvironment } from '../../services/LegalEnvironmentService';
import { depositTaxrateModeLabels } from "../../constants/DepositTaxrateMode";
import { getCountries, getTaxationsByCountryId } from '../../services/TaxationService';
import TaxationTable from '../../components/Table/TaxationTable';
import { Taxation } from '../../types/Taxation';
import { Subdivision } from '../../types/Subdivision';

const depositTaxrateModeOptions = Object.entries(depositTaxrateModeLabels).map(
    ([value, label]) => ({
        value: Number(value),
        label,
    })
);

const LegalEnvironmentCreate = () => {
    const [code, setCode] = useState('');
    const [name, setName] = useState('');
    const [countryId, setCountryId] = useState(0);
    const [depositTaxRateMode, setDepositTaxRateMode] = useState<number | undefined>(undefined);
    const [countryOptions, setCountryOptions] = useState<{ value: number; label: string }[]>([]);
    const [taxationsList, setTaxationsList] = useState<Taxation[]>([]);
    const [selectedTaxations, setSelectedTaxations] = useState<number[]>([]);
    const [subdivisionsList, setSubdivisionsList] = useState<Subdivision[]>([]);
    const [selectedSubdivisions, setSelectedSubdivisions] = useState<Subdivision[]>([]);
    const [errors, setErrors] = useState({
        code: '',
        name: '',
        depositTaxRateMode: '',
        countryCode: '',
        taxations: ''
    });
    const navigate = useNavigate();

    useEffect(() => {
        const fetchCountryData = async () => {
            try {
                const countries = await getCountries();
                const options = countries.map(c => ({ value: c.id, label: c.name }));
                setCountryOptions(options);
            } catch (error) {
                console.error('Error loading country data:', error);
            }
        };
        fetchCountryData();
    }, []);

    useEffect(() => {
        if (countryId) {
            getTaxationsByCountryId(countryId)
                .then(data => {
                    setTaxationsList(data);
                    const allSubdivisions: Subdivision[] = data
                        .flatMap(tax => tax.subdivisions || [])
                        .filter((sub, index, self) =>
                            sub && self.findIndex(s => s.id === sub.id) === index
                        );

                    setSubdivisionsList(allSubdivisions);

                })
                .catch(error => {
                    setTaxationsList([]);
                    console.error(error);
                });
        } else {

        }
    }, [countryId]);

    useEffect(() => {
        setTaxationsList([]);
        setSubdivisionsList([]);
        setSelectedSubdivisions([]);
        errors.taxations = '';
    }, [countryId]);

    const validateCode = (value: string) => {
        const codeRegex = /^[a-zA-Z0-9][A-Za-z0-9-]*$/;
        if (!value) return "Code is required.";
        if (!codeRegex.test(value)) return "Only alphanumeric characters and hyphens are allowed. Cannot begin with a hyphen.";
        return "";
    };

    const validateName = (value: string) => {
        const nameRegex = /^[A-Za-z0-9- ]*$/;
        if (!value) return "Name is required.";
        if (!nameRegex.test(value)) return "Name can only contain letters, numbers, spaces, and hyphens.";
        return "";
    };

    const validateDepositTaxRateMode = (value: number | undefined) => {
        if (value === undefined || value === null) {
            return "Deposit Tax Rate Mode is required.";
        }
        return "";
    };

    const validateCountry = (value: number) => (value == 0 ? 'Country is required.' : '');

    const validateSelectedTaxations = (countryId: number, selectedTaxations: number[]): string => {
        if (countryId > 0 && taxationsList.length > 0 && selectedTaxations.length === 0) {
            return "At least one taxation must be selected.";
        }
        return "";
    };

    const handleSave = async () => {
        const codeError = validateCode(code);
        const nameError = validateName(name);
        const depositTaxRateModeError = validateDepositTaxRateMode(depositTaxRateMode);
        const countryError = validateCountry(countryId);
        const taxationsError = validateSelectedTaxations(countryId, selectedTaxations);

        setErrors({
            code: codeError,
            name: nameError,
            depositTaxRateMode: depositTaxRateModeError,
            countryCode: countryError,
            taxations: taxationsError
        });

        if (!codeError && !nameError && !depositTaxRateModeError && !countryError && !taxationsError) {
            try {
                const newLegalEnvironment = {
                    code: code.toUpperCase(),
                    name,
                    depositTaxRateMode: Number(depositTaxRateMode),
                    taxationIds: selectedTaxations
                };

                const createdLegalEnvironment = await createLegalEnvironment(newLegalEnvironment);

                toast.success('Legal Environment created successfully!')
                navigate(`/legal-environments/legal-environment-detail/${createdLegalEnvironment.code}`);
            } catch (error) {
                console.error(error);
                toast.error('Failed to create Legal Environment.');
            }
        }
    };

    const handleCodeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const val = e.target.value;
        setCode(val);
        setErrors((prev) => ({ ...prev, code: validateCode(val) }));
    };

    const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const val = e.target.value;
        setName(val);
        setErrors((prev) => ({ ...prev, name: validateName(val) }));
    };

    const handleDepositTaxRateModeChange = (selected: SingleValue<Option>) => {
        const val = selected?.value ?? null;
        setDepositTaxRateMode(val);
        setErrors((prev) => ({
            ...prev,
            depositTaxRateMode: validateDepositTaxRateMode(val),
        }));
    };

    const handleCountryChange = (selectedOption: { value: number; label: string } | null) => {
        const value = selectedOption?.value ?? 0;
        setCountryId(value);
        setErrors(prev => ({ ...prev, countryCode: validateCountry(value) }));
    };

    const handleBack = () => {
        navigate("/legal-environments");
    };

    return (
        <div className="p-4">
            <div className="flex justify-between items-center mb-4">
                <h1 className="text-2xl font-semibold">New Legal Environment</h1>
            </div>
            <div className="p-6 border shadow-lg bg-white mx-auto min-h-[200px] transition-all duration-300 ease-in-out">
                <div className="grid grid-cols-[auto_1fr] gap-y-6 gap-x-4">
                    {/* Code Field */}
                    <label className="text-sm font-medium self-center">Code:</label>
                    <div>
                        <input
                            type="text"
                            className={`w-64 max-w-md border rounded-lg p-2 uppercase ${errors.code ? 'border-red-500' : 'border-gray-300'}`}
                            value={code}
                            onChange={handleCodeChange}
                            maxLength={40}
                            placeholder="Enter Code"
                        />
                        {errors.code && <p className="text-red-500 text-xs mt-1">{errors.code}</p>}
                    </div>

                    {/* Name Field */}
                    <label className="text-sm font-medium self-center">Name:</label>
                    <div>
                        <input
                            type="text"
                            className={`w-64 max-w-md border rounded-lg p-2 ${errors.name ? 'border-red-500' : 'border-gray-300'}`}
                            value={name}
                            onChange={handleNameChange}
                            maxLength={255}
                            placeholder="Enter Name"
                        />
                        {errors.name && <p className="text-red-500 text-xs mt-1">{errors.name}</p>}
                    </div>

                    {/* Deposit Tax Rate Mode */}
                    <label className="text-sm font-medium self-center">Deposit Tax Rate Mode:</label>
                    <div className="inline-block" style={{ minWidth: '16rem', maxWidth: 'max-content' }}>
                        <Select
                            options={depositTaxrateModeOptions}
                            value={
                                depositTaxrateModeOptions.find((o) => o.value === depositTaxRateMode) || null
                            }
                            onChange={handleDepositTaxRateModeChange}
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
                                    borderColor: errors.depositTaxRateMode ? '#f87171' : '#d1d5db',
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
                        {errors.depositTaxRateMode && (
                            <p className="text-red-500 text-xs mt-1 validation-error">{errors.depositTaxRateMode}</p>
                        )}
                    </div>

                    {/* Country */}
                    <label className="text-sm font-medium self-center">Country:</label>
                    <div className="inline-block" style={{ minWidth: '16rem', maxWidth: 'max-content' }}>
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
                                menuPortal: (base) => ({
                                    ...base,
                                    zIndex: 9999,
                                }),
                                menu: (base) => ({
                                    ...base,
                                    zIndex: 9999,
                                }),
                            }}
                        />
                        {errors.countryCode && (
                            <p className="text-red-500 text-xs mt-1 validation-error">{errors.countryCode}</p>
                        )}
                    </div>

                    {/* Subdivision */}
                    <label className="text-sm font-medium self-center">Subdivision:</label>
                    {subdivisionsList.length > 0 ? (
                        <div style={{ minWidth: '16rem', maxWidth: 'max-content' }}>
                            <Select
                                isMulti
                                options={subdivisionsList
                                    .filter(opt => !selectedSubdivisions.some(s => s.id === opt.id))
                                    .map(opt => ({
                                        label: opt.name,
                                        value: { id: opt.id, name: opt.name }
                                    }))}
                                value={selectedSubdivisions.map(sub => ({
                                    label: sub.name,
                                    value: { id: sub.id, name: sub.name }
                                }))}
                                onChange={(selected) =>
                                    setSelectedSubdivisions(
                                        (selected?.map(s => s.value) ?? []) as Subdivision[]
                                    )
                                }
                                classNamePrefix="react-select"
                                styles={{
                                    control: (base, state) => ({
                                        ...base,
                                        minHeight: '2.5rem',
                                        borderColor: state.isFocused ? '#3b82f6' : '#d1d5db',
                                        boxShadow: 'none',
                                        flexWrap: 'nowrap',
                                        borderRadius: '0.5rem',
                                        padding: '0 0.5rem',
                                    }),
                                    valueContainer: (base) => ({
                                        ...base,
                                        height: '2rem',
                                        padding: 0,
                                        flexWrap: 'nowrap',
                                    }),
                                    input: (base) => ({
                                        ...base,
                                        margin: 0,
                                        padding: 0,
                                        paddingRight: '1.5rem'
                                    }),
                                    multiValue: (base) => ({
                                        ...base,
                                        fontSize: '0.75rem',
                                    }),
                                    menu: (base) => ({
                                        ...base,
                                        zIndex: 9999,
                                    }),
                                    menuPortal: (base) => ({
                                        ...base,
                                        zIndex: 9999,
                                    }),
                                }}

                                menuPortalTarget={document.body}
                                menuPosition="absolute"
                                placeholder=""
                            />
                        </div>
                    ) : (
                        <p className="text-gray-500 self-center">
                            {countryId === 0
                                ? 'Select a country to see associated subdivisions'
                                : 'No associated subdivisions found'}
                        </p>
                    )}

                    <label className="text-sm font-medium self-start">Taxations:</label>
                    <div className="col-span-1 flex flex-col gap-1">
                        {errors.taxations && (
                            <div className="text-red-500 text-sm validation-error">
                                {errors.taxations}
                            </div>
                        )}
                        {taxationsList.length > 0 ? (
                            <TaxationTable
                                taxations={taxationsList}
                                onSelectionChange={(selectedIds) => {
                                    setSelectedTaxations(selectedIds);
                                    setErrors((prev) => ({
                                        ...prev,
                                        taxations: validateSelectedTaxations(countryId, selectedIds),
                                    }));
                                }}
                            />

                        ) : (
                            <>
                                {countryId === 0 ? (
                                    <p className="text-gray-500">Select a country to see associated taxations</p>
                                ) : taxationsList.length === 0 ? (
                                    <p className="text-gray-500">No associated taxations found</p>
                                ) : null}

                            </>
                        )}
                    </div>
                </div>

                {/* Buttons Section */}
                <div className="flex justify-end gap-4 mt-10">
                    <button
                        onClick={handleSave}
                        className="w-32 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                    >
                        Save
                    </button>
                    <button
                        onClick={handleBack}
                        className="w-32 px-4 py-2 bg-gray-300 text-black rounded-lg hover:bg-gray-400"
                    >
                        Cancel
                    </button>
                </div>
            </div>
        </div>
    );
};

export default LegalEnvironmentCreate;
