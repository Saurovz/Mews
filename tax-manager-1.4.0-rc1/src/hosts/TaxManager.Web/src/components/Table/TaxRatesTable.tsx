import Select from 'react-select';

import { SimpleTaxationDto, Strategy, TaxRate, Type } from '../../types/TaxRate';
import { TableColumn, RowError } from '../../constants/TaxRatesData';
import DateTimeRangeInput from '../../Utils/DateTimeRangeInput';
import { Taxation } from '../../types/Taxation';

/**
 * Props for the TaxRatesTable component.
 */
interface TaxRatesTableProps {
  columns: TableColumn[];
  data: TaxRate[];
  onChange: (updatedData: TaxRate[]) => void;
  rowErrors?: RowError[];
  strategyOptions: Strategy[];
  dependentOptions: Taxation[];
  currencyOptions: string[];
  typeOptions: Type[];
  timeZoneOptions: string[];
}

/**
 * Determines if a given field is editable based on the selected strategy.
 * @param field The field key.
 * @param strategy The current strategy value.
 * @returns True if the field is editable, false otherwise.
 */
const isEditableField = (field: keyof TaxRate, strategy: Strategy): boolean => {
  if (field === 'strategy') return true;
  if (strategy.name !== 'Flat Rate' && strategy.name !== 'Relative Rate' && strategy.name !== 'Relative Rate With Dependencies') return false;
  if (strategy.name === 'Relative Rate With Dependencies')
    return ['code', 'value', 'startsOn', 'endsOn', 'dependents', 'startsOnHour', 'endsOnHour', 'startDateTimeZone', 'endDateTimeZone'].includes(field);
  return ['code', 'value', 'startsOn', 'endsOn', 'startsOnHour', 'endsOnHour', 'startDateTimeZone', 'endDateTimeZone'].includes(field);
};

/**
 * A helper component to render a single table cell.
 */
interface TableCellProps {
  column: TableColumn;
  entry: TaxRate;
  index: number;
  handleChange: (index: number, field: keyof TaxRate, value: string | number) => void;
  handleMultiChange: (index: number, changes: Partial<TaxRate>) => void;
  getFieldError: (rowIndex: number, field: keyof TaxRate) => string | null;
  dependentOptions: Taxation[];
  strategyOptions: Strategy[];
  currencyOptions: string[];
  typeOptions: Type[];
  timeZoneOptions: string[];
}

const TableCell = ({
  column,
  entry,
  index,
  handleChange,
  handleMultiChange,
  getFieldError,
  dependentOptions,
  strategyOptions,
  currencyOptions,
  timeZoneOptions
}: TableCellProps) => {
  const field = column.key as keyof TaxRate;
  const isDisabled = !isEditableField(field, entry.strategy);
  const errorMessage = getFieldError(index, field);
  const errorClass = errorMessage ? 'border-red-500' : '';
  const inputBaseClass = `p-1 border rounded disabled:bg-gray-100 h-7`;
  const isRelativeRate = entry.strategy?.name === 'Relative Rate' || entry.strategy?.name === 'Relative Rate With Dependencies';

  switch (field) {
    case 'type':
      return (
        <td className="border p-2">
          <span className="w-32 text-gray-800 block">{entry.type?.name}</span>
        </td>
      );
    case 'strategy':
      return (
        <td className="border p-2">
          <select
            value={entry.strategy?.name}
            onChange={(e) => handleChange(index, 'strategy', e.target.value)}
            title={errorMessage || ''}
            className={`w-54 ${inputBaseClass} ${errorClass}`}
          >
            <option value=""></option>
            {strategyOptions.map((opt) => (
              <option key={opt.id} value={opt.name}>
                {opt.name}
              </option>
            ))}
          </select>
        </td>
      );
    case 'code':
      return (
        <td className="border p-2">
          <input
            type="text"
            value={entry.code || ''}
            disabled={isDisabled}
            maxLength={1}
            onChange={(e) => {
              const val = e.target.value;
              if (/^[A-Za-z0-9$%-]?$/.test(val)) {
                handleChange(index, 'code', val);
              }
            }}
            title={errorMessage || ''}
            className={`w-10 ${inputBaseClass} ${errorClass}`}
          />
        </td>
      );
    case 'value':
      return (
        <td className="border p-2">
          <div className="flex gap-2">
            {isRelativeRate ? (
              <>
                <input
                  type="number"
                  value={entry.value || ''}
                  disabled={isDisabled}
                  onChange={(e) => {
                    handleChange(index, field, e.target.value);
                  }}
                  title={errorMessage || ''}
                  className={`w-20 ${inputBaseClass} ${errorClass}`}
                />
                <span className="text-sm pt-1">%</span>
              </>
            ) : (
              <>
                <input
                  type="number"
                  value={entry.value?.split(' ')[0] || ''}
                  disabled={isDisabled}
                  onChange={(e) => {
                    const amount = e.target.value;
                    let [, currency] = entry.value?.split(' ') || [];

                    // Only assign 'USD' if user is typing a value and no currency is already set
                    if (amount && (!currency || currency === '')) {
                      currency = 'USD';
                    }

                    const newValue = amount ? `${amount} ${currency || ''}`.trim() : '';
                    handleChange(index, field, newValue);
                  }}
                  title={errorMessage || ''}
                  className={`w-20 ${inputBaseClass} ${errorClass}`}
                />
                <select
                  value={entry.value?.split(' ')[1] || ''}
                  disabled={isDisabled}
                  onChange={(e) => {
                    const currency = e.target.value;
                    const amount = entry.value?.split(' ')[0] || '';
                    handleChange(index, field, `${amount} ${currency}`);
                  }}
                  className={`w-20 ${inputBaseClass} ${errorClass}`}
                >
                  <option value=""></option>
                  {currencyOptions.map((code) => (
                    <option key={code} value={code}>
                      {code}
                    </option>
                  ))}
                </select>
              </>
            )}
          </div>
        </td>
      );
    case 'dependents':
      const selectedIds = (entry.dependents || []).map((d) => d.id);

      const options = dependentOptions
        .filter((opt) => opt.code && opt.code !== entry.code && !selectedIds.includes(opt.id!))
        .map((opt) => ({
          label: opt.code,
          value: { id: opt.id, code: opt.code },
        }));

      const selectedOptions = (entry.dependents || []).map((dep: SimpleTaxationDto) => ({
        label: dep.code,
        value: { id: dep.id, code: dep.code },
      }));


      return (
        <Select
          isMulti
          isDisabled={isDisabled}
          options={options}
          value={selectedOptions}
          onChange={(selected) =>
            handleChange(index, 'dependents', (selected?.map((s) => s.value) ?? []) as { id: number; code: string }[])
          }          
          classNamePrefix="react-select"
          className={`min-w-[250px] text-xs h-[1.75rem] p-2 ${errorClass}`}
          styles={{
            control: (base, state) => ({
              ...base,
              height: '1.75rem',
              minHeight: '1.75rem',
              flexWrap: 'nowrap',
              borderColor: state.isFocused ? '#3b82f6' : base.borderColor, // optional focus style
              boxShadow: 'none',
            }),
            valueContainer: (base) => ({
              ...base,
              height: '1.75rem',
              flexWrap: 'nowrap',
            }),
            multiValue: (base) => ({
              ...base,
              fontSize: '0.75rem',
            }),
            menu: (base) => ({
              ...base,
              zIndex: 9999,
            }),
          }}
          menuPortalTarget={document.body} // ensures dropdown isn't clipped
          menuPosition="absolute"
          placeholder=""
        />

      );
    case 'startsOn':
    case 'endsOn':
      return (
        <td className="border p-2">
          <DateTimeRangeInput
            row={entry}
            baseField={field}
            onChange={(changes) => handleMultiChange(index, changes)}
            error={getFieldError(index, field)}
            disabled={isDisabled}
            timeZoneOptions={timeZoneOptions}
          />
        </td>
      );
    default:
      return (
        <td className="border p-2">
          <input
            type="text"
            value={entry[field] as string}
            disabled={isDisabled}
            onChange={(e) => handleChange(index, field, e.target.value)}
            title={errorMessage || ''}
            className={`w-full ${inputBaseClass} ${errorClass}`}
          />
        </td>
      );
  }
};

const TaxRatesTable: React.FC<TaxRatesTableProps> = ({
  columns,
  data,
  onChange,
  rowErrors,
  dependentOptions = [],
  strategyOptions = [],
  currencyOptions = [],
  typeOptions,
  timeZoneOptions = []
}) => {

  /**
   * Handles changes to a single field within a tax rate row.
   * If the strategy changes to anything other than 'Flat Rate',
   * relevant fields are reset to their default empty states.
   */
  const handleChange = (
    index: number,
    field: keyof TaxRate,
    value: any
  ) => {
    const updatedData = [...data];
    const row = { ...updatedData[index] };

    if (field === 'strategy') {
      const selectedStrategy = strategyOptions.find(opt => opt.name === value);

      if (!selectedStrategy ||
        ['Flat Rate', 'Relative Rate', 'Relative Rate With Dependencies'].includes(selectedStrategy.name)) {
        Object.assign(row, {
          code: '',
          value: '',
          valueType: '',
          startsOn: '',
          startsOnHour: '',
          endsOn: '',
          endsOnHour: '',
          startsOnTimeZone: '',
          endsOnTimeZone: '',
          startDate: undefined,
          endDate: undefined,
          dependents: [],
        });
      }

      row.strategy = selectedStrategy ?? { id: 0, name: '' };
    } else {
      if (field === 'dependents' && Array.isArray(value)) {
        row.dependents = value as SimpleTaxationDto[];
      }
      if (
        typeof value === 'string' &&
        ['code', 'value', 'valueType', 'startsOn', 'endsOn', 'startsOnHour', 'endsOnHour', 'startDateTimeZone', 'endDateTimeZone'].includes(field)
      ) {
        row[field] = value as any;
      }

      if ((field === 'startDate' || field === 'endDate') && (value === '' || (typeof value === 'object' && value instanceof Date))) {
        row[field] = value as any;
      }
    }

    updatedData[index] = row;
    onChange(updatedData);
  };

  /**
   * Handles multiple changes to a tax rate row simultaneously,
   * typically used by complex inputs like DateTimeRangeInput.
   */
  const handleMultiChange = (
    index: number,
    changes: Partial<TaxRate>
  ) => {
    const updatedData = [...data];
    updatedData[index] = { ...updatedData[index], ...changes };
    onChange(updatedData);
  };

  /**
   * Retrieves the error message for a specific field in a given row.
   */
  const getFieldError = (
    rowIndex: number,
    field: keyof TaxRate
  ): string | null => {
    return (
      rowErrors?.find(
        (err) => err.rowIndex === rowIndex && err.field === field
      )?.message || null
    );
  };

  return (
    <div>
      <table className="w-full text-sm border border-collapse border-gray-300">
        <thead className="bg-gray-100">
          <tr>
            {columns.map((column) => (
              <th key={column.key} className="border p-2">
                {column.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((entry, index) => (
            <tr key={index} className="border-t">
              {columns.map((column) => (
                <TableCell
                  key={column.key}
                  column={column}
                  entry={entry}
                  index={index}
                  handleChange={handleChange}
                  handleMultiChange={handleMultiChange}
                  getFieldError={getFieldError}
                  dependentOptions={dependentOptions}
                  strategyOptions={strategyOptions}
                  currencyOptions={currencyOptions}
                  typeOptions={typeOptions}
                  timeZoneOptions={timeZoneOptions}
                />
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default TaxRatesTable;
