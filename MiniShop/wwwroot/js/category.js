let categoryModal;
let categoriesList = [];

document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/pages/login.html';
        return;
    }
    
    categoryModal = new bootstrap.Modal(document.getElementById('categoryModal'));
    fetchCategories();
});

async function fetchCategories() {
    try {
        const res = await axios.get('/api/categories');
        categoriesList = res.data;
        const tbody = document.querySelector('#categoriesTable tbody');
        tbody.innerHTML = '';
        
        if (categoriesList.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4" class="text-center py-4 text-muted">Không có danh mục nào.</td></tr>';
            return;
        }

        categoriesList.forEach(c => {
            tbody.innerHTML += `
                <tr>
                    <td><span class="badge bg-light text-dark border">#${c.id}</span></td>
                    <td class="fw-bold text-primary">${c.name}</td>
                    <td class="text-muted">${c.description || '-'}</td>
                    <td class="text-center">
                        <button class="btn btn-sm btn-outline-primary me-1" title="Sửa" onclick="openEditModal(${c.id})">
                            <i class="fa-solid fa-pen-to-square"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger" title="Xóa" onclick="deleteCategory(${c.id})">
                            <i class="fa-solid fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
        });
    } catch (error) {
        console.error('Failed to load categories', error);
        alert('Lỗi khi tải danh sách danh mục');
    }
}

function openAddModal() {
    document.getElementById('categoryForm').reset();
    document.getElementById('categoryId').value = '';
    document.getElementById('categoryModalTitle').innerHTML = '<i class="fa-solid fa-folder-plus me-2"></i>Thêm Danh Mục';
}

function openEditModal(id) {
    const cat = categoriesList.find(c => c.id === id);
    if (!cat) return;
    
    document.getElementById('categoryId').value = cat.id;
    document.getElementById('categoryName').value = cat.name;
    document.getElementById('categoryDescription').value = cat.description;
    document.getElementById('categoryModalTitle').innerHTML = '<i class="fa-solid fa-pen-to-square me-2"></i>Sửa Danh Mục';
    
    categoryModal.show();
}

async function saveCategory() {
    const id = document.getElementById('categoryId').value;
    const name = document.getElementById('categoryName').value;
    const description = document.getElementById('categoryDescription').value;
    
    if (!name) {
        alert('Vui lòng nhập tên danh mục');
        return;
    }
    
    try {
        if (id) {
            // Update
            await axios.put(`/api/categories/${id}`, { name, description });
            alert('Cập nhật danh mục thành công!');
        } else {
            // Create
            await axios.post('/api/categories', { name, description });
            alert('Tạo danh mục thành công!');
        }
        
        categoryModal.hide();
        fetchCategories();
    } catch (error) {
        console.error('Failed to save category', error);
        alert('Lỗi lưu danh mục: ' + (error.response?.data?.message || 'Unknown Error'));
    }
}

async function deleteCategory(id) {
    if (!confirm('Bạn có chắc chắn muốn xóa danh mục này? Tất cả sản phẩm trong danh mục có thể bị ảnh hưởng.')) return;
    
    try {
        await axios.delete(`/api/categories/${id}`);
        alert('Xóa danh mục thành công!');
        fetchCategories();
    } catch (error) {
        console.error('Delete category error', error);
        alert('Lỗi xóa danh mục. Có thể danh mục này đang chứa sản phẩm.');
    }
}
