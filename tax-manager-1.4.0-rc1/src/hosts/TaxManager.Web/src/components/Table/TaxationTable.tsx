import React, { useMemo, useState } from 'react';
import { Taxation } from '../../types/Taxation';

interface Props {
  taxations: Taxation[];
  onSelectionChange?: (selectedIds: number[]) => void;
}

const TaxationTable: React.FC<Props> = ({ taxations, onSelectionChange }) => {
  const [selectedIds, setSelectedIds] = useState<Set<number>>(new Set());

  const sortedTaxations = useMemo(() => {
    return [...taxations].sort((a, b) => a.code.localeCompare(b.code));
  }, [taxations]);

  const toggleCheckbox = (id: number | undefined) => {
    if (id === undefined) return;

    const updated = new Set(selectedIds);
    updated.has(id) ? updated.delete(id) : updated.add(id);
    setSelectedIds(updated);
    onSelectionChange?.(Array.from(updated));
  };

  return (
    <div className="w-1/2 max-h-64 overflow-y-auto border border-gray-300">
      <table className="w-full border-collapse">
        <thead className="bg-gray-100 sticky top-0 z-10">
          <tr>
            <th className="w-8 border px-2 py-1 bg-gray-100"> </th>
            <th className="text-left border px-2 py-1 bg-gray-100">Code</th>
            <th className="text-left border px-2 py-1 bg-gray-100">Name</th>
          </tr>
        </thead>
        <tbody>
          {sortedTaxations.map((taxation) => (
            <tr key={taxation.id} className="border-t">
              <td className="text-center border px-2">
                <input
                  type="checkbox"
                  checked={taxation.id !== undefined && selectedIds.has(taxation.id)}
                  onChange={() => toggleCheckbox(taxation.id)}
                />
              </td>
              <td className="border px-2 py-1">{taxation.code}</td>
              <td className="border px-2 py-1">{taxation.name}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default TaxationTable;