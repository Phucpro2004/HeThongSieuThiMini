document.addEventListener('DOMContentLoaded', () => {
    const header = document.querySelector('header');
    
    // Header scroll effect
    window.addEventListener('scroll', () => {
        if (window.scrollY > 50) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }
    });

    // --- Authentication & Role Management ---
    const authModal = document.getElementById('auth-modal');
    const btnLoginModal = document.getElementById('btn-login-modal');
    const closeAuth = document.getElementById('close-auth');
    const authForm = document.getElementById('auth-form');
    const authTitle = document.getElementById('auth-title');
    const authSwitchBtn = document.getElementById('auth-switch-btn');
    const authSwitchText = document.getElementById('auth-switch-text');
    const authError = document.getElementById('auth-error');
    const userSection = document.getElementById('user-section');
    const navAdmin = document.getElementById('nav-admin');

    let isLoginMode = true;

    // Open Modal
    if (btnLoginModal) {
        btnLoginModal.addEventListener('click', () => {
            authModal.style.display = 'flex';
            authError.style.display = 'none';
        });
    }

    // Close Modal
    closeAuth.addEventListener('click', () => {
        authModal.style.display = 'none';
    });
    window.addEventListener('click', (e) => {
        if (e.target === authModal) {
            authModal.style.display = 'none';
        }
    });

    // Mobile Menu Toggle
    const mobileMenuBtn = document.getElementById('mobile-menu-btn');
    const mainNav = document.querySelector('nav ul');
    if (mobileMenuBtn && mainNav) {
        mobileMenuBtn.addEventListener('click', () => {
            mainNav.classList.toggle('active');
        });
    }

    // Switch Login/Register
    authSwitchBtn.addEventListener('click', (e) => {
        e.preventDefault();
        isLoginMode = !isLoginMode;
        const confirmPasswordGroup = document.getElementById('confirm-password-group');
        const confirmPasswordInput = document.getElementById('confirm-password');

        if (isLoginMode) {
            authTitle.textContent = 'Login';
            authSwitchText.textContent = "Don't have an account?";
            authSwitchBtn.textContent = 'Register';
            confirmPasswordGroup.style.display = 'none';
            confirmPasswordInput.required = false;
        } else {
            authTitle.textContent = 'Register';
            authSwitchText.textContent = "Already have an account?";
            authSwitchBtn.textContent = 'Login';
            confirmPasswordGroup.style.display = 'block';
            confirmPasswordInput.required = true;
        }
        authError.style.display = 'none';
    });

    // Toggle Password Visibility
    document.querySelectorAll('.toggle-password').forEach(icon => {
        icon.addEventListener('click', function() {
            const targetId = this.getAttribute('data-target');
            const input = document.getElementById(targetId);
            if (input.type === 'password') {
                input.type = 'text';
                this.setAttribute('name', 'eye-off-outline');
            } else {
                input.type = 'password';
                this.setAttribute('name', 'eye-outline');
            }
        });
    });

    // Handle Form Submit
    authForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;
        
        if (!isLoginMode) {
            const confirmPassword = document.getElementById('confirm-password').value;
            if (password !== confirmPassword) {
                authError.textContent = 'Passwords do not match.';
                authError.style.display = 'block';
                return;
            }
        }

        const endpoint = isLoginMode ? '/api/Auth/login' : '/api/Auth/register';

        try {
            const res = await fetch(endpoint, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, password })
            });
            const data = await res.json();

            if (!res.ok) {
                authError.textContent = data.message || 'An error occurred';
                authError.style.display = 'block';
                return;
            }

            if (isLoginMode) {
                localStorage.setItem('token', data.token);
                authModal.style.display = 'none';
                checkAuthState();
            } else {
                // Registration successful, switch to login
                authError.textContent = 'Registration successful! Please login.';
                authError.style.color = '#00ff00';
                authError.style.display = 'block';
                setTimeout(() => {
                    authSwitchBtn.click();
                }, 1500);
            }
        } catch (err) {
            authError.textContent = 'Network error. Please try again.';
            authError.style.display = 'block';
        }
    });

    function decodeJWT(token) {
        try {
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
                return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
            }).join(''));
            return JSON.parse(jsonPayload);
        } catch (e) {
            return null;
        }
    }

    function checkAuthState() {
        const token = localStorage.getItem('token');
        if (token) {
            const payload = decodeJWT(token);
            if (payload) {
                // Usually claims are URL schemas, but we can look for "role" or the full schema
                const role = payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
                const username = payload.unique_name || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
                
                let adminLink = '';
                if (role === 'Admin') {
                    const navAdmin = document.getElementById('nav-admin');
                    if (navAdmin) navAdmin.style.display = 'none'; // hide it from the old spot
                    adminLink = `<a href="admin.html" style="color: var(--primary); font-weight: bold; border: 1px solid var(--primary); padding: 5px 15px; border-radius: 15px; text-decoration: none; margin-right: 10px;">Admin</a>`;
                }

                userSection.innerHTML = `
                    ${adminLink}
                    <a href="profile.html" style="color: var(--secondary); font-weight: 600; text-decoration: none; margin-right: 15px;">Hi, ${username}</a>
                    <button id="btn-logout" class="btn btn-outline" style="padding: 5px 15px; font-size: 0.8rem; border: 1px solid #ff4d4d; color: #ff4d4d; background: transparent; border-radius: 15px; cursor: pointer;">Logout</button>
                `;

                document.getElementById('btn-logout').addEventListener('click', () => {
                    localStorage.removeItem('token');
                    location.reload();
                });
                return;
            }
        }
        
        // Not logged in state
        userSection.innerHTML = `
            <button id="btn-login-modal" class="btn btn-outline" style="padding: 8px 20px; font-size: 0.9rem; border: 1px solid var(--primary); color: white; background: transparent; border-radius: 20px; cursor: pointer;">Login</button>
        `;
        navAdmin.style.display = 'none';
        document.getElementById('btn-login-modal').addEventListener('click', () => {
            authModal.style.display = 'flex';
        });
    }

    // --- Dynamic Products Fetching & Filtering ---
    let allProducts = [];

    async function loadCategories() {
        try {
            const res = await fetch('/api/Categories');
            if (!res.ok) return;
            const categories = await res.json();
            const filterCategory = document.getElementById('filter-category');
            if (filterCategory) {
                categories.forEach(c => {
                    const opt = document.createElement('option');
                    opt.value = c.name;
                    opt.textContent = c.name;
                    opt.style.color = 'black';
                    filterCategory.appendChild(opt);
                });
            }
        } catch (e) {
            console.error('Failed to load categories', e);
        }
    }

    async function loadProducts() {
        const productsGrid = document.getElementById('products-grid');
        try {
            const res = await fetch('/api/Products');
            if (!res.ok) throw new Error('Failed to fetch');
            
            const data = await res.json();
            allProducts = data.items || data;
            
            renderProductsGrid(allProducts);
        } catch (err) {
            productsGrid.innerHTML = '<p style="text-align:center; grid-column: 1/-1; color:#ff4d4d;">Failed to load products.</p>';
            console.error(err);
        }
    }

    function renderProductsGrid(products) {
        const productsGrid = document.getElementById('products-grid');
        if (products.length === 0) {
            productsGrid.innerHTML = '<p style="text-align:center; grid-column: 1/-1;">No products found.</p>';
            return;
        }

        productsGrid.innerHTML = products.map(p => `
            <div class="product-card" onclick="openProductDetail(${p.id})" style="cursor: pointer;">
                <div class="product-img-wrap">
                    <img src="${p.imageUrl || '../images/product1.png'}" alt="${p.name}">
                </div>
                <div class="product-info">
                    <span class="product-category">${p.categoryName || 'General'}</span>
                    <h3>${p.name}</h3>
                    <div class="product-price">$${p.price.toFixed(2)}</div>
                    <button class="add-to-cart" data-id="${p.id}" onclick="event.stopPropagation()">Add to Cart</button>
                </div>
            </div>
        `).join('');

        attachCartEvents();
    }

    const productDetailModal = document.getElementById('product-detail-modal');
    const closeProductDetail = document.getElementById('close-product-detail');
    if(closeProductDetail) closeProductDetail.addEventListener('click', () => productDetailModal.style.display = 'none');

    window.openProductDetail = function(id) {
        const product = allProducts.find(p => p.id === id);
        if(!product) return;
        
        document.getElementById('detail-img').src = product.imageUrl || '../images/product1.png';
        document.getElementById('detail-category').textContent = product.categoryName || 'General';
        document.getElementById('detail-name').textContent = product.name;
        document.getElementById('detail-price').textContent = `$${product.price.toFixed(2)}`;
        document.getElementById('detail-desc').textContent = product.description || 'No description available.';
        
        const addToCartBtn = document.getElementById('detail-add-cart');
        addToCartBtn.setAttribute('data-id', product.id);
        
        productDetailModal.style.display = 'flex';
        
        // Re-attach cart event specifically for the modal button if not already attached
        // Or just let attachCartEvents() handle all
        attachCartEvents();
    };

    function applyFilters() {
        const searchText = (document.getElementById('search-product')?.value || '').toLowerCase();
        const selectedCategory = document.getElementById('filter-category')?.value || '';

        const filtered = allProducts.filter(p => {
            const matchSearch = p.name.toLowerCase().includes(searchText) || (p.description || '').toLowerCase().includes(searchText);
            const matchCategory = selectedCategory ? (p.categoryName === selectedCategory) : true;
            return matchSearch && matchCategory;
        });

        renderProductsGrid(filtered);
    }

    // Attach filter events
    document.getElementById('search-product')?.addEventListener('input', applyFilters);
    document.getElementById('filter-category')?.addEventListener('change', applyFilters);

    // --- Cart System ---
    let cart = [];
    const cartCountEl = document.querySelector('.cart-count');
    const cartModal = document.getElementById('cart-modal');
    const closeCart = document.getElementById('close-cart');
    const btnCartModal = document.getElementById('btn-cart-modal');
    const cartItemsEl = document.getElementById('cart-items');
    const cartTotalEl = document.getElementById('cart-total');
    const btnCheckout = document.getElementById('btn-checkout');

    if (btnCartModal) {
        btnCartModal.addEventListener('click', () => {
            renderCart();
            cartModal.style.display = 'flex';
        });
    }
    if (closeCart) {
        closeCart.addEventListener('click', () => cartModal.style.display = 'none');
    }

    function updateCartCount() {
        const count = cart.reduce((acc, item) => acc + item.quantity, 0);
        cartCountEl.textContent = count;
        cartCountEl.style.transform = 'scale(1.5)';
        setTimeout(() => cartCountEl.style.transform = 'scale(1)', 150);
    }

    function renderCart() {
        if (cart.length === 0) {
            cartItemsEl.innerHTML = '<p style="text-align: center; color: var(--text-muted);">Your cart is empty.</p>';
            cartTotalEl.textContent = '$0.00';
            return;
        }

        let total = 0;
        cartItemsEl.innerHTML = cart.map((item, index) => {
            const itemTotal = item.product.price * item.quantity;
            total += itemTotal;
            return `
                <div style="display: flex; justify-content: space-between; align-items: center; background: rgba(255,255,255,0.05); padding: 10px; border-radius: 8px; margin-bottom: 10px;">
                    <div>
                        <h4 style="margin: 0; font-size: 1rem;">${item.product.name}</h4>
                        <span style="font-size: 0.85rem; color: var(--text-muted);">$${item.product.price.toFixed(2)} x ${item.quantity}</span>
                    </div>
                    <div style="display: flex; align-items: center; gap: 15px;">
                        <span style="font-weight: bold; color: white;">$${itemTotal.toFixed(2)}</span>
                        <ion-icon name="trash-outline" style="color: #ff4d4d; cursor: pointer; font-size: 1.2rem;" onclick="removeFromCart(${index})"></ion-icon>
                    </div>
                </div>
            `;
        }).join('');
        cartTotalEl.textContent = `$${total.toFixed(2)}`;
    }

    window.removeFromCart = function(index) {
        cart.splice(index, 1);
        updateCartCount();
        renderCart();
    };

    function attachCartEvents() {
        const cartButtons = document.querySelectorAll('.add-to-cart');
        
        cartButtons.forEach(btn => {
            // Remove previous event listeners to avoid duplication
            const newBtn = btn.cloneNode(true);
            btn.parentNode.replaceChild(newBtn, btn);
            
            newBtn.addEventListener('click', function(e) {
                const button = e.target;
                const productId = parseInt(button.getAttribute('data-id'));
                const product = allProducts.find(p => p.id === productId);
                
                if (product) {
                    const existingItem = cart.find(i => i.product.id === productId);
                    if (existingItem) {
                        existingItem.quantity++;
                    } else {
                        cart.push({ product: product, quantity: 1 });
                    }
                    
                    // Visual feedback
                    button.innerHTML = 'Added! ✓';
                    button.style.background = 'linear-gradient(90deg, #00d2ff, #3a7bd5)';
                    
                    updateCartCount();
                    
                    setTimeout(() => {
                        button.innerHTML = 'Add to Cart';
                        button.style.background = '';
                    }, 2000);
                }
            });
        });
    }

    // Payment Method Toggle Logic
    const paymentMethodSelect = document.getElementById('payment-method');
    const creditCardDetails = document.getElementById('credit-card-details');
    const bankTransferDetails = document.getElementById('bank-transfer-details');
    const vnPayDetails = document.vnpayDetails || document.getElementById('vnpay-details');

    if (paymentMethodSelect) {
        paymentMethodSelect.addEventListener('change', function() {
            const val = this.value;
            creditCardDetails.style.display = 'none';
            bankTransferDetails.style.display = 'none';
            vnPayDetails.style.display = 'none';

            if (val === 'Credit Card') {
                creditCardDetails.style.display = 'block';
            } else if (val === 'Bank Transfer') {
                bankTransferDetails.style.display = 'block';
            } else if (val === 'VNPay') {
                vnPayDetails.style.display = 'block';
            }
        });
    }


    if (btnCheckout) {
        btnCheckout.addEventListener('click', async () => {
            const token = localStorage.getItem('token');
            if (!token) {
                cartModal.style.display = 'none';
                authModal.style.display = 'flex';
                return;
            }

            if (cart.length === 0) return;

            const paymentMethod = document.getElementById('payment-method').value;
            const items = cart.map(item => ({ productId: item.product.id, quantity: item.quantity }));
            
            const msgEl = document.getElementById('checkout-msg');
            msgEl.style.color = 'white';
            msgEl.textContent = 'Processing...';

            try {
                const res = await fetch('/api/Orders', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${token}`
                    },
                    body: JSON.stringify({ paymentMethod: paymentMethod, items: items })
                });

                if (res.ok) {
                    msgEl.style.color = '#00ff00';
                    msgEl.textContent = 'Order placed successfully!';
                    cart = [];
                    updateCartCount();
                    setTimeout(() => {
                        window.location.href = 'profile.html';
                    }, 1500);
                } else {
                    msgEl.style.color = '#ff4d4d';
                    msgEl.textContent = 'Checkout failed.';
                }
            } catch (err) {
                msgEl.style.color = '#ff4d4d';
                msgEl.textContent = 'Network error.';
            }
        });
    }

    // Initialize
    checkAuthState();
    loadCategories();
    loadProducts();
});

// Global callback for Google Sign-In
window.handleGoogleCallback = async function(response) {
    const token = response.credential;
    try {
        const res = await fetch('/api/Auth/google-login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ token: token })
        });

        const data = await res.json();
        if (res.ok) {
            localStorage.setItem('token', data.token);
            document.getElementById('auth-modal').style.display = 'none';
            // update UI manually or reload
            alert('Google Login Successful!');
            window.location.reload();
        } else {
            const err = document.getElementById('auth-error');
            if(err) {
                err.textContent = data.message || 'Google login failed';
                err.style.display = 'block';
            } else {
                alert(data.message || 'Google login failed');
            }
        }
    } catch (err) {
        console.error('Google Login Error:', err);
    }
};
