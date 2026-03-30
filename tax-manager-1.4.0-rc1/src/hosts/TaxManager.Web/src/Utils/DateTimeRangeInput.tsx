import React, { useEffect, useMemo } from 'react';
import DatePicker from 'react-datepicker';
import { FaCalendarAlt } from 'react-icons/fa';
import 'react-datepicker/dist/react-datepicker.css';
import { format } from 'date-fns';

import { hourOptions } from '../constants/TaxRatesData';
import { TaxRate } from '../types/TaxRate';

/**
 * Props for the DateTimeRangeInput component.
 */
interface DateTimeRangeInputProps {
  row: TaxRate;
  baseField: 'startsOn' | 'endsOn';
  onChange: (changes: Partial<TaxRate>) => void;
  error?: string | null;
  disabled?: boolean;
  timeZoneOptions: string[]
}

/**
 * Custom input component for the DatePicker to allow custom styling and icon.
 */
const CustomDateInput = React.forwardRef(
  ({ value, onClick, disabled, error, errorMessage }: any, ref: React.Ref<HTMLInputElement>) => (
    <div className="relative w-32" onClick={onClick} ref={ref}>
      <input
        type="text"
        value={value}
        readOnly
        disabled={disabled}
        className={`w-32 p-1 pr-8 border rounded disabled:bg-gray-100 cursor-pointer h-7 ${
          error ? 'border-red-500 bg-red-100' : 'border-gray-300'
        }`}
        title={errorMessage || ''}
      />
      <FaCalendarAlt className="absolute right-2 top-1/2 transform -translate-y-1/2 text-gray-500 pointer-events-none" />
    </div>
  )
);

/**
 * DateTimeRangeInput component provides a combined date, hour, and timezone input
 * for 'startsOn' or 'endsOn' fields of a TaxRate.
 */
const DateTimeRangeInput = ({
  row,
  baseField,
  onChange,
  error,
  disabled,
  timeZoneOptions
}: DateTimeRangeInputProps) => {
  // Destructure relevant fields from the row using computed property names
  const dateField = row[baseField] as string;
  const hourField = row[`${baseField}Hour` as keyof TaxRate] as string;
  const timeZoneField = row[`${baseField}TimeZone` as keyof TaxRate] as string;
  const selectedDate = useMemo(() => (dateField ? new Date(dateField) : null), [dateField]);

  useEffect(() => {
    if (dateField) {
      const changes: Partial<TaxRate> = {};
      const hourKey = `${baseField}Hour` as 'startsOnHour' | 'endsOnHour';
      const timeZoneKey = `${baseField}TimeZone` as 'startsOnTimeZone' | 'endsOnTimeZone';

      if (!hourField) {
        changes[hourKey] = '00';
      }

      if (!timeZoneField) {
        changes[timeZoneKey] = 'UTC';
      }

      if (Object.keys(changes).length > 0) {
        onChange(changes);
      }
    }
  }, [dateField, hourField, timeZoneField, baseField, onChange]);

  const handleClear = () => {
    onChange({
      [baseField]: '',
      [`${baseField}Hour`]: '',
      [`${baseField}TimeZone`]: '',
    });
  };

  return (
    <div className="flex items-center gap-2">
      {/* Date Picker */}
      <DatePicker
        selected={selectedDate}
        onChange={(date: Date | null) =>
          onChange({ [baseField]: date ? format(date, 'yyyy-MM-dd') : '' })
        }
        placeholderText=""
        disabled={disabled}
        customInput={
          <CustomDateInput
            error={!!error}
            errorMessage={error}
            disabled={disabled} 
          />
        }
        dateFormat="MM-dd-yyyy"
      />

      {/* Hour Selector */}
      <select
        value={hourField || ''}
        disabled={disabled || !dateField}
        onChange={(e) => onChange({ [`${baseField}Hour`]: e.target.value })}
        className="w-12 p-1 border rounded disabled:bg-gray-100 h-7"
      >
        <option value=""></option>
        {hourOptions.map((opt) => (
          <option key={opt} value={opt}>
            {opt}
          </option>
        ))}
      </select>

      {/* Time Zone Selector (only for startsOn and endsOn) */}
      {(baseField === 'startsOn' || baseField === 'endsOn') && (
        <select
          value={timeZoneField || ''}
          disabled={disabled || !dateField}
          onChange={(e) => onChange({ [`${baseField}TimeZone`]: e.target.value })}
          className="w-40 p-1 border rounded disabled:bg-gray-100 h-7"
        >
          <option value=""></option>
          {timeZoneOptions.map((tz) => (
            <option key={tz} value={tz}>
              {tz}
            </option>
          ))}
        </select>
      )}

      {/* Clear Button (only visible if a date is selected) */}
      {dateField && (
        <button
          type="button"
          onClick={handleClear}
          className="text-gray-500 text-lg hover:text-red-500 p-0 px-1"
          title="Clear"
          disabled={disabled}
        >
          &times;
        </button>
      )}
    </div>
  );
};

export default DateTimeRangeInput;