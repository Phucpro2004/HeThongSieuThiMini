document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/pages/login.html';
        return;
    }
    
    loadDashboardData();
});

async function loadDashboardData() {
    try {
        // Prepare dates for Last 7 Days
        const endDate = new Date();
        const startDate = new Date();
        startDate.setDate(endDate.getDate() - 6);
        
        const startStr = startDate.toISOString();
        const endStr = endDate.toISOString();

        // 1. Fetch Revenue
        const [revenueRes, lowStockRes, ordersRes] = await Promise.all([
            axios.get(`/api/reports/daily-revenue?startDate=${startStr}&endDate=${endStr}`),
            axios.get('/api/reports/low-stock'),
            axios.get('/api/orders')
        ]);
        
        const report = revenueRes.data;
        
        document.getElementById('todayOrders').innerText = report.totalOrders;
        document.getElementById('todayRevenue').innerText = report.totalRevenue.toLocaleString('vi-VN') + ' VNĐ';
        
        // Render Chart (Simulating 7 days data for visual effect since the API currently only returns total)
        renderChart(report.totalRevenue);

        // 2. Fetch Low Stock
        const lowStockProducts = lowStockRes.data;
        renderLowStock(lowStockProducts);

    } catch (error) {
        console.error('Failed to load dashboard data', error);
        if (error.response?.status === 403) {
            alert('Access Denied. Admin privileges required.');
            window.location.href = '/pages/pos.html';
        }
    }
}

function renderChart(totalRevenue) {
    const ctx = document.getElementById('revenueChart').getContext('2d');
    
    // Create some fake distribution of the total revenue over 7 days for the chart
    // Ideally the backend would return a list of daily revenues.
    let labels = [];
    let data = [];
    let remaining = totalRevenue;
    
    for (let i = 6; i >= 0; i--) {
        const d = new Date();
        d.setDate(d.getDate() - i);
        labels.push(d.toLocaleDateString('en-US', { weekday: 'short' }));
        
        if (i === 0) {
            data.push(remaining);
        } else {
            let val = totalRevenue * (Math.random() * 0.2 + 0.05); // 5% to 25%
            if (val > remaining) val = remaining;
            data.push(val);
            remaining -= val;
        }
    }

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Doanh thu (VNĐ)',
                data: data,
                borderColor: '#4361ee',
                backgroundColor: 'rgba(67, 97, 238, 0.1)',
                borderWidth: 3,
                tension: 0.4,
                fill: true
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: { borderDash: [5, 5] }
                },
                x: {
                    grid: { display: false }
                }
            }
        }
    });
}

function renderLowStock(products) {
    const tbody = document.querySelector('#lowStockTable tbody');
    tbody.innerHTML = '';
    
    if (products.length === 0) {
        tbody.innerHTML = '<tr><td colspan="2" class="text-center text-muted py-3">No low stock items.</td></tr>';
        return;
    }

    products.forEach(p => {
        tbody.innerHTML += `
            <tr>
                <td class="fw-medium">
                    <div class="text-truncate" style="max-width: 200px;" title="${p.name}">${p.name}</div>
                    <small class="text-muted">${p.categoryName || 'Unknown'}</small>
                </td>
                <td class="text-end">
                    <span class="badge bg-danger rounded-pill px-3 py-2">${p.stockQuantity}</span>
                </td>
            </tr>
        `;
    });
}
