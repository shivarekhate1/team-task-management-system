import React, { useState, useEffect } from 'react';
import { X, Save, Plus } from 'lucide-react';
import axiosClient from '../api/axiosClient';

const CreateTaskModal = ({ onClose, onTaskCreated, initialData }) => {
  const [title, setTitle] = useState(initialData?.title || '');
  const [description, setDescription] = useState(initialData?.description || '');
  const [priority, setPriority] = useState(initialData?.priority || 'Medium');
  const [deadline, setDeadline] = useState(initialData?.deadline ? initialData.deadline.split('T')[0] : '');
  const [teamId, setTeamId] = useState(initialData?.teamId || '');
  const [assignedToId, setAssignedToId] = useState(initialData?.assignedToId || '');

  const [teams, setTeams] = useState([]);
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchData = async () => {
      try {
        const teamsRes = await axiosClient.get('/teams');
        setTeams(teamsRes.data);
        const usersRes = await axiosClient.get('/users');
        setUsers(usersRes.data);
      } catch (err) {
        console.error('Error loading dropdown options:', err);
      }
    };
    fetchData();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!title.trim()) {
      setError('Title cannot be empty.');
      return;
    }

    if (!deadline) {
      setError('Please select a deadline.');
      return;
    }

    setLoading(true);
    try {
      const payload = {
        title: title.trim(),
        description: description.trim(),
        priority,
        deadline: new Date(deadline).toISOString(),
        teamId: teamId || null,
        assignedToId: assignedToId || null
      };

      if (initialData) {
        await axiosClient.put(`/tasks/${initialData.id}`, payload);
      } else {
        await axiosClient.post('/tasks', payload);
      }

      onTaskCreated();
      onClose();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to save task.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '560px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px', borderBottom: '1px solid var(--border-color)', paddingBottom: '12px' }}>
          <h2 style={{ fontSize: '1.3rem', fontWeight: '800' }}>
            {initialData ? 'Edit Task' : 'Create New Task'}
          </h2>
          <button className="btn btn-secondary btn-sm" onClick={onClose}>
            <X size={16} />
          </button>
        </div>

        {error && (
          <div style={{ background: 'rgba(239, 68, 68, 0.15)', border: '1px solid rgba(239, 68, 68, 0.3)', color: '#fca5a5', padding: '10px', borderRadius: 'var(--radius-sm)', fontSize: '0.85rem', marginBottom: '16px' }}>
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Task Title *</label>
            <input
              type="text"
              className="form-input"
              placeholder="e.g. Implement User Authentication"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">Description</label>
            <textarea
              className="form-input"
              rows={3}
              placeholder="Detailed description of deliverables..."
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '14px' }}>
            <div className="form-group">
              <label className="form-label">Priority</label>
              <select
                className="form-input"
                style={{ background: 'rgba(15, 23, 42, 0.8)' }}
                value={priority}
                onChange={(e) => setPriority(e.target.value)}
              >
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
                <option value="Urgent">Urgent</option>
              </select>
            </div>

            <div className="form-group">
              <label className="form-label">Deadline *</label>
              <input
                type="date"
                className="form-input"
                value={deadline}
                onChange={(e) => setDeadline(e.target.value)}
                required
              />
            </div>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '14px' }}>
            <div className="form-group">
              <label className="form-label">Assign to Team (Optional)</label>
              <select
                className="form-input"
                style={{ background: 'rgba(15, 23, 42, 0.8)' }}
                value={teamId}
                onChange={(e) => setTeamId(e.target.value)}
              >
                <option value="">No Team Assigned</option>
                {teams.map((t) => (
                  <option key={t.id} value={t.id}>{t.name}</option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label className="form-label">Assignee (Optional)</label>
              <select
                className="form-input"
                style={{ background: 'rgba(15, 23, 42, 0.8)' }}
                value={assignedToId}
                onChange={(e) => setAssignedToId(e.target.value)}
              >
                <option value="">Unassigned</option>
                {users.map((u) => (
                  <option key={u.id} value={u.id}>{u.fullName} ({u.role})</option>
                ))}
              </select>
            </div>
          </div>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: '20px' }}>
            <button type="button" className="btn btn-secondary" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {initialData ? <><Save size={16} /> Save Changes</> : <><Plus size={16} /> Create Task</>}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CreateTaskModal;
