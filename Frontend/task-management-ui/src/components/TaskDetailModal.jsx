import React, { useState, useEffect } from 'react';
import { X, Calendar, User, Shield, MessageSquare, Send, CheckCircle, Clock, PlayCircle } from 'lucide-react';
import axiosClient from '../api/axiosClient';

const TaskDetailModal = ({ taskId, onClose, onTaskUpdated, currentUser }) => {
  const [task, setTask] = useState(null);
  const [loading, setLoading] = useState(true);
  const [newComment, setNewComment] = useState('');
  const [commentSubmitting, setCommentSubmitting] = useState(false);
  const [error, setError] = useState(null);

  const fetchTaskDetails = async () => {
    setLoading(true);
    try {
      const res = await axiosClient.get(`/tasks/${taskId}`);
      setTask(res.data);
      setError(null);
    } catch (err) {
      console.error(err);
      setError(err.response?.data?.message || 'Failed to load task details.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (taskId) {
      fetchTaskDetails();
    }
  }, [taskId]);

  const handleStatusChange = async (newStatus) => {
    try {
      await axiosClient.put(`/tasks/${taskId}/status`, { status: newStatus });
      fetchTaskDetails();
      if (onTaskUpdated) onTaskUpdated();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to update status.');
    }
  };

  const handleAddComment = async (e) => {
    e.preventDefault();
    if (!newComment.trim()) return;
    setCommentSubmitting(true);
    try {
      await axiosClient.post('/comments', { taskId, commentText: newComment.trim() });
      setNewComment('');
      fetchTaskDetails();
      if (onTaskUpdated) onTaskUpdated();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to post comment.');
    } finally {
      setCommentSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="modal-overlay">
        <div className="modal-content" style={{ textAlign: 'center', padding: '40px' }}>
          Loading task details...
        </div>
      </div>
    );
  }

  if (error || !task) {
    return (
      <div className="modal-overlay">
        <div className="modal-content" style={{ padding: '24px', textAlign: 'center' }}>
          <div style={{ color: '#fca5a5', marginBottom: '16px' }}>{error || 'Task not found.'}</div>
          <button className="btn btn-secondary" onClick={onClose}>Close</button>
        </div>
      </div>
    );
  }

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '680px' }}>
        {/* Header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '16px', borderBottom: '1px solid var(--border-color)', paddingBottom: '14px' }}>
          <div>
            <div style={{ display: 'flex', gap: '8px', alignItems: 'center', marginBottom: '6px' }}>
              <span className={`badge badge-${task.priority.toLowerCase()}`}>{task.priority}</span>
              <span className={`badge badge-${task.status.toLowerCase()}`}>
                {task.status === 'ToDo' ? 'To Do' : task.status === 'InProgress' ? 'In Progress' : 'Done'}
              </span>
              {task.teamName && (
                <span className="badge badge-secondary" style={{ background: 'rgba(255,255,255,0.08)' }}>
                  Team: {task.teamName}
                </span>
              )}
            </div>
            <h2 style={{ fontSize: '1.4rem', fontWeight: '800' }}>{task.title}</h2>
          </div>
          <button className="btn btn-secondary btn-sm" onClick={onClose}>
            <X size={16} />
          </button>
        </div>

        {/* Task Description */}
        <div style={{ background: 'rgba(15, 23, 42, 0.4)', padding: '14px', borderRadius: 'var(--radius-sm)', marginBottom: '20px', fontSize: '0.92rem', color: 'var(--text-primary)', minHeight: '60px' }}>
          {task.description || <em style={{ color: 'var(--text-muted)' }}>No description provided.</em>}
        </div>

        {/* Info Grid */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '12px', marginBottom: '20px', fontSize: '0.85rem' }}>
          <div>
            <div style={{ color: 'var(--text-muted)', marginBottom: '2px' }}>Assigned To</div>
            <div style={{ fontWeight: '600', display: 'flex', alignItems: 'center', gap: '6px' }}>
              <User size={14} color="var(--primary)" /> {task.assignedToName || 'Unassigned'}
            </div>
          </div>

          <div>
            <div style={{ color: 'var(--text-muted)', marginBottom: '2px' }}>Created By</div>
            <div style={{ fontWeight: '600', display: 'flex', alignItems: 'center', gap: '6px' }}>
              <Shield size={14} color="var(--info)" /> {task.createdByName}
            </div>
          </div>

          <div>
            <div style={{ color: 'var(--text-muted)', marginBottom: '2px' }}>Deadline</div>
            <div style={{ fontWeight: '600', display: 'flex', alignItems: 'center', gap: '6px' }}>
              <Calendar size={14} color="var(--warning)" /> {new Date(task.deadline).toLocaleDateString()}
            </div>
          </div>

          <div>
            <div style={{ color: 'var(--text-muted)', marginBottom: '2px' }}>Created Date</div>
            <div style={{ fontWeight: '600' }}>{new Date(task.createdAt).toLocaleDateString()}</div>
          </div>
        </div>

        {/* Quick Status Update Controls */}
        <div style={{ marginBottom: '24px', borderTop: '1px solid var(--border-color)', paddingTop: '16px' }}>
          <div style={{ fontSize: '0.85rem', fontWeight: '700', color: 'var(--text-secondary)', marginBottom: '10px' }}>
            Update Task Status:
          </div>
          <div style={{ display: 'flex', gap: '8px' }}>
            <button
              className={`btn ${task.status === 'ToDo' ? 'btn-primary' : 'btn-secondary'}`}
              onClick={() => handleStatusChange('ToDo')}
              disabled={task.status === 'ToDo'}
              style={{ flex: 1, padding: '8px' }}
            >
              <Clock size={16} /> To Do
            </button>
            <button
              className={`btn ${task.status === 'InProgress' ? 'btn-primary' : 'btn-secondary'}`}
              onClick={() => handleStatusChange('InProgress')}
              disabled={task.status === 'InProgress'}
              style={{ flex: 1, padding: '8px' }}
            >
              <PlayCircle size={16} /> In Progress
            </button>
            <button
              className={`btn ${task.status === 'Done' ? 'btn-primary' : 'btn-secondary'}`}
              onClick={() => handleStatusChange('Done')}
              disabled={task.status === 'Done'}
              style={{ flex: 1, padding: '8px' }}
            >
              <CheckCircle size={16} /> Done
            </button>
          </div>
        </div>

        {/* Comments Section */}
        <div style={{ borderTop: '1px solid var(--border-color)', paddingTop: '16px' }}>
          <h3 style={{ fontSize: '1rem', fontWeight: '700', marginBottom: '12px', display: 'flex', alignItems: 'center', gap: '6px' }}>
            <MessageSquare size={16} color="var(--primary)" /> Comments ({task.comments?.length || 0})
          </h3>

          {/* Comment List */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', maxHeight: '200px', overflowY: 'auto', marginBottom: '16px', paddingRight: '4px' }}>
            {task.comments?.length === 0 ? (
              <div style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>No comments yet. Be the first to comment!</div>
            ) : (
              task.comments.map((c) => (
                <div
                  key={c.id}
                  style={{
                    background: 'rgba(15, 23, 42, 0.5)',
                    border: '1px solid var(--border-color)',
                    borderRadius: 'var(--radius-sm)',
                    padding: '10px 12px',
                    fontSize: '0.85rem'
                  }}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
                    <strong style={{ color: 'var(--primary)' }}>{c.userName}</strong>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                      {new Date(c.createdAt).toLocaleDateString()} {new Date(c.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </span>
                  </div>
                  <div style={{ color: 'var(--text-primary)' }}>{c.commentText}</div>
                </div>
              ))
            )}
          </div>

          {/* Add Comment Input */}
          <form onSubmit={handleAddComment} style={{ display: 'flex', gap: '8px' }}>
            <input
              type="text"
              className="form-input"
              style={{ flex: 1 }}
              placeholder="Write a comment..."
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              required
            />
            <button type="submit" className="btn btn-primary" disabled={commentSubmitting}>
              <Send size={16} /> Comment
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};

export default TaskDetailModal;
