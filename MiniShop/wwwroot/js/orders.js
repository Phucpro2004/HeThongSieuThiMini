let orderModal;
let ordersList = [];

document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/pages/login.html';
        return;
    }
    
    orderModal = new bootstrap.Modal(document.getElementById('orderModal'));
    fetchOrders();
});

async function fetchOrders() {
    try {
        const res = await axios.get('/api/orders');
        ordersList = res.data;
        renderOrders(ordersList);
    } catch (error) {
        console.error('Failed to load orders', error);
        alert('Lỗi khi tải danh sách hóa đơn');
    }
}

function renderOrders(orders) {
    const tbody = document.querySelector('#ordersTable tbody');
    tbody.innerHTML = '';
    
    if (orders.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="text-center py-4 text-muted">Không có hóa đơn nào.</td></tr>';
        return;
    }

    orders.forEach(o => {
        const d = new Date(o.createdAt);
        const dateStr = d.toLocaleDateString('vi-VN') + ' ' + d.toLocaleTimeString('vi-VN');
        
        tbody.innerHTML += `
            <tr>
                <td><span class="badge bg-light text-dark border fw-bold">#${o.id}</span></td>
                <td>${dateStr}</td>
                <td>${o.customerPhone || 'Khách vãng lai'}</td>
                <td><span class="badge bg-info text-dark"><i class="fa-solid fa-user me-1"></i> ${o.cashierName || 'NV'}</span></td>
                <td class="fw-bold text-success">${o.totalAmount.toLocaleString('vi-VN')} VNĐ</td>
                <td><span class="badge bg-success rounded-pill">Hoàn tất</span></td>
                <td class="text-center">
                    <button class="btn btn-sm btn-outline-primary" title="Xem chi tiết" onclick="viewOrder(${o.id})">
                        <i class="fa-solid fa-eye"></i>
                    </button>
                </td>
            </tr>
        `;
    });
}

function handleSearch(e) {
    const term = e.target.value.toLowerCase();
    if (!term) {
        renderOrders(ordersList);
        return;
    }
    
    const filtered = ordersList.filter(o => 
        o.id.toString().includes(term) || 
        (o.customerPhone && o.customerPhone.includes(term))
    );
    renderOrders(filtered);
}

async function viewOrder(id) {
    try {
        const res = await axios.get('/api/orders/' + id);
        const order = res.data;
        
        document.getElementById('modalOrderId').innerText = order.id;
        const d = new Date(order.createdAt);
        document.getElementById('modalOrderDate').innerText = d.toLocaleDateString('vi-VN') + ' ' + d.toLocaleTimeString('vi-VN');
        document.getElementById('modalOrderTotal').innerText = order.totalAmount.toLocaleString('vi-VN') + ' VNĐ';
        
        const tbody = document.querySelector('#orderItemsTable tbody');
        tbody.innerHTML = '';
        
        if (order.orderDetails && order.orderDetails.length > 0) {
            order.orderDetails.forEach(item => {
                const total = item.quantity * item.unitPrice;
                tbody.innerHTML += `
                    <tr>
                        <td class="fw-medium">${item.productName}</td>
                        <td class="text-center">${item.quantity}</td>
                        <td class="text-end">${item.unitPrice.toLocaleString('vi-VN')}</td>
                        <td class="text-end fw-bold text-primary">${total.toLocaleString('vi-VN')}</td>
                    </tr>
                `;
            });
        } else {
            tbody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Không có dữ liệu sản phẩm</td></tr>';
        }
        
        orderModal.show();
    } catch (error) {
        console.error('Failed to load order details', error);
        alert('Lỗi khi tải chi tiết hóa đơn');
    }
}
