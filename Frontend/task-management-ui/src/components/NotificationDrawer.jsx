import React from 'react';
import { Bell, Check, CheckCheck, X, Calendar } from 'lucide-react';

const NotificationDrawer = ({ notifications, onClose, onMarkAsRead, onMarkAllAsRead }) => {
  return (
    <div className="glass-panel" style={{
      position: 'absolute',
      top: '60px',
      right: '20px',
      width: '360px',
      maxHeight: '480px',
      overflowY: 'auto',
      zIndex: 999,
      padding: '16px',
      boxShadow: 'var(--shadow-lg)'
    }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '12px', borderBottom: '1px solid var(--border-color)', paddingBottom: '10px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', fontWeight: '700' }}>
          <Bell size={18} color="var(--primary)" /> Notifications
        </div>
        <div style={{ display: 'flex', gap: '8px' }}>
          <button className="btn btn-secondary btn-sm" onClick={onMarkAllAsRead} title="Mark all as read">
            <CheckCheck size={14} /> Read All
          </button>
          <button className="btn btn-secondary btn-sm" onClick={onClose}>
            <X size={14} />
          </button>
        </div>
      </div>

      {notifications.length === 0 ? (
        <div style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '24px 0', fontSize: '0.9rem' }}>
          No notifications yet.
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
          {notifications.map((n) => (
            <div
              key={n.id}
              style={{
                background: n.isRead ? 'rgba(255, 255, 255, 0.03)' : 'rgba(99, 102, 241, 0.1)',
                border: n.isRead ? '1px solid rgba(255, 255, 255, 0.05)' : '1px solid var(--primary-glow)',
                borderRadius: 'var(--radius-sm)',
                padding: '10px 12px',
                fontSize: '0.85rem',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'flex-start',
                gap: '8px'
              }}
            >
              <div>
                <div style={{ color: n.isRead ? 'var(--text-secondary)' : 'var(--text-primary)', fontWeight: n.isRead ? '400' : '600', marginBottom: '4px' }}>
                  {n.message}
                </div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', display: 'flex', alignItems: 'center', gap: '4px' }}>
                  <Calendar size={12} /> {new Date(n.createdAt).toLocaleDateString()} {new Date(n.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </div>
              </div>
              {!n.isRead && (
                <button
                  onClick={() => onMarkAsRead(n.id)}
                  style={{ background: 'none', border: 'none', color: 'var(--primary)', cursor: 'pointer', padding: '4px' }}
                  title="Mark as read"
                >
                  <Check size={16} />
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default NotificationDrawer;
