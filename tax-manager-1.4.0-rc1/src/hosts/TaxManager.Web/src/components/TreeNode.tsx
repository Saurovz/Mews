import React, { useState, useRef, useEffect } from 'react';
import { ChevronDown, ChevronRight, Plus, Trash2 } from 'lucide-react';
import { TreeNodeType } from '../types';

interface TreeNodeProps {
    node: TreeNodeType;
    onAdd: (parentId: string) => void;
    onDelete: (nodeId: string) => void;
    onRename: (nodeId: string, newName: string) => void;
}

const TreeNode: React.FC<TreeNodeProps> = ({ node, onAdd, onDelete, onRename }) => {
    const [isExpanded, setIsExpanded] = useState(true);
    const [isEditing, setIsEditing] = useState(false);
    const [editValue, setEditValue] = useState(node.name);
    const inputRef = useRef<HTMLInputElement>(null);

    const hasChildren = node.children && node.children.length > 0;

    // Determine if this node is a property name (not editable) or a value (editable)
    const isPropertyName = node.id.includes('property-') && !node.id.includes('-value');
    const isPeriodNode = node.id.startsWith('period-');
    const isValueNode = node.id.includes('-value') || node.id.startsWith('tax-');
    const canAddChildren = node.id.startsWith('tax-') ||
        (node.id.includes('property-periods') && !node.id.includes('-value'));

    // Get the value node if this is a property name node
    const getValueNode = () => {
        if (isPropertyName && node.children.length > 0) {
            return node.children[0];
        }
        return null;
    };

    const valueNode = getValueNode();

    // Determine the data type of a value
    const getValueType = (value: string): string => {
        if (value.toLowerCase() === 'true' || value.toLowerCase() === 'false') {
            return 'boolean';
        }
        if (!isNaN(Number(value)) && value.includes('.')) {
            return 'double';
        }
        if (!isNaN(Number(value))) {
            return 'number';
        }
        if (value.match(/^\d{4}-\d{2}-\d{2}$/)) {
            return 'date';
        }
        return 'string';
    };

    const toggleExpand = () => {
        setIsExpanded(!isExpanded);
    };

    const startEditing = (e: React.MouseEvent, targetNode: TreeNodeType) => {
        e.stopPropagation();
        setIsEditing(true);
        setEditValue(targetNode.name);
    };

    const handleRename = () => {
        if (editValue.trim() !== '') {
            onRename(node.id, editValue.trim());
        } else {
            setEditValue(node.name); // Reset to original if empty
        }
        setIsEditing(false);
    };

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter') {
            handleRename();
        } else if (e.key === 'Escape') {
            setEditValue(node.name);
            setIsEditing(false);
        }
    };

    // Focus input when editing starts
    useEffect(() => {
        if (isEditing && inputRef.current) {
            inputRef.current.focus();
            inputRef.current.select();
        }
    }, [isEditing]);

    // Get appropriate label based on node type
    const getNodeLabel = () => {
        if (node.id.startsWith('tax-') && !node.id.includes('property')) {
            return 'Tax Entry';
        }
        if (isPeriodNode) {
            return 'Period';
        }
        return node.name;
    };

    // Render a property row with name, type, and value columns
    if (isPropertyName && valueNode) {
        return (
            <div className="select-none">
                <div className="flex items-center py-1">
                    <div
                        className="w-6 h-6 flex items-center justify-center cursor-pointer text-gray-500 hover:text-gray-700"
                        onClick={toggleExpand}
                    >
                        {hasChildren && (
                            isExpanded ? <ChevronDown size={18} /> : <ChevronRight size={18} />
                        )}
                    </div>

                    <div className="flex-grow grid grid-cols-3 gap-4">
                        {/* Property Name Column */}
                        <div className="font-medium text-blue-600 px-2 py-1">
                            {node.name}
                        </div>

                        {/* Type Column */}
                        <div className="font-medium text-purple-600 px-2 py-1">
                            {getValueType(valueNode.name)}
                        </div>

                        {/* Property Value Column - Directly Editable */}
                        <div className="font-medium text-gray-700">
                            {isEditing ? (
                                <input
                                    ref={inputRef}
                                    type="text"
                                    value={editValue}
                                    onChange={(e) => setEditValue(e.target.value)}
                                    onBlur={handleRename}
                                    onKeyDown={handleKeyDown}
                                    className="w-full px-2 py-1 border border-blue-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                                    autoComplete="off"
                                />
                            ) : (
                                <div
                                    className="px-2 py-1 rounded hover:bg-gray-100 cursor-pointer border border-transparent hover:border-gray-200"
                                    onClick={(e) => startEditing(e, valueNode)}
                                >
                                    {valueNode.name}
                                </div>
                            )}
                        </div>
                    </div>

                    <div className="flex items-center gap-1 ml-2">
                        {canAddChildren && (
                            <button
                                onClick={() => onAdd(node.id)}
                                className="p-1 text-blue-500 hover:text-blue-700 hover:bg-blue-50 rounded"
                                title={node.id.includes('periods') ? "Add period" : "Add property"}
                            >
                                <Plus size={16} />
                            </button>
                        )}

                        <button
                            onClick={() => onDelete(node.id)}
                            className="p-1 text-red-500 hover:text-red-700 hover:bg-red-50 rounded"
                            title="Delete"
                        >
                            <Trash2 size={16} />
                        </button>
                    </div>
                </div>

                {isExpanded && hasChildren && valueNode.children.length > 0 && (
                    <div className="pl-6 border-l border-gray-200 ml-3">
                        {valueNode.children.map(childNode => (
                            <TreeNode
                                key={childNode.id}
                                node={childNode}
                                onAdd={onAdd}
                                onDelete={onDelete}
                                onRename={onRename}
                            />
                        ))}
                    </div>
                )}
            </div>
        );
    }

    // Standard node rendering (for tax entries, periods, etc.)
    return (
        <div className="select-none">
            <div className="flex items-center py-1">
                <div
                    className="w-6 h-6 flex items-center justify-center cursor-pointer text-gray-500 hover:text-gray-700"
                    onClick={toggleExpand}
                >
                    {hasChildren && (
                        isExpanded ? <ChevronDown size={18} /> : <ChevronRight size={18} />
                    )}
                </div>

                <div className={`flex-grow font-medium ml-1 ${isPropertyName ? 'text-blue-600' : 'text-gray-700'}`}>
                    {isEditing ? (
                        <input
                            ref={inputRef}
                            type="text"
                            value={editValue}
                            onChange={(e) => setEditValue(e.target.value)}
                            onBlur={handleRename}
                            onKeyDown={handleKeyDown}
                            className="w-full px-2 py-1 border border-blue-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                            autoComplete="off"
                        />
                    ) : (
                        <div
                            className={`px-2 py-1 rounded ${isValueNode ? 'hover:bg-gray-100 cursor-pointer' : ''}`}
                            onClick={isValueNode ? (e) => startEditing(e, node) : undefined}
                        >
                            {getNodeLabel()}
                        </div>
                    )}
                </div>

                <div className="flex items-center gap-1">
                    {canAddChildren && (
                        <button
                            onClick={() => onAdd(node.id)}
                            className="p-1 text-blue-500 hover:text-blue-700 hover:bg-blue-50 rounded"
                            title={node.id.includes('periods') ? "Add period" : "Add property"}
                        >
                            <Plus size={16} />
                        </button>
                    )}

                    <button
                        onClick={() => onDelete(node.id)}
                        className="p-1 text-red-500 hover:text-red-700 hover:bg-red-50 rounded"
                        title="Delete"
                    >
                        <Trash2 size={16} />
                    </button>
                </div>
            </div>

            {isExpanded && hasChildren && (
                <div className="pl-6 border-l border-gray-200 ml-3">
                    {node.children.map(childNode => (
                        <TreeNode
                            key={childNode.id}
                            node={childNode}
                            onAdd={onAdd}
                            onDelete={onDelete}
                            onRename={onRename}
                        />
                    ))}
                </div>
            )}
        </div>
    );
};

export default TreeNode;