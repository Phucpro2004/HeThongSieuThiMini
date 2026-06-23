let userModal;
let resetPassModal;

document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/pages/login.html';
        return;
    }
    
    userModal = new bootstrap.Modal(document.getElementById('userModal'));
    if(document.getElementById('resetPassModal')) {
        resetPassModal = new bootstrap.Modal(document.getElementById('resetPassModal'));
    }
    loadUsers();
});

async function loadUsers() {
    try {
        const res = await axios.get('/api/users');
        const users = res.data;
        const tbody = document.querySelector('#usersTable tbody');
        tbody.innerHTML = '';
        
        if (users.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center py-4 text-muted">Không tìm thấy tài khoản.</td></tr>';
            return;
        }

        users.forEach(u => {
            const roleBadge = u.role === 'Admin' 
                ? '<span class="badge bg-danger rounded-pill px-3 py-2"><i class="fa-solid fa-crown me-1"></i> Quản trị viên</span>' 
                : '<span class="badge bg-primary rounded-pill px-3 py-2"><i class="fa-solid fa-cash-register me-1"></i> Thu ngân</span>';
                
            const statusBadge = u.isActive !== false
                ? '<span class="badge bg-success rounded-pill px-3 py-2">Hoạt động</span>'
                : '<span class="badge bg-secondary rounded-pill px-3 py-2">Đã khóa</span>';
                
            const toggleColor = u.isActive !== false ? 'btn-outline-danger' : 'btn-outline-success';
            const toggleIcon = u.isActive !== false ? 'fa-lock' : 'fa-unlock';
            const toggleTitle = u.isActive !== false ? 'Khóa tài khoản' : 'Mở khóa tài khoản';

            tbody.innerHTML += `
                <tr>
                    <td><span class="badge bg-light text-dark border">#${u.id}</span></td>
                    <td class="fw-medium text-primary">${u.email}</td>
                    <td class="fw-bold">${u.fullName || '-'}</td>
                    <td>${roleBadge}</td>
                    <td>${statusBadge}</td>
                    <td class="text-center">
                        <button class="btn btn-sm btn-warning me-1" title="Đổi mật khẩu" onclick="showResetPassModal(${u.id})">
                            <i class="fa-solid fa-key"></i>
                        </button>
                        <button class="btn btn-sm ${toggleColor}" title="${toggleTitle}" onclick="toggleUserStatus(${u.id})">
                            <i class="fa-solid ${toggleIcon}"></i>
                        </button>
                    </td>
                </tr>
            `;
        });
    } catch (error) {
        console.error('Failed to load users', error);
        if (error.response?.status === 403) {
            alert('Từ chối truy cập. Bạn phải là Admin.');
            window.location.href = '/pages/pos.html';
        } else {
            alert('Lỗi khi tải danh sách nhân sự');
        }
    }
}

function openAddModal() {
    document.getElementById('userForm').reset();
    document.getElementById('newUsername').value = 'dummy_user_' + Date.now(); // Not used by backend, but keep it if required
}

async function saveUser() {
    const password = document.getElementById('newPassword').value;
    const fullName = document.getElementById('newFullName').value;
    const email = document.getElementById('newEmail').value;
    const role = document.getElementById('newRole').value;
    const username = email; // Backend expects username or email, we map email to username
    
    if (!password || !fullName || !email) {
        alert('Vui lòng điền đầy đủ thông tin');
        return;
    }
    
    try {
        await axios.post('/api/users', {
            username,
            password,
            fullName,
            email,
            role
        });
        
        userModal.hide();
        loadUsers();
        alert('Tạo tài khoản thành công');
    } catch (error) {
        console.error('Failed to create user', error);
        alert('Tạo tài khoản thất bại. Email có thể đã tồn tại.');
    }
}

function showResetPassModal(id) {
    document.getElementById('resetPassForm').reset();
    document.getElementById('resetUserId').value = id;
    if(resetPassModal) resetPassModal.show();
}

async function submitResetPassword() {
    const id = document.getElementById('resetUserId').value;
    const newPassword = document.getElementById('resetNewPassword').value;
    
    if (!newPassword) {
        alert('Vui lòng nhập mật khẩu mới');
        return;
    }
    
    try {
        await axios.put(`/api/users/${id}/reset-password`, { newPassword });
        resetPassModal.hide();
        alert('Cấp lại mật khẩu thành công!');
    } catch (error) {
        console.error('Reset password error', error);
        alert('Lỗi cấp lại mật khẩu');
    }
}

async function toggleUserStatus(id) {
    if (!confirm('Bạn có chắc chắn muốn thay đổi trạng thái tài khoản này?')) return;
    
    try {
        await axios.put(`/api/users/${id}/toggle-status`);
        alert('Thay đổi trạng thái thành công!');
        loadUsers();
    } catch (error) {
        console.error('Toggle status error', error);
        alert('Lỗi đổi trạng thái tài khoản');
    }
}
