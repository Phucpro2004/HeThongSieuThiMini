const API_BASE_URL = '/api';

// Set up Axios interceptor for JWT
axios.interceptors.request.use(
    config => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers['Authorization'] = 'Bearer ' + token;
        }
        return config;
    },
    error => {
        return Promise.reject(error);
    }
);

axios.interceptors.response.use(
    response => response,
    error => {
        if (error.response && error.response.status === 401) {
            // Only redirect if not already on the login page
            if (!window.location.pathname.includes('login.html')) {
                localStorage.removeItem('token');
                window.location.href = '/pages/login.html';
            }
        }
        return Promise.reject(error);
    }
);

function logout() {
    localStorage.removeItem('token');
    window.location.href = '/pages/login.html';
}

function checkAuth() {
    const token = localStorage.getItem('token');
    if (!token && window.location.pathname !== '/pages/login.html') {
        window.location.href = '/pages/login.html';
    } else if (token && window.location.pathname === '/pages/login.html') {
        window.location.href = '/pages/pos.html';
    }
}

function parseJwt(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch (e) {
        return null;
    }
}

function applyUnifiedUI() {
    const token = localStorage.getItem('token');
    if (!token) return;

    const payload = parseJwt(token);
    if (!payload) return;

    // The Name Claim is typically formatted like this in .NET JWT:
    const nameClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
    const roleClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    const userName = payload[nameClaim] || payload.unique_name || 'User';
    let role = payload[roleClaim] || payload.role || 'Cashier';
    
    // Map roles to Vietnamese for better UX
    let roleDisplay = role === 'Admin' ? 'Quản trị viên' : 'Thu ngân';
    let roleBadge = role === 'Admin' ? 'bg-danger' : 'bg-success';

    // 1. Inject User Profile in Header
    const userProfileDiv = document.querySelector('.user-profile');
    if (userProfileDiv) {
        userProfileDiv.innerHTML = `
            <div class="text-end me-3 d-none d-md-block">
                <div class="fw-bold text-dark" style="font-size: 15px;">Xin chào, ${userName}</div>
                <span class="badge ${roleBadge} rounded-pill" style="font-size: 11px;">${roleDisplay}</span>
            </div>
            <div class="bg-primary text-white rounded-circle d-flex justify-content-center align-items-center shadow-sm" style="width: 45px; height: 45px; font-size: 1.2rem;">
                <i class="fa-solid fa-user"></i>
            </div>
        `;
    }

    // 2. Inject POS Header Profile (if in pos.html)
    const posHeaderControls = document.querySelector('.pos-header .d-flex.align-items-center:last-child');
    if (posHeaderControls) {
         // It doesn't exist uniquely by default, but we can target the div with logout button
    }
    
    // Alternative approach for POS Header
    const posHeader = document.querySelector('.pos-header');
    if (posHeader && !document.querySelector('.pos-user-badge')) {
        const div = document.createElement('div');
        div.className = 'pos-user-badge me-4 text-end d-none d-md-block';
        div.innerHTML = `
            <div class="fw-bold text-white" style="font-size: 15px;">Xin chào, ${userName}</div>
            <span class="badge ${roleBadge} rounded-pill bg-opacity-75" style="font-size: 11px;">${roleDisplay}</span>
        `;
        // Insert right before the buttons container
        const buttonsContainer = posHeader.querySelector('div:last-child');
        posHeader.insertBefore(div, buttonsContainer);

        // Nút Về trang quản trị (Admin only)
        if (role === 'Admin') {
            const adminBtn = document.createElement('a');
            adminBtn.href = '/pages/admin-dashboard.html';
            adminBtn.className = 'btn btn-warning btn-sm me-2 fw-bold text-dark';
            adminBtn.innerHTML = '🔙 Về trang Quản trị';
            buttonsContainer.insertBefore(adminBtn, buttonsContainer.firstChild);
        }
    }

    // 3. Inject Unified Sidebar
    const sidebar = document.querySelector('.sidebar');
    if (sidebar) {
        // Cập nhật background color theo yêu cầu (Sáng)
        sidebar.style.backgroundColor = '#ffffff';
        sidebar.style.boxShadow = '2px 0 10px rgba(0,0,0,0.05)';
        sidebar.style.borderRight = '1px solid #eee';
        
        // Inject dynamic CSS for sidebar links to override page styles
        if (!document.getElementById('dynamicSidebarCss')) {
            const style = document.createElement('style');
            style.id = 'dynamicSidebarCss';
            style.innerHTML = `
                .sidebar .logo { 
                    padding: 20px; 
                    font-size: 1.5rem; 
                    font-weight: 700; 
                    color: #333 !important; 
                    border-bottom: 1px solid #eee !important; 
                    text-align: center;
                }
                .sidebar .nav-link { color: #495057 !important; transition: all 0.3s; margin: 2px 10px; border-radius: 8px; padding: 12px 20px !important; border-right: none !important;}
                .sidebar .nav-link:hover { background-color: #f8f9fa !important; color: #0d6efd !important; }
                .sidebar .nav-link.active { background-color: #e7f1ff !important; color: #0d6efd !important; font-weight: 600; }
            `;
            document.head.appendChild(style);
        }
        
        const currentPath = window.location.pathname.toLowerCase();
        sidebar.innerHTML = `
            <div class="logo">
                <i class="fa-solid fa-store me-2 text-primary"></i>MiniShop
            </div>
            <nav class="nav flex-column mt-3">
                ${role === 'Admin' ? `
                <a class="nav-link ${currentPath.includes('admin-dashboard') ? 'active' : ''}" href="/pages/admin-dashboard.html">
                    <i class="fa-solid fa-chart-pie"></i> Dashboard
                </a>
                ` : ''}
                <a class="nav-link ${currentPath.includes('category.html') ? 'active' : ''}" href="/pages/category.html">
                    <i class="fa-solid fa-folder-open"></i> Quản lý Danh mục
                </a>
                <a class="nav-link ${currentPath.includes('product.html') ? 'active' : ''}" href="/pages/product.html">
                    <i class="fa-solid fa-box-open"></i> Quản lý Sản phẩm
                </a>
                <a class="nav-link ${currentPath.includes('orders.html') ? 'active' : ''}" href="/pages/orders.html">
                    <i class="fa-solid fa-receipt"></i> Lịch sử Hóa đơn
                </a>
                ${role === 'Admin' ? `
                <a class="nav-link ${currentPath.includes('users.html') ? 'active' : ''}" href="/pages/users.html">
                    <i class="fa-solid fa-users-gear"></i> Quản lý Nhân sự
                </a>
                ` : ''}
                <a class="nav-link ${currentPath.includes('pos.html') ? 'active' : ''}" href="/pages/pos.html">
                    <i class="fa-solid fa-cart-shopping"></i> POS Bán hàng
                </a>
                <a class="nav-link text-danger mt-4 border-top pt-3 mx-3" href="#" onclick="logout()" style="border-radius: 0;">
                    <i class="fa-solid fa-door-open"></i> Đăng xuất
                </a>
            </nav>
        `;
    }
}

// Check auth on load
document.addEventListener('DOMContentLoaded', () => {
    checkAuth();
    applyUnifiedUI();
});
