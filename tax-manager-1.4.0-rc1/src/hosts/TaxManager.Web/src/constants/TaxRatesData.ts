import { TaxRate } from "../types/TaxRate";

export interface TableColumn {
    key: string;
    label: string;
};

export interface RowError {
    rowIndex: number;
    field: keyof TaxRate;
    message: string;
}

export const TaxRateColumns: TableColumn[] = [
    { key: "type", label: "Type" },
    { key: "strategy", label: "Strategy" },
    { key: "code", label: "Code" },
    { key: "value", label: "Value" },
    { key: "dependents", label: "Dependents" }, 
    { key: "startsOn", label: "Starts On" },
    { key: "endsOn", label: "Ends On" },
];


export const hourOptions = Array.from({ length: 24 }, (_, i) =>
    i.toString().padStart(2, '0')
);
