let productModal;
let productsList = [];
let categoriesList = [];

document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/pages/login.html';
        return;
    }
    
    productModal = new bootstrap.Modal(document.getElementById('productModal'));
    loadCategories();
    fetchProducts();
});

async function loadCategories() {
    try {
        const res = await axios.get('/api/categories');
        categoriesList = res.data;
        const filterSelect = document.getElementById('categoryFilter');
        const modalSelect = document.getElementById('productCategoryId');
        
        filterSelect.innerHTML = '<option value="">Tất cả danh mục</option>';
        modalSelect.innerHTML = '';
        
        categoriesList.forEach(c => {
            filterSelect.innerHTML += `<option value="${c.id}">${c.name}</option>`;
            modalSelect.innerHTML += `<option value="${c.id}">${c.name}</option>`;
        });
    } catch (error) {
        console.error('Failed to load categories for products', error);
    }
}

async function fetchProducts() {
    try {
        const res = await axios.get('/api/products?pageNumber=1&pageSize=1000');
        productsList = res.data.data || res.data; // Handle PagedResponse
        renderProducts(productsList);
    } catch (error) {
        console.error('Failed to load products', error);
        alert('Lỗi khi tải danh sách sản phẩm');
    }
}

function renderProducts(products) {
    const tbody = document.querySelector('#productsTable tbody');
    tbody.innerHTML = '';
    
    if (products.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="text-center py-4 text-muted">Không tìm thấy sản phẩm nào.</td></tr>';
        return;
    }

    products.forEach(p => {
        let stockBadge = '';
        if (p.stockQuantity > 10) stockBadge = `<span class="badge bg-success rounded-pill px-3 py-2">${p.stockQuantity} (Còn hàng)</span>`;
        else if (p.stockQuantity > 0) stockBadge = `<span class="badge bg-warning text-dark rounded-pill px-3 py-2">${p.stockQuantity} (Sắp hết)</span>`;
        else stockBadge = `<span class="badge bg-danger rounded-pill px-3 py-2">0 (Hết hàng)</span>`;

        const catName = categoriesList.find(c => c.id === p.categoryId)?.name || 'Không xác định';

        tbody.innerHTML += `
            <tr>
                <td><img src="${p.imageUrl || 'https://via.placeholder.com/50'}" class="product-img" alt="Product"></td>
                <td class="fw-medium text-secondary">${p.barcode}</td>
                <td class="fw-bold text-primary">${p.name}</td>
                <td><span class="badge bg-light text-dark border">${catName}</span></td>
                <td class="fw-bold text-success">${p.price.toLocaleString('vi-VN')} VNĐ</td>
                <td>${stockBadge}</td>
                <td class="text-center">
                    <button class="btn btn-sm btn-outline-primary me-1" title="Sửa" onclick="openEditModal(${p.id})">
                        <i class="fa-solid fa-pen-to-square"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger" title="Xóa" onclick="deleteProduct(${p.id})">
                        <i class="fa-solid fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    });
}

function handleSearch(e) {
    const term = e.target.value.toLowerCase();
    const catId = document.getElementById('categoryFilter').value;
    filterData(term, catId);
}

function filterByCategory() {
    const term = document.getElementById('searchInput').value.toLowerCase();
    const catId = document.getElementById('categoryFilter').value;
    filterData(term, catId);
}

function filterData(term, catId) {
    let filtered = productsList;
    if (term) {
        filtered = filtered.filter(p => p.name.toLowerCase().includes(term) || p.barcode.toLowerCase().includes(term));
    }
    if (catId) {
        filtered = filtered.filter(p => p.categoryId == catId);
    }
    renderProducts(filtered);
}

function openAddModal() {
    document.getElementById('productForm').reset();
    document.getElementById('productId').value = '';
    document.getElementById('productImage').value = '';
    document.getElementById('productImageFile').value = '';
    document.getElementById('imagePreview').src = 'https://via.placeholder.com/50';
    document.getElementById('productModalTitle').innerHTML = '<i class="fa-solid fa-box-open me-2"></i>Thêm Sản Phẩm';
}

function openEditModal(id) {
    const p = productsList.find(x => x.id === id);
    if (!p) return;
    
    document.getElementById('productId').value = p.id;
    document.getElementById('productName').value = p.name;
    document.getElementById('productBarcode').value = p.barcode;
    document.getElementById('productCategoryId').value = p.categoryId;
    document.getElementById('productPrice').value = p.price;
    document.getElementById('productStock').value = p.stockQuantity;
    document.getElementById('productImage').value = p.imageUrl || '';
    document.getElementById('productImageFile').value = '';
    document.getElementById('imagePreview').src = p.imageUrl || 'https://via.placeholder.com/50';
    
    document.getElementById('productModalTitle').innerHTML = '<i class="fa-solid fa-pen-to-square me-2"></i>Sửa Sản Phẩm';
    productModal.show();
}

function previewImage(event) {
    const file = event.target.files[0];
    if (file) {
        document.getElementById('imagePreview').src = URL.createObjectURL(file);
    }
}

async function saveProduct() {
    const id = document.getElementById('productId').value;
    
    // Upload image first if selected
    const fileInput = document.getElementById('productImageFile');
    let finalImageUrl = document.getElementById('productImage').value;

    if (fileInput.files.length > 0) {
        const formData = new FormData();
        formData.append('file', fileInput.files[0]);
        try {
            const uploadRes = await axios.post('/api/uploads/image', formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            finalImageUrl = uploadRes.data.url;
        } catch (error) {
            console.error('Image upload failed', error);
            alert('Lỗi khi tải ảnh lên: ' + (error.response?.data?.message || 'Unknown Error'));
            return; // Stop saving product if upload fails
        }
    }

    const data = {
        name: document.getElementById('productName').value,
        barcode: document.getElementById('productBarcode').value,
        categoryId: parseInt(document.getElementById('productCategoryId').value),
        price: parseFloat(document.getElementById('productPrice').value),
        stockQuantity: parseInt(document.getElementById('productStock').value),
        imageUrl: finalImageUrl
    };
    
    if (!data.name || !data.barcode || isNaN(data.price) || isNaN(data.stockQuantity)) {
        alert('Vui lòng điền đủ thông tin hợp lệ');
        return;
    }
    
    try {
        if (id) {
            await axios.put(`/api/products/${id}`, data);
            alert('Cập nhật sản phẩm thành công!');
        } else {
            await axios.post('/api/products', data);
            alert('Tạo sản phẩm thành công!');
        }
        productModal.hide();
        fetchProducts();
    } catch (error) {
        console.error('Failed to save product', error);
        alert('Lỗi lưu sản phẩm: ' + (error.response?.data?.message || 'Unknown Error'));
    }
}

async function deleteProduct(id) {
    if (!confirm('Bạn có chắc chắn muốn xóa sản phẩm này?')) return;
    
    try {
        await axios.delete(`/api/products/${id}`);
        alert('Xóa sản phẩm thành công!');
        fetchProducts();
    } catch (error) {
        console.error('Delete product error', error);
        alert('Lỗi xóa sản phẩm');
    }
}
