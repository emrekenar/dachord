import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import Register from './Register';
import * as api from '../api';

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom');
  return { ...actual, useNavigate: () => mockNavigate };
});

describe('Register', () => {
  beforeEach(() => {
    mockNavigate.mockClear();
    localStorage.clear();
  });

  it('renders email and password fields', () => {
    render(<MemoryRouter><Register /></MemoryRouter>);
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
  });

  it('logs in and redirects to / after successful registration', async () => {
    vi.spyOn(api, 'apiFetch')
      .mockResolvedValueOnce(new Response(null, { status: 200 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ token: 'tok456' }), { status: 200 }));

    render(<MemoryRouter><Register /></MemoryRouter>);
    await userEvent.type(screen.getByLabelText(/email/i), 'new@user.com');
    await userEvent.type(screen.getByLabelText(/password/i), 'pass');
    await userEvent.click(screen.getByRole('button', { name: /register/i }));

    await waitFor(() => {
      expect(localStorage.getItem('token')).toBe('tok456');
      expect(mockNavigate).toHaveBeenCalledWith('/', { state: { toast: 'Registered successfully!' } });
    });
  });

  it('shows error when email already in use', async () => {
    vi.spyOn(api, 'apiFetch').mockResolvedValue(new Response(null, { status: 400 }));

    render(<MemoryRouter><Register /></MemoryRouter>);
    await userEvent.type(screen.getByLabelText(/email/i), 'dup@user.com');
    await userEvent.type(screen.getByLabelText(/password/i), 'pass');
    await userEvent.click(screen.getByRole('button', { name: /register/i }));

    await waitFor(() => {
      expect(screen.getByText(/email already in use/i)).toBeInTheDocument();
    });
    expect(mockNavigate).not.toHaveBeenCalled();
  });

  it('shows error on network failure', async () => {
    vi.spyOn(api, 'apiFetch').mockRejectedValue(new Error('Network error'));

    render(<MemoryRouter><Register /></MemoryRouter>);
    await userEvent.click(screen.getByRole('button', { name: /register/i }));

    await waitFor(() => {
      expect(screen.getByText(/failed to register/i)).toBeInTheDocument();
    });
  });
});
