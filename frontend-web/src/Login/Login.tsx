import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { apiFetch } from '../api';

export default function Login() {
  const { t } = useTranslation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');
  const navigate = useNavigate();

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const data = { Email: email, Password: password };
    apiFetch('/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    })
      .then(res => {
        if (res.ok) return res.json();
        setMessage(res.status === 400 ? t('login.invalidCredentials') : t('login.failed'));
      })
      .then(data => {
        if (data?.token) {
          localStorage.setItem('token', data.token);
          navigate('/', { state: { toast: t('common.loggedIn') } });
        }
      })
      .catch(() => setMessage(t('login.failed')));
  }

  return (
    <div className="login-page">
      <h2>{t('login.title')}</h2>
      <form className="login-form" onSubmit={handleSubmit}>
        <label htmlFor="email">{t('login.email')}</label>
        <input id="email" name="email" type="email" placeholder={t('login.emailPlaceholder')} value={email} onChange={e => setEmail(e.target.value)} />
        <label htmlFor="password">{t('login.password')}</label>
        <input id="password" name="password" type="password" placeholder={t('login.passwordPlaceholder')} value={password} onChange={e => setPassword(e.target.value)} />
        <button type="submit">{t('login.submit')}</button>
      </form>
      {message && <div className="submit-message">{message}</div>}
    </div>
  );
}
