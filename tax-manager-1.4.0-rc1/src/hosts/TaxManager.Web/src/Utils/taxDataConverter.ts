import { TreeNodeType, TaxData } from '../types';

// Convert Tax Data array to tree structure
export function convertTaxDataToTree(taxData: TaxData[]): TreeNodeType[] {
    return taxData.map((tax, index) => {
        const taxNode: TreeNodeType = {
            id: `tax-${index}`,
            name: tax.taxCode,
            children: []
        };

        // Add all properties as children
        Object.entries(tax).forEach(([key, value]) => {
            if (key === 'periods' && Array.isArray(value)) {
                // Handle periods array specially
                const periodsNode: TreeNodeType = {
                    id: `tax-${index}-property-${key}`,
                    name: key,
                    children: value.map((period, periodIndex) => ({
                        id: `period-${index}-${periodIndex}`,
                        name: `Period ${periodIndex + 1}`,
                        children: Object.entries(period).map(([periodKey, periodValue]) => ({
                            id: `tax-${index}-property-periods-${periodIndex}-${periodKey}-value`,
                            name: periodValue.toString(),
                            children: []
                        }))
                    }))
                };
                taxNode.children.push(periodsNode);
            } else {
                // Regular property
                const propertyNode: TreeNodeType = {
                    id: `tax-${index}-property-${key}`,
                    name: key,
                    children: [{
                        id: `tax-${index}-property-${key}-value`,
                        name: value.toString(),
                        children: []
                    }]
                };
                taxNode.children.push(propertyNode);
            }
        });

        return taxNode;
    });
}

// Convert tree structure back to tax data array
export function convertTreeToTaxData(treeData: TreeNodeType[]): TaxData[] {
    const result: TaxData[] = [];

    treeData.forEach(taxNode => {
        if (!taxNode.id.startsWith('tax-')) return;

        const taxData: any = {
            taxCode: taxNode.name
        };

        // Process all property nodes
        taxNode.children.forEach(propertyNode => {
            const propertyName = propertyNode.name;

            if (propertyName === 'periods') {
                // Handle periods specially
                taxData.periods = propertyNode.children.map(periodNode => {
                    const period: any = {};

                    periodNode.children.forEach(periodPropertyNode => {
                        const parts = periodPropertyNode.id.split('-');
                        const propertyName = parts[parts.length - 2];
                        const value = periodPropertyNode.name;

                        // Convert values to appropriate types
                        if (propertyName === 'rate') {
                            period[propertyName] = parseFloat(value);
                        } else {
                            period[propertyName] = value;
                        }
                    });

                    return period;
                });
            } else if (propertyNode.children.length > 0) {
                // Regular property with a value
                const value = propertyNode.children[0].name;

                // Convert values to appropriate types
                if (propertyName === 'rate') {
                    taxData[propertyName] = parseFloat(value);
                } else if (propertyName === 'perPerson') {
                    taxData[propertyName] = value.toLowerCase() === 'true';
                } else {
                    taxData[propertyName] = value;
                }
            }
        });

        result.push(taxData);
    });

    return result;
}
