document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = 'index.html';
        return;
    }

    // Auth Header Logic
    function decodeJWT(token) {
        try {
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
                return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
            }).join(''));
            return JSON.parse(jsonPayload);
        } catch (e) { return null; }
    }

    const payload = decodeJWT(token);
    const username = payload.unique_name || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
    const role = payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    
    let adminLink = '';
    if (role === 'Admin') {
        adminLink = `<a href="admin.html" style="color: var(--primary); font-weight: bold; border: 1px solid var(--primary); padding: 5px 15px; border-radius: 15px; text-decoration: none; margin-right: 10px;">Admin</a>`;
    }

    document.getElementById('user-section').innerHTML = `
        ${adminLink}
        <span style="color: var(--secondary); font-weight: 600; margin-right: 15px;">Hi, ${username}</span>
        <button id="btn-logout" class="btn btn-outline" style="padding: 5px 15px; font-size: 0.8rem; border: 1px solid #ff4d4d; color: #ff4d4d; background: transparent; border-radius: 15px; cursor: pointer;">Logout</button>
    `;

    document.getElementById('btn-logout').addEventListener('click', () => {
        localStorage.removeItem('token');
        window.location.href = 'index.html';
    });

    // Tab Logic
    const tabs = document.querySelectorAll('.profile-tabs button');
    const contents = document.querySelectorAll('.tab-content');
    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            tabs.forEach(t => t.classList.remove('active'));
            contents.forEach(c => c.classList.remove('active'));
            tab.classList.add('active');
            document.getElementById(tab.getAttribute('data-target')).classList.add('active');
        });
    });

    // Load Profile
    async function loadProfile() {
        try {
            const res = await fetch('/api/Users/profile', {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (res.ok) {
                const profile = await res.json();
                document.getElementById('prof-fullname').value = profile.fullName || '';
                document.getElementById('prof-email').value = profile.email || '';
                document.getElementById('prof-phone').value = profile.phoneNumber || '';
                document.getElementById('prof-address').value = profile.address || '';
            }
        } catch (err) { console.error('Failed to load profile', err); }
    }

    // Save Profile
    document.getElementById('profile-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            fullName: document.getElementById('prof-fullname').value,
            email: document.getElementById('prof-email').value,
            phoneNumber: document.getElementById('prof-phone').value,
            address: document.getElementById('prof-address').value
        };

        const msgEl = document.getElementById('profile-msg');
        try {
            const res = await fetch('/api/Users/profile', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(data)
            });

            if (res.ok) {
                msgEl.textContent = 'Profile updated successfully!';
                msgEl.style.color = '#00ff00';
            } else {
                msgEl.textContent = 'Failed to update profile.';
                msgEl.style.color = '#ff4d4d';
            }
        } catch (err) {
            msgEl.textContent = 'Network error.';
            msgEl.style.color = '#ff4d4d';
        }
        setTimeout(() => msgEl.textContent = '', 3000);
    });

    // Load Orders
    async function loadOrders() {
        try {
            const res = await fetch('/api/Orders/my-orders', {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (res.ok) {
                const orders = await res.json();
                renderOrders(orders);
            }
        } catch (err) { console.error('Failed to load orders', err); }
    }

    function getStatusClass(status) {
        if (!status) return '';
        const s = status.toLowerCase();
        if (s === 'processing') return 'status-processing';
        if (s === 'shipped') return 'status-shipped';
        if (s === 'delivered') return 'status-delivered';
        if (s === 'cancelled') return 'status-cancelled';
        if (s === 'paid') return 'status-paid';
        if (s === 'pending') return 'status-pending';
        return '';
    }

    function renderOrders(orders) {
        const tbody = document.getElementById('orders-tbody');
        if (orders.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">No orders found.</td></tr>';
            return;
        }

        tbody.innerHTML = orders.map(o => {
            const date = new Date(o.orderDate).toLocaleDateString();
            const itemsCount = o.orderDetails ? o.orderDetails.reduce((acc, curr) => acc + curr.quantity, 0) : 0;
            return `
                <tr>
                    <td>#${o.id}</td>
                    <td>${date}</td>
                    <td style="font-weight: bold;">$${o.totalAmount.toFixed(2)}</td>
                    <td>${o.paymentMethod || 'Cash'}</td>
                    <td><span class="status-badge ${getStatusClass(o.paymentStatus)}">${o.paymentStatus}</span></td>
                    <td><span class="status-badge ${getStatusClass(o.orderStatus)}">${o.orderStatus}</span></td>
                    <td>${itemsCount} items</td>
                </tr>
            `;
        }).join('');
    }

    loadProfile();
    loadOrders();
});
