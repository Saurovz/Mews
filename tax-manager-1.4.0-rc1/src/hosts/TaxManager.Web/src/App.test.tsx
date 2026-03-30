import { render, screen } from '@testing-library/react';
import App from './App';

describe('app component', () => {
    it('renders Mews Tax Manager', () => {
        render(<App />);
        const element = screen.getByText(/Mews Tax Manager/i);
        expect(element).toBeInTheDocument();
    });
});
