import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { apiFetch } from '../api';

export default function Feedback() {
  const { t } = useTranslation();
  const [message, setMessage] = useState('');
  const [status, setStatus] = useState('');
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const token = localStorage.getItem('token');
    if (!token) { setStatus(t('feedback.loginRequired')); return; }
    setSubmitting(true);
    setStatus('');
    try {
      const res = await apiFetch('/feedback', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
        body: JSON.stringify({ Message: message }),
      });
      if (res.ok) { setStatus(t('feedback.thanks')); setMessage(''); }
      else if (res.status === 401) setStatus(t('feedback.loginRequired'));
      else setStatus(t('feedback.failed'));
    } catch {
      setStatus(t('feedback.failed'));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="feedback-page">
      <h2>{t('feedback.title')}</h2>
      <p className="section-hint">{t('feedback.subtitle')}</p>
      <form className="feedback-form" onSubmit={handleSubmit}>
        <textarea
          className="feedback-input"
          rows={6}
          placeholder={t('feedback.placeholder')}
          value={message}
          maxLength={2000}
          onChange={e => { setMessage(e.target.value); setStatus(''); }}
        />
        <button type="submit" disabled={submitting || message.trim().length === 0}>
          {submitting ? t('feedback.sending') : t('feedback.submit')}
        </button>
      </form>
      {status && <div className="submit-message">{status}</div>}
    </div>
  );
}
