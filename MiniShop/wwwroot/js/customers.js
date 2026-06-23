let customersList = [];

document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/pages/login.html';
        return;
    }
    fetchCustomers();
});

async function fetchCustomers() {
    try {
        const res = await axios.get('/api/customers');
        customersList = res.data;
        renderCustomers(customersList);
    } catch (error) {
        console.error('Failed to load customers', error);
        alert('Lỗi khi tải danh sách khách hàng');
    }
}

function renderCustomers(customers) {
    const tbody = document.querySelector('#customersTable tbody');
    tbody.innerHTML = '';
    
    if (customers.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center py-4 text-muted">Không có khách hàng nào.</td></tr>';
        return;
    }

    customers.forEach(c => {
        const d = new Date(c.createdAt);
        const dateStr = d.toLocaleDateString('vi-VN');
        
        tbody.innerHTML += `
            <tr>
                <td><span class="badge bg-light text-dark border">#${c.id}</span></td>
                <td class="fw-bold">${c.fullName || '-'}</td>
                <td class="text-primary fw-medium">${c.phone}</td>
                <td>${c.email || '-'}</td>
                <td><span class="badge bg-warning text-dark px-3 py-2 fs-6"><i class="fa-solid fa-star text-danger me-1"></i> ${c.rewardPoints} đ</span></td>
                <td class="text-muted">${dateStr}</td>
            </tr>
        `;
    });
}

function handleSearch(e) {
    const term = e.target.value.toLowerCase();
    if (!term) {
        renderCustomers(customersList);
        return;
    }
    
    const filtered = customersList.filter(c => 
        c.phone.includes(term) || 
        (c.fullName && c.fullName.toLowerCase().includes(term))
    );
    renderCustomers(filtered);
}
