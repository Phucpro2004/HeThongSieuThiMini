// File: security.js
// Logic phân quyền khắt khe

document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('token');
    
    // Nếu chưa đăng nhập, đá về login
    if (!token && window.location.pathname !== '/pages/login.html') {
        window.location.href = '/pages/login.html';
        return;
    }

    // Nếu đã đăng nhập, kiểm tra Role
    if (token) {
        try {
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
                return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
            }).join(''));
            
            const payload = JSON.parse(jsonPayload);
            const roleClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            const role = payload[roleClaim] || payload.role || 'Cashier';

            const currentPath = window.location.pathname.toLowerCase();
            
            // Các trang chỉ dành cho Admin
            const adminPages = ['admin-dashboard.html', 'users.html', 'product.html', 'category.html', 'customers.html', 'orders.html'];
            
            let isAdminPage = false;
            adminPages.forEach(page => {
                if (currentPath.includes(page)) {
                    isAdminPage = true;
                }
            });

            // Nếu là Cashier mà cố tình vào trang Admin -> Đá về POS
            if (role !== 'Admin' && isAdminPage) {
                alert("Bạn không có quyền truy cập trang này!");
                window.location.href = '/pages/pos.html';
            }
            
            // Nếu là Admin hoặc Cashier đã vào trang login -> Đá về trang tương ứng
            if (currentPath.includes('login.html')) {
                if (role === 'Admin') {
                    window.location.href = '/pages/admin-dashboard.html';
                } else {
                    window.location.href = '/pages/pos.html';
                }
            }

        } catch (e) {
            console.error("Lỗi xác thực Token:", e);
            localStorage.removeItem('token');
            window.location.href = '/pages/login.html';
        }
    }
});
