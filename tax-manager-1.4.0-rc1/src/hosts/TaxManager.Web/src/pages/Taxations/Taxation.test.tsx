import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest'
import { toast } from 'react-toastify';
import TaxationCreate from './TaxationCreate';
import {createTaxation} from "../../services/TaxationService.ts";

describe('TaxationCreate', () => {
    afterEach(() => {
        vi.clearAllMocks();
    });
    
    vi.mock('react-router-dom', () => ({
        ...vi.importActual('react-router-dom'),
        useNavigate: () => vi.fn(),
    }));

    vi.mock('react-toastify', () => ({
        toast: {
            success: vi.fn(),
            error: vi.fn(),
            info: vi.fn(),
            warning: vi.fn(),
        },
    }));
    
    vi.mock('../../services/TaxationService', () => ({
        getCountries: vi.fn(() => [
            {
                'code': 'AL',
                'name': 'Albania'
            },
            {
                'code': 'DZ',
                'name': 'Algeria'
            },
        ]),
        createTaxation: vi.fn(),
    }));

    it('renders error on submit when code is empty', async () => {
        render(<TaxationCreate />);
        await userEvent.click(screen.getByText('Save'));
        const errorMessage = screen.getByText(/Code is required./i);
        expect(errorMessage).toBeInTheDocument();
    });

    it('renders error on submit when name is empty', async () => {
        render(<TaxationCreate />);
        await userEvent.click(screen.getByText('Save'));
        const errorMessage = screen.getByText(/Name is required./i);
        expect(errorMessage).toBeInTheDocument();
    });

    it('renders error on submit when country is not selected', async () => {
        render(<TaxationCreate />);
        await userEvent.click(screen.getByText('Save'));
        const errorMessage = screen.getByText(/Country is required./i);
        expect(errorMessage).toBeInTheDocument();
    });

    it('renders error message when code is invalid', async () => {
        render(<TaxationCreate />);
        await userEvent.type(screen.getByPlaceholderText('Enter Code'),'Tax?');
        const errorMessage = screen.getByText(/Only alphanumeric characters and hyphens are allowed. Cannot begin with a hyphen./i);
        expect(errorMessage).toBeInTheDocument();
    });

    it('renders error message when name is invalid', async () => {
        render(<TaxationCreate />);
        await userEvent.type(screen.getByPlaceholderText('Enter Name'),'Tax?');
        const errorMessage = screen.getByText(/Name can only contain letters, numbers, spaces, and hyphens./i);
        expect(errorMessage).toBeInTheDocument();
    });

    it('renders error on submit when code is not unique', async () => {
        vi.mocked(createTaxation).mockRejectedValueOnce('error');
        render(<TaxationCreate />);
        
        await userEvent.type(screen.getByPlaceholderText('Enter Code'),'CA-ON');
        await userEvent.type(screen.getByPlaceholderText('Enter Name'),'Test');
        await userEvent.selectOptions(screen.getByRole('combobox'),
            screen.getByRole('option', { name: 'Algeria' }),);
        await userEvent.click(screen.getByText('Save'));
        expect(toast.error).toHaveBeenCalledWith('Failed to create taxation.');
    });

    it('renders success toast on submit when code is unique', async () => {
        vi.mocked(createTaxation).mockResolvedValueOnce({'code': 'CA-ON1', 'name': 'Test', 'country': 'DZ'});
        render(<TaxationCreate />);

        await userEvent.type(screen.getByPlaceholderText('Enter Code'),'CA-ON1');
        await userEvent.type(screen.getByPlaceholderText('Enter Name'),'Test');
        await userEvent.selectOptions(screen.getByRole('combobox'),
            screen.getByRole('option', { name: 'Algeria' }),);
        await userEvent.click(screen.getByText('Save'));
        expect(toast.success).toHaveBeenCalledWith('Taxation created successfully!'); 
    });
});
