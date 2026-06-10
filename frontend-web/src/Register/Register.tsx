import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { apiFetch } from '../api';

export default function Register() {
  const { t } = useTranslation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');
  const navigate = useNavigate();

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const data = { Email: email, Password: password };
    apiFetch('/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    })
      .then(res => {
        if (res.ok) {
          return apiFetch('/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
          }).then(r => r.json()).then(d => {
            if (d?.token) {
              localStorage.setItem('token', d.token);
              navigate('/', { state: { toast: t('register.success') } });
            }
          });
        }
        setMessage(res.status === 400 ? t('register.emailInUse') : t('register.failed'));
      })
      .catch(() => setMessage(t('register.failed')));
  }

  return (
    <div className="register-page">
      <h2>{t('register.title')}</h2>
      <form className="register-form" onSubmit={handleSubmit}>
        <label htmlFor="email">{t('register.email')}</label>
        <input id="email" name="email" type="email" placeholder={t('register.emailPlaceholder')} value={email} onChange={e => setEmail(e.target.value)} />
        <label htmlFor="password">{t('register.password')}</label>
        <input id="password" name="password" type="password" placeholder={t('register.passwordPlaceholder')} value={password} onChange={e => setPassword(e.target.value)} />
        <button type="submit">{t('register.submit')}</button>
      </form>
      {message && <div className="submit-message">{message}</div>}
    </div>
  );
}
