let customersList = [];
let addCustomerModal = null;

document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/pages/login.html';
        return;
    }
    
    addCustomerModal = new bootstrap.Modal(document.getElementById('addCustomerModal'));
    
    document.getElementById('addCustomerForm').addEventListener('submit', handleAddCustomer);
    
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
        tbody.innerHTML += `
            <tr>
                <td class="fw-bold">${c.fullName || '-'}</td>
                <td>${c.phone || '-'}</td>
                <td>${c.email || '-'}</td>
                <td>${c.address || '-'}</td>
                <td class="text-center"><span class="points-text"><i class="fa-solid fa-star"></i> ${c.points || 0}</span></td>
                <td class="text-center">
                    <button class="btn btn-sm btn-outline-primary me-1" onclick="editCustomer(${c.id})" title="Sửa"><i class="fa-solid fa-pen"></i></button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteCustomer(${c.id})" title="Xóa"><i class="fa-solid fa-trash"></i></button>
                </td>
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
        (c.phone && c.phone.includes(term)) || 
        (c.fullName && c.fullName.toLowerCase().includes(term))
    );
    renderCustomers(filtered);
}

function showAddCustomerModal() {
    document.getElementById('addCustomerForm').reset();
    document.getElementById('customerId').value = '';
    document.getElementById('customerModalTitle').innerHTML = '<i class="fa-solid fa-user-plus me-2"></i>Thêm Khách hàng mới';
    addCustomerModal.show();
}

function editCustomer(id) {
    const customer = customersList.find(c => c.id === id);
    if (!customer) return;

    document.getElementById('customerId').value = customer.id;
    document.getElementById('custFullName').value = customer.fullName || '';
    document.getElementById('custPhone').value = customer.phone || '';
    document.getElementById('custEmail').value = customer.email || '';
    document.getElementById('custAddress').value = customer.address || '';

    document.getElementById('customerModalTitle').innerHTML = '<i class="fa-solid fa-user-pen me-2"></i>Cập nhật Khách hàng';
    addCustomerModal.show();
}

async function deleteCustomer(id) {
    if (!confirm('Bạn có chắc chắn muốn xóa khách hàng này không?')) return;

    try {
        await axios.delete(`/api/customers/${id}`);
        await fetchCustomers();
    } catch (error) {
        console.error('Lỗi khi xóa khách hàng:', error);
        alert('Lỗi hệ thống khi xóa khách hàng.');
    }
}

async function handleAddCustomer(e) {
    e.preventDefault();
    
    const btnSave = document.getElementById('btnSaveCustomer');
    const originalText = btnSave.innerText;
    btnSave.innerText = 'Đang lưu...';
    btnSave.disabled = true;

    const id = document.getElementById('customerId').value;
    const isUpdate = !!id;

    const customerData = {
        fullName: document.getElementById('custFullName').value.trim(),
        phone: document.getElementById('custPhone').value.trim(),
        email: document.getElementById('custEmail').value.trim(),
        address: document.getElementById('custAddress').value.trim()
    };
    
    if (!isUpdate) {
        customerData.points = 0; // Default starting points only on create
    } else {
        const existing = customersList.find(c => c.id == id);
        if (existing) customerData.points = existing.points;
    }

    try {
        if (isUpdate) {
            await axios.put(`/api/customers/${id}`, customerData);
        } else {
            await axios.post('/api/customers', customerData);
        }
        
        // Refresh the list
        await fetchCustomers();
        addCustomerModal.hide();
        // Optional: show a success toast
    } catch (error) {
        console.error('Lỗi khi lưu khách hàng:', error);
        if (error.response && error.response.data && error.response.data.message) {
            alert(error.response.data.message);
        } else {
            alert('Lỗi hệ thống khi lưu khách hàng. Vui lòng thử lại.');
        }
    } finally {
        btnSave.innerText = originalText;
        btnSave.disabled = false;
    }
}
