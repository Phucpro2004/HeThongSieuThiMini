let products = [];
let cart = [];
let currentCustomer = null;

document.addEventListener('DOMContentLoaded', async () => {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/pages/login.html';
        return;
    }
    
    await loadCategories();
    await loadProducts();
});

async function loadCategories() {
    try {
        const res = await axios.get('/api/Categories');
        const select = document.getElementById('posCategory');
        res.data.forEach(c => {
            select.innerHTML += `<option value="${c.id}">${c.name}</option>`;
        });
    } catch (error) {
        console.error('Failed to load categories', error);
    }
}

async function loadProducts() {
    try {
        // Load a large page size for POS
        const res = await axios.get('/api/Products?pageNumber=1&pageSize=100');
        products = res.data.data || res.data;
        renderProducts(products);
    } catch (error) {
        console.error('Failed to load products', error);
    }
}

function renderProducts(items) {
    const grid = document.getElementById('posProductGrid');
    grid.innerHTML = '';
    
    items.forEach(p => {
        const img = p.imageUrl ? p.imageUrl : 'https://via.placeholder.com/150';
        grid.innerHTML += `
            <div class="card h-100 product-card shadow-sm border-0" onclick="addToCart(${p.id})">
                <img src="${img}" class="card-img-top" alt="${p.name}">
                <div class="card-body p-2 text-center">
                    <h6 class="card-title mb-1 text-truncate fw-bold" title="${p.name}">${p.name}</h6>
                    <div class="text-danger fw-bold">${p.price.toLocaleString('vi-VN')} đ</div>
                    <small class="text-muted">Tồn kho: ${p.stockQuantity}</small>
                </div>
            </div>
        `;
    });
}

function filterProducts() {
    const search = document.getElementById('posSearch').value.toLowerCase();
    const catId = document.getElementById('posCategory').value;
    
    const filtered = products.filter(p => {
        const matchName = p.name.toLowerCase().includes(search);
        const matchCat = catId ? p.categoryId == catId : true;
        return matchName && matchCat;
    });
    
    renderProducts(filtered);
}

function addToCart(productId) {
    const product = products.find(p => p.id === productId);
    if (!product) return;
    
    if (product.stockQuantity <= 0) {
        alert('Out of stock!');
        return;
    }

    const existing = cart.find(c => c.productId === productId);
    if (existing) {
        if (existing.quantity >= product.stockQuantity) {
            alert('Not enough stock!');
            return;
        }
        existing.quantity++;
    } else {
        cart.push({
            productId: product.id,
            name: product.name,
            price: product.price,
            quantity: 1,
            maxStock: product.stockQuantity
        });
    }
    
    renderCart();
}

function removeFromCart(productId) {
    cart = cart.filter(c => c.productId !== productId);
    renderCart();
}

function changeQuantity(productId, delta) {
    const item = cart.find(c => c.productId === productId);
    if (!item) return;
    
    const newQty = item.quantity + delta;
    if (newQty <= 0) {
        removeFromCart(productId);
    } else if (newQty > item.maxStock) {
        alert('Not enough stock!');
    } else {
        item.quantity = newQty;
        renderCart();
    }
}

function renderCart() {
    const container = document.getElementById('cartItemsContainer');
    container.innerHTML = '';
    
    cart.forEach(item => {
        container.innerHTML += `
            <div class="d-flex justify-content-between align-items-center mb-2 p-2 border rounded bg-light">
                <div class="flex-grow-1">
                    <div class="fw-bold text-truncate" style="max-width: 150px;" title="${item.name}">${item.name}</div>
                    <div class="text-danger small fw-medium">${item.price.toLocaleString('vi-VN')} đ</div>
                </div>
                <div class="d-flex align-items-center">
                    <button class="btn btn-sm btn-outline-secondary px-2" onclick="changeQuantity(${item.productId}, -1)"><i class="fa-solid fa-minus"></i></button>
                    <span class="mx-2 fw-bold text-primary">${item.quantity}</span>
                    <button class="btn btn-sm btn-outline-secondary px-2" onclick="changeQuantity(${item.productId}, 1)"><i class="fa-solid fa-plus"></i></button>
                    <button class="btn btn-sm btn-outline-danger px-2 ms-2" onclick="removeFromCart(${item.productId})"><i class="fa-solid fa-trash"></i></button>
                </div>
            </div>
        `;
    });
    
    updateCartTotals();
}

function updateCartTotals() {
    let subtotal = 0;
    cart.forEach(item => {
        subtotal += item.price * item.quantity;
    });
    
    let discount = 0;
    const checkbox = document.getElementById('usePointsCheckbox');
    if (checkbox && checkbox.checked && currentCustomer) {
        discount = currentCustomer.points * 1000;
        // Don't discount more than subtotal
        if (discount > subtotal) discount = subtotal;
    }
    
    const total = Math.max(0, subtotal - discount);
    
    // Check if subtotal container exists. If it was removed, just store it as an attribute
    document.getElementById('cartTotal').setAttribute('data-total', total);
    document.getElementById('cartTotal').innerText = total.toLocaleString('vi-VN') + ' VNĐ';
    
    calculateChange();
}

function calculateChange() {
    const total = parseFloat(document.getElementById('cartTotal').getAttribute('data-total')) || 0;
    const received = parseFloat(document.getElementById('amountReceived').value) || 0;
    
    const change = received - total;
    if (received === 0 || isNaN(received)) {
        document.getElementById('changeAmount').innerText = '0 đ';
        document.getElementById('changeAmount').classList.remove('text-danger');
    } else {
        document.getElementById('changeAmount').innerText = (change >= 0 ? change.toLocaleString('vi-VN') : '0') + ' VNĐ';
        if (change < 0) {
            document.getElementById('changeAmount').classList.add('text-danger');
        } else {
            document.getElementById('changeAmount').classList.remove('text-danger');
        }
    }
}

async function searchCustomer() {
    const phone = document.getElementById('customerPhone').value;
    if (!phone) {
        currentCustomer = null;
        document.getElementById('customerInfo').classList.add('d-none');
        updateCartTotals();
        return;
    }
    
    try {
        const res = await axios.get(`/api/customers/search?phone=${phone}`);
        currentCustomer = res.data;
        document.getElementById('customerName').innerText = currentCustomer.fullName;
        document.getElementById('customerPoints').innerText = currentCustomer.points;
        document.getElementById('customerInfo').classList.remove('d-none');
        updateCartTotals();
    } catch (error) {
        alert('Không tìm thấy khách hàng!');
        currentCustomer = null;
        document.getElementById('customerInfo').classList.add('d-none');
        updateCartTotals();
    }
}

async function checkout() {
    if (cart.length === 0) {
        alert('Giỏ hàng đang trống!');
        return;
    }
    
    const total = parseFloat(document.getElementById('cartTotal').getAttribute('data-total')) || 0;
    const received = parseFloat(document.getElementById('amountReceived').value) || 0;
    
    if (received < total) {
        alert('Tiền khách đưa không đủ!');
        return;
    }
    
    let discount = 0;
    const checkbox = document.getElementById('usePointsCheckbox');
    if (checkbox && checkbox.checked && currentCustomer) {
        discount = currentCustomer.points * 1000;
        if (discount > (total + discount)) discount = total + discount; // The original subtotal
    }
    
    const payload = {
        customerId: currentCustomer ? currentCustomer.id : null,
        discount: discount,
        amountReceived: received,
        paymentMethod: 'Cash',
        cartItems: cart.map(c => ({ productId: c.productId, quantity: c.quantity }))
    };
    
    try {
        const res = await axios.post('/api/orders/checkout', payload);
        alert('Thanh toán thành công!');
        
        // Reset POS
        cart = [];
        currentCustomer = null;
        document.getElementById('customerPhone').value = '';
        document.getElementById('customerInfo').classList.add('d-none');
        if(checkbox) checkbox.checked = false;
        document.getElementById('amountReceived').value = '';
        renderCart();
        loadProducts(); // reload stock
    } catch (error) {
        console.error('Lỗi thanh toán', error);
        alert('Lỗi thanh toán: ' + (error.response?.data?.message || 'Unknown error'));
    }
}
