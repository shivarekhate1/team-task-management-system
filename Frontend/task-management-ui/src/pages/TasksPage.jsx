import React, { useState, useEffect } from 'react';
import axiosClient from '../api/axiosClient';
import { useAuth } from '../context/AuthContext';
import { Plus, Search, Filter, LayoutGrid, List, Calendar, User, MessageSquare, Trash2, Edit } from 'lucide-react';
import CreateTaskModal from '../components/CreateTaskModal';
import TaskDetailModal from '../components/TaskDetailModal';

const TasksPage = () => {
  const { user } = useAuth();
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [viewMode, setViewMode] = useState('kanban'); // 'kanban' or 'table'

  // Filters
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [priorityFilter, setPriorityFilter] = useState('');

  // Modals
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingTask, setEditingTask] = useState(null);
  const [selectedTaskId, setSelectedTaskId] = useState(null);

  const fetchTasks = async () => {
    setLoading(true);
    try {
      const params = {};
      if (search) params.search = search;
      if (statusFilter) params.status = statusFilter;
      if (priorityFilter) params.priority = priorityFilter;

      const res = await axiosClient.get('/tasks', { params });
      setTasks(res.data);
    } catch (err) {
      console.error('Error fetching tasks:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTasks();
  }, [search, statusFilter, priorityFilter]);

  const handleDeleteTask = async (e, taskId) => {
    e.stopPropagation();
    if (!window.confirm('Are you sure you want to delete this task?')) return;
    try {
      await axiosClient.delete(`/tasks/${taskId}`);
      fetchTasks();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to delete task.');
    }
  };

  const handleEditClick = (e, task) => {
    e.stopPropagation();
    setEditingTask(task);
    setShowCreateModal(true);
  };

  const canCreateOrEdit = user?.role === 'Admin' || user?.role === 'Manager';

  // Group tasks by status for Kanban Board
  const todoTasks = tasks.filter(t => t.status === 'ToDo');
  const inProgressTasks = tasks.filter(t => t.status === 'InProgress');
  const doneTasks = tasks.filter(t => t.status === 'Done');

  const renderKanbanCard = (task) => (
    <div
      key={task.id}
      className="glass-panel"
      onClick={() => setSelectedTaskId(task.id)}
      style={{
        padding: '16px',
        cursor: 'pointer',
        transition: 'var(--transition)',
        borderLeft: `4px solid ${
          task.priority === 'Urgent' ? 'var(--priority-urgent)' :
          task.priority === 'High' ? 'var(--priority-high)' :
          task.priority === 'Medium' ? 'var(--priority-medium)' : 'var(--priority-low)'
        }`
      }}
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '8px' }}>
        <span className={`badge badge-${task.priority.toLowerCase()}`}>{task.priority}</span>
        {canCreateOrEdit && (
          <div style={{ display: 'flex', gap: '4px' }}>
            <button
              className="btn btn-secondary btn-sm"
              style={{ padding: '4px 6px' }}
              onClick={(e) => handleEditClick(e, task)}
              title="Edit Task"
            >
              <Edit size={14} />
            </button>
            <button
              className="btn btn-secondary btn-sm"
              style={{ padding: '4px 6px', color: 'var(--danger)' }}
              onClick={(e) => handleDeleteTask(e, task.id)}
              title="Delete Task"
            >
              <Trash2 size={14} />
            </button>
          </div>
        )}
      </div>

      <h4 style={{ fontSize: '1rem', fontWeight: '700', marginBottom: '6px', color: 'var(--text-primary)' }}>
        {task.title}
      </h4>

      {task.description && (
        <p style={{ fontSize: '0.82rem', color: 'var(--text-secondary)', marginBottom: '12px', display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
          {task.description}
        </p>
      )}

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', fontSize: '0.78rem', color: 'var(--text-muted)', paddingTop: '10px', borderTop: '1px solid var(--border-color)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
          <User size={13} /> {task.assignedToName || 'Unassigned'}
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '3px' }}>
            <Calendar size={13} /> {new Date(task.deadline).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })}
          </div>
          {task.commentCount > 0 && (
            <div style={{ display: 'flex', alignItems: 'center', gap: '3px', color: 'var(--primary)' }}>
              <MessageSquare size={13} /> {task.commentCount}
            </div>
          )}
        </div>
      </div>
    </div>
  );

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
      {/* Top Header & Actions */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '16px' }}>
        <div>
          <h1 style={{ fontSize: '1.75rem', fontWeight: '800' }}>Tasks Management</h1>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
            Track task progress, assign deliverables, and collaborate across teams.
          </p>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          {/* View Toggle */}
          <div style={{ background: 'rgba(255,255,255,0.05)', padding: '4px', borderRadius: 'var(--radius-sm)', border: '1px solid var(--border-color)', display: 'flex', gap: '4px' }}>
            <button
              className={`btn btn-sm ${viewMode === 'kanban' ? 'btn-primary' : 'btn-secondary'}`}
              onClick={() => setViewMode('kanban')}
            >
              <LayoutGrid size={16} /> Kanban
            </button>
            <button
              className={`btn btn-sm ${viewMode === 'table' ? 'btn-primary' : 'btn-secondary'}`}
              onClick={() => setViewMode('table')}
            >
              <List size={16} /> Table
            </button>
          </div>

          {canCreateOrEdit && (
            <button
              className="btn btn-primary"
              onClick={() => { setEditingTask(null); setShowCreateModal(true); }}
            >
              <Plus size={18} /> Create Task
            </button>
          )}
        </div>
      </div>

      {/* Filter Bar */}
      <div className="glass-panel" style={{ padding: '14px 18px', display: 'flex', flexWrap: 'wrap', gap: '14px', alignItems: 'center' }}>
        <div style={{ position: 'relative', flex: 1, minWidth: '220px' }}>
          <input
            type="text"
            className="form-input"
            style={{ width: '100%', paddingLeft: '36px' }}
            placeholder="Search tasks..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <Search size={16} color="var(--text-muted)" style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)' }} />
        </div>

        <select
          className="form-input"
          style={{ width: '150px', background: 'rgba(15, 23, 42, 0.8)' }}
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
        >
          <option value="">All Statuses</option>
          <option value="ToDo">To Do</option>
          <option value="InProgress">In Progress</option>
          <option value="Done">Done</option>
        </select>

        <select
          className="form-input"
          style={{ width: '150px', background: 'rgba(15, 23, 42, 0.8)' }}
          value={priorityFilter}
          onChange={(e) => setPriorityFilter(e.target.value)}
        >
          <option value="">All Priorities</option>
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
          <option value="Urgent">Urgent</option>
        </select>
      </div>

      {loading ? (
        <div style={{ textAlign: 'center', padding: '60px 0', color: 'var(--text-secondary)' }}>
          Loading tasks...
        </div>
      ) : viewMode === 'kanban' ? (
        /* KANBAN BOARD VIEW */
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '20px' }}>
          {/* Column 1: To Do */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: '14px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '10px 14px', background: 'rgba(100, 116, 139, 0.15)', borderRadius: 'var(--radius-sm)', borderLeft: '4px solid #64748b' }}>
              <span style={{ fontWeight: '700', fontSize: '0.95rem' }}>TO DO</span>
              <span className="badge badge-todo">{todoTasks.length}</span>
            </div>
            {todoTasks.map(renderKanbanCard)}
            {todoTasks.length === 0 && <div style={{ color: 'var(--text-muted)', fontSize: '0.85rem', textAlign: 'center', padding: '20px 0' }}>No tasks in To Do</div>}
          </div>

          {/* Column 2: In Progress */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: '14px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '10px 14px', background: 'rgba(59, 130, 246, 0.15)', borderRadius: 'var(--radius-sm)', borderLeft: '4px solid #3b82f6' }}>
              <span style={{ fontWeight: '700', fontSize: '0.95rem' }}>IN PROGRESS</span>
              <span className="badge badge-inprogress">{inProgressTasks.length}</span>
            </div>
            {inProgressTasks.map(renderKanbanCard)}
            {inProgressTasks.length === 0 && <div style={{ color: 'var(--text-muted)', fontSize: '0.85rem', textAlign: 'center', padding: '20px 0' }}>No tasks in progress</div>}
          </div>

          {/* Column 3: Done */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: '14px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '10px 14px', background: 'rgba(16, 185, 129, 0.15)', borderRadius: 'var(--radius-sm)', borderLeft: '4px solid #10b981' }}>
              <span style={{ fontWeight: '700', fontSize: '0.95rem' }}>DONE</span>
              <span className="badge badge-done">{doneTasks.length}</span>
            </div>
            {doneTasks.map(renderKanbanCard)}
            {doneTasks.length === 0 && <div style={{ color: 'var(--text-muted)', fontSize: '0.85rem', textAlign: 'center', padding: '20px 0' }}>No completed tasks</div>}
          </div>
        </div>
      ) : (
        /* TABLE VIEW */
        <div className="glass-panel" style={{ padding: '0', overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.9rem' }}>
            <thead>
              <tr style={{ borderBottom: '1px solid var(--border-color)', background: 'rgba(15, 23, 42, 0.4)', color: 'var(--text-muted)', fontSize: '0.8rem', textTransform: 'uppercase' }}>
                <th style={{ padding: '14px 18px' }}>Task</th>
                <th style={{ padding: '14px 18px' }}>Priority</th>
                <th style={{ padding: '14px 18px' }}>Status</th>
                <th style={{ padding: '14px 18px' }}>Assigned To</th>
                <th style={{ padding: '14px 18px' }}>Deadline</th>
                <th style={{ padding: '14px 18px', textAlign: 'right' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {tasks.length === 0 ? (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center', padding: '30px', color: 'var(--text-muted)' }}>No tasks match your criteria.</td>
                </tr>
              ) : (
                tasks.map((task) => (
                  <tr
                    key={task.id}
                    onClick={() => setSelectedTaskId(task.id)}
                    style={{ borderBottom: '1px solid var(--border-color)', cursor: 'pointer', transition: 'var(--transition)' }}
                  >
                    <td style={{ padding: '14px 18px', fontWeight: '700', color: 'var(--text-primary)' }}>
                      {task.title}
                    </td>
                    <td style={{ padding: '14px 18px' }}>
                      <span className={`badge badge-${task.priority.toLowerCase()}`}>{task.priority}</span>
                    </td>
                    <td style={{ padding: '14px 18px' }}>
                      <span className={`badge badge-${task.status.toLowerCase()}`}>
                        {task.status === 'ToDo' ? 'To Do' : task.status === 'InProgress' ? 'In Progress' : 'Done'}
                      </span>
                    </td>
                    <td style={{ padding: '14px 18px', color: 'var(--text-secondary)' }}>
                      {task.assignedToName || 'Unassigned'}
                    </td>
                    <td style={{ padding: '14px 18px', color: 'var(--text-secondary)' }}>
                      {new Date(task.deadline).toLocaleDateString()}
                    </td>
                    <td style={{ padding: '14px 18px', textAlign: 'right' }}>
                      <div style={{ display: 'inline-flex', gap: '6px' }}>
                        {canCreateOrEdit && (
                          <>
                            <button
                              className="btn btn-secondary btn-sm"
                              onClick={(e) => handleEditClick(e, task)}
                              title="Edit Task"
                            >
                              <Edit size={14} />
                            </button>
                            <button
                              className="btn btn-secondary btn-sm"
                              style={{ color: 'var(--danger)' }}
                              onClick={(e) => handleDeleteTask(e, task.id)}
                              title="Delete Task"
                            >
                              <Trash2 size={14} />
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* Modals */}
      {showCreateModal && (
        <CreateTaskModal
          initialData={editingTask}
          onClose={() => { setShowCreateModal(false); setEditingTask(null); }}
          onTaskCreated={fetchTasks}
        />
      )}

      {selectedTaskId && (
        <TaskDetailModal
          taskId={selectedTaskId}
          onClose={() => setSelectedTaskId(null)}
          onTaskUpdated={fetchTasks}
          currentUser={user}
        />
      )}
    </div>
  );
};

export default TasksPage;
