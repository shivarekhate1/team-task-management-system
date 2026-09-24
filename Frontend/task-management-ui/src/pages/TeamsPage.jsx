import React, { useState, useEffect } from 'react';
import axiosClient from '../api/axiosClient';
import { useAuth } from '../context/AuthContext';
import { Users, Plus, Shield, UserCheck, Trash2, UserPlus, X } from 'lucide-react';

const TeamsPage = () => {
  const { user } = useAuth();
  const [teams, setTeams] = useState([]);
  const [loading, setLoading] = useState(true);

  // Modals & Selections
  const [selectedTeam, setSelectedTeam] = useState(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showAddMemberModal, setShowAddMemberModal] = useState(false);

  // Form states
  const [newTeamName, setNewTeamName] = useState('');
  const [newTeamDesc, setNewTeamDesc] = useState('');
  const [newTeamManagerId, setNewTeamManagerId] = useState('');

  const [allUsers, setAllUsers] = useState([]);
  const [selectedMemberId, setSelectedMemberId] = useState('');
  const [error, setError] = useState('');

  const fetchTeams = async () => {
    setLoading(true);
    try {
      const res = await axiosClient.get('/teams');
      setTeams(res.data);
    } catch (err) {
      console.error('Error fetching teams:', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchAllUsers = async () => {
    try {
      const res = await axiosClient.get('/users');
      setAllUsers(res.data);
    } catch (err) {
      console.error('Error fetching users:', err);
    }
  };

  useEffect(() => {
    fetchTeams();
    fetchAllUsers();
  }, []);

  const handleCreateTeam = async (e) => {
    e.preventDefault();
    if (!newTeamName.trim()) return;
    setError('');
    try {
      await axiosClient.post('/teams', {
        name: newTeamName.trim(),
        description: newTeamDesc.trim(),
        managerId: newTeamManagerId || null
      });
      setNewTeamName('');
      setNewTeamDesc('');
      setNewTeamManagerId('');
      setShowCreateModal(false);
      fetchTeams();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create team.');
    }
  };

  const fetchTeamDetails = async (teamId) => {
    try {
      const res = await axiosClient.get(`/teams/${teamId}`);
      setSelectedTeam(res.data);
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to load team details.');
    }
  };

  const handleAddMember = async (e) => {
    e.preventDefault();
    if (!selectedMemberId || !selectedTeam) return;
    setError('');
    try {
      await axiosClient.post(`/teams/${selectedTeam.id}/members`, { userId: selectedMemberId });
      fetchTeamDetails(selectedTeam.id);
      fetchTeams();
      setShowAddMemberModal(false);
      setSelectedMemberId('');
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to add member.');
    }
  };

  const handleRemoveMember = async (userId) => {
    if (!window.confirm('Remove this member from the team?')) return;
    try {
      await axiosClient.delete(`/teams/${selectedTeam.id}/members/${userId}`);
      fetchTeamDetails(selectedTeam.id);
      fetchTeams();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to remove member.');
    }
  };

  const canManageTeams = user?.role === 'Admin' || user?.role === 'Manager';

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1 style={{ fontSize: '1.75rem', fontWeight: '800' }}>Teams Management</h1>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
            Organize users into functional teams and manage team leaders.
          </p>
        </div>

        {canManageTeams && (
          <button className="btn btn-primary" onClick={() => setShowCreateModal(true)}>
            <Plus size={18} /> Create Team
          </button>
        )}
      </div>

      {loading ? (
        <div style={{ textAlign: 'center', padding: '60px 0', color: 'var(--text-secondary)' }}>
          Loading teams...
        </div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '20px' }}>
          {teams.map((t) => (
            <div
              key={t.id}
              className="glass-panel"
              style={{ padding: '24px', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}
            >
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '8px' }}>
                  <h3 style={{ fontSize: '1.2rem', fontWeight: '800', color: 'var(--text-primary)' }}>
                    {t.name}
                  </h3>
                  <span className="badge badge-secondary" style={{ background: 'rgba(99, 102, 241, 0.15)', color: 'var(--primary)' }}>
                    {t.memberCount} Members
                  </span>
                </div>

                <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '16px' }}>
                  {t.description || 'No team description.'}
                </p>

                <div style={{ fontSize: '0.85rem', color: 'var(--text-muted)', marginBottom: '8px', display: 'flex', alignItems: 'center', gap: '6px' }}>
                  <Shield size={14} color="var(--info)" /> Team Manager: <strong style={{ color: 'var(--text-primary)' }}>{t.managerName || 'Unassigned'}</strong>
                </div>

                <div style={{ fontSize: '0.85rem', color: 'var(--text-muted)', display: 'flex', alignItems: 'center', gap: '6px' }}>
                  <Users size={14} color="var(--primary)" /> Assigned Tasks: <strong style={{ color: 'var(--text-primary)' }}>{t.taskCount}</strong>
                </div>
              </div>

              <div style={{ marginTop: '20px', paddingTop: '14px', borderTop: '1px solid var(--border-color)', display: 'flex', justifyContent: 'flex-end' }}>
                <button
                  className="btn btn-secondary btn-sm"
                  onClick={() => fetchTeamDetails(t.id)}
                >
                  <Users size={14} /> View Members & Details
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Team Details Modal */}
      {selectedTeam && (
        <div className="modal-overlay" onClick={() => setSelectedTeam(null)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '640px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px', borderBottom: '1px solid var(--border-color)', paddingBottom: '12px' }}>
              <div>
                <h2 style={{ fontSize: '1.4rem', fontWeight: '800' }}>{selectedTeam.name}</h2>
                <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  Manager: {selectedTeam.managerName || 'None'}
                </div>
              </div>
              <button className="btn btn-secondary btn-sm" onClick={() => setSelectedTeam(null)}>
                <X size={16} />
              </button>
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '12px' }}>
              <h3 style={{ fontSize: '1rem', fontWeight: '700' }}>Team Roster ({selectedTeam.members?.length || 0})</h3>
              {canManageTeams && (
                <button className="btn btn-primary btn-sm" onClick={() => setShowAddMemberModal(true)}>
                  <UserPlus size={14} /> Add Member
                </button>
              )}
            </div>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', maxHeight: '300px', overflowY: 'auto' }}>
              {selectedTeam.members?.map((m) => (
                <div
                  key={m.id}
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
                    <div style={{ fontWeight: '700', fontSize: '0.9rem' }}>{m.fullName}</div>
                    <div style={{ fontSize: '0.78rem', color: 'var(--text-muted)' }}>{m.email}</div>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <span className={`badge badge-role-${m.role.toLowerCase()}`}>{m.role}</span>
                    {canManageTeams && m.id !== selectedTeam.managerId && (
                      <button
                        className="btn btn-secondary btn-sm"
                        style={{ padding: '4px 6px', color: 'var(--danger)' }}
                        onClick={() => handleRemoveMember(m.id)}
                        title="Remove Member"
                      >
                        <Trash2 size={14} />
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Create Team Modal */}
      {showCreateModal && (
        <div className="modal-overlay" onClick={() => setShowCreateModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '480px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px', borderBottom: '1px solid var(--border-color)', paddingBottom: '12px' }}>
              <h2 style={{ fontSize: '1.2rem', fontWeight: '800' }}>Create New Team</h2>
              <button className="btn btn-secondary btn-sm" onClick={() => setShowCreateModal(false)}>
                <X size={16} />
              </button>
            </div>

            {error && (
              <div style={{ background: 'rgba(239, 68, 68, 0.15)', color: '#fca5a5', padding: '8px', borderRadius: 'var(--radius-sm)', fontSize: '0.85rem', marginBottom: '12px' }}>
                {error}
              </div>
            )}

            <form onSubmit={handleCreateTeam}>
              <div className="form-group">
                <label className="form-label">Team Name *</label>
                <input
                  type="text"
                  className="form-input"
                  placeholder="e.g. Core Platform Team"
                  value={newTeamName}
                  onChange={(e) => setNewTeamName(e.target.value)}
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Description</label>
                <textarea
                  className="form-input"
                  rows={2}
                  placeholder="Team domain and purpose..."
                  value={newTeamDesc}
                  onChange={(e) => setNewTeamDesc(e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="form-label">Assign Manager</label>
                <select
                  className="form-input"
                  style={{ background: 'rgba(15, 23, 42, 0.8)' }}
                  value={newTeamManagerId}
                  onChange={(e) => setNewTeamManagerId(e.target.value)}
                >
                  <option value="">Select Manager</option>
                  {allUsers.filter(u => u.role === 'Manager' || u.role === 'Admin').map((u) => (
                    <option key={u.id} value={u.id}>{u.fullName} ({u.role})</option>
                  ))}
                </select>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: '20px' }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowCreateModal(false)}>Cancel</button>
                <button type="submit" className="btn btn-primary">Create Team</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Add Member Modal */}
      {showAddMemberModal && (
        <div className="modal-overlay" onClick={() => setShowAddMemberModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '440px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px', borderBottom: '1px solid var(--border-color)', paddingBottom: '12px' }}>
              <h2 style={{ fontSize: '1.2rem', fontWeight: '800' }}>Add Member to {selectedTeam?.name}</h2>
              <button className="btn btn-secondary btn-sm" onClick={() => setShowAddMemberModal(false)}>
                <X size={16} />
              </button>
            </div>

            {error && (
              <div style={{ background: 'rgba(239, 68, 68, 0.15)', color: '#fca5a5', padding: '8px', borderRadius: 'var(--radius-sm)', fontSize: '0.85rem', marginBottom: '12px' }}>
                {error}
              </div>
            )}

            <form onSubmit={handleAddMember}>
              <div className="form-group">
                <label className="form-label">Select User</label>
                <select
                  className="form-input"
                  style={{ background: 'rgba(15, 23, 42, 0.8)' }}
                  value={selectedMemberId}
                  onChange={(e) => setSelectedMemberId(e.target.value)}
                  required
                >
                  <option value="">Select User to Add</option>
                  {allUsers.map((u) => (
                    <option key={u.id} value={u.id}>{u.fullName} ({u.email} - {u.role})</option>
                  ))}
                </select>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px', marginTop: '20px' }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowAddMemberModal(false)}>Cancel</button>
                <button type="submit" className="btn btn-primary">Add Member</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default TeamsPage;
