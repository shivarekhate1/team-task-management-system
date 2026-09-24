import React, { useState, useEffect } from 'react';
import axiosClient from '../api/axiosClient';
import { useAuth } from '../context/AuthContext';
import { LayoutDashboard, CheckSquare, Clock, AlertTriangle, CheckCircle, Flame, Users, Calendar, Filter } from 'lucide-react';

const DashboardPage = () => {
  const { user } = useAuth();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Filters
  const [statusFilter, setStatusFilter] = useState('');
  const [priorityFilter, setPriorityFilter] = useState('');

  const fetchDashboardData = async () => {
    setLoading(true);
    try {
      const res = await axiosClient.get('/dashboard/overview');
      setData(res.data);
      setError(null);
    } catch (err) {
      console.error('Error loading dashboard:', err);
      setError('Failed to load dashboard data from backend API.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardData();
  }, []);

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '60px 0', color: 'var(--text-secondary)' }}>
        Loading dashboard metrics...
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="glass-panel" style={{ padding: '24px', textAlign: 'center', color: '#fca5a5' }}>
        {error || 'Unable to display dashboard overview.'}
        <br />
        <button className="btn btn-secondary btn-sm" onClick={fetchDashboardData} style={{ marginTop: '12px' }}>
          Retry Loading
        </button>
      </div>
    );
  }

  const { totalTasks, toDoCount, inProgressCount, doneCount, overdueCount, tasksByPriority, perUserSummaries, recentNotifications } = data;

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '24px' }}>
      {/* Title & Quick Info */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1 style={{ fontSize: '1.75rem', fontWeight: '800', letterSpacing: '-0.5px' }}>
            System Overview
          </h1>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
            Welcome back, <strong style={{ color: 'var(--text-primary)' }}>{user?.fullName}</strong> ({user?.role}). Here is your live task activity.
          </p>
        </div>
      </div>

      {/* Top 4 Summary Cards */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '16px' }}>
        <div className="glass-panel" style={{ padding: '20px', display: 'flex', alignItems: 'center', gap: '16px' }}>
          <div style={{ background: 'rgba(99, 102, 241, 0.15)', padding: '14px', borderRadius: 'var(--radius-md)', color: 'var(--primary)' }}>
            <CheckSquare size={26} />
          </div>
          <div>
            <div style={{ fontSize: '0.8rem', fontWeight: '700', color: 'var(--text-muted)', textTransform: 'uppercase' }}>Total Tasks</div>
            <div style={{ fontSize: '1.8rem', fontWeight: '800' }}>{totalTasks}</div>
          </div>
        </div>

        <div className="glass-panel" style={{ padding: '20px', display: 'flex', alignItems: 'center', gap: '16px' }}>
          <div style={{ background: 'rgba(100, 116, 139, 0.2)', padding: '14px', borderRadius: 'var(--radius-md)', color: '#cbd5e1' }}>
            <Clock size={26} />
          </div>
          <div>
            <div style={{ fontSize: '0.8rem', fontWeight: '700', color: 'var(--text-muted)', textTransform: 'uppercase' }}>To Do</div>
            <div style={{ fontSize: '1.8rem', fontWeight: '800', color: '#cbd5e1' }}>{toDoCount}</div>
          </div>
        </div>

        <div className="glass-panel" style={{ padding: '20px', display: 'flex', alignItems: 'center', gap: '16px' }}>
          <div style={{ background: 'rgba(59, 130, 246, 0.15)', padding: '14px', borderRadius: 'var(--radius-md)', color: '#60a5fa' }}>
            <Flame size={26} />
          </div>
          <div>
            <div style={{ fontSize: '0.8rem', fontWeight: '700', color: 'var(--text-muted)', textTransform: 'uppercase' }}>In Progress</div>
            <div style={{ fontSize: '1.8rem', fontWeight: '800', color: '#60a5fa' }}>{inProgressCount}</div>
          </div>
        </div>

        <div className="glass-panel" style={{ padding: '20px', display: 'flex', alignItems: 'center', gap: '16px' }}>
          <div style={{ background: 'rgba(16, 185, 129, 0.15)', padding: '14px', borderRadius: 'var(--radius-md)', color: '#34d399' }}>
            <CheckCircle size={26} />
          </div>
          <div>
            <div style={{ fontSize: '0.8rem', fontWeight: '700', color: 'var(--text-muted)', textTransform: 'uppercase' }}>Completed</div>
            <div style={{ fontSize: '1.8rem', fontWeight: '800', color: '#34d399' }}>{doneCount}</div>
          </div>
        </div>
      </div>

      {/* Overdue Warning Alert */}
      {overdueCount > 0 && (
        <div style={{
          background: 'rgba(239, 68, 68, 0.12)',
          border: '1px solid rgba(239, 68, 68, 0.3)',
          borderRadius: 'var(--radius-md)',
          padding: '16px 20px',
          display: 'flex',
          alignItems: 'center',
          gap: '12px',
          color: '#fca5a5'
        }}>
          <AlertTriangle size={22} color="var(--danger)" />
          <div>
            <strong style={{ fontSize: '0.95rem' }}>Attention: {overdueCount} task(s) are past deadline!</strong>
            <div style={{ fontSize: '0.85rem', opacity: 0.9 }}>Check the Tasks page to review or adjust deadlines.</div>
          </div>
        </div>
      )}

      {/* Grid Section: Priority & User Breakdown */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '20px' }}>
        {/* Priority Breakdown */}
        <div className="glass-panel" style={{ padding: '24px' }}>
          <h3 style={{ fontSize: '1.1rem', fontWeight: '700', marginBottom: '16px', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Filter size={18} color="var(--primary)" /> Priority Breakdown
          </h3>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '14px' }}>
            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem', marginBottom: '4px' }}>
                <span className="badge badge-urgent">Urgent</span>
                <strong>{tasksByPriority.urgent} tasks</strong>
              </div>
              <div style={{ background: 'rgba(255,255,255,0.08)', borderRadius: '4px', height: '8px', overflow: 'hidden' }}>
                <div style={{ width: `${totalTasks ? (tasksByPriority.urgent / totalTasks) * 100 : 0}%`, background: 'var(--priority-urgent)', height: '100%' }}></div>
              </div>
            </div>

            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem', marginBottom: '4px' }}>
                <span className="badge badge-high">High</span>
                <strong>{tasksByPriority.high} tasks</strong>
              </div>
              <div style={{ background: 'rgba(255,255,255,0.08)', borderRadius: '4px', height: '8px', overflow: 'hidden' }}>
                <div style={{ width: `${totalTasks ? (tasksByPriority.high / totalTasks) * 100 : 0}%`, background: 'var(--priority-high)', height: '100%' }}></div>
              </div>
            </div>

            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem', marginBottom: '4px' }}>
                <span className="badge badge-medium">Medium</span>
                <strong>{tasksByPriority.medium} tasks</strong>
              </div>
              <div style={{ background: 'rgba(255,255,255,0.08)', borderRadius: '4px', height: '8px', overflow: 'hidden' }}>
                <div style={{ width: `${totalTasks ? (tasksByPriority.medium / totalTasks) * 100 : 0}%`, background: 'var(--priority-medium)', height: '100%' }}></div>
              </div>
            </div>

            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem', marginBottom: '4px' }}>
                <span className="badge badge-low">Low</span>
                <strong>{tasksByPriority.low} tasks</strong>
              </div>
              <div style={{ background: 'rgba(255,255,255,0.08)', borderRadius: '4px', height: '8px', overflow: 'hidden' }}>
                <div style={{ width: `${totalTasks ? (tasksByPriority.low / totalTasks) * 100 : 0}%`, background: 'var(--priority-low)', height: '100%' }}></div>
              </div>
            </div>
          </div>
        </div>

        {/* Per-User Task Summaries (For Manager & Admin) */}
        <div className="glass-panel" style={{ padding: '24px' }}>
          <h3 style={{ fontSize: '1.1rem', fontWeight: '700', marginBottom: '16px', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Users size={18} color="var(--primary)" /> Team Member Progress
          </h3>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
            {perUserSummaries.length === 0 ? (
              <div style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>No user task data available.</div>
            ) : (
              perUserSummaries.map((u) => (
                <div
                  key={u.userId}
                  style={{
                    padding: '10px 14px',
                    background: 'rgba(15, 23, 42, 0.4)',
                    border: '1px solid var(--border-color)',
                    borderRadius: 'var(--radius-sm)',
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center'
                  }}
                >
                  <div>
                    <div style={{ fontWeight: '700', fontSize: '0.9rem' }}>{u.fullName}</div>
                    <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                      Total Assigned: {u.totalAssigned}
                    </div>
                  </div>
                  <div style={{ display: 'flex', gap: '6px' }}>
                    <span className="badge badge-todo">{u.toDoCount} To Do</span>
                    <span className="badge badge-inprogress">{u.inProgressCount} In Prog</span>
                    <span className="badge badge-done">{u.doneCount} Done</span>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;
