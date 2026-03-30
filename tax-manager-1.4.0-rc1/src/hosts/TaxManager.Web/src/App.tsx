import { BrowserRouter as Router } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

import AppNavbar from './pages/HomePage';
import Loader from './components/Loader/Loader';
import { LoaderProvider } from './components/Loader/loaderContext';
import LoaderInitializer from './components/Loader/LoaderInitializer';
import { ErrorProvider } from './components/GlobalError/ErrorContext';
import ErrorDisplay from './components/GlobalError/ErrorDisplay';
import ErrorInitializer from './components/GlobalError/ErrorInitializer';

const App = () => {
    return (
        <LoaderProvider>
            <ErrorProvider>
                <LoaderInitializer />
                <ErrorInitializer />
                <Loader />
                <ErrorDisplay />
                <Router>
                    <AppNavbar />
                    <ToastContainer position="top-right" autoClose={3000} />
                </Router>
            </ErrorProvider>
        </LoaderProvider>
    );
};

export default App;

// import React, { useState, useEffect } from 'react';
// import { ChevronDown, ChevronRight, Plus, X, Save } from 'lucide-react';
// import axios from 'axios';

// interface Record {
//     id: string;
//     field: string;
//     type: string;
//     value: string;
// }

// interface RootTable {
//     id: string;
//     records: Record[];
//     children: RootTable[];
//     headerName?: string;
// }

// const TYPE_OPTIONS = ['String', 'Date', 'Percentage', 'Decimal', 'CHILD'] as const;

// function TableHeader({ rootNumber, level = 0, childNumber, headerName }: {
//     rootNumber: number;
//     level?: number;
//     childNumber?: number;
//     headerName?: string;
// }) {
//     return (
//         <div className="grid grid-cols-[auto,1fr,1fr,1fr,auto] gap-4 bg-gray-100 p-2 border-b font-semibold">
//             <div className="w-24">
//                 {level === 0 ? `Root${rootNumber}` : headerName || `Child${childNumber}`}
//             </div>
//             <div>Field</div>
//             <div>Type</div>
//             <div>Value</div>
//             <div className="w-8"></div>
//         </div>
//     );
// }

// function RecordRow({
//                        record,
//                        onRemove,
//                        onUpdate,
//                        onAdd,
//                        onAddChild,
//                        onAddRecord,
//                        isLastRecord,
//                        isOnlyRecord
//                    }: {
//     record: Record;
//     onRemove: (id: string) => void;
//     onUpdate: (id: string, field: string, value: any) => void;
//     onAdd: (id: string) => void;
//     onAddChild: (headerName: string) => void;
//     onAddRecord: () => void;
//     isLastRecord: boolean;
//     isOnlyRecord: boolean;
// }) {
//     const showArrow = isLastRecord || isOnlyRecord;

//     const handleArrowClick = () => {
//         if (!record.field.trim()) {
//             alert('Please enter a field name before creating a child table');
//             return;
//         }

//         if (record.type === 'CHILD') {
//             onAddChild(record.field);
//             onRemove(record.id);
//         } else {
//             onAddRecord();
//         }
//     };

//     const handleFieldChange = (e: React.ChangeEvent<HTMLInputElement>) => {
//         onUpdate(record.id, 'field', e.target.value);
//     };

//     const handleTypeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
//         onUpdate(record.id, 'type', e.target.value);
//         if (e.target.value === 'CHILD') {
//             onUpdate(record.id, 'value', '');
//         }
//     };

//     return (
//         <div className="grid grid-cols-[auto,1fr,1fr,1fr,auto] gap-4 items-center border-b border-gray-200 p-2">
//             <div className="flex items-center space-x-1">
//                 <div className="flex items-center space-x-1">
//                     <button
//                         onClick={() => onAdd(record.id)}
//                         className="p-1 hover:bg-gray-100 rounded text-blue-500"
//                         title="Add record above"
//                     >
//                         <Plus className="w-4 h-4" />
//                     </button>
//                     <button
//                         onClick={() => onRemove(record.id)}
//                         className="p-1 hover:bg-gray-100 rounded text-red-500"
//                     >
//                         <X className="w-4 h-4" />
//                     </button>
//                 </div>
//             </div>
//             <div className="relative">
//                 <input
//                     type="text"
//                     value={record.field}
//                     onChange={handleFieldChange}
//                     className={`border rounded px-2 py-1 w-full ${!record.field.trim() ? 'border-red-500' : ''}`}
//                     placeholder="Required field name"
//                 />
//                 {!record.field.trim() && (
//                     <div className="absolute text-xs text-red-500 mt-1">Field name is required</div>
//                 )}
//             </div>
//             <select
//                 value={record.type}
//                 onChange={handleTypeChange}
//                 className="border rounded px-2 py-1 bg-white"
//             >
//                 <option value="">Select Type</option>
//                 {TYPE_OPTIONS.map((type) => (
//                     <option key={type} value={type}>
//                         {type}
//                     </option>
//                 ))}
//             </select>
//             <input
//                 type="text"
//                 value={record.value}
//                 onChange={(e) => onUpdate(record.id, 'value', e.target.value)}
//                 className="border rounded px-2 py-1"
//                 disabled={record.type === 'CHILD'}
//             />
//             <div>
//                 {showArrow && (
//                     <button
//                         onClick={handleArrowClick}
//                         className="px-2 py-1 text-blue-500 hover:bg-gray-100 rounded"
//                     >
//                         ⇒
//                     </button>
//                 )}
//             </div>
//         </div>
//     );
// }

// function TableComponent({
//                             table,
//                             level = 0,
//                             tableIndex,
//                             childNumber,
//                             parentPath = [],
//                             onAddRecord,
//                             onRemoveRecord,
//                             onUpdateRecord,
//                             onAddChildTable,
//                             onRemoveTable
//                         }: {
//     table: RootTable;
//     level?: number;
//     tableIndex: number;
//     childNumber?: number;
//     parentPath?: string[];
//     onAddRecord: (path: string[], recordId?: string) => void;
//     onRemoveRecord: (path: string[], recordId: string) => void;
//     onUpdateRecord: (path: string[], recordId: string, field: string, value: any) => void;
//     onAddChildTable: (path: string[], recordId: string, headerName: string) => void;
//     onRemoveTable: (path: string[]) => void;
// }) {
//     const currentPath = [...parentPath, table.id];

//     useEffect(() => {
//         if (table.records.length === 0) {
//             onRemoveTable(currentPath);
//         }
//     }, [table.records.length, currentPath, onRemoveTable]);

//     if (table.records.length === 0) {
//         return null;
//     }

//     return (
//         <div className={`${level > 0 ? 'ml-8 mt-4' : ''}`}>
//             <div className="bg-white rounded-lg shadow">
//                 <TableHeader
//                     rootNumber={tableIndex + 1}
//                     level={level}
//                     childNumber={childNumber}
//                     headerName={table.headerName}
//                 />
//                 {table.records.map((record, index) => (
//                     <React.Fragment key={record.id}>
//                         <RecordRow
//                             record={record}
//                             onRemove={(recordId) => onRemoveRecord(currentPath, recordId)}
//                             onUpdate={(recordId, field, value) => onUpdateRecord(currentPath, recordId, field, value)}
//                             onAdd={(recordId) => onAddRecord(currentPath, recordId)}
//                             onAddChild={(headerName) => onAddChildTable(currentPath, record.id, headerName)}
//                             onAddRecord={() => onAddRecord(currentPath)}
//                             isLastRecord={index === table.records.length - 1}
//                             isOnlyRecord={table.records.length === 1}
//                         />
//                     </React.Fragment>
//                 ))}
//             </div>
//             {table.children.map((childTable, index) => (
//                 <TableComponent
//                     key={childTable.id}
//                     table={childTable}
//                     level={level + 1}
//                     tableIndex={index}
//                     childNumber={index + 1}
//                     parentPath={currentPath}
//                     onAddRecord={onAddRecord}
//                     onRemoveRecord={onRemoveRecord}
//                     onUpdateRecord={onUpdateRecord}
//                     onAddChildTable={onAddChildTable}
//                     onRemoveTable={onRemoveTable}
//                 />
//             ))}
//         </div>
//     );
// }

// function JsonFormatter({ rootTables }: { rootTables: RootTable[] }) {
//     const convertToJson = (tables: RootTable[]): any => {
//         return tables.map(table => {
//             const result: Record<string, any> = {};

//             table.records.forEach(record => {
//                 if (record.field && record.type !== 'CHILD') {
//                     let value = record.value;
//                     switch (record.type) {
//                         case 'Decimal':
//                         case 'Percentage':
//                             value = parseFloat(record.value) || 0;
//                             break;
//                         case 'Date':
//                             value = record.value || null;
//                             break;
//                         default:
//                             value = record.value || '';
//                     }
//                     result[record.field] = value;
//                 }
//             });

//             if (table.children.length > 0) {
//                 const childrenJson = convertToJson(table.children);
//                 childrenJson.forEach(childObj => {
//                     Object.assign(result, childObj);
//                 });
//             }

//             if (table.headerName) {
//                 return { [table.headerName]: result };
//             }

//             return result;
//         });
//     };

//     const jsonData = convertToJson(rootTables);

//     return (
//         <div className="bg-white rounded-lg shadow p-4">
//             <h2 className="text-lg font-semibold mb-4">JSON Output</h2>
//             <pre className="bg-gray-50 p-4 rounded overflow-auto max-h-[calc(100vh-200px)] text-sm">
//                 {JSON.stringify(jsonData, null, 2)}
//             </pre>
//         </div>
//     );
// }

// function SavedDataViewer({ data }: { data: Array<{ id: number; jsonString: string; }> }) {


//     return (
//         <div className="bg-white rounded-lg shadow p-4 mt-4">
//             <h2 className="text-lg font-semibold mb-4">Saved Data History</h2>
//             <div className="space-y-4">
//                 {data.map((item) => (
//                     <div key={item.id} className="border rounded-lg p-4">
//                         <pre className="bg-gray-50 p-3 rounded text-sm overflow-auto max-h-[200px]">
//                             {item.jsonString}
//                         </pre>
//                     </div>
//                 ))}
//             </div>
//         </div>
//     );
// }

// function App() {
//     const [rootTables, setRootTables] = useState<RootTable[]>([]);
//     const [savedData, setSavedData] = useState<Array<{ id: number; jsonString: string;}>>([]);
//     const [isSaving, setIsSaving] = useState(false);
//     const [isLoading, setIsLoading] = useState(true);

//     useEffect(() => {
//         fetchSavedData();
//     }, []);

//     const fetchSavedData = async () => {
//         try {
//             const response = await axios.get('https://localhost:44313/api/Taxdata');
//             setSavedData(response.data);
//         } catch (error) {
//             console.error('Error fetching data:', error);
//         } finally {
//             setIsLoading(false);
//         }
//     };

//     const handleSave = async () => {
//         try {
//             setIsSaving(true);
//             const combinedJson = {};
//             const jsonData = convertToJson(rootTables);

//             // Combine all root tables into a single object
//             jsonData.forEach((tableData: any) => {
//                 Object.assign(combinedJson, tableData);
//             });

//             const response = await axios.post('https://localhost:44313/api/Taxdata', {
//                 jsonString: JSON.stringify(combinedJson)
//             });

//             if (response.data) {
//                 setSavedData([response.data, ...savedData]);
//                 alert('Data saved successfully!');
//             }
//         } catch (error) {
//             console.error('Error saving data:', error);
//             alert('Failed to save data');
//         } finally {
//             setIsSaving(false);
//         }
//     };

//     const handleAddRoot = () => {
//         const newRoot: RootTable = {
//             id: Math.random().toString(36).substr(2, 9),
//             records: [{
//                 id: Math.random().toString(36).substr(2, 9),
//                 field: '',
//                 type: '',
//                 value: ''
//             }],
//             children: []
//         };
//         setRootTables(prevTables => [...prevTables, newRoot]);
//     };

//     const findAndUpdateTable = (tables: RootTable[], path: string[], updater: (table: RootTable) => RootTable): RootTable[] => {
//         if (path.length === 0) return tables;

//         const [currentId, ...remainingPath] = path;
//         return tables.map(table => {
//             if (table.id === currentId) {
//                 if (remainingPath.length === 0) {
//                     return updater(table);
//                 }
//                 return {
//                     ...table,
//                     children: findAndUpdateTable(table.children, remainingPath, updater)
//                 };
//             }
//             return table;
//         });
//     };

//     const addRecord = (path: string[], recordId?: string) => {
//         setRootTables(prevTables =>
//             findAndUpdateTable(prevTables, path, (table) => {
//                 const newRecord = {
//                     id: Math.random().toString(36).substr(2, 9),
//                     field: '',
//                     type: '',
//                     value: ''
//                 };

//                 if (recordId) {
//                     const index = table.records.findIndex(r => r.id === recordId);
//                     const newRecords = [...table.records];
//                     newRecords.splice(index, 0, newRecord);
//                     return {
//                         ...table,
//                         records: newRecords
//                     };
//                 }

//                 return {
//                     ...table,
//                     records: [...table.records, newRecord]
//                 };
//             })
//         );
//     };

//     const removeRecord = (path: string[], recordId: string) => {
//         setRootTables(prevTables =>
//             findAndUpdateTable(prevTables, path, (table) => ({
//                 ...table,
//                 records: table.records.filter(record => record.id !== recordId)
//             }))
//         );
//     };

//     const updateRecord = (path: string[], recordId: string, field: string, value: any) => {
//         setRootTables(prevTables =>
//             findAndUpdateTable(prevTables, path, (table) => ({
//                 ...table,
//                 records: table.records.map(record =>
//                     record.id === recordId ? { ...record, [field]: value } : record
//                 )
//             }))
//         );
//     };

//     const addChildTable = (path: string[], recordId: string, headerName: string) => {
//         setRootTables(prevTables =>
//             findAndUpdateTable(prevTables, path, (table) => {
//                 const parentRecord = table.records.find(r => r.id === recordId);
//                 if (!parentRecord || !parentRecord.field.trim()) {
//                     return table;
//                 }

//                 return {
//                     ...table,
//                     children: [...table.children, {
//                         id: Math.random().toString(36).substr(2, 9),
//                         headerName: headerName,
//                         records: [{
//                             id: Math.random().toString(36).substr(2, 9),
//                             field: '',
//                             type: '',
//                             value: ''
//                         }],
//                         children: []
//                     }]
//                 };
//             })
//         );
//     };

//     const removeTable = (path: string[]) => {
//         if (path.length === 1) {
//             setRootTables(prevTables => prevTables.filter(table => table.id !== path[0]));
//             return;
//         }

//         const parentPath = path.slice(0, -1);
//         const tableId = path[path.length - 1];

//         setRootTables(prevTables =>
//             findAndUpdateTable(prevTables, parentPath, (table) => ({
//                 ...table,
//                 children: table.children.filter(child => child.id !== tableId)
//             }))
//         );
//     };

//     const convertToJson = (tables: RootTable[]): any => {
//         return tables.map(table => {
//             const result: Record<string, any> = {};

//             table.records.forEach(record => {
//                 if (record.field && record.type !== 'CHILD') {
//                     let value = record.value;
//                     switch (record.type) {
//                         case 'Decimal':
//                         case 'Percentage':
//                             value = parseFloat(record.value) || 0;
//                             break;
//                         case 'Date':
//                             value = record.value || null;
//                             break;
//                         default:
//                             value = record.value || '';
//                     }
//                     result[record.field] = value;
//                 }
//             });

//             if (table.children.length > 0) {
//                 const childrenJson = convertToJson(table.children);
//                 childrenJson.forEach(childObj => {
//                     Object.assign(result, childObj);
//                 });
//             }

//             if (table.headerName) {
//                 return { [table.headerName]: result };
//             }

//             return result;
//         });
//     };

//     return (
//         <div className="min-h-screen bg-gray-50 p-8">
//             <div className="grid grid-cols-[1fr,400px] gap-8 max-w-[1400px] mx-auto">
//                 <div>
//                     <div className="bg-white rounded-lg shadow mb-4 p-4 flex justify-between items-center">
//                         <button
//                             onClick={handleAddRoot}
//                             className="flex items-center space-x-1 px-3 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
//                         >
//                             <Plus className="w-4 h-4" />
//                             <span>Add ROOT</span>
//                         </button>
//                         <button
//                             onClick={handleSave}
//                             disabled={isSaving || rootTables.length === 0}
//                             className={`flex items-center space-x-2 px-4 py-2 rounded-md ${isSaving || rootTables.length === 0
//                                 ? 'bg-gray-300 cursor-not-allowed'
//                                 : 'bg-green-500 hover:bg-green-600 text-white'
//                             }`}
//                         >
//                             <Save className="w-4 h-4" />
//                             <span>{isSaving ? 'Saving...' : 'Save All'}</span>
//                         </button>
//                     </div>

//                     <div className="space-y-4">
//                         {rootTables.map((root, index) => (
//                             <TableComponent
//                                 key={root.id}
//                                 table={root}
//                                 tableIndex={index}
//                                 onAddRecord={addRecord}
//                                 onRemoveRecord={removeRecord}
//                                 onUpdateRecord={updateRecord}
//                                 onAddChildTable={addChildTable}
//                                 onRemoveTable={removeTable}
//                             />
//                         ))}
//                     </div>
//                 </div>

//                 <div className="sticky top-8 space-y-4">
//                     <JsonFormatter rootTables={rootTables} />
//                     {isLoading ? (
//                         <div className="bg-white rounded-lg shadow p-4">
//                             <p className="text-gray-500">Loading saved data...</p>
//                         </div>
//                     ) : (
//                         <SavedDataViewer data={savedData} />
//                     )}
//                 </div>
//             </div>
//         </div>
//     );
// }

// export default App;
