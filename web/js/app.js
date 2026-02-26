// Automatically use the current host (works for localhost and network IP)
const API_BASE_URL = window.location.origin;

window.debounce = function (func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};

window.formatMoney = function (val) {
    if (val === undefined || val === null || isNaN(val)) return '0,00';
    return parseFloat(val).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
};

window.showToast = function (message, type = 'success') {
    const container = document.getElementById('toastContainer');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `toast-notification ${type}`;

    let icon = '🔔';
    if (type === 'success') icon = '✅';
    if (type === 'error') icon = '❌';
    if (type === 'warning') icon = '⚠️';

    toast.innerHTML = `<span style="font-size:1.2rem;">${icon}</span> <span>${message}</span>`;
    container.appendChild(toast);

    // Auto remove
    setTimeout(() => {
        toast.classList.add('fade-out');
        setTimeout(() => toast.remove(), 500);
    }, 4000);
};

window.restoreEntity = async function (type, id) {
    if (!(await showConfirm('Geri Yükle', 'Bu öğeyi tekrar aktif hale getirmek istediğinize emin misiniz?'))) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/system/restore/${type}/${id}`, {
            method: 'POST'
        });
        if (res.ok) {
            showToast('Öğe başarıyla geri yüklendi');
            // Refresh current view based on type
            const t = type.toLowerCase();
            if (t === 'product') loadProducts();
            else if (t === 'warehouse') loadWarehouses();
            else if (t === 'brand') loadBrands();
            else if (t === 'category') loadCategories();
            else if (t === 'unit') loadUnits();
            else if (t === 'supplier') loadSuppliers();
            else if (t === 'customercompany') loadCompanies();
            else if (t === 'customer') loadCustomers();
            else if (t === 'offer') loadOffers();
            else if (t === 'invoice') loadInvoices();
            else if (t === 'user') loadUsers();
            else if (t === 'company') loadCompanies();
        } else {
            const err = await res.text();
            showToast('Geri yükleme başarısız: ' + err, 'error');
        }
    } catch (e) {
        showToast('Hata: ' + e.message, 'error');
    }
}

// Intercept all fetch requests to add user headers
const originalFetch = window.fetch;
window.fetch = async function (input, init) {
    init = init || {};
    init.headers = init.headers || {};

    const userJson = localStorage.getItem('user');
    if (userJson) {
        try {
            const user = JSON.parse(userJson);

            // Helper to sanitize header values (Browser fetch fails with non-ISO-8859-1 chars like 'ı')
            const safeValue = (val) => {
                if (!val) return "";
                let s = String(val);
                const map = {
                    'ğ': 'g', 'Ğ': 'G',
                    'ü': 'u', 'Ü': 'U',
                    'ş': 's', 'Ş': 'S',
                    'ı': 'i', 'İ': 'I',
                    'ö': 'o', 'Ö': 'O',
                    'ç': 'c', 'Ç': 'C'
                };
                Object.keys(map).forEach(key => {
                    s = s.replace(new RegExp(key, 'g'), map[key]);
                });
                // Final fallback: remove anything still non-ASCII to prevent fetch crash
                return s.replace(/[^\x00-\x7F]/g, "");
            };

            const userId = String(user.id);
            const userName = safeValue(user.userName);
            const userRole = safeValue(user.role);

            if (init.headers instanceof Headers) {
                init.headers.append('X-User-Id', userId);
                init.headers.append('X-User-Name', userName);
                init.headers.append('X-User-Role', userRole);
            } else if (Array.isArray(init.headers)) {
                init.headers.push(['X-User-Id', userId]);
                init.headers.push(['X-User-Name', userName]);
                init.headers.push(['X-User-Role', userRole]);
            } else {
                init.headers['X-User-Id'] = userId;
                init.headers['X-User-Name'] = userName;
                init.headers['X-User-Role'] = userRole;
            }
        } catch (e) {
            console.error("Auth header injection failed", e);
        }
    }
    return originalFetch(input, init);
};

document.addEventListener('DOMContentLoaded', () => {
    checkApiStatus();

    const loginForm = document.getElementById('loginForm');
    if (loginForm) loginForm.addEventListener('submit', handleLogin);

    if (window.location.pathname.includes('dashboard.html')) {
        checkAuth();
        loadDashboard();

        // Restore last view
        const lastView = localStorage.getItem('lastView') || 'dashboard';
        const lastTab = localStorage.getItem('lastTab');
        if (lastView) {
            window.switchView(lastView, lastTab);
        }

        // Check if we were in the middle of a restart
        if (localStorage.getItem('isRestarting') === 'true') {
            localStorage.removeItem('isRestarting');
            showToast("Sistem geri geldi.", "success");
        }

        document.querySelectorAll('.menu-item').forEach(item => {
            item.addEventListener('click', (e) => {
                const text = e.target.innerText;
                if (text.includes('Dashboard') || text.includes('Anasayfa')) switchView('dashboard');
                else if (text.includes('Kullanıcı')) switchView('users');
                else if (text.includes('Ayarlar')) switchView('settings');
                else if (text.includes('Teklifler')) switchView('offers');
                else if (text.includes('Faturalar')) switchView('invoices');
                else if (text.includes('Stoklar')) switchView('inventory');
                else if (text.includes('Stoklar')) switchView('inventory');
                else if (text.includes('Şirketler')) switchView('companies');
                else if (text.includes('Denetim Kayıtları')) switchView('logs');

                document.querySelectorAll('.menu-item').forEach(i => i.classList.remove('active'));
                item.classList.add('active');
            });
        });

        loadSystemInfo(true);

        const invSearchInput = document.getElementById('inventorySearchInput');
        if (invSearchInput) {
            invSearchInput.addEventListener('input', window.debounce(() => {
                const activeTabBtn = document.querySelector('.tab-inv-btn.active');
                if (!activeTabBtn) return;
                const tabName = activeTabBtn.getAttribute('data-tab');

                if (tabName === 'products') loadProducts();
                else if (tabName === 'warehouses') loadWarehouses();
                else if (tabName === 'brands') loadBrands();
                else if (tabName === 'categories') loadCategories();
                else if (tabName === 'units') loadUnits();
            }, 500));
        }
    }
});

// CORE
async function checkApiStatus() {
    const statusDot = document.getElementById('apiStatus');
    const statusText = document.getElementById('apiStatusText');
    if (!statusDot) return;

    try {
        await fetch(`${API_BASE_URL}/swagger/index.html`, { method: 'HEAD' });
        document.querySelectorAll('.status-dot').forEach(el => el.className = 'status-dot status-online');
        if (statusText) {
            statusText.innerText = 'Sunucu Bağlantısı Aktif';
            statusText.style.color = 'var(--success)';
        }
    } catch (error) {
        document.querySelectorAll('.status-dot').forEach(el => el.className = 'status-dot status-offline');
        if (statusText) {
            statusText.innerText = 'Sunucuya Bağlanılamıyor!';
            statusText.style.color = 'var(--error)';
        }
    }
}

async function handleLogin(e) {
    e.preventDefault();
    const usernameInput = document.getElementById('username');
    const passwordInput = document.getElementById('password');
    const errorMsg = document.getElementById('errorMessage');
    const btnText = document.getElementById('btnText');
    const btnSpinner = document.getElementById('btnSpinner');

    btnText.style.display = 'none';
    btnSpinner.style.display = 'inline-block';
    errorMsg.style.display = 'none';

    try {
        const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                userName: usernameInput.value,
                password: passwordInput.value
            })
        });

        if (!response.ok) throw new Error('Giriş başarısız');

        const data = await response.json();
        localStorage.setItem('user', JSON.stringify(data));
        window.location.href = 'dashboard.html';

    } catch (error) {
        errorMsg.style.display = 'block';
        errorMsg.innerText = 'Kullanıcı adı veya şifre hatalı!';
    } finally {
        btnText.style.display = 'inline';
        btnSpinner.style.display = 'none';
    }
}

window.showForgotModal = function () {
    document.getElementById('forgotModal').style.display = 'flex';
}

window.closeForgotModal = function () {
    document.getElementById('forgotModal').style.display = 'none';
    const msg = document.getElementById('forgotMsg');
    if (msg) msg.style.display = 'none';
}

// FINANCE REPORTS CODE
let financeCharts = {};
window.loadFinanceReports = async function () {
    if (!window.hasPermission('FinanceReport', 'read')) {
        showToast('Finans raporlarını görüntüleme yetkiniz yok.', 'error');
        return;
    }

    const financeReportTypeDropdown = document.getElementById('financeReportType');
    if (financeReportTypeDropdown && !financeReportTypeDropdown.hasAttribute('data-listener-added')) {
        financeReportTypeDropdown.setAttribute('data-listener-added', 'true');
        financeReportTypeDropdown.addEventListener('change', function (e) {
            const logisticsFilters = document.getElementById('financeLogisticsFilters');
            if (e.target.value === 'logisticsPerformance') {
                if (logisticsFilters) logisticsFilters.style.display = 'flex';
                // Kullanıcıları bir defaya mahsus çek
                const userFilter = document.getElementById('financeFilterUser');
                if (userFilter && userFilter.options.length <= 1) {
                    fetch(`${API_BASE_URL}/api/users?pageSize=500`)
                        .then(res => res.json())
                        .then(pagedData => {
                            const users = pagedData.items || [];
                            users.forEach(u => {
                                const opt = document.createElement('option');
                                opt.value = u.id;
                                opt.text = `${u.firstName} ${u.lastName} (${u.userName})`;
                                userFilter.appendChild(opt);
                            });
                        }).catch(err => console.error("Kullanıcılar çekilemedi", err));
                }
            } else {
                if (logisticsFilters) logisticsFilters.style.display = 'none';
            }
        });

        // Initial trigger
        financeReportTypeDropdown.dispatchEvent(new Event('change'));
    }

    const reportType = document.getElementById('financeReportType')?.value || 'userPerformance';
    const startDate = document.getElementById('financeStartDate')?.value;
    const endDate = document.getElementById('financeEndDate')?.value;

    document.getElementById('report-container-userPerformance').style.display = 'none';
    document.getElementById('report-container-profitability').style.display = 'none';
    document.getElementById('report-container-dailyTrend').style.display = 'none';
    document.getElementById('report-container-agingInventory').style.display = 'none';
    if (document.getElementById('report-container-logisticsPerformance')) {
        document.getElementById('report-container-logisticsPerformance').style.display = 'none';
    }
    if (document.getElementById('report-container-incompleteDeliveries')) {
        document.getElementById('report-container-incompleteDeliveries').style.display = 'none';
    }
    if (document.getElementById('report-container-transferHistory')) {
        document.getElementById('report-container-transferHistory').style.display = 'none';
    }

    document.getElementById(`report-container-${reportType}`).style.display = 'block';

    let queryParams = [];
    if (startDate) queryParams.push(`startDate=${startDate}`);
    if (endDate) queryParams.push(`endDate=${endDate}`);

    if (reportType === 'logisticsPerformance') {
        const invNo = document.getElementById('financeFilterInvoiceNo')?.value;
        const uId = document.getElementById('financeFilterUser')?.value;
        if (invNo) queryParams.push(`invoiceNumber=${encodeURIComponent(invNo)}`);
        if (uId) queryParams.push(`delivererUserId=${uId}`);
    }

    const qStr = queryParams.length > 0 ? '?' + queryParams.join('&') : '';

    try {
        if (reportType === 'userPerformance') {
            const tbodyUsers = document.getElementById('financeReportUsersBody');
            tbodyUsers.innerHTML = '<tr><td colspan="5" style="text-align:center;">Rapor getiriliyor...</td></tr>';

            const res = await fetch(`${API_BASE_URL}/api/financereport/user-performance${qStr}`);
            if (!res.ok) throw new Error("Rapor verileri çekilemedi. Yetkinizi kontrol edin.");
            const usersData = await res.json();

            tbodyUsers.innerHTML = '';
            if (usersData.length === 0) {
                tbodyUsers.innerHTML = '<tr><td colspan="5" style="text-align:center;">Data bulunamadı.</td></tr>';
            } else {
                usersData.forEach(r => {
                    const tr = document.createElement('tr');
                    tr.innerHTML = `
                        <td>${r.userName}</td>
                        <td>${r.totalOffersPrepared}</td>
                        <td>${r.pendingOffersCount}</td>
                        <td>${r.invoicedOffersCount}</td>
                        <td>${formatMoney(r.totalOffersVolume)} ₺</td>
                    `;
                    tbodyUsers.appendChild(tr);
                });
            }

            if (financeCharts['userVolume']) financeCharts['userVolume'].destroy();
            const ctxUser = document.getElementById('chartUserVolume').getContext('2d');
            financeCharts['userVolume'] = new Chart(ctxUser, {
                type: 'bar',
                data: {
                    labels: usersData.map(u => u.userName),
                    datasets: [{
                        label: 'Hazırlanan Teklif Hacmi (₺)',
                        data: usersData.map(u => u.totalOffersVolume),
                        backgroundColor: 'rgba(59, 130, 246, 0.6)',
                        borderColor: 'rgba(59, 130, 246, 1)',
                        borderWidth: 1
                    }]
                },
                options: { responsive: true, maintainAspectRatio: false }
            });

            if (financeCharts['conversionPie']) financeCharts['conversionPie'].destroy();
            const ctxPie = document.getElementById('chartConversionPie').getContext('2d');
            const totalPending = usersData.reduce((sum, u) => sum + u.pendingOffersCount, 0);
            const totalInvoiced = usersData.reduce((sum, u) => sum + u.invoicedOffersCount, 0);

            financeCharts['conversionPie'] = new Chart(ctxPie, {
                type: 'doughnut',
                data: {
                    labels: ['Faturaya Dönenler', 'Bekleyen / Açık'],
                    datasets: [{
                        data: [totalInvoiced, totalPending],
                        backgroundColor: ['#22c55e', '#f59e0b'],
                    }]
                },
                options: { responsive: true, maintainAspectRatio: false }
            });
        }
        else if (reportType === 'profitability') {
            const tbodyProfit = document.getElementById('financeReportProfitBody');
            tbodyProfit.innerHTML = '<tr><td colspan="5" style="text-align:center;">Rapor getiriliyor...</td></tr>';

            const res = await fetch(`${API_BASE_URL}/api/financereport/profitability${qStr}`);
            if (!res.ok) throw new Error("Rapor verileri çekilemedi.");
            const profitData = await res.json();

            tbodyProfit.innerHTML = '';
            if (profitData.length === 0) {
                tbodyProfit.innerHTML = '<tr><td colspan="5" style="text-align:center;">Data bulunamadı.</td></tr>';
            } else {
                profitData.forEach(r => {
                    const tr = document.createElement('tr');
                    tr.innerHTML = `
                        <td>${r.productName || '-'}</td>
                        <td>${r.totalSoldQuantity}</td>
                        <td>${formatMoney(r.totalCost)} ₺</td>
                        <td>${formatMoney(r.totalRevenue)} ₺</td>
                        <td style="color:${r.totalProfit < 0 ? '#ef4444' : 'var(--success)'}; font-weight:bold;">${formatMoney(r.totalProfit)} ₺</td>
                    `;
                    tbodyProfit.appendChild(tr);
                });
            }

            if (financeCharts['profitLine']) financeCharts['profitLine'].destroy();
            const ctxLine = document.getElementById('chartProfitabilityLine').getContext('2d');
            const topProfits = profitData.slice(0, 10);
            financeCharts['profitLine'] = new Chart(ctxLine, {
                type: 'bar', // Better for comparative items than a line chart
                data: {
                    labels: topProfits.map(p => p.productName?.substring(0, 15) + '..'),
                    datasets: [{
                        label: 'Net Kar (₺)',
                        data: topProfits.map(p => p.totalProfit),
                        backgroundColor: 'rgba(139, 92, 246, 0.6)',
                        borderColor: '#8b5cf6',
                        borderWidth: 1
                    }]
                },
                options: { responsive: true, maintainAspectRatio: false }
            });
        }
        else if (reportType === 'dailyTrend') {
            const tbodyDaily = document.getElementById('financeReportDailyBody');
            tbodyDaily.innerHTML = '<tr><td colspan="4" style="text-align:center;">Rapor getiriliyor...</td></tr>';

            const res = await fetch(`${API_BASE_URL}/api/financereport/daily-trend${qStr}`);
            if (!res.ok) throw new Error("Günlük trend verileri çekilemedi.");
            const trendData = await res.json();

            tbodyDaily.innerHTML = '';
            if (trendData.length === 0) {
                tbodyDaily.innerHTML = '<tr><td colspan="4" style="text-align:center;">Data bulunamadı.</td></tr>';
            } else {
                trendData.forEach(r => {
                    const tr = document.createElement('tr');
                    tr.innerHTML = `
                        <td>${r.date}</td>
                        <td>${formatMoney(r.totalCost)} ₺</td>
                        <td>${formatMoney(r.totalRevenue)} ₺</td>
                        <td style="color:${r.totalProfit < 0 ? '#ef4444' : 'var(--success)'}; font-weight:bold;">${formatMoney(r.totalProfit)} ₺</td>
                    `;
                    tbodyDaily.appendChild(tr);
                });
            }

            if (financeCharts['dailyTrend']) financeCharts['dailyTrend'].destroy();
            const ctxLine = document.getElementById('chartDailyTrend').getContext('2d');
            financeCharts['dailyTrend'] = new Chart(ctxLine, {
                type: 'line',
                data: {
                    labels: trendData.map(p => p.date),
                    datasets: [
                        {
                            label: 'Ciro (₺)',
                            data: trendData.map(p => p.totalRevenue),
                            borderColor: '#3b82f6',
                            backgroundColor: 'rgba(59, 130, 246, 0.1)',
                            fill: true,
                            tension: 0.3
                        },
                        {
                            label: 'Net Kar (₺)',
                            data: trendData.map(p => p.totalProfit),
                            borderColor: '#10b981',
                            backgroundColor: 'rgba(16, 185, 129, 0.1)',
                            fill: true,
                            tension: 0.3
                        }
                    ]
                },
                options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true } } }
            });
        }
        else if (reportType === 'agingInventory') {
            const tbodyAging = document.getElementById('financeReportAgingBody');
            tbodyAging.innerHTML = '<tr><td colspan="4" style="text-align:center;">Rapor getiriliyor...</td></tr>';

            const res = await fetch(`${API_BASE_URL}/api/financereport/aging-inventory`);
            if (!res.ok) throw new Error("Stok yaş analizi verileri çekilemedi.");
            const agingData = await res.json();

            tbodyAging.innerHTML = '';
            if (agingData.length === 0) {
                tbodyAging.innerHTML = '<tr><td colspan="4" style="text-align:center;">Data bulunamadı.</td></tr>';
            } else {
                agingData.forEach(r => {
                    const tr = document.createElement('tr');
                    tr.innerHTML = `
                        <td>${r.productName}</td>
                        <td style="font-weight:bold;">${r.currentStock}</td>
                        <td>${r.oldestEntryDate}</td>
                        <td style="color:${r.daysInStock > 90 ? '#ef4444' : (r.daysInStock > 30 ? '#f59e0b' : '#22c55e')}; font-weight:bold; text-align:right;">${r.daysInStock} Gün</td>
                    `;
                    tbodyAging.appendChild(tr);
                });
            }

            if (financeCharts['agingBar']) financeCharts['agingBar'].destroy();
            const ctxAging = document.getElementById('chartAgingBar').getContext('2d');

            const topRows = agingData.slice(0, 15);

            financeCharts['agingBar'] = new Chart(ctxAging, {
                type: 'bar',
                data: {
                    labels: topRows.map(p => p.productName.substring(0, 20) + '...'),
                    datasets: [{
                        label: 'Bekleme Süresi (Gün)',
                        data: topRows.map(p => p.daysInStock),
                        backgroundColor: topRows.map(p => p.daysInStock > 90 ? 'rgba(239, 68, 68, 0.7)' : (p.daysInStock > 30 ? 'rgba(245, 158, 11, 0.7)' : 'rgba(34, 197, 94, 0.7)')),
                        borderColor: topRows.map(p => p.daysInStock > 90 ? '#ef4444' : (p.daysInStock > 30 ? '#f59e0b' : '#22c55e')),
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: { y: { beginAtZero: true, title: { display: true, text: 'Gün' } } }
                }
            });
        }
        else if (reportType === 'logisticsPerformance') {
            const tbodyLogistics = document.getElementById('financeReportLogisticsBody');
            tbodyLogistics.innerHTML = '<tr><td colspan="5" style="text-align:center;">Rapor getiriliyor...</td></tr>';

            const res = await fetch(`${API_BASE_URL}/api/financereport/logistics-performance${qStr}`);
            if (!res.ok) throw new Error("Lojistik performansı verileri çekilemedi.");
            const logData = await res.json();

            tbodyLogistics.innerHTML = '';
            if (logData.length === 0) {
                tbodyLogistics.innerHTML = '<tr><td colspan="5" style="text-align:center;">Data bulunamadı.</td></tr>';
            } else {
                logData.forEach(r => {
                    const tr = document.createElement('tr');

                    let statColor = '#94a3b8';
                    if (r.status === 'Delivered') statColor = '#22c55e';
                    else if (r.status === 'InPreparation') statColor = '#f59e0b';
                    else if (r.status === 'WaitingForWarehouse') statColor = '#3b82f6';

                    tr.innerHTML = `
                        <td><strong>${r.invoiceNumber}</strong><br><span style="font-size:0.8rem; color:#64748b;">${r.issueDate}</span></td>
                        <td>${r.delivererName}</td>
                        <td style="text-align:center; font-weight:bold; color:${r.waitTimeMinutes > 60 ? '#ef4444' : '#333'}">${r.waitTimeMinutes != null ? r.waitTimeMinutes + ' dk' : '-'}</td>
                        <td style="text-align:center; font-weight:bold; color:${r.preparationTimeMinutes > 120 ? '#ef4444' : '#333'}">${r.preparationTimeMinutes != null ? r.preparationTimeMinutes + ' dk' : '-'}</td>
                        <td style="color:${statColor}; font-weight:bold; text-align:right;">${r.status}</td>
                    `;
                    tbodyLogistics.appendChild(tr);
                });
            }

            if (financeCharts['logisticsBar']) financeCharts['logisticsBar'].destroy();
            const ctxLog = document.getElementById('chartLogisticsBar').getContext('2d');

            // Ortalama hazırlama süresini personel bazında hesapla
            const personalStats = {};
            logData.forEach(r => {
                if (r.delivererName && r.delivererName !== 'Atanmadı' && r.preparationTimeMinutes != null) {
                    if (!personalStats[r.delivererName]) personalStats[r.delivererName] = { count: 0, totalPrep: 0 };
                    personalStats[r.delivererName].count++;
                    personalStats[r.delivererName].totalPrep += r.preparationTimeMinutes;
                }
            });

            const labels = Object.keys(personalStats);
            const avgData = labels.map(l => personalStats[l].totalPrep / personalStats[l].count);
            const counts = labels.map(l => personalStats[l].count);

            financeCharts['logisticsBar'] = new Chart(ctxLog, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [
                        {
                            label: 'Ortalama Hazırlama Süresi (Dk)',
                            data: avgData,
                            backgroundColor: 'rgba(59, 130, 246, 0.7)',
                            borderColor: '#3b82f6',
                            borderWidth: 1,
                            yAxisID: 'y'
                        },
                        {
                            label: 'Tamamlanan Sipariş Adedi',
                            data: counts,
                            backgroundColor: 'rgba(16, 185, 129, 0.7)',
                            borderColor: '#10b981',
                            borderWidth: 1,
                            yAxisID: 'y1'
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: { type: 'linear', display: true, position: 'left', title: { display: true, text: 'Dakika' } },
                        y1: { type: 'linear', display: true, position: 'right', title: { display: true, text: 'Adet' }, grid: { drawOnChartArea: false } }
                    }
                }
            });
        }
        else if (reportType === 'incompleteDeliveries') {
            const tbodyIncs = document.getElementById('financeReportIncompleteBody');
            tbodyIncs.innerHTML = '<tr><td colspan="4" style="text-align:center;">Rapor getiriliyor...</td></tr>';

            const res = await fetch(`${API_BASE_URL}/api/financereport/incomplete-deliveries${qStr}`);
            if (!res.ok) throw new Error("Yarım kalan sevkiyatları çekerken hata oluştu.");
            const incData = await res.json();

            tbodyIncs.innerHTML = '';
            if (incData.length === 0) {
                tbodyIncs.innerHTML = '<tr><td colspan="4" style="text-align:center;">Data bulunamadı.</td></tr>';
            } else {
                incData.forEach(r => {
                    const tr = document.createElement('tr');
                    tr.innerHTML = `
                        <td><strong>${r.invoiceNumber}</strong><br><span style="font-size:0.8rem; color:#64748b;">${r.issueDate}</span></td>
                        <td>${r.delivererName}</td>
                        <td>${r.incompleteDate || '-'}</td>
                        <td style="color:${r.waitTimeMinutes > 120 ? '#ef4444' : '#f59e0b'}; font-weight:bold; text-align:right;">
                            ${r.waitTimeMinutes} Dk.
                        </td>
                    `;
                    tbodyIncs.appendChild(tr);
                });
            }

            // Hide the canvas if we don't have a chart for incomplete deliveries right now
            if (financeCharts['incompleteChart']) financeCharts['incompleteChart'].destroy();
            const canvasEq = document.getElementById('chartIncompleteLine');
            if (canvasEq) {
                // Not generating chart right now for this.
            }
        } else if (reportType === 'transferHistory') {
            const tbodyHist = document.getElementById('financeReportTransferHistoryBody');
            tbodyHist.innerHTML = '<tr><td colspan="5" style="text-align:center;">Rapor getiriliyor...</td></tr>';

            const res = await fetch(`${API_BASE_URL}/api/financereport/transfer-history`);
            if (!res.ok) throw new Error("Sevkiyat tarihçesini çekerken hata oluştu.");
            const histData = await res.json();

            tbodyHist.innerHTML = '';
            if (histData.length === 0) {
                tbodyHist.innerHTML = '<tr><td colspan="5" style="text-align:center;">Tarihçe kaydı (Log) bulunamadı.</td></tr>';
            } else {
                histData.forEach(r => {
                    const tr = document.createElement('tr');
                    let badgeColor = r.action.includes('Transfer') ? '#8b5cf6' :
                        r.action.includes('Unassign') ? '#ef4444' :
                            r.action.includes('Assign') ? '#3b82f6' : '#9ca3af';

                    tr.innerHTML = `
                        <td><strong>${r.invoiceNumber}</strong><br><span style="font-size:0.8rem; color:#64748b;">Kesim: ${r.issueDate}</span></td>
                        <td>${r.userName}</td>
                        <td style="font-weight:bold; color:${badgeColor};">${r.action}</td>
                        <td>${r.status}</td>
                        <td>${r.logDate}</td>
                    `;
                    tbodyHist.appendChild(tr);
                });
            }
        }
    } catch (e) {
        showToast(e.message, 'error');
    }
}

window.exportFinanceReportToExcel = function () {
    const reportType = document.getElementById('financeReportType')?.value || 'userPerformance';
    let tableId = '';
    let fileName = '';

    if (reportType === 'userPerformance') {
        tableId = 'financeReportUsersBody';
        fileName = 'kullanici_performans_raporu';
    } else if (reportType === 'profitability') {
        tableId = 'financeReportProfitBody';
        fileName = 'urun_karlilik_raporu';
    } else if (reportType === 'dailyTrend') {
        tableId = 'financeReportDailyBody';
        fileName = 'gunluk_ciro_kar_analizi';
    } else if (reportType === 'agingInventory') {
        tableId = 'financeReportAgingBody';
        fileName = 'stok_yas_analizi';
    } else if (reportType === 'logisticsPerformance') {
        tableId = 'financeReportLogisticsBody';
        fileName = 'lojistik_sevkiyat_performans';
    } else if (reportType === 'incompleteDeliveries') {
        tableId = 'financeReportIncompleteBody';
        fileName = 'yarim_sevkiyatlar_raporu';
    } else if (reportType === 'transferHistory') {
        tableId = 'financeReportTransferHistoryBody';
        fileName = 'sevkiyat_devir_tarihce_raporu';
    }

    const tbody = document.getElementById(tableId);
    if (!tbody || tbody.innerHTML.includes('Rapor getiriliyor...') || tbody.innerHTML.includes('Data bulunamadı')) {
        showToast('Dışa aktarılacak veri bulunamadı.', 'error');
        return;
    }

    const tableElement = tbody.closest('table');
    if (!tableElement) return;

    try {
        const wb = XLSX.utils.table_to_book(tableElement, { sheet: "Sheet1" });
        XLSX.writeFile(wb, `${fileName}.xlsx`);
        showToast("Excel dosyası indirildi.", "success");
    } catch (e) {
        showToast('Excel dışa aktarılırken hata: ' + e.message, 'error');
    }
}

window.exportFinanceReportToPDF = function () {
    const reportType = document.getElementById('financeReportType')?.value || 'userPerformance';
    let tableId = '';
    let title = '';

    if (reportType === 'userPerformance') {
        tableId = 'financeReportUsersBody';
        title = 'Kullanici Performans Raporu';
    } else if (reportType === 'profitability') {
        tableId = 'financeReportProfitBody';
        title = 'Urun Karlilik Raporu';
    } else if (reportType === 'dailyTrend') {
        tableId = 'financeReportDailyBody';
        title = 'Gunluk Ciro ve Kar Analizi';
    } else if (reportType === 'agingInventory') {
        tableId = 'financeReportAgingBody';
        title = 'Stok Yas Analizi';
    } else if (reportType === 'logisticsPerformance') {
        tableId = 'financeReportLogisticsBody';
        title = 'Lojistik ve Sevkiyat Performans Raporu';
    } else if (reportType === 'incompleteDeliveries') {
        tableId = 'financeReportIncompleteBody';
        title = 'Yarım Kalan / Bekleyen Sevkiyatlar';
    } else if (reportType === 'transferHistory') {
        tableId = 'financeReportTransferHistoryBody';
        title = 'Sevkiyat Devir & Tarihçe (Log) Raporu';
    }

    const tbody = document.getElementById(tableId);
    if (!tbody || tbody.innerHTML.includes('Rapor getiriliyor...') || tbody.innerHTML.includes('Data bulunamadı')) {
        showToast('Dışa aktarılacak veri bulunamadı.', 'error');
        return;
    }

    const tableElement = tbody.closest('table');
    if (!tableElement) return;

    try {
        const { jsPDF } = window.jspdf;
        const doc = new jsPDF('landscape');

        let yPos = 20;
        doc.text(title, 14, 15);

        const activeContainer = document.getElementById(`report-container-${reportType}`);
        if (activeContainer) {
            const canvases = activeContainer.querySelectorAll('canvas');
            canvases.forEach((canvas) => {
                // To prevent transparent bg issues, draw on white canvas first
                const whiteCanvas = document.createElement('canvas');
                whiteCanvas.width = canvas.width;
                whiteCanvas.height = canvas.height;
                const ctx = whiteCanvas.getContext('2d');
                ctx.fillStyle = "#ffffff";
                ctx.fillRect(0, 0, whiteCanvas.width, whiteCanvas.height);
                ctx.drawImage(canvas, 0, 0);

                const imgData = whiteCanvas.toDataURL('image/jpeg', 1.0);

                const pdfWidth = 260; // Max width in landscape (297 total)
                const aspect = canvas.height / canvas.width;
                const pdfHeight = pdfWidth * aspect;

                if (yPos + pdfHeight > 190) { // Max height in landscape is ~210
                    doc.addPage();
                    yPos = 20;
                }

                doc.addImage(imgData, 'JPEG', 18, yPos, pdfWidth, pdfHeight);
                yPos += pdfHeight + 10;
            });
        }

        // Next page for the table if it's too squeezed
        if (yPos > 150) {
            doc.addPage();
            yPos = 20;
            doc.text(title + ' (Tablo)', 14, 15);
        }

        doc.autoTable({
            html: tableElement,
            startY: yPos,
            theme: 'grid',
            styles: { font: 'helvetica', fontSize: 10 },
            headStyles: { fillColor: [41, 128, 185], textColor: 255 }
        });

        doc.save(`${title.replace(/ /g, '_')}.pdf`);
        showToast("PDF dosyası indirildi.", "success");
    } catch (e) {
        showToast('PDF dışa aktarılırken hata: ' + e.message, 'error');
    }
}

window.hasPermission = function (module, type) {
    const user = JSON.parse(localStorage.getItem('user'));
    if (!user) return false;
    if (user.id === 1) return true; // Root is god

    const perm = (user.permissions || []).find(p => p.moduleName === module);
    if (!perm) return false;
    if (perm.isFull) return true;

    const t = (type || '').toLowerCase();
    if (t === 'read') return perm.canRead;
    if (t === 'write') return perm.canWrite;
    if (t === 'delete') return perm.canDelete;
    return false;
}

window.requestReset = async function () {
    const email = document.getElementById('forgotEmail').value;
    const msg = document.getElementById('forgotMsg');
    const btn = document.getElementById('resetBtn');

    if (!email) return showToast('Lütfen e-posta adresinizi girin.', 'warning');

    try {
        btn.disabled = true;
        btn.innerText = 'Gönderiliyor...';

        const res = await fetch(`${API_BASE_URL}/api/users/forgot-password`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: email })
        });

        const data = await res.json();
        msg.style.display = 'block';
        msg.innerText = data.message;
        msg.style.color = res.ok ? 'var(--success)' : 'var(--error)';

    } catch (e) {
        msg.style.display = 'block';
        msg.innerText = 'Hata: ' + e.message;
        msg.style.color = 'var(--error)';
    } finally {
        btn.disabled = false;
        btn.innerText = 'Bağlantı Gönder';
    }
}

const ID_MAPPINGS = {
    'UpdatedByUserId': {},
    'CreatedByUserId': {},
    'UserId': {},
    'ActorUserId': {},
    'CategoryId': {},
    'BrandId': {},
    'UnitId': {},
    'WarehouseId': {},
    'TargetWarehouseId': {},
    'CompanyId': {},
    'RoleId': {},
    'ModuleId': {}
};

async function refreshIdMappings() {
    try {
        const userRes = await fetch(`${API_BASE_URL}/api/users?pageSize=1000`);
        if (userRes.ok) {
            const pagedUsers = await userRes.json();
            const users = pagedUsers.items || [];
            users.forEach(u => {
                const name = `${u.firstName} ${u.lastName}`.trim() || u.userName;
                ID_MAPPINGS['UpdatedByUserId'][u.id] = name;
                ID_MAPPINGS['CreatedByUserId'][u.id] = name;
                ID_MAPPINGS['UserId'][u.id] = name;
                ID_MAPPINGS['ActorUserId'][u.id] = name;
            });
        }

        const [catRes, brandRes, unitRes, whRes, companyRes, roleRes] = await Promise.all([
            fetch(`${API_BASE_URL}/api/product/categories`),
            fetch(`${API_BASE_URL}/api/product/brands`),
            fetch(`${API_BASE_URL}/api/product/units`),
            fetch(`${API_BASE_URL}/api/warehouse?pageSize=1000`),
            fetch(`${API_BASE_URL}/api/companies?pageSize=1000`),
            fetch(`${API_BASE_URL}/api/users/roles`)
        ]);

        if (catRes && catRes.ok) (await catRes.json()).forEach(x => ID_MAPPINGS['CategoryId'][x.id] = x.name);
        if (brandRes && brandRes.ok) (await brandRes.json()).forEach(x => ID_MAPPINGS['BrandId'][x.id] = x.name);
        if (unitRes && unitRes.ok) (await unitRes.json()).forEach(x => ID_MAPPINGS['UnitId'][x.id] = x.name);

        if (whRes && whRes.ok) {
            const pagedWh = await whRes.json();
            const whItems = pagedWh.items || [];
            whItems.forEach(x => {
                ID_MAPPINGS['WarehouseId'][x.id] = x.warehouseName;
                ID_MAPPINGS['TargetWarehouseId'][x.id] = x.warehouseName;
            });
        }

        if (companyRes && companyRes.ok) {
            const pagedComp = await companyRes.json();
            const compItems = pagedComp.items || [];
            compItems.forEach(x => ID_MAPPINGS['CompanyId'][x.id] = x.companyName || x.name);
        }

        if (roleRes && roleRes.ok) (await roleRes.json()).forEach(x => ID_MAPPINGS['RoleId'][x.id] = x.name);

    } catch (e) {
        console.warn("Failed to refresh ID mappings", e);
    }
}

function checkAuth() {
    const user = localStorage.getItem('user');
    if (!user) window.location.href = 'index.html';
}

function loadDashboard() {
    const user = JSON.parse(localStorage.getItem('user'));
    const userDisplay = document.getElementById('userNameDisplay');
    const roleDisplay = document.getElementById('userRoleDisplay');
    if (userDisplay && user) userDisplay.innerText = (user.firstName || '') + ' ' + (user.lastName || '');
    if (roleDisplay && user) roleDisplay.innerText = user.role;

    if (user) {
        renderQuickActions();
        refreshIdMappings(); // Pre-fetch names for logs
    }

    applyPermissions();

    // Default View
    switchView('dashboard');
}

function applyPermissions() {
    const userJson = localStorage.getItem('user');
    if (!userJson) return;

    const user = JSON.parse(userJson);
    const userId = user.id;

    // Root user see everything
    if (userId === 1) {
        document.querySelectorAll('.menu-item[data-module]').forEach(item => {
            item.style.display = 'flex';
        });

        // Make sidebar visible after DOM update for root
        const sidebar = document.getElementById('sidebar');
        if (sidebar) sidebar.style.visibility = 'visible';

        return;
    }

    const permissions = user.permissions || [];

    document.querySelectorAll('.menu-item[data-module]').forEach(item => {
        const moduleAttr = item.getAttribute('data-module');
        const modules = moduleAttr.split(',').map(m => m.trim());

        // Find if ANY of the modules has Read permission
        const hasAccess = modules.some(modName => {
            const perm = permissions.find(p => p.moduleName === modName);
            return perm && (perm.canRead || perm.isFull);
        });

        if (!hasAccess) {
            item.style.display = 'none';
        } else {
            item.style.display = 'flex';
        }
    });

    // Hide empty groups
    document.querySelectorAll('.menu-group-content').forEach(group => {
        const items = Array.from(group.querySelectorAll('.menu-item'));
        const visibleItems = items.filter(i => i.style.display !== 'none');
        const label = group.previousElementSibling;

        if (items.length > 0 && visibleItems.length === 0) {
            group.style.display = 'none';
            if (label && label.classList.contains('menu-group-label')) {
                label.style.display = 'none';
            }
        } else {
            if (label && label.classList.contains('menu-group-label')) {
                label.style.display = 'block';
            }
        }
    });

    // Button Level Permissions (e.g. data-permission="Warehouse:Write")
    document.querySelectorAll('[data-permission]').forEach(el => {
        const [mod, type] = el.getAttribute('data-permission').split(':');
        if (!window.hasPermission(mod, type.toLowerCase())) {
            el.style.display = 'none';
        } else {
            // If it's a menu item, we might want 'flex', otherwise 'inline-block' or just remove 'none'
            if (el.classList.contains('menu-item')) {
                el.style.display = 'flex';
            } else {
                el.style.display = ''; // Reset to default CSS
                if (window.getComputedStyle(el).display === 'none') el.style.display = 'inline-block'; // Fallback if CSS is hidden by default
            }
        }
    });

    // Make sidebar visible after DOM update
    const sidebar = document.getElementById('sidebar');
    if (sidebar) sidebar.style.visibility = 'visible';
}

window.logout = function () {
    localStorage.removeItem('user');
    window.location.href = 'index.html';
}

window.showConfirm = function (title, message, yesText = 'Evet, Sil', noText = 'İptal') {
    return new Promise((resolve) => {
        const modal = document.getElementById('deleteConfirmModal');
        if (!modal) {
            // Fallback to native confirm if modal not found
            resolve(confirm(message || title));
            return;
        }
        const titleEl = document.getElementById('confirmTitle');
        const msgEl = document.getElementById('confirmMessage');
        const yesBtn = document.getElementById('confirmYesBtn');
        const noBtn = document.getElementById('confirmNoBtn');

        titleEl.innerText = title || 'Emin misiniz?';
        msgEl.innerText = message || 'Bu işlem geri alınamaz.';
        yesBtn.innerText = yesText;
        noBtn.innerText = noText;
        modal.style.display = 'flex';

        const handleYes = () => {
            cleanup();
            resolve(true);
        };

        const handleNo = () => {
            cleanup();
            resolve(false);
        };

        const cleanup = () => {
            modal.style.display = 'none';
            yesBtn.removeEventListener('click', handleYes);
            noBtn.removeEventListener('click', handleNo);
        };

        yesBtn.addEventListener('click', handleYes, { once: true });
        noBtn.addEventListener('click', handleNo, { once: true });
    });
};

window.currentStatusFilter = {};

window.renderPagination = function (container, totalPages, currentPage, pageSize, onPageChange) {
    if (!container) return;

    // UI creation
    let html = `
        <div style="display: flex; justify-content: center; align-items: center; gap: 20px; margin-top: 10px; font-size: 0.9rem;">
            <div style="display: flex; align-items: center; gap: 5px;">
                <label style="margin:0;">Sayfa Başına:</label>
                <select class="page-size-select" style="padding: 4px; border-radius: 4px; border: 1px solid #ccc; background: white;">
                    <option value="10" ${pageSize === 10 ? 'selected' : ''}>10</option>
                    <option value="20" ${pageSize === 20 ? 'selected' : ''}>20</option>
                    <option value="50" ${pageSize === 50 ? 'selected' : ''}>50</option>
                </select>
            </div>
            
            <div style="display: flex; gap: 5px; align-items:center;">
                <button class="btn-page" data-page="1" ${currentPage === 1 ? 'disabled' : ''}>&laquo;</button>
                <button class="btn-page" data-page="${currentPage - 1}" ${currentPage === 1 ? 'disabled' : ''}>&lsaquo;</button>
                <span style="margin: 0 10px;">Sayfa ${currentPage} / ${totalPages === 0 ? 1 : totalPages}</span>
                <button class="btn-page" data-page="${currentPage + 1}" ${currentPage >= totalPages ? 'disabled' : ''}>&rsaquo;</button>
                <button class="btn-page" data-page="${totalPages}" ${currentPage >= totalPages ? 'disabled' : ''}>&raquo;</button>
            </div>
        </div>
    `;
    container.innerHTML = html;

    // Events
    const select = container.querySelector('.page-size-select');
    select.addEventListener('change', (e) => {
        onPageChange(1, parseInt(e.target.value));
    });

    const btns = container.querySelectorAll('.btn-page');
    btns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            if (btn.hasAttribute('disabled')) return;
            const newPage = parseInt(btn.getAttribute('data-page'));
            if (newPage > 0 && newPage <= totalPages) {
                onPageChange(newPage, pageSize);
            }
        });
    });
};

window.handleSearch = function (callbackName) {
    if (!window[`debounce_${callbackName}`]) {
        window[`debounce_${callbackName}`] = window.debounce(() => {
            if (window[callbackName]) {
                // Reset to page 1 for new search
                const pageVar = callbackName.replace('load', '').toLowerCase() + 'Page';
                if (window[pageVar]) window[pageVar] = 1;
                else {
                    // fallbacks
                    if (callbackName === 'loadProducts') window.productPage = 1;
                    if (callbackName === 'loadUsers') window.userPage = 1;
                    if (callbackName === 'loadCompanies') window.companyPage = 1;
                    if (callbackName === 'loadSuppliers') window.supplierPage = 1;
                    if (callbackName === 'loadInvoices') window.invoicePage = 1;
                    if (callbackName === 'loadOffers') window.offerPage = 1;
                    if (callbackName === 'loadPriceLists') window.priceListPage = 1;
                    if (callbackName === 'loadCustomerCompanies') window.customerCompanyPage = 1;
                    if (callbackName === 'loadCustomers') window.customerPage = 1;
                    if (callbackName === 'loadWarehouses') window.warehousePage = 1;
                }
                window[callbackName]();
            }
        }, 500);
    }
    window[`debounce_${callbackName}`]();
};

window.renderStatusFilter = function (sectionName, containerId, callback) {
    const container = document.getElementById(containerId);
    if (!container) return;

    const current = window.currentStatusFilter[sectionName] || 'active';
    const callbackName = typeof callback === 'function' ? (callback.name || "") : callback;

    const searchInputMap = {
        'users': 'search-users',
        'inventory': 'inventorySearchInput',
        'pricelists': 'search-pricelists',
        'invoices': 'search-invoices',
        'offers': 'search-offers',
        'suppliers': 'search-suppliers',
        'customer-companies': 'search-customer-companies',
        'customers': 'search-customers',
        'companies': 'search-companies'
    };
    const searchId = searchInputMap[sectionName] || `search-${sectionName}`;

    let searchHtml = `
        <div class="search-box-container" style="flex: 1; min-width: 250px;">
            <input type="text" id="${searchId}" class="form-control" placeholder="Ara..." 
                   onkeyup="window.handleSearch('${callbackName}')">
        </div>
    `;

    let filterHtml = "";
    if (window.hasPermission('ShowDeletedItems', 'read')) {
        filterHtml = `
            <div class="status-filter-container">
                <span>💾 Veri Filtresi:</span>
                <div class="status-options">
                    <button class="status-btn ${current === 'active' ? 'active' : ''}" onclick="window.setStatusFilter('${sectionName}', 'active', '${containerId}', '${callbackName}')">Aktif</button>
                    <button class="status-btn ${current === 'passive' ? 'active' : ''}" onclick="window.setStatusFilter('${sectionName}', 'passive', '${containerId}', '${callbackName}')">Pasif (Silinenler)</button>
                    <button class="status-btn ${current === 'all' ? 'active' : ''}" onclick="window.setStatusFilter('${sectionName}', 'all', '${containerId}', '${callbackName}')">Tümü</button>
                </div>
            </div>
        `;
    }

    container.style.display = 'flex';
    container.style.justifyContent = 'space-between';
    container.style.alignItems = 'center';
    container.style.gap = '1rem';
    container.style.flexWrap = 'wrap';

    container.innerHTML = searchHtml + filterHtml;
};

window.setStatusFilter = function (section, status, containerId, callback) {
    window.currentStatusFilter[section] = status;
    // Re-render filter to show active state
    window.renderStatusFilter(section, containerId, callback);
    // Execute data reload
    const callbackFunc = typeof callback === 'string' ? window[callback] : callback;
    if (callbackFunc) callbackFunc();
};

window.switchView = function (viewName, defaultTab) {
    localStorage.setItem('lastView', viewName);
    if (defaultTab) localStorage.setItem('lastTab', defaultTab);

    const currentUser = JSON.parse(localStorage.getItem('user'));

    // Render Status Filter for specific sections if user has permission
    const filterMap = {
        'users': { container: 'filter-container-users', callback: 'loadUsers' },
        'inventory': { container: 'filter-container-inventory', callback: 'loadProducts' },
        'warehouses': { container: 'filter-container-inventory', callback: 'loadWarehouses' },
        'companies': { container: 'filter-container-companies', callback: 'loadCompanies' },
        'shelves': { container: 'filter-container-shelves', callback: 'loadShelves' },
        'offers': { container: 'filter-container-offers', callback: 'loadOffers' },
        'invoices': { container: 'filter-container-invoices', callback: 'loadInvoices' },
        'customers': { container: 'filter-container-customers', callback: 'loadCustomers' },
        'suppliers': { container: 'filter-container-suppliers', callback: 'loadSuppliers' },
        'customer-companies': { container: 'filter-container-customer-companies', callback: 'loadCustomerCompanies' }
    };

    if (filterMap[viewName]) {
        window.renderStatusFilter(viewName, filterMap[viewName].container, filterMap[viewName].callback);
    }

    // Hide filter containers for views that don't use them
    if (viewName !== 'suppliers') document.getElementById('filter-container-suppliers').style.display = 'none';
    if (viewName !== 'customers') document.getElementById('filter-container-customers').style.display = 'none';
    if (viewName !== 'finance-reports') {
        // Hide other filter containers if they exist and are not for the current view
        const allFilterContainers = document.querySelectorAll('.status-filter-container');
        allFilterContainers.forEach(container => {
            if (!filterMap[viewName] || container.id !== filterMap[viewName].container) {
                container.style.display = 'none';
            }
        });
    }


    if (viewName === 'finance-reports') {
        const sel = document.getElementById('financeReportType');
        if (sel) {
            sel.addEventListener('change', loadFinanceReports);
        }
        loadFinanceReports();
    }

    // Permission enforcement based on data-module attribute of the menu item
    const menuItem = document.querySelector(`.menu-item[onclick*="switchView('${viewName}')"]`);
    if (menuItem && menuItem.dataset.module && currentUser && currentUser.id !== 1) {
        const perm = (currentUser.permissions || []).find(p => p.moduleName === menuItem.dataset.module);
        if (!perm || (!perm.canRead && !perm.isFull)) {
            alert('Bu sayfaya erişim yetkiniz bulunmamaktadır.');
            return;
        }
    }

    // Handle Active Menu State
    document.querySelectorAll('.menu-item').forEach(el => el.classList.remove('active'));
    const menuItems = document.querySelectorAll('.menu-item');
    menuItems.forEach(item => {
        if (item.getAttribute('onclick')?.includes(`'${viewName}'`)) {
            item.classList.add('active');
        }
        // Also check for innerText for dynamic menu items
        if (item.innerText.toLowerCase().includes(viewName.toLowerCase())) {
            item.classList.add('active');
        }
    });

    // Close sidebar on mobile
    const sidebar = document.getElementById('sidebar');
    if (sidebar) sidebar.classList.remove('open');

    document.querySelectorAll('.view-section').forEach(el => el.style.display = 'none');
    const target = document.getElementById(`section-${viewName}`);
    if (target) target.style.display = 'block';

    const titleMap = {
        'dashboard': 'Dashboard',
        'users': 'Kullanıcı Yönetimi',
        'settings': 'Sistem Ayarları',
        'offers': 'Teklif Yönetimi',
        'invoices': 'Fatura Yönetimi',
        'invoice-details': 'Fatura Detayı',
        'finance-reports': 'Finans Raporları',
        'inventory': 'Stok Yönetimi',
        'stock-entry': 'Stok Girişi',
        'reports': 'Raporlar',
        'companies': 'Şirket Yönetimi',
        'suppliers': 'Tedarikçi Yönetimi',
        'pricelists': 'Fiyat Listesi Yönetimi',
        'customer-companies': 'Müşteri Şirket Yönetimi',
        'customers': 'Müşteri Yönetimi',
        'logs': 'Denetim Kayıtları',
        'profile': 'Kullanıcı Profili',
        'warehouses': 'Depo Tanımları',
        'waybill-history': 'İrsaliye Geçmişi',
        'titles': 'Ünvan/Bölüm Tanımları',
        'shelves': 'Raf Yönetimi',
        'warehouse-dashboard': 'Depo Paneli'
    };
    const titleEl = document.getElementById('pageTitle');
    if (titleEl) titleEl.innerText = titleMap[viewName] || 'Sayfa';

    if (viewName === 'users') loadUsers();
    if (viewName === 'titles' && window.loadTitles) loadTitles();
    if (viewName === 'warehouses') loadWarehouses();
    if (viewName === 'settings') loadSystemInfo();
    if (viewName === 'offers') loadOffers();
    if (viewName === 'suppliers') loadSuppliers();
    if (viewName === 'pricelists') loadPriceLists();
    if (viewName === 'customer-companies') loadCustomerCompanies();
    if (viewName === 'customers') loadCustomers();
    if (viewName === 'invoices') loadInvoices();
    if (viewName === 'waybill-history') loadWaybillHistoryView();
    if (viewName === 'shelves') loadShelves();
    if (viewName === 'warehouse-dashboard') loadPendingDeliveries();
    if (viewName === 'inventory') {
        if (defaultTab) {
            switchInvTab(defaultTab);
        } else {
            const tabs = Array.from(document.querySelectorAll('#section-inventory .tab-inv-btn'));
            const firstVisible = tabs.find(t => t.style.display !== 'none');
            if (firstVisible) {
                switchInvTab(firstVisible.getAttribute('data-tab'));
            } else {
                // Check if user has permission for products at least
                if (window.hasPermission('Product', 'read')) {
                    switchInvTab('products');
                } else if (window.hasPermission('Category', 'read')) {
                    switchInvTab('categories');
                } else {
                    switchInvTab('products'); // Last resort
                }
            }
        }
    }
    if (viewName === 'stock-entry') loadStockEntry();
    if (viewName === 'reports') initReportsView();
    if (viewName === 'companies') loadCompanies();
    if (viewName === 'dashboard') {
        renderQuickActions();
        loadSystemInfo(); // Refresh system info
    } else if (viewName === 'users') {
        loadUsers();
    } else if (viewName === 'logs') {
        loadLogs();
    } else if (viewName === 'settings') {
        loadSystemInfo(true);
        // Try to load DB settings if root. check inside the function
        loadDbSettings();
    } else if (viewName === 'profile') {
        loadProfile();
    }
}

window.loadStockEntry = async function () {
    const pSel = document.getElementById('seProduct');
    const wSel = document.getElementById('seWarehouse');
    const sSel = document.getElementById('seSupplier');
    if (!pSel || !wSel || !sSel) return;

    pSel.innerHTML = '<option value="">Yükleniyor...</option>';
    wSel.innerHTML = '<option value="">Yükleniyor...</option>';
    sSel.innerHTML = '<option value="">Yükleniyor...</option>';

    try {
        const [productsRes, warehousesRes, suppliersRes] = await Promise.all([
            fetch(`${API_BASE_URL}/api/product?pageSize=1000`).then(async r => r.status === 403 ? null : r.json()),
            fetch(`${API_BASE_URL}/api/warehouse?pageSize=1000`).then(async r => r.status === 403 ? null : r.json()),
            fetch(`${API_BASE_URL}/api/supplier?pageSize=1000`).then(async r => r.status === 403 ? null : r.json())
        ]);

        const products = Array.isArray(productsRes) ? productsRes : (productsRes?.items || []);
        const warehouses = Array.isArray(warehousesRes) ? warehousesRes : (warehousesRes?.items || []);
        const suppliers = Array.isArray(suppliersRes) ? suppliersRes : (suppliersRes?.items || []);

        pSel.innerHTML = '<option value="">Ürün Seçiniz...</option>';
        products.forEach(p => {
            pSel.innerHTML += `<option value="${p.id}">${p.productCode} - ${p.productName} (Mevcut: ${p.currentStock})</option>`;
        });

        wSel.innerHTML = '<option value="">Depo Seçiniz...</option>';
        warehouses.forEach(w => {
            wSel.innerHTML += `<option value="${w.id}">${w.warehouseName}</option>`;
        });

        sSel.innerHTML = '<option value="">Tedarikçi Seçiniz...</option>';
        suppliers.forEach(s => {
            sSel.innerHTML += `<option value="${s.id}">${s.supplierCompanyName}</option>`;
        });
    } catch (e) {
        pSel.innerHTML = `<option value="">Hata!</option>`;
        wSel.innerHTML = `<option value="">Hata!</option>`;
        sSel.innerHTML = `<option value="">Hata!</option>`;
    }
}

window.updateFileLabel = function (input) {
    const label = document.getElementById('seFileLabel');
    if (input.files && input.files[0]) {
        label.innerText = input.files[0].name;
        label.style.color = 'var(--primary)';
    } else {
        label.innerText = 'Dosya seçilmedi (JPG, PNG, PDF)';
        label.style.color = 'var(--muted)';
    }
}

window.loadWaybillHistoryView = async function () {
    const sSel = document.getElementById('whSupplier');
    const typeSel = document.getElementById('whType');
    if (!sSel) return;

    sSel.innerHTML = '<option value="">Yükleniyor...</option>';
    try {
        const type = typeSel ? typeSel.value : 'Gelen';
        let items = [];

        if (type === 'Giden') {
            const res = await fetch(`${API_BASE_URL}/api/customer/companies?pageSize=1000`);
            if (res.ok) {
                const paged = await res.json();
                items = Array.isArray(paged) ? paged : (paged.items || []);
            }
        } else {
            const res = await fetch(`${API_BASE_URL}/api/supplier?pageSize=1000`);
            if (res.ok) {
                const paged = await res.json();
                items = paged.items || [];
            }
        }

        sSel.innerHTML = '<option value="">Tümü</option>';
        if (type === 'Giden') {
            items.forEach(c => {
                sSel.innerHTML += `<option value="${c.id}">${c.customerCompanyName}</option>`;
            });
        } else {
            items.forEach(s => {
                sSel.innerHTML += `<option value="${s.id}">${s.supplierCompanyName}</option>`;
            });
        }

        // Initialize view with all records
        runWaybillHistorySearch();
    } catch (e) {
        sSel.innerHTML = '<option value="">Hata!</option>';
    }
}

window.loadSupplierWaybills = async function (supplierId, source = 'se', extraFilters = {}) {
    const isWaybillHistory = source === 'wh';
    const bodyId = isWaybillHistory ? 'whHistoryBody' : 'waybillHistoryBody';
    const body = document.getElementById(bodyId);

    if (!body) return;

    if (!isWaybillHistory && !supplierId) {
        body.innerHTML = `<tr><td colspan="4" style="text-align:center; padding:5rem; color:#94a3b8;"><div style="font-size:3rem; margin-bottom:1rem;">🔍</div>Tedarikçi seçerek geçmiş irsaliyeleri görüntüleyin</td></tr>`;
        return;
    }

    try {
        body.innerHTML = '<tr><td colspan="5" style="text-align:center; padding:2rem; color: #64748b;">🔄 Yükleniyor...</td></tr>';

        let url = `${API_BASE_URL}/api/stock/waybills/${supplierId || 0}`;
        if (isWaybillHistory) {
            const params = new URLSearchParams();
            if (supplierId) params.append('supplierId', supplierId);
            if (extraFilters.waybillNo) params.append('waybillNo', extraFilters.waybillNo);
            if (extraFilters.startDate) params.append('startDate', extraFilters.startDate);
            if (extraFilters.endDate) params.append('endDate', extraFilters.endDate);
            if (extraFilters.type) params.append('type', extraFilters.type);
            url = `${API_BASE_URL}/api/stock/waybills/search?${params.toString()}`;
        }

        const res = await fetch(url);
        const waybills = await res.json();

        if (waybills.length === 0) {
            body.innerHTML = `<tr><td colspan="5" style="text-align:center; padding:3rem; color:#94a3b8;"><div style="font-size:2rem; margin-bottom:1rem;">📂</div>Arkandığınız kriterlere uygun kayıt bulunamadı.</td></tr>`;
            return;
        }

        body.innerHTML = '';
        waybills.forEach(w => {
            const dateStr = new Date(w.date).toLocaleDateString('tr-TR');
            if (w.documentPath) {
                docLink = `<a href="${API_BASE_URL}${w.documentPath}" target="_blank" class="status-badge status-success" style="text-decoration:none;" onclick="event.stopPropagation()">📄 Görüntüle</a>`;
            } else {
                docLink = '<span class="status-badge" style="background:#f1f5f9; color:#94a3b8;">Belge Yok</span>';
            }

            if (isWaybillHistory && extraFilters.type === 'Giden') {
                docLink = `<button onclick="event.stopPropagation(); downloadWaybillPdf('${w.waybillNo}')" class="btn-action" style="background:#8b5cf6;">💾 PDF İndir</button>`;
            }

            if (isWaybillHistory) {
                body.innerHTML += `
                    <tr onclick="toggleWaybillDetails('${w.waybillNo}', this)" style="cursor:pointer; transition:background 0.2s;">
                        <td style="font-weight:700; color:#1e293b;">
                            <span style="color:var(--primary);">${w.waybillNo}</span>
                        </td>
                        <td>
                            <div style="font-weight:600; color:#475569;">${w.supplierName}</div>
                            <small style="color:#94a3b8;">${w.description || 'Genel Giriş'}</small>
                        </td>
                        <td style="font-size:0.9rem; color:#64748b;">
                            ${dateStr}
                        </td>
                        <td>
                            <span class="qty-badge" style="font-size:0.9rem;">${w.totalQuantity}</span>
                        </td>
                        <td style="text-align:right;">
                            ${docLink}
                        </td>
                    </tr>
                `;
            } else {
                body.innerHTML += `
                    <tr>
                        <td style="font-weight:700; color:#1e293b;">${w.waybillNo}</td>
                        <td>${dateStr}</td>
                        <td><span class="qty-badge">${w.totalQuantity}</span></td>
                        <td>${docLink}</td>
                    </tr>
                `;
            }
        });
    } catch (e) {
        console.error(e);
        body.innerHTML = '<tr><td colspan="5" style="text-align:center; padding:2rem; color: var(--error);">⚠️ Geçmiş yüklenemedi.</td></tr>';
    }
}

window.downloadWaybillPdf = async function (waybillNo) {
    const loadingMsg = document.createElement('div');
    loadingMsg.id = 'tempLoadingMsg';
    loadingMsg.style = 'position:fixed; top:20px; right:20px; background:var(--primary); color:white; padding:1rem 1.5rem; border-radius:8px; z-index:9999; box-shadow:0 4px 16px rgba(0,0,0,0.15); font-weight:600;';
    loadingMsg.innerText = '📄 PDF oluşturuluyor...';
    document.body.appendChild(loadingMsg);

    try {
        const res = await fetch(`${API_BASE_URL}/api/invoice/by-number/${encodeURIComponent(waybillNo)}`);
        if (!res.ok) throw new Error('İrsaliye verisi alınamadı');
        const invoice = await res.json();

        const buyerName = invoice.buyerCompanyName || invoice.receiverName || 'Bilinmiyor';
        const buyerAddress = invoice.buyerCompanyAddress || '';
        const buyerTax = invoice.buyerCompanyTaxInfo || '';

        const html = generateDispatchHtmlFromData(
            invoice.items || [],
            buyerName,
            buyerAddress,
            window.systemInfo,
            {
                invoiceNumber: invoice.invoiceNumber,
                date: invoice.issueDate ? new Date(invoice.issueDate).toLocaleDateString('tr-TR') : new Date().toLocaleDateString('tr-TR'),
                dueDate: invoice.dueDate ? new Date(invoice.dueDate).toLocaleDateString('tr-TR') : '',
                grandTotal: invoice.grandTotal || 0,
                assignedUser: invoice.assignedDelivererUserName || '',
                receiverName: invoice.receiverName || '',
                buyerCompanyTaxInfo: buyerTax
            }
        );

        const printWindow = window.open('', '_blank');
        printWindow.document.write(html);
        printWindow.document.close();

        printWindow.onload = () => {
            const pdfScript = printWindow.document.createElement('script');
            pdfScript.src = 'https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js';
            pdfScript.onload = () => {
                const opt = {
                    margin: 0,
                    filename: `sevk_irsaliyesi_${invoice.invoiceNumber}.pdf`,
                    image: { type: 'jpeg', quality: 0.98 },
                    html2canvas: { scale: 2 },
                    jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' }
                };
                printWindow.html2pdf().set(opt).from(printWindow.document.body).save().then(() => {
                    setTimeout(() => printWindow.close(), 800);
                });
            };
            printWindow.document.head.appendChild(pdfScript);
        };
    } catch (e) {
        alert('Hata: ' + e.message);
        console.error(e);
    } finally {
        const msg = document.getElementById('tempLoadingMsg');
        if (msg) document.body.removeChild(msg);
    }
}

window.runWaybillHistorySearch = function () {
    const sId = document.getElementById('whSupplier').value;
    const waybillNo = document.getElementById('whDocNo').value;
    const startDate = document.getElementById('whStartDate').value;
    const endDate = document.getElementById('whEndDate').value;
    const typeEle = document.getElementById('whType');
    const type = typeEle ? typeEle.value : 'Gelen';

    loadSupplierWaybills(sId, 'wh', { waybillNo, startDate, endDate, type });
}

window.submitStockEntry = async function () {
    const supplierId = document.getElementById('seSupplier').value;
    const productId = document.getElementById('seProduct').value;
    const warehouseId = document.getElementById('seWarehouse').value;
    const documentNo = document.getElementById('seDocumentNo').value;
    const quantity = document.getElementById('seQuantity').value;
    const desc = document.getElementById('seDescription').value;
    const fileInput = document.getElementById('seFile');

    if (!supplierId || !productId || !warehouseId || !quantity || parseFloat(quantity) <= 0) {
        alert('Lütfen Tedarikçi, Ürün, Depo, Miktar ve İrsaliye bilgilerini eksiksiz giriniz.');
        return;
    }

    if (!window.hasPermission('Stock', 'write')) {
        alert('Stok girişi yapma yetkiniz yok!');
        return;
    }

    const currentUser = JSON.parse(localStorage.getItem('user'));

    try {
        const formData = new FormData();
        formData.append('ProductId', parseInt(productId));
        formData.append('WarehouseId', parseInt(warehouseId));
        formData.append('MovementType', 1); // Entry
        formData.append('Quantity', parseFloat(quantity));
        formData.append('Description', desc || 'Hızlı Stok Girişi');
        formData.append('UserId', currentUser ? currentUser.id : 0);
        formData.append('DocumentNo', documentNo || 'IRS-' + Date.now());
        formData.append('SupplierId', parseInt(supplierId));

        if (fileInput.files.length > 0) {
            formData.append('documentFile', fileInput.files[0]);
        }

        const res = await fetch(`${API_BASE_URL}/api/stock/movement`, {
            method: 'POST',
            body: formData // No headers, fetch handles multipart
        });

        if (res.ok) {
            alert('Stok ve İrsaliye bilgisi başarıyla kaydedildi.');
            // Clear fields
            document.getElementById('seDocumentNo').value = '';
            document.getElementById('seQuantity').value = '';
            document.getElementById('seDescription').value = '';
            document.getElementById('seFile').value = '';
            document.getElementById('seFileLabel').innerText = 'Dosya seçilmedi (JPG, PNG, PDF)';

            // Reload history
            loadSupplierWaybills(supplierId);
            // Reload select (to update stock labels)
            loadStockEntry();
        } else {
            const err = await res.json();
            alert('Hata: ' + (err.message || 'İşlem başarısız.'));
        }
    } catch (e) {
        alert('Bağlantı hatası: ' + e.message);
    }
}

window.initReportsView = async function () {
    const wSel = document.getElementById('reportWarehouseFilter');
    if (!wSel) return;

    wSel.innerHTML = '<option value="">Tüm Depolar</option>';
    try {
        const res = await fetch(`${API_BASE_URL}/api/warehouse?pageSize=1000`);
        const pagedData = await res.json();
        const warehouses = Array.isArray(pagedData) ? pagedData : (pagedData.items || []);
        warehouses.forEach(w => {
            wSel.innerHTML += `<option value="${w.id}">${w.warehouseName}</option>`;
        });
        loadStockReport(); // Load everything initially
    } catch (e) {
        console.error('Depolar yüklenemedi', e);
    }
}

window.loadStockReport = async function () {
    const warehouseId = document.getElementById('reportWarehouseFilter').value;
    const codeFilter = document.getElementById('reportProductCodeFilter').value.toLowerCase();
    const nameFilter = document.getElementById('reportProductNameFilter').value.toLowerCase();

    const tbody = document.getElementById('reportListBody');
    if (!tbody) return;

    tbody.innerHTML = '<tr><td colspan="9" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        let url = `${API_BASE_URL}/api/stock/report`;
        if (warehouseId) url += `?warehouseId=${warehouseId}`;

        const res = await fetch(url);
        if (res.status === 403) {
            tbody.innerHTML = '<tr><td colspan="9" style="color:var(--error); text-align:center;">🚫 Yetkisiz Erişim</td></tr>';
            return;
        }
        if (!res.ok) {
            const text = await res.text();
            throw new Error(`Sunucu Hatası (${res.status}): ${text || res.statusText}`);
        }

        let data = await res.json();

        // Client-side filtering for Code and Name
        if (codeFilter) {
            data = data.filter(item => item.productCode && item.productCode.toLowerCase().includes(codeFilter));
        }
        if (nameFilter) {
            data = data.filter(item => item.productName && item.productName.toLowerCase().includes(nameFilter));
        }

        tbody.innerHTML = '';
        if (data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="9" style="text-align:center;">Aradığınız kriterlere uygun kayıt bulunamadı.</td></tr>';
            return;
        }

        data.forEach(item => {
            const realStyle = item.currentStock > 0 ? '#d1fae5;color:#065f46;' : '#fee2e2;color:#991b1b;';
            const resvStyle = item.reservedStock > 0 ? '#fef3c7;color:#92400e;' : '#f1f5f9;color:#64748b;';
            const waitStyle = item.waitingInWarehouseStock > 0 ? '#dbeafe;color:#1e40af;' : '#f1f5f9;color:#64748b;';
            const availStyle = item.availableStock > 0 ? '#dcfce3;color:#166534;' : '#fee2e2;color:#991b1b;';

            tbody.innerHTML += `
                <tr>
                    <td>${item.warehouseName}</td>
                    <td><b>${item.productCode}</b></td>
                    <td>${item.productName}</td>
                    <td style="text-align:center;"><span class="badge" style="background:${realStyle}">${item.currentStock}</span></td>
                    <td style="text-align:center;"><span class="badge" style="background:${resvStyle}">${item.reservedStock}</span></td>
                    <td style="text-align:center;"><span class="badge" style="background:${waitStyle}">${item.waitingInWarehouseStock}</span></td>
                    <td style="text-align:center;"><span class="badge" style="background:${availStyle}">${item.availableStock}</span></td>
                    <td>${item.unitName}</td>
                </tr>
            `;
        });
    } catch (e) {
        tbody.innerHTML = '<tr><td colspan="9" style="text-align:center;color:red;">Hata: ' + e.message + '</td></tr>';
    }
}

window.loadSystemInfo = async function () {
    const currentUser = JSON.parse(localStorage.getItem('user'));

    try {
        const res = await fetch(`${API_BASE_URL}/api/system/info`);
        if (res.ok) {
            const info = await res.json();
            document.getElementById('sysDbStatus').innerText = info.databaseStatus;
            document.getElementById('sysVersion').innerText = info.appVersion;
            document.getElementById('sysServerTime').innerText = new Date(info.serverTime).toLocaleString('tr-TR');
            document.getElementById('sysRuntime').innerText = info.runtime;
            document.getElementById('sysOS').innerText = info.os;

            // Fill Restart Time
            if (info.restartTime && document.getElementById('systemRestartTime')) {
                document.getElementById('systemRestartTime').value = info.restartTime;
            }
        }
    } catch (e) {
        console.error("System info error:", e);
    }

    if (currentUser && currentUser.id === 1) {
        const emailCard = document.getElementById('mailSettingsCard');
        if (emailCard) emailCard.style.display = 'block';

        const dbCard = document.getElementById('dbSettingsCard');
        if (dbCard) dbCard.style.display = 'block';

        const restartCard = document.getElementById('restartSettingsCard');
        if (restartCard) restartCard.style.display = 'block';

        window.loadMailSettings();
        window.loadSettings();
    }
};

window.saveSystemSettings = async function () {
    const restartTime = document.getElementById('systemRestartTime').value;
    const forcePwd = document.getElementById('forceStrongPwd')?.checked || false;
    const barcodeType = document.getElementById('barcodeType')?.value || 'QR';

    try {
        const res = await fetch(`${API_BASE_URL}/api/system/settings`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ forceStrongPassword: forcePwd, barcodeType: barcodeType, restartTime: restartTime })
        });

        if (res.ok) {
            showToast("Sistem ayarları kaydedildi.");
        } else {
            showToast("Ayarlar kaydedilemedi.", "error");
        }
    } catch (e) {
        showToast("Bağlantı hatası.", "error");
    }
};

window.triggerRestart = async function () {
    if (!confirm("Sistem yeniden başlatılacak. Devam etmek istiyor musunuz?")) return;

    try {
        localStorage.setItem('isRestarting', 'true');
        showRestartOverlay();
        const res = await fetch(`${API_BASE_URL}/api/system/restart`, { method: 'POST' });
        if (!res.ok) {
            hideRestartOverlay();
            showToast("Restart başlatılamadı.", "error");
        } else {
            // Polling is started by showRestartOverlay
        }
    } catch (e) {
        console.error(e);
        // During restart, a connection error is expected
    }
};

function showRestartOverlay() {
    const overlay = document.getElementById('maintenanceOverlay');
    if (overlay) {
        overlay.style.display = 'flex';
        pollSystemHealth();
    }
}

function hideRestartOverlay() {
    const overlay = document.getElementById('maintenanceOverlay');
    if (overlay) overlay.style.display = 'none';
}

async function pollSystemHealth() {
    const timerEl = document.getElementById('maintenanceTimer');
    let attempts = 0;

    const interval = setInterval(async () => {
        attempts++;
        if (timerEl) timerEl.innerText = `Bağlantı deneniyor... (${attempts})`;

        try {
            const res = await fetch(`${API_BASE_URL}/api/system/health`);
            if (res.ok) {
                clearInterval(interval);
                if (timerEl) timerEl.innerText = "Bağlantı kuruldu! Yönlendiriliyorsunuz...";
                setTimeout(() => {
                    hideRestartOverlay();
                    const lastView = localStorage.getItem('lastView') || 'dashboard';
                    const lastTab = localStorage.getItem('lastTab');
                    window.switchView(lastView, lastTab);
                    showToast("Sistem başarıyla yeniden başlatıldı.", "success");
                }, 1000);
            }
        } catch (e) {
            // Still down
        }
    }, 3000);
}

async function loadLogs() {
    const tbody = document.getElementById('logsListBody');
    if (!tbody) return;

    tbody.innerHTML = '<tr><td colspan="8" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const res = await fetch(`${API_BASE_URL}/api/logs`);
        if (res.status === 403) {
            tbody.innerHTML = '<tr><td colspan="8" style="color:var(--error); text-align:center;">🚫 Yetkisiz Erişim</td></tr>';
            return;
        }
        if (!res.ok) throw new Error('Logs fetch failed');
        const logs = await res.json();

        tbody.innerHTML = '';
        if (logs.length === 0) {
            tbody.innerHTML = '<tr><td colspan="8" style="text-align:center;">Kayıt bulunamadı.</td></tr>';
            return;
        }

        logs.forEach(log => {
            const tr = document.createElement('tr');
            tr.style.cursor = 'pointer';
            const date = new Date(log.createDate).toLocaleString();

            tr.innerHTML = `
                <td>${date}</td>
                <td>${log.actorUserName || log.actorUserId || '-'}</td>
                <td><span class="badge" style="background:#e0e7ff;">${log.actorRole}</span></td>
                <td><span class="badge" style="background:${getActionColor(log.actionType)}; color:white;">${log.actionType}</span></td>
                <td>${log.entityName}</td>
                <td><b title="ID: ${log.entityId}">${log.entityDisplay || log.entityId}</b></td>
                <td>${log.actionDescription || '-'}</td>
                <td style="font-size:0.8rem; color:#6b7280;">${log.source}/${log.ipAddress}</td>
            `;

            // Detail Row
            const detailTr = document.createElement('tr');
            detailTr.style.display = 'none';
            detailTr.innerHTML = `
                <td colspan="8" style="background:#f8fafc; padding:1rem; border-top:none;">
                    <div class="glass-card" style="margin:0; padding:1rem; border:1px solid #e2e8f0;">
                        <h4 style="margin-top:0; font-size:1rem; color:var(--primary); margin-bottom:1rem; display:flex; align-items:center; gap:0.5rem;">
                            🔎 Değişiklik Detayları <small style="font-weight:normal; color:#64748b;">(${log.entityName}: ${log.entityDisplay || log.entityId})</small>
                        </h4>
                        ${renderAuditDiff(log.oldValues, log.newValues)}
                    </div>
                </td>
            `;

            tr.onclick = () => {
                detailTr.style.display = detailTr.style.display === 'none' ? 'table-row' : 'none';
            };

            tbody.appendChild(tr);
            tbody.appendChild(detailTr);
        });
    } catch (e) {
        console.error(e);
        tbody.innerHTML = '<tr><td colspan="8" style="color:var(--error); text-align:center;">Kayıtlar yüklenemedi!</td></tr>';
    }
}

const AUDIT_TRANSLATIONS = {
    'UpdatedByUserId': 'Güncelleyen',
    'CreatedByUserId': 'Oluşturan',
    'UserRegNo': 'Sicil No',
    'UserFirstName': 'Ad',
    'UserLastName': 'Soyad',
    'UserMail': 'E-Posta',
    'ProductName': 'Ürün Adı',
    'ProductCode': 'Ürün Kodu',
    'CurrentStock': 'Mevcut Stok',
    'IsDeleted': 'Silindi Mi?',
    'IsActive': 'Aktif Mi?',
    'RoleId': 'Rol',
    'CategoryId': 'Kategori',
    'BrandId': 'Marka',
    'UnitId': 'Birim',
    'WarehouseId': 'Depo',
    'CompanyName': 'Şirket Adı',
    'SettingKey': 'Ayar Anahtarı',
    'SettingValue': 'Ayar Değeri',
    'CanRead': 'Okuma Yetkisi',
    'CanWrite': 'Yazma Yetkisi',
    'CanDelete': 'Silme Yetkisi',
    'IsFull': 'Tam Yetki',
    'Quantity': 'Miktar',
    'MovementType': 'Hareket Tipi'
};

function renderAuditDiff(oldJson, newJson) {
    if (!oldJson && !newJson) return '<p style="color:#64748b; font-style:italic;">Değişiklik detayı bulunamadı.</p>';

    let oldObj = {};
    let newObj = {};
    try { if (oldJson) oldObj = JSON.parse(oldJson); } catch (e) { }
    try { if (newJson) newObj = JSON.parse(newJson); } catch (e) { }

    const allKeys = [...new Set([...Object.keys(oldObj), ...Object.keys(newObj)])];

    let html = '<table style="width:100%; font-size:0.85rem; border-collapse:separate; border-spacing:0; background:white; border:1px solid #e2e8f0; border-radius:8px; overflow:hidden;">';
    html += '<thead style="background:#f1f5f9;"><tr><th style="padding:8px 12px; text-align:left; border-bottom:1px solid #e2e8f0; width:25%;">Alan</th><th style="padding:8px 12px; text-align:left; border-bottom:1px solid #e2e8f0; width:37.5%;">Eski Değer</th><th style="padding:8px 12px; text-align:left; border-bottom:1px solid #e2e8f0; width:37.5%;">Yeni Değer</th></tr></thead>';
    html += '<tbody>';

    let changeCount = 0;
    allKeys.forEach(key => {
        let oldVal = oldObj[key];
        let newVal = newObj[key];
        const isChanged = oldVal !== newVal;

        if (isChanged) {
            changeCount++;
            const label = AUDIT_TRANSLATIONS[key] || key;

            // Format boolean values and resolve IDs
            const formatValue = (val, key) => {
                if (val === true) return '<span class="badge" style="background:#d1fae5; color:#065f46;">Evet</span>';
                if (val === false) return '<span class="badge" style="background:#fee2e2; color:#991b1b;">Hayır</span>';
                if (val === undefined || val === null || val === '') return '<span style="color:#d1d5db; font-style:italic;">-</span>';

                // Resolve IDs if mapping exists
                if (ID_MAPPINGS[key] && ID_MAPPINGS[key][val]) {
                    return `<span title="ID: ${val}" style="color:var(--primary); font-weight:600;">${ID_MAPPINGS[key][val]}</span>`;
                }

                return val;
            };

            html += `<tr style="border-bottom:1px solid #f1f5f9;">
                <td style="padding:8px 12px; font-weight:600; color:#475569; border-bottom:1px solid #f1f5f9;">${label}</td>
                <td style="padding:8px 12px; background:#fef2f2; color:#991b1b; border-bottom:1px solid #f1f5f9; word-break:break-all;">${formatValue(oldVal, key)}</td>
                <td style="padding:8px 12px; background:#f0fdf4; color:#166534; border-bottom:1px solid #f1f5f9; word-break:break-all;">${formatValue(newVal, key)}</td>
            </tr>`;
        }
    });

    if (changeCount === 0) {
        return '<p style="color:#64748b; font-style:italic;">Fiziksel bir değişiklik saptanmadı (Sadece sistem bilgileri güncellenmiş olabilir).</p>';
    }

    html += '</tbody></table>';
    return html;
}

function getActionColor(action) {
    if (action === 'Added' || action === 'Create') return '#10b981'; // Green
    if (action === 'Modified' || action === 'Update') return '#f59e0b'; // Orange
    if (action === 'Deleted' || action === 'Delete') return '#ef4444'; // Red
    return '#6b7280'; // Gray
}

window.loadUsers = async function () {
    const tbody = document.getElementById('userListBody');
    if (!tbody) return;

    if (!window.userPage) window.userPage = 1;
    if (!window.userPageSize) window.userPageSize = 10;
    const searchTerm = document.getElementById('search-users')?.value || '';

    const currentUser = JSON.parse(localStorage.getItem('user'));

    // User role should never access this function
    if (currentUser.role === 'User') {
        tbody.innerHTML = '<tr><td colspan="5" style="color:var(--error); text-align:center;">Erişim reddedildi!</td></tr>';
        return;
    }

    tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const filter = window.currentStatusFilter['users'] || 'active';
        let url = `${API_BASE_URL}/api/users?status=${filter}&searchTerm=${encodeURIComponent(searchTerm)}&page=${window.userPage}&pageSize=${window.userPageSize}`;

        // Root sees all users, Admin sees only users they created
        if (currentUser.role === 'Admin') {
            url += `&creatorId=${currentUser.id}`;
        }
        // Root role doesn't add creatorId parameter, so sees all users

        const res = await fetch(url);
        if (res.status === 403) {
            tbody.innerHTML = '<tr><td colspan="6" style="color:var(--error); text-align:center;">🚫 Bu verileri görmeye yetkiniz bulunmamaktadır.</td></tr>';
            return;
        }
        if (!res.ok) throw new Error('Yükleme hatası');

        const pagedData = await res.json();
        const users = pagedData.items || [];
        const totalCount = pagedData.totalCount || 0;
        const totalPages = Math.ceil(totalCount / window.userPageSize);

        tbody.innerHTML = '';
        if (users.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Kullanıcı bulunamadı.</td></tr>';
            renderPagination(document.getElementById('userPaginationContainer'), totalPages, window.userPage, window.userPageSize, () => loadUsers());
            return;
        }

        const canWrite = window.hasPermission('Users', 'write');
        const canDelete = window.hasPermission('Users', 'delete');

        users.forEach(u => {
            const tr = document.createElement('tr');
            if (u.isDeleted) tr.style.opacity = '0.6';

            tr.innerHTML = `
                <td>${u.userName} ${u.isDeleted ? '<span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;">PASİF (SİLİNDİ)</span>' : ''}</td>
                <td>${u.firstName} ${u.lastName}</td>
                <td>${u.email}</td>
                <td>${u.titleName || '-'}</td>
                <td><span class="badge ${u.role === 'Admin' ? 'badge-primary' : 'badge-secondary'}">${u.role}</span></td>
                <td style="text-align:right;">
                    <div class="action-btn-container">
                        ${canWrite && !u.isDeleted ? `
                        <button class="btn-action" style="background:#10b981;" 
                            onclick="openPermissionsModal(${u.id}, '${u.userName}')">Yetkiler</button>
                        <button class="btn-action btn-edit" 
                            onclick="openUserModal(${u.id})">Düzenle</button>
                        ` : ''}
                        ${canDelete && !u.isDeleted ? `
                        <button class="btn-action btn-delete" 
                            onclick="deleteUser(${u.id}, '${u.userName}')">Sil</button>
                        ` : ''}
                        ${u.isDeleted ? (window.hasPermission('ShowDeletedItems', 'write') ? `
                            <button class="btn-action" style="background:#10b981;" 
                                onclick="restoreEntity('user', ${u.id})">✅ Geri Yükle</button>
                        ` : '<span style="font-size:0.75rem; color:var(--muted); font-style:italic;">Pasif</span>') : ''}
                    </div>
                </td>
            `;
            tbody.appendChild(tr);
        });

        if (!document.getElementById('userPaginationContainer')) {
            const container = document.createElement('div');
            container.id = 'userPaginationContainer';
            tbody.closest('.glass-card').appendChild(container);
        }

        renderPagination(document.getElementById('userPaginationContainer'), totalPages, window.userPage, window.userPageSize, (newPage, newSize) => {
            window.userPage = newPage;
            window.userPageSize = newSize;
            loadUsers();
        });

    } catch (e) {
        tbody.innerHTML = '<tr><td colspan="6" style="color:var(--error); text-align:center;">Kullanıcı listesi yüklenemedi!</td></tr>';
        console.error(e);
    }
}

window.toggleWaybillDetails = async function (waybillNo, rowElement) {
    const detailRowId = `details-${waybillNo.replace(/[^a-zA-Z0-9]/g, '-')}`;
    let detailRow = document.getElementById(detailRowId);

    if (detailRow) {
        detailRow.style.display = detailRow.style.display === 'none' ? 'table-row' : 'none';
        return;
    }

    // Create detail row
    detailRow = document.createElement('tr');
    detailRow.id = detailRowId;
    detailRow.style.background = '#f8fafc';
    detailRow.innerHTML = `<td colspan="5" style="padding:0;"><div style="padding:1rem; text-align:center; color:var(--muted);"><span class="spinner-small"></span> Yükleniyor...</div></td>`;
    rowElement.parentNode.insertBefore(detailRow, rowElement.nextSibling);

    try {
        const res = await fetch(`${API_BASE_URL}/api/stock/waybill-items?waybillNo=${encodeURIComponent(waybillNo)}`);
        if (!res.ok) {
            const errText = await res.text();
            let msg = 'Yüklenemedi';
            try {
                const errObj = JSON.parse(errText);
                msg = errObj.message || msg;
            } catch (e) { msg = errText || msg; }
            throw new Error(msg);
        }
        const items = await res.json();

        if (items.length === 0) {
            detailRow.innerHTML = `<td colspan="5" style="padding:1rem; text-align:center; color:var(--muted);">İçerik bilgisi bulunamadı.</td>`;
            return;
        }

        let html = `
            <td colspan="5" style="padding: 1.5rem; background: #f8fafc; border-bottom: 2px solid #e2e8f0;">
                <div style="background: white; border-radius: 8px; border: 1px solid #e5e7eb; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.05);">
                    <table style="width:100%; border-collapse: collapse;">
                        <thead style="background: #f1f5f9; border-bottom: 1px solid #e2e8f0;">
                            <tr>
                                <th style="padding: 10px 15px; text-align: left; font-size: 0.7rem; color: #64748b; text-transform: uppercase;">Ürün Kodu</th>
                                <th style="padding: 10px 15px; text-align: left; font-size: 0.7rem; color: #64748b; text-transform: uppercase;">Ürün Adı</th>
                                <th style="padding: 10px 15px; text-align: center; font-size: 0.7rem; color: #64748b; text-transform: uppercase;">Miktar</th>
                                <th style="padding: 10px 15px; text-align: left; font-size: 0.7rem; color: #64748b; text-transform: uppercase;">Birim</th>
                                <th style="padding: 10px 15px; text-align: left; font-size: 0.7rem; color: #64748b; text-transform: uppercase;">Açıklama</th>
                            </tr>
                        </thead>
                        <tbody>
        `;

        items.forEach(item => {
            html += `
                <tr style="border-bottom: 1px solid #f1f5f9;">
                    <td style="padding: 10px 15px; font-size: 0.85rem; color: var(--primary); font-weight: 600;">${item.productCode}</td>
                    <td style="padding: 10px 15px; font-size: 0.85rem; color: #334155;">${item.productName}</td>
                    <td style="padding: 10px 15px; font-size: 0.85rem; color: #1e293b; text-align: center; font-weight: 700;">${item.quantity}</td>
                    <td style="padding: 10px 15px; font-size: 0.85rem; color: #64748b;">${item.unitName}</td>
                    <td style="padding: 10px 15px; font-size: 0.8rem; color: #94a3b8; font-style: italic;">${item.description || '-'}</td>
                </tr>
            `;
        });

        html += `</tbody></table></div></td>`;
        detailRow.innerHTML = html;

    } catch (e) {
        detailRow.innerHTML = `<td colspan="5" style="padding:1rem; text-align:center; color:red;">Hata: ${e.message}</td>`;
    }
}

window.openUserModal = async function (id) {
    if (typeof id === 'string') {
        const el = document.getElementById(id);
        if (el) el.style.display = 'flex';
        return;
    }

    // USER MODAL logic
    document.getElementById('userModal').style.display = 'flex';
    const userIdInput = document.getElementById('uId');
    const titleEl = document.getElementById('userModalTitle');
    const nameGroup = document.getElementById('uNameGroup');
    const passGroup = document.getElementById('uPassGroup');
    const activeGroup = document.getElementById('uIsActiveGroup');

    // Load Companies for dropdown
    const compSel = document.getElementById('uCompany');
    compSel.innerHTML = '<option value="">Şirket Seçin...</option>';
    try {
        const compRes = await fetch(`${API_BASE_URL}/api/companies?pageSize=500`);
        const pagedData = await compRes.json();
        const companies = pagedData.items || [];
        companies.forEach(c => compSel.innerHTML += `<option value="${c.id}">${c.companyName}</option>`);
    } catch (e) { console.error("Companies load failed", e); }

    // Load Titles for dropdown
    const titleSel = document.getElementById('uTitle');
    if (titleSel) {
        titleSel.innerHTML = '<option value="">Yükleniyor...</option>';
        try {
            const titleRes = await fetch(`${API_BASE_URL}/api/users/titles`);
            if (titleRes.ok) {
                const titles = await titleRes.json();
                titleSel.innerHTML = '<option value="">Seçiniz...</option>';
                titles.forEach(t => titleSel.innerHTML += `<option value="${t.id}">${t.titleName}</option>`);
            } else {
                titleSel.innerHTML = '<option value="">Hata!</option>';
            }
        } catch (e) { console.error("Titles load failed", e); titleSel.innerHTML = '<option value="">Seçiniz...</option>'; }
    }

    if (id) {
        // EDIT MODE
        titleEl.innerText = 'Kullanıcı Düzenle';
        userIdInput.value = id;
        nameGroup.style.display = 'none'; // Username usually immutable
        passGroup.style.display = 'none'; // Password changed elsewhere
        activeGroup.style.display = 'block';

        try {
            const res = await fetch(`${API_BASE_URL}/api/users/${id}`);
            if (res.ok) {
                const u = await res.json();
                document.getElementById('uFirst').value = u.firstName || '';
                document.getElementById('uLast').value = u.lastName || '';
                document.getElementById('uEmail').value = u.email || '';
                document.getElementById('uReg').value = u.regNo || '';
                document.getElementById('uRole').value = u.roleId || '3';
                document.getElementById('uCompany').value = u.companyId || '';
                if (document.getElementById('uTitle')) document.getElementById('uTitle').value = u.titleId || '';
                document.getElementById('uIsActive').checked = u.isActive !== false;
            }
        } catch (e) { console.error("User load failed", e); }
    } else {
        // CREATE MODE
        titleEl.innerText = 'Yeni Kullanıcı';
        userIdInput.value = '';
        nameGroup.style.display = 'block';
        passGroup.style.display = 'block';
        activeGroup.style.display = 'none';

        // Clear fields
        document.getElementById('uName').value = '';
        document.getElementById('uPass').value = '';
        document.getElementById('uFirst').value = '';
        document.getElementById('uLast').value = '';
        document.getElementById('uEmail').value = '';
        document.getElementById('uReg').value = '';
        document.getElementById('uRole').value = '3';
        document.getElementById('uCompany').value = '';
        if (document.getElementById('uTitle')) document.getElementById('uTitle').value = '';
        document.getElementById('uIsActive').checked = true;
    }
}

window.closeModal = function (id) {
    const modalId = typeof id === 'string' ? id : 'userModal';
    const el = document.getElementById(modalId);
    if (el) el.style.display = 'none';
}

window.saveUser = async function () {
    const id = document.getElementById('uId').value;
    const currentUser = JSON.parse(localStorage.getItem('user'));

    let url = `${API_BASE_URL}/api/users`;
    let method = 'POST';
    let data = {};

    if (id) {
        // UPDATE
        method = 'PUT';
        url = `${API_BASE_URL}/api/users/${id}`;
        data = {
            firstName: document.getElementById('uFirst').value,
            lastName: document.getElementById('uLast').value,
            email: document.getElementById('uEmail').value,
            regNo: document.getElementById('uReg').value,
            roleId: parseInt(document.getElementById('uRole').value),
            companyId: document.getElementById('uCompany').value ? parseInt(document.getElementById('uCompany').value) : null,
            titleId: document.getElementById('uTitle') && document.getElementById('uTitle').value ? parseInt(document.getElementById('uTitle').value) : null,
            isActive: document.getElementById('uIsActive').checked
        };
    } else {
        // CREATE
        data = {
            userName: document.getElementById('uName').value,
            password: document.getElementById('uPass').value,
            firstName: document.getElementById('uFirst').value,
            lastName: document.getElementById('uLast').value,
            email: document.getElementById('uEmail').value,
            regNo: document.getElementById('uReg').value,
            roleId: parseInt(document.getElementById('uRole').value),
            companyId: document.getElementById('uCompany').value ? parseInt(document.getElementById('uCompany').value) : null,
            titleId: document.getElementById('uTitle') && document.getElementById('uTitle').value ? parseInt(document.getElementById('uTitle').value) : null,
            createdByUserId: currentUser.id
        };
        if (!data.userName || !data.password || !data.firstName) { alert("Lütfen zorunlu alanları doldurun!"); return; }
    }

    try {
        const res = await fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (res.ok) {
            alert(id ? "Kullanıcı güncellendi!" : "Kullanıcı oluşturuldu!");
            closeModal('userModal');
            loadUsers();
        } else {
            const err = await res.json();
            alert("Hata: " + (err.message || 'İşlem başarısız'));
        }
    } catch (e) { alert("Bağlantı hatası!"); }
}

// PERMISSIONS
window.openPermissionsModal = async function (userId, userName) {
    document.getElementById('permUserName').innerText = userName;
    document.getElementById('permUserId').value = userId;
    document.getElementById('permissionsModal').style.display = 'flex';

    const tbody = document.getElementById('permListBody');
    tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const res = await fetch(`${API_BASE_URL}/api/users/${userId}/permissions`);
        const perms = await res.json();

        tbody.innerHTML = '';

        const PERMISSION_GROUPS = {
            'Genel': ['Reports'],
            'Stok Yönetimi': ['Product', 'Category', 'Stock', 'Warehouse', 'Upload', 'Brand', 'ProductUnit', 'StockMovement', 'StockAlert', 'ProductLocation'],
            'Ticari': ['Supplier', 'Customer', 'CustomerCompany'],
            'Finans': ['PriceList', 'Offers', 'Invoices', 'Offer', 'Invoice', 'OfferItem', 'InvoiceItem', 'FinanceReport'],
            'Sistem': ['Users', 'Companies', 'System', 'Logs', 'Auth', 'User', 'Role', 'UserPermission', 'UserApiKey', 'SystemSetting', 'LicenseInfo', 'AuditLog', 'Company', 'Title', 'ShowDeletedItems']
        };

        for (const [groupName, moduleNames] of Object.entries(PERMISSION_GROUPS)) {
            const groupPerms = perms.filter(p => moduleNames.includes(p.moduleName));
            if (groupPerms.length === 0) continue;

            const groupId = groupName.replace(/\s+/g, '-').toLowerCase();
            const groupHeader = document.createElement('tr');
            groupHeader.innerHTML = `
                <td colspan="5" style="background:#f8fafc; font-weight:bold; color:var(--primary); padding:0.5rem 1rem; border-top:2px solid #e2e8f0;">
                    <div style="display:flex; justify-content:space-between; align-items:center;">
                        <span>${groupName}</span>
                        <label style="font-weight:normal; font-size:0.85rem; display:flex; align-items:center; gap:0.5rem; cursor:pointer;">
                            <input type="checkbox" onchange="toggleGroupPermissions(this, '${groupId}')"> Tümünü Seç
                        </label>
                    </div>
                </td>
            `;
            tbody.appendChild(groupHeader);

            groupPerms.forEach(p => {
                const tr = document.createElement('tr');
                tr.classList.add(`group-row-${groupId}`);
                tr.innerHTML = `
                    <td style="padding-left:1.5rem;">${p.moduleName}</td>
                    <td style="text-align:center;"><input type="checkbox" class="perm-chk" data-mod="${p.moduleId}" data-type="full" ${p.isFull ? 'checked' : ''} onchange="permLogic(this)"></td>
                    <td style="text-align:center;"><input type="checkbox" class="perm-chk" data-mod="${p.moduleId}" data-type="read" ${p.canRead ? 'checked' : ''}></td>
                    <td style="text-align:center;"><input type="checkbox" class="perm-chk" data-mod="${p.moduleId}" data-type="write" ${p.canWrite ? 'checked' : ''} onchange="permLogic(this)"></td>
                    <td style="text-align:center;"><input type="checkbox" class="perm-chk" data-mod="${p.moduleId}" data-type="delete" ${p.canDelete ? 'checked' : ''} onchange="permLogic(this)"></td>
                `;
                tbody.appendChild(tr);
            });
        }

        // Modules not in any group
        const groupedModules = Object.values(PERMISSION_GROUPS).flat();
        const otherPerms = perms.filter(p => !groupedModules.includes(p.moduleName));
        if (otherPerms.length > 0) {
            const groupHeader = document.createElement('tr');
            groupHeader.innerHTML = `
                <td colspan="5" style="background:#f8fafc; font-weight:bold; color:var(--primary); padding:0.5rem 1rem; border-top:2px solid #e2e8f0;">
                    <div style="display:flex; justify-content:space-between; align-items:center;">
                        <span>Diğer</span>
                        <label style="font-weight:normal; font-size:0.85rem; display:flex; align-items:center; gap:0.5rem; cursor:pointer;">
                            <input type="checkbox" onchange="toggleGroupPermissions(this, 'other')"> Tümünü Seç
                        </label>
                    </div>
                </td>
            `;
            tbody.appendChild(groupHeader);
            otherPerms.forEach(p => {
                const tr = document.createElement('tr');
                tr.classList.add('group-row-other');
                tr.innerHTML = `
                    <td style="padding-left:1.5rem;">${p.moduleName}</td>
                    <td style="text-align:center;"><input type="checkbox" class="perm-chk" data-mod="${p.moduleId}" data-type="full" ${p.isFull ? 'checked' : ''} onchange="permLogic(this)"></td>
                    <td style="text-align:center;"><input type="checkbox" class="perm-chk" data-mod="${p.moduleId}" data-type="read" ${p.canRead ? 'checked' : ''}></td>
                    <td style="text-align:center;"><input type="checkbox" class="perm-chk" data-mod="${p.moduleId}" data-type="write" ${p.canWrite ? 'checked' : ''} onchange="permLogic(this)"></td>
                    <td style="text-align:center;"><input type="checkbox" class="perm-chk" data-mod="${p.moduleId}" data-type="delete" ${p.canDelete ? 'checked' : ''} onchange="permLogic(this)"></td>
                `;
                tbody.appendChild(tr);
            });
        }

    } catch (e) {
        tbody.innerHTML = '<tr><td colspan="5">İzinler yüklenemedi.</td></tr>';
    }
}

window.toggleGroupPermissions = function (masterChk, groupId) {
    const rows = document.querySelectorAll(`.group-row-${groupId}`);
    rows.forEach(row => {
        const chks = row.querySelectorAll('input[type="checkbox"]');
        chks.forEach(c => c.checked = masterChk.checked);
    });
}

window.permLogic = function (el) {
    const row = el.closest('tr');
    if (el.dataset.type === 'full') {
        const chks = row.querySelectorAll('input[type="checkbox"]');
        chks.forEach(c => c.checked = el.checked);
    } else if (el.checked) {
        const readChk = row.querySelector('[data-type="read"]');
        if (readChk) readChk.checked = true;
    }
}

window.savePermissions = async function () {
    const userId = document.getElementById('permUserId').value;
    const rows = document.querySelectorAll('#permListBody tr');
    const perms = [];

    rows.forEach(row => {
        const readChk = row.querySelector('[data-type="read"]');
        if (!readChk) return;
        perms.push({
            moduleId: parseInt(readChk.getAttribute('data-mod')),
            isFull: row.querySelector('[data-type="full"]').checked,
            canRead: readChk.checked,
            canWrite: row.querySelector('[data-type="write"]').checked,
            canDelete: row.querySelector('[data-type="delete"]').checked
        });
    });

    try {
        const res = await fetch(`${API_BASE_URL}/api/users/${userId}/permissions`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(perms)
        });

        if (res.ok) {
            showToast("Yetkiler güncellendi");
            closePermissionsModal();
        } else {
            const error = await res.json();
            alert("Kaydedilemedi: " + (error.message || "Bilinmeyen hata"));
        }
    } catch (e) {
        alert("Bağlantı hatası: " + e.message);
        console.error(e);
    }
}

window.closePermissionsModal = function () { document.getElementById('permissionsModal').style.display = 'none'; }

// DELETE USER
window.deleteUser = async function (userId, userName) {
    if (!(await showConfirm('Kullanıcı Silme', `"${userName}" kullanıcısını silmek istediğinizden emin misiniz?`))) {
        return;
    }

    try {
        const res = await fetch(`${API_BASE_URL}/api/users/${userId}`, {
            method: 'DELETE'
        });

        if (res.ok) {
            showToast('Kullanıcı başarıyla silindi');
            loadUsers(); // Refresh list
        } else {
            const error = await res.json();
            alert('Hata: ' + (error.message || 'Kullanıcı silinemedi.'));
        }
    } catch (e) {
        alert('Bağlantı hatası!');
        console.error(e);
    }
}


// OFFERS & INVOICES
window.loadOffers = async function () {
    const tbody = document.getElementById('offerListBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const statusFilter = window.currentStatusFilter['offers'] || 'active';

        window.offerPage = window.offerPage || 1;
        window.offerPageSize = window.offerPageSize || 10;

        const res = await fetch(`${API_BASE_URL}/api/offer?status=${statusFilter}&page=${window.offerPage}&pageSize=${window.offerPageSize}`);
        if (res.status === 403) {
            tbody.innerHTML = '<tr><td colspan="7" style="color:var(--error); text-align:center;">🚫 Yetkisiz Erişim</td></tr>';
            return;
        }
        if (!res.ok) throw new Error('Teklifler alınamadı');

        const pagedData = await res.json();
        const offers = pagedData.items || [];
        const totalCount = pagedData.totalCount || 0;
        const totalPages = Math.ceil(totalCount / window.offerPageSize);


        tbody.innerHTML = '';
        if (offers.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">Aranılan filtreye uygun teklif bulunamadı.</td></tr>';
            renderPagination(document.getElementById('offerPaginationContainer'), totalPages, window.offerPage, window.offerPageSize, () => loadOffers());
            return;
        }

        offers.forEach(o => {
            const tr = document.createElement('tr');
            if (o.isDeleted) tr.style.opacity = '0.6';

            // Status: 1=Pending, 2=Approved, 3=Rejected
            let statusBadge = '<span class="badge" style="background:#f3f4f6;">Bekliyor</span>';
            if (o.status === 2) statusBadge = '<span class="badge" style="background:var(--success); color:white;">Onaylı</span>';
            if (o.status === 3) statusBadge = '<span class="badge" style="background:var(--error); color:white;">Red</span>';
            if (o.status === 4) statusBadge = '<span class="badge" style="background:#10b981; color:white;">Faturalandı</span>';

            let actions = '';
            const canWrite = window.hasPermission('Offers', 'write');

            if (canWrite && !o.isDeleted) {
                if (o.status === 4) { // Completed/Invoiced
                    actions = '<span style="color:var(--muted); font-size:0.8rem;">İşlem Tamamlandı</span>';
                } else {
                    actions += `<button class="btn-action btn-edit" onclick="editOffer(${o.id})">Düzenle</button>`;

                    if (o.status === 1) { // Pending
                        actions += `<button class="btn-action" style="background:#10b981;" onclick="approveOffer(${o.id})">Onayla</button>`;
                    } else if (o.status === 2) { // Approved
                        actions += `<button class="btn-action" style="background:#4f46e5; min-width:100px;" onclick="createInvoiceFromOffer(${o.id})">Faturalaştır</button>`;
                    }

                    actions += `<button class="btn-action btn-delete" onclick="deleteOffer(${o.id})">Sil</button>`;
                }
            } else if (o.isDeleted) {
                actions = window.hasPermission('ShowDeletedItems', 'write')
                    ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('offer', ${o.id})">✅ Geri Yükle</button>`
                    : '<span style="font-size:0.75rem; color:var(--muted); font-style:italic;">Silindi</span>';
            }

            tr.innerHTML = `
                <td><strong>${o.offerNumber || '-'}</strong> ${o.isDeleted ? '<br><span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;">PASİF</span>' : ''}</td>
                <td>${o.customerName || o.customerId}</td>
                <td>${new Date(o.offerDate).toLocaleDateString()}</td>
                <td>${new Date(o.validUntil).toLocaleDateString()}</td>
                <td>${formatMoney(o.totalAmount)} ${getCurrencySymbol(o.currency)}</td>
                <td>${statusBadge}</td>
                <td style="text-align:right;">
                    <div class="action-btn-container">
                        ${actions}
                        ${o.isDeleted && window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('offer', ${o.id})">✅ Geri Yükle</button>` : ''}
                    </div>
                </td>
            `;
            tbody.appendChild(tr);
        });

        if (!document.getElementById('offerPaginationContainer')) {
            const table = tbody.closest('table');
            const pagContainer = document.createElement('div');
            pagContainer.id = 'offerPaginationContainer';
            pagContainer.className = 'pagination-container';
            table.parentNode.insertBefore(pagContainer, table.nextSibling);
        }
        renderPagination(document.getElementById('offerPaginationContainer'), totalPages, window.offerPage, window.offerPageSize, (newPage, newSize) => {
            window.offerPage = newPage;
            window.offerPageSize = newSize;
            loadOffers();
        });

    } catch (e) {
        tbody.innerHTML = '<tr><td colspan="7" style="color:var(--error); text-align:center;">Hata oluştu!</td></tr>';
        console.error(e);
    }
}

window.loadInvoices = async function () {
    const tbody = document.getElementById('invoiceListBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const filter = window.currentStatusFilter['invoices'] || 'active';

        window.invoicePage = window.invoicePage || 1;
        window.invoicePageSize = window.invoicePageSize || 10;

        const res = await fetch(`${API_BASE_URL}/api/invoices?status=${filter}&page=${window.invoicePage}&pageSize=${window.invoicePageSize}`);
        if (res.status === 403) {
            tbody.innerHTML = '<tr><td colspan="6" style="color:var(--error); text-align:center;">🚫 Yetkisiz Erişim</td></tr>';
            return;
        }
        if (!res.ok) throw new Error('Faturalar alınamadı');

        const pagedData = await res.json();
        const invoices = pagedData.items || [];
        const totalCount = pagedData.totalCount || 0;
        const totalPages = Math.ceil(totalCount / window.invoicePageSize);


        tbody.innerHTML = '';
        if (invoices.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Fatura bulunamadı.</td></tr>';
            renderPagination(document.getElementById('invoicePaginationContainer'), totalPages, window.invoicePage, window.invoicePageSize, () => loadInvoices());
            return;
        }

        invoices.forEach(i => {
            const tr = document.createElement('tr');
            if (i.isDeleted) tr.style.opacity = '0.6';

            tr.style.cursor = 'pointer';
            tr.onclick = (e) => {
                if (e.target.closest('button') || e.target.closest('.action-btn-container')) return;
                openInvoiceDetails(i.id);
            };

            // Status: 1=Draft, 5=Approved
            let statusBadge = '<span class="badge" style="background:#f3f4f6;">Taslak</span>';
            if (i.status === 5) statusBadge = '<span class="badge" style="background:var(--success); color:white;">Onaylı</span>';
            if (i.isDeleted) statusBadge = '<span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;">PASİF (SİLİNDİ)</span>';

            let actions = '';
            // Only allow approval if has Write permission
            const canWrite = window.hasPermission('Invoices', 'write');
            if (!i.isDeleted && i.status === 1 && canWrite) { // Draft
                actions += `<button class="btn-action" style="min-width:140px; background:var(--primary);" onclick="approveInvoice(${i.id})">Onayla & Stok Düş</button>`;
                actions += `<button class="btn-action" style="background:#ef4444;" onclick="rejectInvoice(${i.id})">Reddet</button>`;
            } else if (i.isDeleted) {
                actions = window.hasPermission('ShowDeletedItems', 'write')
                    ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('invoice', ${i.id})">✅ Geri Yükle</button>`
                    : '<span style="font-size:0.75rem; color:var(--muted); font-style:italic;">İşlem Yapılamaz</span>';
            }

            tr.innerHTML = `
                <td><strong>${i.invoiceNumber || '-'}</strong></td>
                <td>${new Date(i.issueDate).toLocaleDateString()}</td>
                <td>${formatMoney(i.grandTotal)} ₺</td>
                <td>${formatMoney(i.taxTotal)} ₺</td>
                <td>${statusBadge}</td>
                <td style="text-align:right;">
                    <div class="action-btn-container">
                        ${actions}
                        ${i.isDeleted && window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('invoice', ${i.id})">✅ Geri Yükle</button>` : ''}
                    </div>
                </td>
            `;
            tbody.appendChild(tr);
        });

        if (!document.getElementById('invoicePaginationContainer')) {
            const table = tbody.closest('table');
            const pagContainer = document.createElement('div');
            pagContainer.id = 'invoicePaginationContainer';
            pagContainer.className = 'pagination-container';
            table.parentNode.insertBefore(pagContainer, table.nextSibling);
        }
        renderPagination(document.getElementById('invoicePaginationContainer'), totalPages, window.invoicePage, window.invoicePageSize, (newPage, newSize) => {
            window.invoicePage = newPage;
            window.invoicePageSize = newSize;
            loadInvoices();
        });

    } catch (e) {
        tbody.innerHTML = '<tr><td colspan="6" style="color:var(--error); text-align:center;">Hata oluştu!</td></tr>';
        console.error(e);
    }
}

window.rejectInvoice = async function (id) {
    if (!(await showConfirm('Reddet', 'Bu faturayı reddetmek istediğinize emin misiniz? Fatura iptal edilecek ve bağlı teklif tekrar onaysız hale gelecektir.', 'Reddet', 'İptal'))) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/invoice/${id}/reject`, {
            method: 'POST'
        });
        if (res.ok) {
            showToast('Fatura reddedildi');
            loadInvoices();
            if (document.getElementById('section-invoice-details') && document.getElementById('section-invoice-details').style.display !== 'none') {
                openInvoiceDetails(id);
            }
        } else {
            alert('İşlem başarısız.');
        }
    } catch (e) { alert('Hata: ' + e.message); }
}

window.openInvoiceDetails = async function (id) {
    try {
        switchView('invoice-details');
        const grid = document.getElementById('invoiceInfoGrid');
        const tbody = document.getElementById('invoiceDetailsListBody');
        const tfoot = document.getElementById('invoiceDetailsFoot');
        const actions = document.getElementById('invoiceDetailsActions');

        if (grid) grid.innerHTML = 'Yükleniyor...';
        if (tbody) tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">Yükleniyor...</td></tr>';
        if (tfoot) tfoot.innerHTML = '';
        if (actions) actions.innerHTML = '';

        const invoiceRes = await fetch(`${API_BASE_URL}/api/invoices/${id}`);
        if (!invoiceRes.ok) throw new Error('Fatura detayları yüklenemedi.');
        const invoice = await invoiceRes.json();

        // Populate info grid
        let statusBadge = '<span class="badge" style="background:#f3f4f6;">Taslak</span>';
        if (invoice.status === 5) statusBadge = '<span class="badge" style="background:var(--success); color:white;">Onaylı</span>';
        if (invoice.isDeleted) statusBadge = '<span class="badge" style="background:#fecaca;color:#991b1b;">PASİF (SİLİNDİ)</span>';

        if (grid) grid.innerHTML = `
            <div><label style="color:#64748b; font-size:0.8rem;">Fatura No</label><div style="font-weight:600;">${invoice.invoiceNumber || '-'}</div></div>
            <div><label style="color:#64748b; font-size:0.8rem;">Tarih</label><div style="font-weight:600;">${new Date(invoice.issueDate).toLocaleDateString()}</div></div>
            <div><label style="color:#64748b; font-size:0.8rem;">Durum</label><div style="margin-top:0.25rem;">${statusBadge}</div></div>
            <div><label style="color:#64748b; font-size:0.8rem;">İlgili Firma</label><div style="font-weight:600;">${invoice.buyerCompanyName || '-'}</div></div>
            <div><label style="color:#64748b; font-size:0.8rem;">Adres / Diğer</label><div style="font-weight:600;">${invoice.buyerCompanyAddress || '-'}</div></div>
        `;

        // Populate actions
        const canWrite = window.hasPermission('Invoices', 'write');
        if (actions) {
            actions.innerHTML = "";
            if (!invoice.isDeleted && invoice.status === 1 && canWrite) {
                actions.innerHTML += `<button class="btn-action" style="padding:0.75rem 1.5rem; background:#ef4444; border-radius:8px;" onclick="rejectInvoice(${invoice.id})">Reddet</button>`;
                actions.innerHTML += `<button class="btn-action" style="padding:0.75rem 1.5rem; background:var(--primary); border-radius:8px;" onclick="approveInvoice(${invoice.id})">Onayla</button>`;
            }
            actions.innerHTML += `<button class="btn-action" style="padding:0.75rem 1.5rem; background:#64748b; border-radius:8px;" onclick="switchView('invoices')">Sayfayı Kapat</button>`;
        }

        // populate items
        if (tbody) {
            tbody.innerHTML = '';
            if (!invoice.items || invoice.items.length === 0) {
                tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">Kalem bulunamadı.</td></tr>';
            } else {
                let subtotal = 0;
                invoice.items.forEach(item => {
                    const total = item.quantity * item.unitPrice;
                    const totalWithTax = total + (total * (item.vatRate / 100));
                    subtotal += totalWithTax;

                    const tr = document.createElement('tr');
                    tr.innerHTML = `
                        <td>${item.productCode || '-'}</td>
                        <td>${item.productName || '-'}</td>
                        <td>${item.warehouseName || '-'}${item.shelfName ? ' / ' + item.shelfName : ''}</td>
                        <td>${item.quantity}</td>
                        <td>${formatMoney(item.unitPrice)} ₺</td>
                        <td>%${item.vatRate}</td>
                        <td>${formatMoney(totalWithTax)} ₺</td>
                    `;
                    tbody.appendChild(tr);
                });

                if (tfoot) {
                    tfoot.innerHTML = `
                        <tr>
                            <td colspan="6" style="text-align:right;">Genel Toplam:</td>
                            <td>${formatMoney(subtotal)} ₺</td>
                        </tr>
                    `;
                }
            }
        }
    } catch (e) {
        showToast('Hata: ' + e.message, 'error');
    }
}

window.approveOffer = async function (id) {
    if (!(await showConfirm('Onay', 'Teklifi onaylamak istiyor musunuz?', 'Onayla', 'İptal'))) return;
    try {
        const user = JSON.parse(localStorage.getItem('user'));
        const res = await fetch(`${API_BASE_URL}/api/offer/${id}/approve?userId=${user.id}`, {
            method: 'POST'
        });
        if (res.ok) {
            showToast('Teklif onaylandı');
            loadOffers();
        } else {
            alert('İşlem başarısız.');
        }
    } catch (e) { alert('Hata: ' + e.message); }
}

window.deleteOffer = async function (id) {
    if (!(await showConfirm('Silme Onayı', 'Bu teklifi silmek istediğinize emin misiniz? Bu işlem geri alınamaz.', 'Sil', 'Vazgeç'))) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/offer/${id}`, { method: 'DELETE' });
        if (res.ok) {
            showToast('Teklif başarıyla silindi');
            loadOffers();
        } else {
            const err = await res.text();
            alert('Silme işlemi başarısız: ' + err);
        }
    } catch (e) { alert('Hata: ' + e.message); }
}

window.createInvoiceFromOffer = async function (id) {
    if (!(await showConfirm('Fatura Oluştur', 'Bu tekliften fatura oluşturulsun mu?', 'Oluştur', 'İptal'))) return;
    try {
        const user = JSON.parse(localStorage.getItem('user'));
        const res = await fetch(`${API_BASE_URL}/api/offer/${id}/invoice?userId=${user.id}`, { method: 'POST' });
        if (res.ok) {
            const invoiceId = await res.json();
            showToast('Fatura başarıyla oluşturuldu');
            loadOffers();
        } else {
            alert('İşlem başarısız.');
        }
    } catch (e) { alert('Hata: ' + e.message); }
}

window.approveInvoice = async function (id) {
    if (!(await showConfirm('Fatura Onayı', 'Faturayı onaylamak ve stok düşümü yapmak istiyor musunuz?', 'Onayla', 'İptal'))) return;
    try {
        const user = JSON.parse(localStorage.getItem('user'));
        const res = await fetch(`${API_BASE_URL}/api/invoices/${id}/approve?userId=${user.id}`, { method: 'POST' });
        if (res.ok) {
            showToast('Fatura onaylandı ve stok güncellendi');
            loadInvoices();
            if (document.getElementById('section-invoice-details') && document.getElementById('section-invoice-details').style.display !== 'none') {
                openInvoiceDetails(id);
            }
        } else {
            alert('İşlem başarısız.');
        }
    } catch (e) { alert('Hata: ' + e.message); }
}

// COMPANIES & INVENTORY & MODALS

// Quick Close for User Modal specifically (legacy support)
window.createUserModalClose = function () { closeModal('userModal'); }

// COMPANIES
window.loadCompanies = async function () {
    const tbody = document.getElementById('companyListBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;">Yükleniyor...</td></tr>';
    try {
        const filter = window.currentStatusFilter['companies'] || 'active';

        window.companyPage = window.companyPage || 1;
        window.companyPageSize = window.companyPageSize || 10;

        const res = await fetch(`${API_BASE_URL}/api/companies?status=${filter}&page=${window.companyPage}&pageSize=${window.companyPageSize}`);
        if (res.status === 403) {
            tbody.innerHTML = '<tr><td colspan="4" style="color:var(--error); text-align:center;">🚫 Yetkisiz Erişim</td></tr>';
            return;
        }

        const pagedData = await res.json();
        const data = pagedData.items || [];
        const totalCount = pagedData.totalCount || 0;
        const totalPages = Math.ceil(totalCount / window.companyPageSize);

        tbody.innerHTML = '';
        if (data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;">Kayıt yok.</td></tr>';
            renderPagination(document.getElementById('companyPaginationContainer'), totalPages, window.companyPage, window.companyPageSize, () => loadCompanies());
            return;
        }

        const canDelete = window.hasPermission('Companies', 'delete'); // Companies module

        data.forEach(c => {
            const tr = document.createElement('tr');
            tr.innerHTML = `<td>${c.companyName}</td><td>${c.taxNumber || '-'}</td><td>${c.allowNegativeStock ? 'Evet' : 'Hayır'}</td>
                <td style="text-align:right;">
                    <div class="action-btn-container">
                        ${!c.isDeleted ? `
                            <button class="btn-action btn-edit" onclick="openCompanyModal(${c.id})">Düzenle</button>
                            ${canDelete ? `<button class="btn-action btn-delete" onclick="deleteCompany(${c.id}, '${c.companyName}')">Sil</button>` : ''}
                        ` : ''}
                        ${c.isDeleted && window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('company', ${c.id})">✅ Geri Yükle</button>` : ''}
                    </div>
                </td>`;
            tbody.appendChild(tr);
        });

        if (!document.getElementById('companyPaginationContainer')) {
            const table = tbody.closest('table');
            const pagContainer = document.createElement('div');
            pagContainer.id = 'companyPaginationContainer';
            pagContainer.className = 'pagination-container';
            table.parentNode.insertBefore(pagContainer, table.nextSibling);
        }
        renderPagination(document.getElementById('companyPaginationContainer'), totalPages, window.companyPage, window.companyPageSize, (newPage, newSize) => {
            window.companyPage = newPage;
            window.companyPageSize = newSize;
            loadCompanies();
        });

    } catch (e) { console.error(e); tbody.innerHTML = '<tr><td colspan="4" style="color:var(--error);">Hata!</td></tr>'; }
}

window.openCompanyModal = async function (id) {
    const modal = document.getElementById('companyModal');
    modal.style.display = 'flex';
    const cid = document.getElementById('cId');
    const title = document.getElementById('companyModalTitle');

    // Reset
    cid.value = '';
    document.getElementById('cName').value = '';
    document.getElementById('cTax').value = '';
    document.getElementById('cAddr').value = '';
    document.getElementById('cNegative').checked = false;

    if (id) {
        title.innerText = 'Şirket Düzenle';
        cid.value = id;
        try {
            const res = await fetch(`${API_BASE_URL}/api/companies/${id}`);
            if (res.ok) {
                const c = await res.json();
                console.log("Loading company data:", c);
                document.getElementById('cName').value = c.companyName || c.CompanyName || '';
                document.getElementById('cTax').value = c.taxNumber || c.TaxNumber || '';
                document.getElementById('cAddr').value = c.address || c.Address || '';
                document.getElementById('cNegative').checked = (c.allowNegativeStock === true || c.AllowNegativeStock === true);
            } else {
                console.error("Company fetch failed:", res.status);
                showToast("Şirket bilgileri yüklenemedi", "error");
            }
        } catch (e) {
            console.error("Company load failed", e);
            showToast("Bağlantı hatası", "error");
        }
    } else {
        title.innerText = 'Yeni Şirket';
    }
}

window.createCompany = async function () {
    const id = document.getElementById('cId').value;
    const dto = {
        companyName: document.getElementById('cName').value,
        taxNumber: document.getElementById('cTax').value,
        address: document.getElementById('cAddr').value,
        allowNegativeStock: document.getElementById('cNegative').checked
    };
    if (!dto.companyName) return alert('Şirket adı zorunlu.');

    try {
        const url = id ? `${API_BASE_URL}/api/companies/${id}` : `${API_BASE_URL}/api/companies`;
        const method = id ? 'PUT' : 'POST';

        const res = await fetch(url, {
            method: method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(dto)
        });
        if (res.ok) {
            showToast(id ? 'Şirket güncellendi' : 'Şirket eklendi');
            closeModal('companyModal');
            loadCompanies();
        }
        else {
            let errMsg = 'Bilinmeyen hata';
            try {
                const text = await res.text();
                try {
                    const json = JSON.parse(text);
                    errMsg = json.message || json.title || JSON.stringify(json);
                } catch { errMsg = text || res.statusText; }
            } catch (e) { errMsg = e.message; }
            alert('Hata: ' + errMsg);
        }
    } catch (e) { alert('Hata: ' + e.message); }
}

window.deleteCompany = async function (id, name) {
    if (!(await showConfirm('Şirket Silme', `"${name}" şirketini silmek istiyor musunuz?`))) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/companies/${id}`, { method: 'DELETE' });
        if (res.ok) { alert('Şirket silindi.'); loadCompanies(); }
        else {
            let errMsg = await res.text();
            alert('Silinemedi: ' + errMsg);
        }
    } catch (e) { alert('Hata: ' + e.message); }
}

// INVENTORY TABS
window.switchInvTab = function (tabName) {
    const btn = document.querySelector(`.tab-inv-btn[data-tab="${tabName}"]`);
    if (btn) {
        const perm = btn.getAttribute('data-permission');
        if (perm) {
            const [m, t] = perm.split(':');
            if (!window.hasPermission(m, (t || '').toLowerCase())) {
                return;
            }
        }
    }

    document.querySelectorAll('.inv-tab').forEach(el => el.style.display = 'none');
    const targetTab = document.getElementById(`tab-inv-${tabName}`);
    if (targetTab) targetTab.style.display = 'block';

    document.querySelectorAll('.tab-inv-btn').forEach(el => el.classList.remove('active'));
    if (btn) btn.classList.add('active');

    // Update the Filter Callback for the current tab
    const callbackMap = {
        'products': 'loadProducts',
        'warehouses': 'loadWarehouses',
        'brands': 'loadBrands',
        'categories': 'loadCategories',
        'units': 'loadUnits'
    };

    if (callbackMap[tabName]) {
        window.renderStatusFilter('inventory', 'filter-container-inventory', callbackMap[tabName]);
    }

    if (tabName === 'products') loadProducts();
    if (tabName === 'stock-entry') loadStockEntry();
    if (tabName === 'warehouses') loadWarehouses();
    if (tabName === 'brands') loadBrands();
    if (tabName === 'categories') loadCategories();
    if (tabName === 'units') loadUnits();
}

// PRODUCTS
// PRODUCTS
window.loadProducts = async function () {
    const tbody = document.getElementById('productListBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="8">Yükleniyor...</td></tr>';
    try {
        const filter = window.currentStatusFilter['inventory'] || 'active';
        const searchTerm = document.getElementById('inventorySearchInput')?.value || '';

        // Pagination parameters
        window.productPage = window.productPage || 1;
        window.productPageSize = window.productPageSize || 10;

        // Fetch products with status filter, search term, and pagination
        const prodRes = await fetch(`${API_BASE_URL}/api/product?status=${filter}&searchTerm=${encodeURIComponent(searchTerm)}&page=${window.productPage}&pageSize=${window.productPageSize}`);
        if (prodRes.status === 403) {
            tbody.innerHTML = '<tr><td colspan="8" style="color:var(--error); text-align:center;">🚫 Bu verileri görmeye yetkiniz bulunmamaktadır.</td></tr>';
            return;
        }
        if (!prodRes.ok) throw new Error('Ürünler yüklenemedi');

        const pagedData = await prodRes.json();
        const products = pagedData.items || [];
        const totalCount = pagedData.totalCount || 0;
        const totalPages = Math.ceil(totalCount / window.productPageSize);


        // Fetch other data in parallel (status filtering applied there too for consistency)
        const [brands, cats, units, warehouses] = await Promise.all([
            fetch(`${API_BASE_URL}/api/product/brands?status=${filter}`).then(r => r.ok ? r.json() : []),
            fetch(`${API_BASE_URL}/api/product/categories?status=${filter}`).then(r => r.ok ? r.json() : []),
            fetch(`${API_BASE_URL}/api/product/units?status=${filter}`).then(r => r.ok ? r.json() : []),
            fetch(`${API_BASE_URL}/api/warehouse?status=${filter}`).then(r => r.ok ? r.json() : [])
        ]);

        tbody.innerHTML = '';
        if (products.length === 0) {
            tbody.innerHTML = '<tr><td colspan="9">Kayıt yok.</td></tr>';
            renderPagination(document.getElementById('productPaginationContainer'), totalPages, window.productPage, window.productPageSize, () => loadProducts());
            return;
        }

        // Helper to find names
        const getBrandName = (p) => p.brandName || '-';
        const getCatName = (p) => p.categoryName || '-';
        const getUnitName = (p) => p.unitName || '-';
        const getWareName = (p) => p.warehouseName || '-';

        const canWrite = window.hasPermission('Product', 'write');
        const canDelete = window.hasPermission('Product', 'delete');

        products.forEach(p => {
            const tr = document.createElement('tr');
            if (p.isDeleted) tr.style.opacity = '0.6';

            const imgUrl = p.imageUrl || 'https://via.placeholder.com/40';
            tr.innerHTML = `
                <td><img src="${imgUrl}" style="width:40px; height:40px; object-fit:contain; border-radius:4px; border:1px solid #eee;"></td>
                <td>${p.productCode || '-'} ${p.isDeleted ? '<br><span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;">PASİF (SİLİNDİ)</span>' : ''}</td>
                <td>${p.productName}</td>
                <td>${getCatName(p)}</td>
                <td>${getBrandName(p)}</td>
                <td>${getWareName(p)}</td>
                <td>${p.currentStock} ${getUnitName(p)}</td>
                <td style="text-align:right;">
                    <div class="action-btn-container">
                        ${canWrite && !p.isDeleted ? `
                         <button class="btn-action btn-edit" 
                            onclick="openProductModal(${p.id})">Düzenle</button>
                        ` : ''}
                        ${canDelete && !p.isDeleted ? `
                        <button class="btn-action btn-delete" 
                            onclick="deleteProduct(${p.id})">Sil</button>
                        ` : ''}
                        ${p.isDeleted ? (window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('product', ${p.id})">✅ Geri Yükle</button>` : '<span style="font-size:0.75rem; color:var(--muted); font-style:italic;">İşlem Yapılamaz</span>') : ''}
                    </div>
                </td>
            `;
            tbody.appendChild(tr);
        });

        // Add matching pagination container under the table if not existing
        if (!document.getElementById('productPaginationContainer')) {
            const table = tbody.closest('table');
            const pagContainer = document.createElement('div');
            pagContainer.id = 'productPaginationContainer';
            pagContainer.className = 'pagination-container';
            table.parentNode.insertBefore(pagContainer, table.nextSibling);
        }
        renderPagination(document.getElementById('productPaginationContainer'), totalPages, window.productPage, window.productPageSize, (newPage, newSize) => {
            window.productPage = newPage;
            window.productPageSize = newSize;
            loadProducts();
        });

    } catch (e) {
        console.error(e);
        tbody.innerHTML = '<tr><td colspan="8" style="color:red;">Hata: ' + e.message + '</td></tr>';
    }
}

window.loadWarehouses = async function () {
    const tbody = document.getElementById('warehouseListBody');
    const tbodyMain = document.getElementById('warehouseListBody_Main');
    if (!tbody && !tbodyMain) return;

    if (tbody) tbody.innerHTML = '<tr><td colspan="4">Yükleniyor...</td></tr>';
    if (tbodyMain) tbodyMain.innerHTML = '<tr><td colspan="4">Yükleniyor...</td></tr>';

    try {
        const filter = window.currentStatusFilter['inventory'] || 'active';
        const searchTerm = document.getElementById('inventorySearchInput')?.value || '';
        const res = await fetch(`${API_BASE_URL}/api/warehouse?status=${filter}&searchTerm=${encodeURIComponent(searchTerm)}`);
        const data = await res.json();

        const renderRows = (target) => {
            if (!target) return;
            target.innerHTML = '';
            if (data.length === 0) { target.innerHTML = '<tr><td colspan="4">Kayıt yok.</td></tr>'; return; }

            const canDelete = window.hasPermission('WarehouseManagement', 'delete');

            data.forEach(w => {
                const tr = document.createElement('tr');
                if (w.isDeleted) tr.style.opacity = '0.6';
                tr.innerHTML = `
                    <td>${w.warehouseName} ${w.isDeleted ? '<span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;margin-left:5px;">PASİF</span>' : ''}</td>
                    <td>${w.location || '-'}</td>
                    <td>${w.companyName || '-'}</td>
                    <td style="text-align:right;">
                        <div class="action-btn-container">
                            ${!w.isDeleted ? `
                            <button class="btn-action btn-edit" onclick="openWarehouseModal(${w.id})">Düzenle</button>
                            ${canDelete ? `<button class="btn-action btn-delete" onclick="deleteWarehouse(${w.id})">Sil</button>` : ''}
                            ` : (window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('warehouse', ${w.id})">✅ Geri Yükle</button>` : '<span style="font-size:0.75rem; color:var(--muted); font-style:italic;">Silindi</span>')}
                        </div>
                    </td>`;
                target.appendChild(tr);
            });
        };

        renderRows(tbody);
        renderRows(tbodyMain);
    } catch (e) { console.error(e); }
};

window.loadBrands = async function () {
    const tbody = document.getElementById('brandListBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="2">Yükleniyor...</td></tr>';
    try {
        const filter = window.currentStatusFilter['inventory'] || 'active';
        const searchTerm = document.getElementById('inventorySearchInput')?.value || '';
        const res = await fetch(`${API_BASE_URL}/api/product/brands?status=${filter}&searchTerm=${encodeURIComponent(searchTerm)}`);
        const data = await res.json();
        tbody.innerHTML = '';
        if (data.length === 0) { tbody.innerHTML = '<tr><td colspan="2">Kayıt yok.</td></tr>'; return; }
        const canWrite = window.hasPermission('Product', 'write');
        data.forEach(b => {
            const tr = document.createElement('tr');
            if (b.isDeleted) tr.style.opacity = '0.6';
            tr.innerHTML = `<td>${b.brandName} ${b.isDeleted ? '<span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;margin-left:5px;">PASİF</span>' : ''}</td>
                <td style="text-align:right;">
                    ${canWrite && !b.isDeleted ? `<button class="btn-action btn-delete" onclick="deleteSimple('Brand', ${b.id})">Sil</button>` : ''}
                    ${b.isDeleted && window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('brand', ${b.id})">✅ Geri Yükle</button>` : ''}
                </td>`;
            tbody.appendChild(tr);
        });
    } catch (e) { console.error(e); }
};

window.loadCategories = async function () {
    const tbody = document.getElementById('categoryListBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="2">Yükleniyor...</td></tr>';
    try {
        const filter = window.currentStatusFilter['inventory'] || 'active';
        const searchTerm = document.getElementById('inventorySearchInput')?.value || '';
        const res = await fetch(`${API_BASE_URL}/api/product/categories?status=${filter}&searchTerm=${encodeURIComponent(searchTerm)}`);
        const data = await res.json();
        tbody.innerHTML = '';
        if (data.length === 0) { tbody.innerHTML = '<tr><td colspan="2">Kayıt yok.</td></tr>'; return; }
        const canWrite = window.hasPermission('Product', 'write');
        data.forEach(c => {
            const tr = document.createElement('tr');
            if (c.isDeleted) tr.style.opacity = '0.6';
            tr.innerHTML = `<td>${c.categoryName} ${c.isDeleted ? '<span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;margin-left:5px;">PASİF</span>' : ''}</td>
                <td style="text-align:right;">
                    ${canWrite && !c.isDeleted ? `<button class="btn-action btn-delete" onclick="deleteSimple('Category', ${c.id})">Sil</button>` : ''}
                    ${c.isDeleted && window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('category', ${c.id})">✅ Geri Yükle</button>` : ''}
                </td>`;
            tbody.appendChild(tr);
        });
    } catch (e) { console.error(e); }
};

window.loadUnits = async function () {
    const tbody = document.getElementById('unitListBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="3">Yükleniyor...</td></tr>';
    try {
        const filter = window.currentStatusFilter['inventory'] || 'active';
        const searchTerm = document.getElementById('inventorySearchInput')?.value || '';
        const res = await fetch(`${API_BASE_URL}/api/product/units?status=${filter}&searchTerm=${encodeURIComponent(searchTerm)}`);
        const data = await res.json();
        tbody.innerHTML = '';
        if (data.length === 0) { tbody.innerHTML = '<tr><td colspan="3">Kayıt yok.</td></tr>'; return; }
        const canWrite = window.hasPermission('Product', 'write');
        data.forEach(u => {
            const tr = document.createElement('tr');
            if (u.isDeleted) tr.style.opacity = '0.6';
            tr.innerHTML = `<td>${u.unitName} ${u.isDeleted ? '<span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;margin-left:5px;">PASİF</span>' : ''}</td>
                <td>${u.unitShortName}</td>
                <td style="text-align:right;">
                    ${canWrite && !u.isDeleted ? `<button class="btn-action btn-delete" onclick="deleteSimple('Unit', ${u.id})">Sil</button>` : ''}
                    ${u.isDeleted && window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('unit', ${u.id})">✅ Geri Yükle</button>` : ''}
                </td>`;
            tbody.appendChild(tr);
        });
    } catch (e) { console.error(e); }
};

window.loadShelvesForProduct = async function (warehouseId, selectedShelfId = null) {
    const shelfSel = document.getElementById('pShelf');
    if (!shelfSel) return;
    shelfSel.innerHTML = '<option value="">Yükleniyor...</option>';

    if (!warehouseId || warehouseId === "null") {
        shelfSel.innerHTML = '<option value="">Önce Depo Seçin</option>';
        return;
    }

    try {
        const res = await fetch(`${API_BASE_URL}/api/warehouse/${warehouseId}/shelves`);
        if (res.ok) {
            const shelves = await res.json();
            if (shelves.length === 0) {
                shelfSel.innerHTML = '<option value="">Bu depoda raf bulunamadı</option>';
                return;
            }
            shelfSel.innerHTML = '<option value="">Raf Seç...</option>';
            shelves.forEach(s => {
                const selected = (selectedShelfId && s.id == selectedShelfId) ? 'selected' : '';
                shelfSel.innerHTML += `<option value="${s.id}" ${selected}>${s.name}</option>`;
            });
        } else {
            console.error("Shelf load failed:", await res.text());
            shelfSel.innerHTML = '<option value="">Raf Yüklenemedi</option>';
        }
    } catch (e) {
        console.error("Shelf load error:", e);
        shelfSel.innerHTML = '<option value="">Hata!</option>';
    }
};

window.openProductModal = async function (id = null) {
    document.getElementById('productModal').style.display = 'flex';
    document.querySelector('#productModal h3').innerText = id ? 'Ürün Düzenle' : 'Yeni Ürün';
    document.getElementById('pId').value = id || '';

    try {
        // Load Dropdowns FIRST
        const [cats, brands, units, warehousesRes] = await Promise.all([
            fetch(`${API_BASE_URL}/api/product/categories`).then(r => r.json()),
            fetch(`${API_BASE_URL}/api/product/brands`).then(r => r.json()),
            fetch(`${API_BASE_URL}/api/product/units`).then(r => r.json()),
            fetch(`${API_BASE_URL}/api/warehouse?pageSize=1000`).then(r => r.json())
        ]);
        const warehouses = Array.isArray(warehousesRes) ? warehousesRes : (warehousesRes.items || []);

        const catSel = document.getElementById('pCategory');
        catSel.innerHTML = '<option value="">Kategori Seç...</option>';
        cats.forEach(c => catSel.innerHTML += `<option value="${c.id}">${c.categoryName}</option>`);

        const brandSel = document.getElementById('pBrand');
        brandSel.innerHTML = '<option value="">Marka Seç...</option>';
        brands.forEach(b => brandSel.innerHTML += `<option value="${b.id}">${b.brandName}</option>`);

        const unitSel = document.getElementById('pUnit');
        unitSel.innerHTML = '<option value="">Birim Seç...</option>';
        units.forEach(u => unitSel.innerHTML += `<option value="${u.id}">${u.unitName}</option>`);

        const whSel = document.getElementById('pWarehouse');
        whSel.innerHTML = '<option value="">Depo Seç...</option>';
        warehouses.forEach(w => whSel.innerHTML += `<option value="${w.id}">${w.warehouseName}</option>`);

        // THEN Set Values
        if (id) {
            // Edit Mode - Fetch product details
            const res = await fetch(`${API_BASE_URL}/api/product/${id}`);
            const p = await res.json();
            document.getElementById('pCode').value = p.productCode || '';
            document.getElementById('pSystemCode').value = p.systemCode || '';
            document.getElementById('pName').value = p.productName;
            document.getElementById('pCategory').value = p.categoryId;
            document.getElementById('pBrand').value = p.brandId;
            document.getElementById('pUnit').value = p.unitId;
            document.getElementById('pImageUrl').value = p.imageUrl || '';
            document.getElementById('pImagePreview').src = p.imageUrl || 'https://via.placeholder.com/100';

            // Warehouse & Shelf
            document.getElementById('pWarehouse').value = p.warehouseId || '';
            document.getElementById('pIsPhysical').checked = (p.isPhysical !== undefined) ? p.isPhysical : true;

            if (p.warehouseId) {
                await loadShelvesForProduct(p.warehouseId, p.shelfId);
            } else {
                document.getElementById('pShelf').innerHTML = '<option value="">Raf Seç...</option>';
            }

        } else {
            // New Mode - Clear fields
            document.getElementById('pCode').value = '';
            document.getElementById('pSystemCode').value = '';
            document.getElementById('pName').value = '';
            document.getElementById('pCategory').value = '';
            document.getElementById('pBrand').value = '';
            document.getElementById('pUnit').value = '';
            document.getElementById('pImageUrl').value = '';
            document.getElementById('pImagePreview').src = 'https://via.placeholder.com/100';

            document.getElementById('pWarehouse').value = '';
            document.getElementById('pShelf').innerHTML = '<option value="">Raf Seç...</option>';
            document.getElementById('pIsPhysical').checked = true;
        }
    } catch (e) {
        alert('Veriler yüklenirken hata oluştu: ' + e.message);
        console.error(e);
    }
}

window.createProduct = async function () {
    const id = document.getElementById('pId').value;
    const pCode = document.getElementById('pCode').value;
    const pName = document.getElementById('pName').value;
    const catVal = document.getElementById('pCategory').value;
    const brandVal = document.getElementById('pBrand').value;
    const unitVal = document.getElementById('pUnit').value;

    if (!pName) return alert('Ürün Adı zorunludur.');
    if (!pCode) return alert('Ürün Kodu zorunludur.');
    if (!catVal) return alert('Lütfen bir Kategori seçiniz.');
    if (!brandVal) return alert('Lütfen bir Marka seçiniz.');
    if (!unitVal) return alert('Lütfen bir Birim seçiniz.');

    const whVal = document.getElementById('pWarehouse').value;
    const shelfVal = document.getElementById('pShelf').value;
    const isPhysical = document.getElementById('pIsPhysical').checked;

    const dto = {
        productCode: pCode,
        productName: pName,
        categoryId: parseInt(catVal),
        brandId: parseInt(brandVal),
        unitId: parseInt(unitVal),
        imageUrl: document.getElementById('pImageUrl').value,
        warehouseId: whVal ? parseInt(whVal) : null,
        shelfId: shelfVal ? parseInt(shelfVal) : null,
        isPhysical: isPhysical,
        initialStock: id ? 0 : 0 // Managed by separate stock update if needed, but for NEW we can set it if UI has input
    };

    if (id) {
        dto.id = parseInt(id);
    }

    try {
        const method = id ? 'PUT' : 'POST';
        const res = await fetch(`${API_BASE_URL}/api/product`, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });

        if (res.ok) {
            showToast(id ? 'Ürün güncellendi' : 'Ürün başarıyla eklendi');
            closeModal('productModal');
            loadProducts();
        } else {
            let errMsg = 'Bilinmeyen hata';
            try {
                const text = await res.text();
                try {
                    const errJson = JSON.parse(text);
                    errMsg = errJson.message || errJson.title || JSON.stringify(errJson);
                } catch {
                    errMsg = text;
                }
            } catch (e) { errMsg = e.message; }
            alert('Sunucu Hatası: ' + errMsg);
        }
    } catch (e) {
        alert('Bağlantı Hatası: ' + e.message);
    }
}

window.uploadProductImage = async function (input) {
    if (!input.files || input.files.length === 0) return;
    const file = input.files[0];

    // Client-side Resize Logic
    const MAX_WIDTH = 800;
    const MAX_HEIGHT = 800;
    const MAX_SIZE_MB = 2;

    const reader = new FileReader();
    reader.onload = function (e) {
        const img = new Image();
        img.onload = async function () {
            let width = img.width;
            let height = img.height;

            // Calculate new dimensions
            if (width > height) {
                if (width > MAX_WIDTH) {
                    height *= MAX_WIDTH / width;
                    width = MAX_WIDTH;
                }
            } else {
                if (height > MAX_HEIGHT) {
                    width *= MAX_HEIGHT / height;
                    height = MAX_HEIGHT;
                }
            }

            const canvas = document.createElement('canvas');
            canvas.width = width;
            canvas.height = height;
            const ctx = canvas.getContext('2d');
            ctx.drawImage(img, 0, 0, width, height);

            // Convert to Blob (JPEG for compression)
            canvas.toBlob(async (blob) => {
                if (!blob) return alert('Resim işlenemedi.');

                if (blob.size > MAX_SIZE_MB * 1024 * 1024) {
                    alert(`Resim çok büyük. Maksimum ${MAX_SIZE_MB} MB sınırını aşıyor.`);
                    return;
                }

                const formData = new FormData();
                formData.append('file', blob, file.name);

                try {
                    const res = await fetch(`${API_BASE_URL}/api/upload`, {
                        method: 'POST',
                        body: formData
                    });
                    if (res.ok) {
                        const data = await res.json();
                        document.getElementById('pImageUrl').value = data.url;
                        document.getElementById('pImagePreview').src = data.url;
                        input.value = '';
                    } else {
                        const err = await res.text();
                        alert('Yükleme başarısız: ' + err);
                    }
                } catch (err) {
                    alert('Hata: ' + err.message);
                }
            }, 'image/jpeg', 0.85); // 0.85 quality JPEG
        };
        img.src = e.target.result;
    };
    reader.readAsDataURL(file);
}

window.toggleSidebar = function () {
    const sidebar = document.getElementById('sidebar');
    sidebar.classList.toggle('open');
}

window.toggleUserDropdown = function (e) {
    e.stopPropagation();
    const dd = document.getElementById('userDropdown');
    dd.classList.toggle('show');
}

// Close dropdown when clicking outside
window.addEventListener('click', () => {
    const dd = document.getElementById('userDropdown');
    if (dd) dd.classList.remove('show');
});

window.toggleMenuGroup = function (header) {
    const content = header.nextElementSibling;
    header.classList.toggle('collapsed');
    content.classList.toggle('collapsed');
}

// Quick Actions Logic
const MASTER_QUICK_ACTIONS = [
    { id: 'users', label: '👤 Kullanıcı Yönetimi', color: 'var(--primary)', view: 'users' },
    { id: 'stock-entry', label: '📦 Stok Girişi Yap', color: '#10b981', view: 'stock-entry' },
    { id: 'offer-wizard', label: '✨ Teklif Oluştur', color: '#4F46E5', func: 'startOfferWizard' },
    { id: 'inventory', label: '🔍 Stok Kontrolü', color: '#6366f1', view: 'inventory' },
    { id: 'new-customer', label: '🤝 Yeni Müşteri', color: '#f59e0b', func: 'openCustomerModal' },
    { id: 'reports', label: '📊 Raporları Gör', color: '#8b5cf6', view: 'reports' },
    { id: 'suppliers', label: '🚚 Tedarikçiler', color: '#ec4899', view: 'suppliers' }
];

window.renderQuickActions = function () {
    const user = JSON.parse(localStorage.getItem('user'));
    const container = document.getElementById('quickActionsContainer');
    if (!container) return;

    let selectedIds = [];
    try {
        selectedIds = user.quickActionsJson ? JSON.parse(user.quickActionsJson) : ['users', 'stock-entry', 'offer-wizard'];
    } catch (e) {
        selectedIds = ['users', 'stock-entry', 'offer-wizard'];
    }

    const selectedActions = MASTER_QUICK_ACTIONS.filter(a => selectedIds.includes(a.id));

    if (selectedActions.length === 0) {
        container.innerHTML = '<div style="color: var(--muted); font-size: 0.9rem;">Henüz hızlı işlem eklenmemiş. Ayarlar sekmesinden ekleyebilirsiniz.</div>';
        return;
    }

    container.innerHTML = selectedActions.map(a => `
        <button class="btn-primary" onclick="${a.func ? a.func + '()' : "switchView('" + a.view + "')"}" 
                style="width:auto; background:${a.color};">
            ${a.label}
        </button>
    `).join('');
};

window.initQuickActionConfig = function () {
    const user = JSON.parse(localStorage.getItem('user'));
    const grid = document.getElementById('qaConfigGrid');
    if (!grid) return;

    let selectedIds = [];
    try {
        selectedIds = user.quickActionsJson ? JSON.parse(user.quickActionsJson) : ['users', 'stock-entry', 'offer-wizard'];
    } catch (e) {
        selectedIds = ['users', 'stock-entry', 'offer-wizard'];
    }

    grid.innerHTML = MASTER_QUICK_ACTIONS.map(a => `
        <div class="qa-config-card" onclick="this.querySelector('input').click(); event.stopPropagation();">
            <input type="checkbox" id="qa-${a.id}" data-id="${a.id}" ${selectedIds.includes(a.id) ? 'checked' : ''} onclick="event.stopPropagation();">
            <label for="qa-${a.id}" onclick="event.stopPropagation();">${a.label}</label>
        </div>
    `).join('');
};

window.saveQuickActionSettings = async function () {
    const checkboxes = document.querySelectorAll('#qaConfigGrid input[type="checkbox"]');
    const selectedIds = Array.from(checkboxes).filter(cb => cb.checked).map(cb => cb.dataset.id);

    const user = JSON.parse(localStorage.getItem('user'));
    const json = JSON.stringify(selectedIds);

    try {
        // We use the existing update user endpoint
        const dto = {
            email: user.email,
            firstName: user.firstName,
            lastName: user.lastName,
            regNo: user.regNo || '',
            roleId: user.roleId,
            companyId: user.companyId || null,
            isActive: true,
            quickActionsJson: json
        };

        const res = await fetch(`${API_BASE_URL}/api/users/${user.id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });

        if (!res.ok) throw new Error('Ayarlar kaydedilemedi');

        const updatedUser = await res.json();
        // Update local storage
        localStorage.setItem('user', JSON.stringify(updatedUser));

        showToast('Hızlı işlem ayarlarınız başarıyla kaydedildi!');
        switchView('dashboard');
    } catch (e) {
        alert('Hata: ' + e.message);
    }
};

window.loadProfile = function () {
    const user = JSON.parse(localStorage.getItem('user'));
    if (!user) return;

    // Fill form
    const fName = document.getElementById('profileFirstName');
    const lName = document.getElementById('profileLastName');
    const email = document.getElementById('profileEmail');
    const regNo = document.getElementById('profileRegNo');

    if (fName) fName.value = user.firstName || '';
    if (lName) lName.value = user.lastName || '';
    if (email) email.value = user.email || '';
    if (regNo) regNo.value = user.regNo || '';

    // Load Quick Actions
    initQuickActionConfig();
};

window.updateProfile = async function () {
    const user = JSON.parse(localStorage.getItem('user'));

    // Get values
    const firstName = document.getElementById('profileFirstName').value;
    const lastName = document.getElementById('profileLastName').value;
    const email = document.getElementById('profileEmail').value;
    const regNo = document.getElementById('profileRegNo').value;

    const dto = {
        email: email,
        firstName: firstName,
        lastName: lastName,
        regNo: regNo,
        roleId: user.roleId,
        companyId: user.companyId || null,
        isActive: true, // Assuming active since they are logged in
        quickActionsJson: user.quickActionsJson // Preserve existing
    };

    try {
        const res = await fetch(`${API_BASE_URL}/api/users/${user.id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });

        if (!res.ok) throw new Error('Profil güncellenemedi.');

        const updatedUser = await res.json();
        // Manually ensure roleId is preserved if backend didn't return it (though it should have per recent changes)
        if (!updatedUser.roleId) updatedUser.roleId = user.roleId;

        localStorage.setItem('user', JSON.stringify(updatedUser)); // Update local storage
        showToast('Profil bilgileriniz güncellendi');

        // Update header name display if Dashboard is loaded
        const userDisplay = document.getElementById('userNameDisplay');
        if (userDisplay) userDisplay.innerText = updatedUser.firstName + ' ' + updatedUser.lastName;

    } catch (e) {
        alert('Hata: ' + e.message);
    }
};

// PASSWORD STRENGTH METER
let GLOBAL_FORCE_STRONG_PASSWORD = false;

window.checkPasswordStrength = function (password) {
    const bar = document.getElementById('passwordStrengthBar');
    const text = document.getElementById('passwordStrengthText');
    if (!bar || !text) return;

    let strength = 0;
    const isLengthOk = password.length >= 6;
    const hasLower = /[a-z]/.test(password);
    const hasUpper = /[A-Z]/.test(password);
    const hasDigit = /[0-9]/.test(password);
    const hasSpecial = /[^a-zA-Z0-9]/.test(password);

    if (isLengthOk) strength++;
    if (hasLower) strength++;
    if (hasUpper) strength++;
    if (hasDigit) strength++;
    if (hasSpecial) strength++;

    bar.className = 'pass-strength-bar';
    text.style.color = 'var(--muted)'; // Default

    if (password.length === 0) {
        bar.style.width = '0%';
        text.innerText = GLOBAL_FORCE_STRONG_PASSWORD ? 'En az 6 karakter, Büyük/Küçük harf ve Rakam gereklidir' : 'Lütfen şifre girin';
        if (GLOBAL_FORCE_STRONG_PASSWORD) text.style.color = 'var(--error)';
        return;
    }

    const isComplex = hasLower && hasUpper && hasDigit;

    if (!isLengthOk || (GLOBAL_FORCE_STRONG_PASSWORD && !isComplex)) {
        bar.style.width = '30%';
        bar.classList.add('pass-weak');

        let missing = [];
        if (!isLengthOk) missing.push('En az 6 karakter');
        if (!hasUpper) missing.push('Büyük Harf');
        if (!hasLower) missing.push('Küçük Harf');
        if (!hasDigit) missing.push('Rakam');

        text.innerText = 'Yetersiz: ' + missing.join(', ') + ' gerekli';
        text.style.color = 'var(--error)';
    } else if (strength < 4) {
        bar.style.width = '60%';
        bar.classList.add('pass-medium');
        text.innerText = 'Orta (Güçlendirmek için sembol ekleyebilirsiniz)';
        text.style.color = 'var(--warning)';
    } else {
        bar.style.width = '100%';
        bar.classList.add('pass-strong');
        text.innerText = 'Güçlü ✅';
        text.style.color = 'var(--success)';
    }
}

window.changePassword = async function () {
    const oldPass = document.getElementById('oldPassword').value;
    const newPass = document.getElementById('newPassword').value;
    const confirmPass = document.getElementById('confirmPassword').value;

    if (!oldPass || !newPass) {
        alert('Lütfen tüm alanları doldurunuz.');
        return;
    }

    if (newPass !== confirmPass) {
        alert('Yeni şifreler eşleşmiyor.');
        return;
    }

    const user = JSON.parse(localStorage.getItem('user'));

    try {
        const res = await fetch(`${API_BASE_URL}/api/users/${user.id}/change-password`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ oldPassword: oldPass, newPassword: newPass })
        });

        const data = await res.json();
        if (!res.ok) throw new Error(data.message || 'Şifre değiştirilemedi.');

        alert('Şifreniz başarıyla değiştirildi. Lütfen yeni şifrenizle tekrar giriş yapın.');

        // Log out user
        localStorage.removeItem('user');
        window.location.href = 'index.html';

    } catch (e) {
        alert('Hata: ' + e.message);
    }
};

// DATABASE SETTINGS
window.loadDbSettings = async function () {
    const card = document.getElementById('dbSettingsCard');
    if (!card) return;

    // First check local user object to see if ID is 1
    const user = JSON.parse(localStorage.getItem('user'));
    if (!user || user.id !== 1) {
        card.style.display = 'none';
        return;
    }

    try {
        const res = await fetch(`${API_BASE_URL}/api/system/db-config`);
        if (res.status === 403) {
            card.style.display = 'none';
            return;
        }
        if (!res.ok) throw new Error('Ayarlar yüklenemedi.');

        card.style.display = 'block'; // Show card if successful
        loadSystemSettings(); // Also load system settings if root

        const data = await res.json();
        const connStr = data.connectionString;

        // Parse connection string
        const parts = connStr.split(';');

        const getVal = (key) => {
            const part = parts.find(p => p.trim().toLowerCase().startsWith(key.toLowerCase() + '='));
            return part ? part.split('=')[1].trim() : '';
        };

        const server = getVal('Server') || getVal('Data Source');
        const db = getVal('Database') || getVal('Initial Catalog');
        const user = getVal('User Id') || getVal('UID');
        const pass = getVal('Password') || getVal('Pwd');
        const integrated = parts.some(p => p.toLowerCase().includes('integrated security=true') || p.toLowerCase().includes('trusted_connection=true'));

        const sEl = document.getElementById('dbServer');
        const dEl = document.getElementById('dbName');
        const iEl = document.getElementById('dbIntegrated');
        const uEl = document.getElementById('dbUser');
        const pEl = document.getElementById('dbPassword');

        if (sEl) sEl.value = server;
        if (dEl) dEl.value = db;
        if (iEl) iEl.checked = integrated;

        toggleDbAuth(integrated);

        if (!integrated) {
            if (uEl) uEl.value = user;
            if (pEl) pEl.value = pass;
        }

    } catch (e) {
        console.error(e);
        alert('Hata: ' + e.message);
    }
};

window.toggleDbAuth = function (isIntegrated) {
    const authFields = document.getElementById('dbAuthFields');
    if (!authFields) return;
    if (isIntegrated) authFields.style.display = 'none';
    else authFields.style.display = 'block';
};

window.saveDbSettings = async function () {
    if (!confirm('Veritabanı bağlantı ayarlarını değiştirmek üzeresiniz. Yanlış ayar uygulamanın çalışmasını durdurur. Devam edilsin mi?')) return;

    const server = document.getElementById('dbServer').value;
    const db = document.getElementById('dbName').value;
    const integrated = document.getElementById('dbIntegrated').checked;
    const user = document.getElementById('dbUser').value;
    const pass = document.getElementById('dbPassword').value;

    const dto = {
        server: server,
        database: db,
        integratedSecurity: integrated,
        user: user,
        password: pass
    };

    try {
        const res = await fetch(`${API_BASE_URL}/api/system/db-config`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });

        const data = await res.json();
        if (!res.ok) throw new Error(data.message || 'Kaydedilemedi.');

        alert('Ayarlar kaydedildi. Lütfen API sunucusunu manuel olarak yeniden başlatın.');
    } catch (e) {
        alert('Hata: ' + e.message);
    }
};

window.testDbConnection = async function () {
    alert('Şu anki aktif bağlantı durumu kontrol ediliyor...');
    try {
        const res = await fetch(`${API_BASE_URL}/api/system/info`);
        const data = await res.json();
        alert(`Bağlantı Durumu: ${data.databaseStatus}\n${data.databaseError || ''}`);
    } catch (e) {
        alert('Bağlantı kontrolü başarısız.');
    }
};

// SYSTEM SETTINGS
window.loadSystemSettings = async function () {
    const card = document.getElementById('systemSettingsCard');
    if (!card) return;

    try {
        const res = await fetch(`${API_BASE_URL}/api/system/settings`);
        if (!res.ok) return;
        const data = await res.json();
        GLOBAL_FORCE_STRONG_PASSWORD = data.forceStrongPassword;
        document.getElementById('forceStrongPassword').checked = data.forceStrongPassword;
        if (document.getElementById('sysBarcodeType')) {
            document.getElementById('sysBarcodeType').value = data.barcodeType || 'QR';
        }
        card.style.display = 'block';

        loadMailSettings(); // NEW: Load mail settings too
    } catch (e) { console.error(e); }
};

// MAIL SETTINGS
window.loadMailSettings = async function () {
    const card = document.getElementById('mailSettingsCard');
    if (!card) return;

    try {
        const res = await fetch(`${API_BASE_URL}/api/system/mail-settings`);
        if (!res.ok) return;
        const data = await res.json();

        document.getElementById('mailHost').value = data.smtpHost || '';
        document.getElementById('mailPort').value = data.smtpPort || 587;
        document.getElementById('mailUser').value = data.username || '';
        document.getElementById('mailPass').value = data.password || '';
        document.getElementById('mailFrom').value = data.fromEmail || '';
        document.getElementById('mailFromName').value = data.fromName || '';
        document.getElementById('mailSsl').checked = data.enableSsl;

        card.style.display = 'block';
    } catch (e) { console.error(e); }
};

window.saveMailSettings = async function () {
    const dto = {
        smtpHost: document.getElementById('mailHost').value,
        smtpPort: parseInt(document.getElementById('mailPort').value) || 587,
        username: document.getElementById('mailUser').value,
        password: document.getElementById('mailPass').value,
        fromEmail: document.getElementById('mailFrom').value,
        fromName: document.getElementById('mailFromName').value,
        enableSsl: document.getElementById('mailSsl').checked
    };

    try {
        const res = await fetch(`${API_BASE_URL}/api/system/mail-settings`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });
        if (!res.ok) {
            const err = await res.text();
            throw new Error(err || 'Kaydedilemedi');
        }
        alert('Mail ayarları güncellendi.');
    } catch (e) { alert('Hata: ' + e.message); }
};

window.testMailSettings = async function () {
    const dto = {
        smtpHost: document.getElementById('mailHost').value,
        smtpPort: parseInt(document.getElementById('mailPort').value) || 587,
        username: document.getElementById('mailUser').value,
        password: document.getElementById('mailPass').value,
        fromEmail: document.getElementById('mailFrom').value,
        fromName: document.getElementById('mailFromName').value,
        enableSsl: document.getElementById('mailSsl').checked
    };

    if (!dto.smtpHost || !dto.fromEmail) {
        alert('Lütfen en azından SMTP Sunucu ve Gönderen E-Posta bilgilerini giriniz.');
        return;
    }

    try {
        const res = await fetch(`${API_BASE_URL}/api/system/test-mail`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });
        const data = await res.json();
        if (!res.ok) throw new Error(data.message || 'Sınama maili gönderilemedi.');
        alert(data.message);
    } catch (e) { alert('Hata: ' + e.message); }
};

window.saveSystemSettings = async function () {
    const force = document.getElementById('forceStrongPassword').checked;
    const barcodeType = document.getElementById('sysBarcodeType') ? document.getElementById('sysBarcodeType').value : 'QR';
    try {
        const res = await fetch(`${API_BASE_URL}/api/system/settings`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                forceStrongPassword: force,
                barcodeType: barcodeType
            })
        });
        if (!res.ok) {
            const err = await res.text();
            throw new Error(err || 'Kaydedilemedi');
        }
        alert('Sistem ayarları güncellendi.');
        GLOBAL_FORCE_STRONG_PASSWORD = force;
    } catch (e) { alert('Hata: ' + e.message); }
};

window.deleteProduct = async function (id) {
    if (!(await showConfirm('Ürün Silme', 'Bu ürünü silmek istediğinize emin misiniz?'))) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/product/${id}`, { method: 'DELETE' });
        if (res.ok) { showToast('Ürün silindi'); loadProducts(); }
        else showToast('Silinemedi', 'error');
    } catch (e) { showToast(e.message, 'error'); }
}

// WAREHOUSES
window.loadWarehouses = async function () {
    const tableIds = ['warehouseListBody', 'warehouseListBody_Main'];
    const tables = tableIds.map(id => document.getElementById(id)).filter(el => el);

    if (tables.length === 0) return;

    if (!window.warehousePage) window.warehousePage = 1;
    if (!window.warehousePageSize) window.warehousePageSize = 10;
    const searchTerm = ''; // For now, simple search if needed

    try {
        const filter = window.currentStatusFilter['inventory'] || 'active';
        const res = await fetch(`${API_BASE_URL}/api/warehouse?status=${filter}&page=${window.warehousePage}&pageSize=${window.warehousePageSize}`);
        if (res.status === 403) {
            tables.forEach(tbody => {
                tbody.innerHTML = '<tr><td colspan="4" style="color:var(--error); text-align:center;">🚫 Bu verileri görmeye yetkiniz bulunmamaktadır.</td></tr>';
            });
            return;
        }
        if (!res.ok) throw new Error('Yükleme hatası');

        const pagedData = await res.json();
        const data = pagedData.items || [];
        const totalCount = pagedData.totalCount || 0;
        const totalPages = Math.ceil(totalCount / window.warehousePageSize);

        const html = (!data || data.length === 0)
            ? '<tr><td colspan="4">Kayıt yok.</td></tr>'
            : data.map(w => {
                const canWrite = window.hasPermission('Warehouse', 'write');
                const canDelete = window.hasPermission('Warehouse', 'delete');
                const rowStyle = w.isDeleted ? 'style="opacity:0.6"' : '';
                const passiveBadge = w.isDeleted ? '<span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;margin-left:5px;">PASİF</span>' : '';
                return `
                <tr ${rowStyle}>
                    <td><strong>${w.warehouseName}</strong> ${passiveBadge}</td>
                    <td>${w.location}</td>
                    <td>${w.companyName || w.companyId}</td>
                    <td style="text-align:right;">
                        <div class="action-btn-container">
                            ${canWrite && !w.isDeleted ? `
                            <button class="btn-action btn-edit"
                                onclick="openWarehouseModal(${w.id}, '${w.warehouseName}', '${w.location}', ${w.companyId})">Düzenle</button>
                            ` : ''}
                            ${canDelete && !w.isDeleted ? `
                            <button class="btn-action btn-delete"
                                onclick="deleteWarehouse(${w.id})">Sil</button>
                            ` : ''}
                            ${w.isDeleted ? (window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('warehouse', ${w.id})">✅ Geri Yükle</button>` : '<span style="font-size:0.75rem; color:var(--muted); font-style:italic;">Pasif</span>') : ''}
                        </div>
                    </td>
                </tr>`;
            }).join('');

        tables.forEach(tbody => tbody.innerHTML = html);

        // Render pagination for the main warehouse list if it exists
        const mainPagination = document.getElementById('warehousePaginationContainer');
        if (mainPagination) {
            renderPagination(mainPagination, totalPages, window.warehousePage, window.warehousePageSize, (newPage, newSize) => {
                window.warehousePage = newPage;
                window.warehousePageSize = newSize;
                loadWarehouses();
            });
        }

    } catch (e) {
        console.error(e);
        tables.forEach(tbody => {
            tbody.innerHTML = `<tr><td colspan="4" style="color:var(--error); text-align:center;">Hata: ${e.message}</td></tr>`;
        });
    }
}

window.openWarehouseModal = async function (id, name, loc, compId) {
    document.getElementById('warehouseModal').style.display = 'flex';
    document.querySelector('#warehouseModal h3').innerText = id ? 'Depo Düzenle' : 'Yeni Depo';

    document.getElementById('wId').value = id || '';
    document.getElementById('wName').value = name || '';
    document.getElementById('wLoc').value = loc || '';

    // Load Companies
    const res = await fetch(`${API_BASE_URL}/api/companies?pageSize=500`);
    const pagedData = await res.json();
    const companies = pagedData.items || [];
    const sel = document.getElementById('wCompany');
    sel.innerHTML = '<option value="">Şirket Seç...</option>';
    companies.forEach(c => {
        const selected = (compId && c.id == compId) ? 'selected' : '';
        sel.innerHTML += `<option value="${c.id}" ${selected}>${c.companyName}</option>`;
    });
}

window.createWarehouse = async function () {
    const id = document.getElementById('wId').value;
    const dto = {
        warehouseName: document.getElementById('wName').value,
        location: document.getElementById('wLoc').value,
        companyId: parseInt(document.getElementById('wCompany').value)
    };
    if (id) dto.id = parseInt(id);

    if (!dto.warehouseName || !dto.companyId) return alert('Eksik bilgi.');

    try {
        const method = id ? 'PUT' : 'POST';
        const res = await fetch(`${API_BASE_URL}/api/warehouse`, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });

        if (res.ok) {
            showToast(id ? 'Depo güncellendi' : 'Depo eklendi');
            closeModal('warehouseModal');
            loadWarehouses();
        }
        else alert('Hata.');
    } catch (e) { alert(e.message); }
}

window.deleteWarehouse = async function (id) {
    if (!(await showConfirm('Depo Silme', 'Bu depoyu silmek istediğinize emin misiniz?'))) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/warehouse/${id}`, { method: 'DELETE' });
        if (res.ok) { showToast('Depo silindi'); loadWarehouses(); }
        else alert('Silinemedi.');
    } catch (e) { alert(e.message); }
}

// SIMPLE ENTITIES (Brand, Category, Unit)
window.loadBrands = async function () { loadSimple('brands', 'brandListBody', 'brandName'); }
window.loadCategories = async function () { loadSimple('categories', 'categoryListBody', 'categoryName'); }
window.loadUnits = async function () { loadSimple('units', 'unitListBody', 'unitName'); }

async function loadSimple(endpoint, tbodyId, nameField) {
    const tbody = document.getElementById(tbodyId);
    if (!tbody) return;
    try {
        const filter = window.currentStatusFilter['inventory'] || 'active';
        const res = await fetch(`${API_BASE_URL}/api/product/${endpoint}?status=${filter}`);
        if (res.status === 403) {
            let colSpan = endpoint === 'units' ? 3 : 2;
            tbody.innerHTML = `<tr><td colspan="${colSpan}" style="color:var(--error); text-align:center;">🚫 Yetkisiz Erişim</td></tr>`;
            return;
        }

        const data = await res.json();
        tbody.innerHTML = '';
        if (data.length === 0) {
            let colSpan = endpoint === 'units' ? 3 : 2;
            tbody.innerHTML = `<tr><td colspan="${colSpan}" style="text-align:center;">Kayıt yok.</td></tr>`;
            return;
        }

        // Determine type for modal title and permission module
        let type = 'Brand';
        let permModule = 'Product';

        if (endpoint === 'categories') {
            type = 'Category';
            permModule = 'Category';
        }
        if (endpoint === 'units') type = 'Unit';

        const canWrite = window.hasPermission(permModule, 'write');
        const canDelete = window.hasPermission(permModule, 'delete');

        data.forEach(item => {
            let extra = '';
            let extraVal = '';
            if (endpoint === 'units') {
                extra = `<td>${item.unitShortName || ''}</td>`;
                extraVal = item.unitShortName || '';
            }

            const tr = document.createElement('tr');
            if (item.isDeleted) tr.style.opacity = '0.6';

            tr.innerHTML = `
                <td><strong>${item[nameField]}</strong> ${item.isDeleted ? '<span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;margin-left:5px;">PASİF</span>' : ''}</td>
                ${extra}
                <td style="text-align:right;">
                    <div class="action-btn-container">
                        ${canWrite && !item.isDeleted ? `
                        <button class="btn-action btn-edit" 
                            onclick="openSimpleModal('${type}', ${item.id}, '${item[nameField]}', '${extraVal}')">Düzenle</button>
                        ` : ''}
                        ${canDelete && !item.isDeleted ? `
                        <button class="btn-action btn-delete" 
                            onclick="deleteSimple('${endpoint}', ${item.id})">Sil</button>
                        ` : ''}
                        ${item.isDeleted ? (window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('${endpoint.substring(0, endpoint.length - 1)}', ${item.id})">✅ Geri Yükle</button>` : '<span style="font-size:0.75rem; color:var(--muted); font-style:italic;">Silindi</span>') : ''}
                    </div>
                </td>`;
            tbody.appendChild(tr);
        });
    } catch (e) { console.error(e); }
}

window.openSimpleModal = function (type, id, name, extra) {
    document.getElementById('simpleModal').style.display = 'flex';
    document.getElementById('simpleModalTitle').innerText = id ? (type + ' Düzenle') : ('Yeni ' + type);
    document.getElementById('simpleModalType').value = type;
    document.getElementById('simpleId').value = id || '';
    document.getElementById('simpleName').value = name || '';

    const extraGroup = document.getElementById('simpleExtraGroup');
    const descGroup = document.getElementById('simpleDescGroup');
    const decimalGroup = document.getElementById('simpleDecimalGroup');

    // Reset styles
    extraGroup.style.display = 'none';
    if (descGroup) {
        descGroup.style.display = 'none';
        document.getElementById('simpleDesc').value = '';
    }
    if (decimalGroup) {
        decimalGroup.style.display = 'none';
        document.getElementById('simpleDecimal').checked = false;
    }

    if (type === 'Unit') {
        extraGroup.style.display = 'block';
        document.getElementById('simpleExtra').placeholder = 'Kısa Ad (örn: kg)';
        document.getElementById('simpleExtra').value = extra || '';
        if (decimalGroup) decimalGroup.style.display = 'block';
    }
    else if (type === 'Category' || type === 'Brand') {
        if (descGroup) descGroup.style.display = 'block';
    }
}

window.createSimpleEntity = async function () {
    const type = document.getElementById('simpleModalType').value;
    const id = document.getElementById('simpleId').value;
    const name = document.getElementById('simpleName').value;
    if (!name) return alert('Ad zorunlu.');

    const descEl = document.getElementById('simpleDesc');
    const desc = descEl ? descEl.value : '';

    let endpoint = '', body = {};
    if (type === 'Brand') {
        endpoint = 'brands';
        body = { brandName: name, brandDescription: desc || '-' };
    }
    if (type === 'Category') {
        endpoint = 'categories';
        body = { categoryName: name, categoryDescription: desc || '-' };
    }
    if (type === 'Unit') {
        endpoint = 'units';
        const short = document.getElementById('simpleExtra').value;
        const isDec = document.getElementById('simpleDecimal') ? document.getElementById('simpleDecimal').checked : false;
        body = { unitName: name, unitShortName: short, isDecimal: isDec };
    }

    if (id) body.id = parseInt(id);

    try {
        const method = id ? 'PUT' : 'POST';
        const res = await fetch(`${API_BASE_URL}/api/product/${endpoint}`, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });

        if (res.ok) {
            showToast(id ? 'Güncellendi' : 'Kaydedildi');
            closeModal('simpleModal');
            if (type === 'Brand') loadBrands();
            if (type === 'Category') loadCategories();
            if (type === 'Unit') loadUnits();
        }
        else {
            let errMsg = 'Bilinmeyen hata';
            try {
                const text = await res.text();
                try {
                    const json = JSON.parse(text);
                    errMsg = json.message || json.title || JSON.stringify(json);
                } catch { errMsg = text; }
            } catch (e) { errMsg = e.message; }
            alert('Hata: ' + errMsg);
        }
    } catch (e) { alert(e.message); }
}

window.deleteSimple = async function (endpoint, id) {
    if (!(await showConfirm('Kaydı Sil', 'Bu kaydı silmek istediğinize emin misiniz?'))) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/product/${endpoint}/${id}`, { method: 'DELETE' });
        if (res.ok) {
            alert('Silindi.');
            if (endpoint === 'brands') loadBrands();
            if (endpoint === 'categories') loadCategories();
            if (endpoint === 'units') loadUnits();
        } else {
            alert('Silinemedi.');
        }
    } catch (e) { alert(e.message); }
}









// SUPPLIERS
window.loadSuppliers = async function () {
    const tbody = document.getElementById('supplierListBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Yükleniyor...</td></tr>';
    try {
        const filter = window.currentStatusFilter['suppliers'] || 'active';

        window.supplierPage = window.supplierPage || 1;
        window.supplierPageSize = window.supplierPageSize || 10;

        const res = await fetch(`${API_BASE_URL}/api/supplier?status=${filter}&page=${window.supplierPage}&pageSize=${window.supplierPageSize}`);
        if (res.status === 403) {
            tbody.innerHTML = '<tr><td colspan="5" style="color:var(--error); text-align:center;">🚫 Yetkisiz Erişim</td></tr>';
            return;
        }

        const pagedData = await res.json();
        const data = pagedData.items || [];
        const totalCount = pagedData.totalCount || 0;
        const totalPages = Math.ceil(totalCount / window.supplierPageSize);

        tbody.innerHTML = '';
        if (data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">Kayıt yok.</td></tr>';
            renderPagination(document.getElementById('supplierPaginationContainer'), totalPages, window.supplierPage, window.supplierPageSize, () => loadSuppliers());
            return;
        }

        const canWrite = window.hasPermission('Supplier', 'write');
        const canDelete = window.hasPermission('Supplier', 'delete');

        data.forEach(s => {
            const tr = document.createElement('tr');
            if (s.isDeleted) tr.style.opacity = '0.6';
            tr.innerHTML = `
                <td><strong>${s.supplierCompanyName}</strong> ${s.isDeleted ? '<span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;margin-left:5px;">PASİF</span>' : ''}</td>
                <td>${s.supplierContactName || '-'}</td>
                <td>${s.supplierContactMail || '-'}</td>
                <td>${s.supplierAddress || '-'}</td>
                <td style="text-align:right;">
                    <div class="action-btn-container">
                        ${canWrite && !s.isDeleted ? `
                        <button class="btn-action btn-edit" 
                            onclick="openSupplierModal(${s.id}, \`${s.supplierCompanyName}\`, \`${s.supplierContactName}\`, \`${s.supplierContactMail}\`, \`${s.supplierAddress}\`)">Düzenle</button>
                        ` : ''}
                        ${canDelete && !s.isDeleted ? `
                        <button class="btn-action btn-delete" 
                            onclick="deleteSupplier(${s.id}, '${s.supplierCompanyName}')">Sil</button>
                        ` : ''}
                        ${s.isDeleted ? (window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('supplier', ${s.id})">✅ Geri Yükle</button>` : '<span style="font-size:0.75rem; color:var(--muted); font-style:italic;">Silindi</span>') : ''}
                    </div>
                </td>`;
            tbody.appendChild(tr);
        });

        if (!document.getElementById('supplierPaginationContainer')) {
            const table = tbody.closest('table');
            const pagContainer = document.createElement('div');
            pagContainer.id = 'supplierPaginationContainer';
            pagContainer.className = 'pagination-container';
            table.parentNode.insertBefore(pagContainer, table.nextSibling);
        }
        renderPagination(document.getElementById('supplierPaginationContainer'), totalPages, window.supplierPage, window.supplierPageSize, (newPage, newSize) => {
            window.supplierPage = newPage;
            window.supplierPageSize = newSize;
            loadSuppliers();
        });

    } catch (e) {
        console.error(e);
        tbody.innerHTML = '<tr><td colspan="5" style="color:var(--error); text-align:center;">Hata oluştu!</td></tr>';
    }
}

window.openSupplierModal = function (id, name, contact, mail, addr) {
    document.getElementById('supplierModal').style.display = 'flex';
    document.querySelector('#supplierModal h3').innerText = id ? 'Tedarikçi Düzenle' : 'Yeni Tedarikçi';
    document.getElementById('sId').value = id || '';
    document.getElementById('sName').value = name || '';
    document.getElementById('sContact').value = contact || '';
    document.getElementById('sMail').value = mail || '';
    document.getElementById('sAddr').value = addr || '';
}

window.saveSupplier = async function () {
    const id = document.getElementById('sId').value;
    const dto = {
        supplierCompanyName: document.getElementById('sName').value,
        supplierContactName: document.getElementById('sContact').value,
        supplierContactMail: document.getElementById('sMail').value,
        supplierAddress: document.getElementById('sAddr').value
    };
    if (id) dto.id = parseInt(id);

    if (!dto.supplierCompanyName) return alert('Şirket adı zorunludur.');

    try {
        const method = id ? 'PUT' : 'POST';
        const res = await fetch(`${API_BASE_URL}/api/supplier`, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });

        if (res.ok) {
            alert(id ? 'Tedarikçi güncellendi.' : 'Tedarikçi eklendi.');
            closeModal('supplierModal');
            loadSuppliers();
        } else {
            alert('Hata oluştu.');
        }
    } catch (e) { alert(e.message); }
}

window.deleteSupplier = async function (id, name) {
    if (!(await showConfirm('Tedarikçi Silme', `"${name}" tedarikçisini silmek istediğinize emin misiniz?`))) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/supplier/${id}`, { method: 'DELETE' });
        if (res.ok) { alert('Tedarikçi silindi.'); loadSuppliers(); }
        else alert('Silinemedi.');
    } catch (e) { alert(e.message); }
}

// PRICELISTS
window.loadPriceLists = async function () {
    const tbody = document.getElementById('priceListBody');
    if (!tbody) return;

    if (!window.priceListPage) window.priceListPage = 1;
    if (!window.priceListPageSize) window.priceListPageSize = 10;
    const searchTerm = document.getElementById('search-pricelists')?.value || '';

    tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">Yükleniyor...</td></tr>';
    try {
        const filter = window.currentStatusFilter['pricelists'] || 'active';
        const res = await fetch(`${API_BASE_URL}/api/pricelist?status=${filter}&searchTerm=${encodeURIComponent(searchTerm)}&page=${window.priceListPage}&pageSize=${window.priceListPageSize}`);
        if (res.status === 403) {
            tbody.innerHTML = '<tr><td colspan="7" style="color:var(--error); text-align:center;">🚫 Yetkisiz Erişim</td></tr>';
            return;
        }

        const pagedData = await res.json();
        const data = pagedData.items || [];
        const totalCount = pagedData.totalCount || 0;
        const totalPages = Math.ceil(totalCount / window.priceListPageSize);

        tbody.innerHTML = '';
        if (data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">Henüz fiyat kaydı yok.</td></tr>';
            renderPagination(document.getElementById('priceListPaginationContainer'), totalPages, window.priceListPage, window.priceListPageSize, () => loadPriceLists());
            return;
        }

        const canWrite = window.hasPermission('PriceList', 'write');
        const canDelete = window.hasPermission('PriceList', 'delete');

        data.forEach(item => {
            const tr = document.createElement('tr');
            if (item.isDeleted) tr.style.opacity = '0.6';
            tr.innerHTML = `
                <td><strong>${item.productCode} - ${item.productName}</strong> ${item.isDeleted ? '<span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;margin-left:5px;">PASİF</span>' : ''}</td>
                <td>${item.supplierName || '-'}</td>
                <td>${item.purchasePrice}</td>
                <td>${item.salePrice}</td>
                <td>${item.currency}</td>
                <td><span class="badge" style="background:${item.isActivePrice ? '#d1fae5;color:#065f46;' : '#f3f4f6; color:#6b7280;'}">${item.isActivePrice ? 'Aktif' : 'Pasif'}</span></td>
                <td style="text-align:right;">
                    <div class="action-btn-container">
                        ${canWrite && !item.isDeleted ? `
                        <button class="btn-action btn-edit" 
                            onclick="openPriceListModal(${item.id})">Düzenle</button>
                        ` : ''}
                        ${canDelete && !item.isDeleted ? `
                        <button class="btn-action btn-delete" 
                            onclick="deletePriceList(${item.id})">Sil</button>
                        ` : ''}
                        ${item.isDeleted ? '<span style="font-size:0.75rem; color:var(--muted); font-style:italic;">Silindi</span>' : ''}
                    </div>
                </td>`;
            tbody.appendChild(tr);
        });

        if (!document.getElementById('priceListPaginationContainer')) {
            const container = document.createElement('div');
            container.id = 'priceListPaginationContainer';
            tbody.closest('.glass-card').appendChild(container);
        }

        renderPagination(document.getElementById('priceListPaginationContainer'), totalPages, window.priceListPage, window.priceListPageSize, (newPage, newSize) => {
            window.priceListPage = newPage;
            window.priceListPageSize = newSize;
            loadPriceLists();
        });

    } catch (e) { console.error(e); tbody.innerHTML = '<tr><td colspan="7" style="color:var(--error); text-align:center;">Hata oluştu!</td></tr>'; }
}

window.openPriceListModal = async function (id = null) {
    document.getElementById('priceListModal').style.display = 'flex';
    document.querySelector('#priceListModal h3').innerText = id ? 'Fiyat Düzenle' : 'Yeni Fiyat Kaydı';
    document.getElementById('plId').value = id || '';

    const pSel = document.getElementById('plProduct');
    const sSel = document.getElementById('plSupplier');

    pSel.innerHTML = '<option value="">Yükleniyor...</option>';
    sSel.innerHTML = '<option value="">Yükleniyor...</option>';

    try {
        const [productsRes, suppliersRes] = await Promise.all([
            fetch(`${API_BASE_URL}/api/product?pageSize=1000`).then(r => r.json()),
            fetch(`${API_BASE_URL}/api/supplier?pageSize=1000`).then(r => r.json())
        ]);

        const products = Array.isArray(productsRes) ? productsRes : (productsRes.items || []);
        const suppliers = Array.isArray(suppliersRes) ? suppliersRes : (suppliersRes.items || []);

        pSel.innerHTML = '<option value="">Ürün Seçin...</option>';
        products.forEach(p => pSel.innerHTML += `<option value="${p.id}">${p.productCode} - ${p.productName}</option>`);

        sSel.innerHTML = '<option value="">Tedarikçi Seçin (Opsiyonel)...</option>';
        suppliers.forEach(s => sSel.innerHTML += `<option value="${s.id}">${s.supplierCompanyName}</option>`);

        if (id) {
            const res = await fetch(`${API_BASE_URL}/api/pricelist/${id}`);
            const data = await res.json();
            document.getElementById('plProduct').value = data.productId;
            document.getElementById('plSupplier').value = data.supplierId || '';
            document.getElementById('plPurchasePrice').value = data.purchasePrice;
            document.getElementById('plSalePrice').value = data.salePrice;
            document.getElementById('plVat').value = data.vatRate;
            document.getElementById('plCurrency').value = data.currency;
            document.getElementById('plIsActive').checked = data.isActivePrice;
        } else {
            document.getElementById('plProduct').value = '';
            document.getElementById('plSupplier').value = '';
            document.getElementById('plPurchasePrice').value = '';
            document.getElementById('plSalePrice').value = '';
            document.getElementById('plVat').value = '20';
            document.getElementById('plCurrency').value = 'TRY';
            document.getElementById('plIsActive').checked = true;
        }
    } catch (e) { console.error(e); alert('Veriler yüklenemedi!'); }
}

window.savePriceList = async function () {
    const id = document.getElementById('plId').value;
    const dto = {
        productId: parseInt(document.getElementById('plProduct').value),
        supplierId: document.getElementById('plSupplier').value ? parseInt(document.getElementById('plSupplier').value) : null,
        purchasePrice: parseFloat(document.getElementById('plPurchasePrice').value || 0),
        salePrice: parseFloat(document.getElementById('plSalePrice').value || 0),
        vatRate: parseInt(document.getElementById('plVat').value || 0),
        currency: document.getElementById('plCurrency').value,
        isActivePrice: document.getElementById('plIsActive').checked,
        discountRate: 0
    };

    if (!dto.productId) return alert('Lütfen ürün seçiniz.');
    if (dto.purchasePrice < 0 || dto.salePrice < 0) return alert('Fiyatlar negatif olamaz.');
    if (dto.vatRate < 0) return alert('KDV negatif olamaz.');

    try {
        const method = id ? 'PUT' : 'POST';
        if (id) dto.id = parseInt(id);

        const res = await fetch(`${API_BASE_URL}/api/pricelist`, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });

        if (res.ok) {
            alert('Fiyat kaydı kaydedildi.');
            closeModal('priceListModal');
            loadPriceLists();
        } else {
            alert('Hata oluştu.');
        }
    } catch (e) { alert(e.message); }
}

window.deletePriceList = async function (id) {
    if (!(await showConfirm('Fiyat Silme', 'Bu fiyat kaydını silmek istediğinize emin misiniz?'))) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/pricelist/${id}`, { method: 'DELETE' });
        if (res.ok) { alert('Fiyat kaydı silindi.'); loadPriceLists(); }
        else alert('Silinemedi.');
    } catch (e) { alert(e.message); }
}

// CUSTOMER COMPANIES
window.loadCustomerCompanies = async function () {
    const tbody = document.getElementById('customerCompanyListBody');
    if (!tbody) return;

    if (!window.customerCompanyPage) window.customerCompanyPage = 1;
    if (!window.customerCompanyPageSize) window.customerCompanyPageSize = 10;
    const searchTerm = document.getElementById('search-customer-companies')?.value || '';

    tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;">Yükleniyor...</td></tr>';
    try {
        const filter = window.currentStatusFilter['customer-companies'] || 'active';
        const res = await fetch(`${API_BASE_URL}/api/customer/companies?status=${filter}&searchTerm=${encodeURIComponent(searchTerm)}&page=${window.customerCompanyPage}&pageSize=${window.customerCompanyPageSize}`);
        if (res.status === 403) {
            tbody.innerHTML = '<tr><td colspan="4" style="color:var(--error); text-align:center;">🚫 Yetkisiz Erişim</td></tr>';
            return;
        }

        const pagedData = await res.json();
        const data = pagedData.items || [];
        const totalCount = pagedData.totalCount || 0;
        const totalPages = Math.ceil(totalCount / window.customerCompanyPageSize);

        tbody.innerHTML = '';
        if (data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;">Kayıt yok.</td></tr>';
            renderPagination(document.getElementById('customerCompanyPaginationContainer'), totalPages, window.customerCompanyPage, window.customerCompanyPageSize, () => loadCustomerCompanies());
            return;
        }

        const canWrite = window.hasPermission('Customer', 'write');
        const canDelete = window.hasPermission('Customer', 'delete');

        data.forEach(item => {
            const tr = document.createElement('tr');
            if (item.isDeleted) tr.style.opacity = '0.6';
            tr.innerHTML = `
                <td><strong>${item.customerCompanyName}</strong> ${item.isDeleted ? '<span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;margin-left:5px;">PASİF</span>' : ''}</td>
                <td>${item.customerCompanyMail || '-'}</td>
                <td>${item.customerCompanyAddress || '-'}</td>
                <td style="text-align:right;">
                    <div class="action-btn-container">
                        ${canWrite && !item.isDeleted ? `
                        <button class="btn-action btn-edit" 
                            onclick="openCustomerCompanyModal(${item.id}, \`${item.customerCompanyName}\`, \`${item.customerCompanyMail}\`, \`${item.customerCompanyAddress}\`)">Düzenle</button>
                        ` : ''}
                        ${canDelete && !item.isDeleted ? `
                        <button class="btn-action btn-delete" 
                            onclick="deleteCustomerCompany(${item.id}, '${item.customerCompanyName}')">Sil</button>
                        ` : ''}
                        ${item.isDeleted ? (window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('customercompany', ${item.id})">✅ Geri Yükle</button>` : '<span style="font-size:0.75rem; color:var(--muted); font-style:italic;">Silindi</span>') : ''}
                    </div>
                </td>`;
            tbody.appendChild(tr);
        });

        if (!document.getElementById('customerCompanyPaginationContainer')) {
            const container = document.createElement('div');
            container.id = 'customerCompanyPaginationContainer';
            tbody.closest('.glass-card').appendChild(container);
        }

        renderPagination(document.getElementById('customerCompanyPaginationContainer'), totalPages, window.customerCompanyPage, window.customerCompanyPageSize, (newPage, newSize) => {
            window.customerCompanyPage = newPage;
            window.customerCompanyPageSize = newSize;
            loadCustomerCompanies();
        });

    } catch (e) { console.error(e); tbody.innerHTML = '<tr><td colspan="4" style="color:var(--error); text-align:center;">Hata oluştu!</td></tr>'; }
}

window.openCustomerCompanyModal = function (id, name, mail, addr) {
    document.getElementById('customerCompanyModal').style.display = 'flex';
    document.querySelector('#customerCompanyModal h3').innerText = id ? 'Müşteri Şirket Düzenle' : 'Yeni Müşteri Şirketi';
    document.getElementById('ccId').value = id || '';
    document.getElementById('ccName').value = name || '';
    document.getElementById('ccMail').value = mail || '';
    document.getElementById('ccAddr').value = addr || '';
}

window.saveCustomerCompany = async function () {
    const id = document.getElementById('ccId').value;
    const dto = {
        customerCompanyName: document.getElementById('ccName').value,
        customerCompanyMail: document.getElementById('ccMail').value,
        customerCompanyAddress: document.getElementById('ccAddr').value
    };
    if (id) dto.id = parseInt(id);

    if (!dto.customerCompanyName) return alert('Şirket adı zorunludur.');

    try {
        const method = id ? 'PUT' : 'POST';
        const res = await fetch(`${API_BASE_URL}/api/customer/companies`, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });

        if (res.ok) {
            alert('Başarıyla kaydedildi.');
            closeModal('customerCompanyModal');
            loadCustomerCompanies();
        } else alert('Hata oluştu.');
    } catch (e) { alert(e.message); }
}

window.deleteCustomerCompany = async function (id, name) {
    if (!(await showConfirm('Şirket Silme', `"${name}" müşteri şirketini silmek istiyor musunuz?`))) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/customer/companies/${id}`, { method: 'DELETE' });
        if (res.ok) { alert('Silindi.'); loadCustomerCompanies(); }
        else alert('Silinemedi.');
    } catch (e) { alert(e.message); }
}

// CUSTOMERS (CONTACTS)
window.loadCustomers = async function () {
    const tbody = document.getElementById('customerListBody');
    if (!tbody) return;

    if (!window.customerPage) window.customerPage = 1;
    if (!window.customerPageSize) window.customerPageSize = 10;
    const searchTerm = document.getElementById('search-customers')?.value || '';

    tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Yükleniyor...</td></tr>';
    try {
        const filter = window.currentStatusFilter['customers'] || 'active';
        const res = await fetch(`${API_BASE_URL}/api/customer?status=${filter}&searchTerm=${encodeURIComponent(searchTerm)}&page=${window.customerPage}&pageSize=${window.customerPageSize}`);
        if (res.status === 403) {
            tbody.innerHTML = '<tr><td colspan="5" style="color:var(--error); text-align:center;">🚫 Yetkisiz Erişim</td></tr>';
            return;
        }

        const pagedData = await res.json();
        const data = pagedData.items || [];
        const totalCount = pagedData.totalCount || 0;
        const totalPages = Math.ceil(totalCount / window.customerPageSize);

        tbody.innerHTML = '';
        if (data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">Kayıt yok.</td></tr>';
            renderPagination(document.getElementById('customerPaginationContainer'), totalPages, window.customerPage, window.customerPageSize, () => loadCustomers());
            return;
        }

        const canWrite = window.hasPermission('Customer', 'write');
        const canDelete = window.hasPermission('Customer', 'delete');

        data.forEach(item => {
            const tr = document.createElement('tr');
            if (item.isDeleted) tr.style.opacity = '0.6';
            tr.innerHTML = `
                <td><strong>${item.customerContactPersonName} ${item.customerContactPersonLastName}</strong> ${item.isDeleted ? '<br><span class="badge" style="background:#fecaca;color:#991b1b;font-size:0.65rem;">PASİF</span>' : ''}</td>
                <td>${item.customerCompanyName || '-'}</td>
                <td>${item.customerContactPersonMobilPhone || '-'}</td>
                <td>${item.customerContactPersonMail || '-'}</td>
                <td style="text-align:right;">
                    <div class="action-btn-container">
                        ${canWrite && !item.isDeleted ? `
                        <button class="btn-action btn-edit" 
                            onclick="openCustomerModal(${item.id})">Düzenle</button>
                        ` : ''}
                        ${canDelete && !item.isDeleted ? `
                        <button class="btn-action btn-delete" 
                            onclick="deleteCustomer(${item.id}, '${item.customerContactPersonName}')">Sil</button>
                        ` : ''}
                        ${item.isDeleted ? (window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('customer', ${item.id})">✅ Geri Yükle</button>` : '<span style="font-size:0.75rem; color:var(--muted); font-style:italic;">Silindi</span>') : ''}
                    </div>
                </td>`;
            tbody.appendChild(tr);
        });

        if (!document.getElementById('customerPaginationContainer')) {
            const container = document.createElement('div');
            container.id = 'customerPaginationContainer';
            tbody.closest('.glass-card').appendChild(container);
        }

        renderPagination(document.getElementById('customerPaginationContainer'), totalPages, window.customerPage, window.customerPageSize, (newPage, newSize) => {
            window.customerPage = newPage;
            window.customerPageSize = newSize;
            loadCustomers();
        });

    } catch (e) { console.error(e); tbody.innerHTML = '<tr><td colspan="6" style="color:var(--error); text-align:center;">Hata oluştu!</td></tr>'; }
}

window.openCustomerModal = async function (id = null) {
    document.getElementById('customerModal').style.display = 'flex';
    document.querySelector('#customerModal h3').innerText = id ? 'Müşteri Düzenle' : 'Yeni Müşteri';
    document.getElementById('custId').value = id || '';

    const compSel = document.getElementById('custCompany');
    compSel.innerHTML = '<option value="">Yükleniyor...</option>';

    try {
        console.log("Fetching customer companies...");
        const res = await fetch(`${API_BASE_URL}/api/customer/companies?pageSize=500`);
        if (!res.ok) throw new Error("Şirket listesi alınamadı: " + res.status);
        const pagedData = await res.json();
        const companies = pagedData.items || [];
        console.log("Companies received:", companies);

        compSel.innerHTML = '<option value="">Şirket Seçin...</option>';
        if (Array.isArray(companies)) {
            if (companies.length === 0) {
                compSel.innerHTML = '<option value="">(Kayıtlı Şirket Yok)</option>';
            }
            companies.forEach(c => {
                compSel.innerHTML += `<option value="${c.id}">${c.customerCompanyName || 'İsimsiz'}</option>`;
            });
        }

        if (id) {
            const resC = await fetch(`${API_BASE_URL}/api/customer/${id}`);
            const data = await resC.json();
            document.getElementById('custCompany').value = data.customerCompanyId;
            document.getElementById('custFirst').value = data.customerContactPersonName;
            document.getElementById('custLast').value = data.customerContactPersonLastName;
            document.getElementById('custPhone').value = data.customerContactPersonMobilPhone;
            document.getElementById('custMail').value = data.customerContactPersonMail;
        } else {
            document.getElementById('custFirst').value = '';
            document.getElementById('custLast').value = '';
            document.getElementById('custPhone').value = '';
            document.getElementById('custMail').value = '';
        }
    } catch (e) { console.error(e); alert('Şirketler yüklenemedi!'); }
}

window.saveCustomer = async function () {
    const id = document.getElementById('custId').value;
    const dto = {
        customerCompanyId: parseInt(document.getElementById('custCompany').value),
        customerContactPersonName: document.getElementById('custFirst').value,
        customerContactPersonLastName: document.getElementById('custLast').value,
        customerContactPersonMobilPhone: document.getElementById('custPhone').value,
        customerContactPersonMail: document.getElementById('custMail').value
    };
    if (id) dto.id = parseInt(id);

    if (!dto.customerCompanyId || !dto.customerContactPersonName) return alert('Şirket ve İsim zorunludur.');

    try {
        const method = id ? 'PUT' : 'POST';
        const res = await fetch(`${API_BASE_URL}/api/customer`, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });

        if (res.ok) {
            alert('Başarıyla kaydedildi.');
            closeModal('customerModal');
            loadCustomers();
        } else alert('Hata oluştu.');
    } catch (e) { alert(e.message); }
}

window.deleteCustomer = async function (id, name) {
    if (!(await showConfirm('Müşteri Silme', `"${name}" müşterisini silmek istiyor musunuz?`))) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/customer/${id}`, { method: 'DELETE' });
        if (res.ok) { alert('Silindi.'); loadCustomers(); }
        else alert('Silinemedi.');
    } catch (e) { alert(e.message); }
}

// --- OFFER WIZARD LOGIC ---
let wizardState = {
    step: 1,
    items: [], // { id, name, code, quantity, price, discount, unit, stock }
    customerId: null,
    warehouseId: null,
    categoryId: null,
    customers: [],
    warehouses: [],
    categories: [],
    lastLoadedProducts: [],
    offerId: null
};

window.startOfferWizard = async function () {
    wizardState = { step: 1, items: [], customers: [], warehouses: [], categories: [], lastLoadedProducts: [], offerId: null };
    document.getElementById('offerWizardModal').style.display = 'flex';
    wizardNext(1);

    try {
        // Load lists for Step 1
        const [resCust, resWh, resCat] = await Promise.all([
            fetch(`${API_BASE_URL}/api/customer?pageSize=1000`),
            fetch(`${API_BASE_URL}/api/warehouse?pageSize=1000`),
            fetch(`${API_BASE_URL}/api/product/categories`)
        ]);

        if (!resCust.ok || !resWh.ok || !resCat.ok) {
            throw new Error("Veriler yüklenemedi. Lütfen bağlantınızı kontrol edin.");
        }

        const [pagedCust, pagedWh, pagedCat] = await Promise.all([
            resCust.json(),
            resWh.json(),
            resCat.json()
        ]);

        wizardState.customers = Array.isArray(pagedCust) ? pagedCust : (pagedCust.items || []);
        wizardState.warehouses = Array.isArray(pagedWh) ? pagedWh : (pagedWh.items || []);
        wizardState.categories = pagedCat;
    } catch (e) {
        alert("Bağlantı veya Yetki Hatası: " + e.message);
        console.error("Wizard Load Error:", e);
        return; // Stop if data cannot be loaded
    }

    const mSel = document.getElementById('wMusteri');
    const dSel = document.getElementById('wDepo');
    const kSel = document.getElementById('wKategori');

    // Display Company Name - Contact Person
    mSel.innerHTML = '<option value="">Müşteri Seçin...</option>' + wizardState.customers.map(c => `<option value="${c.id}">${c.customerCompanyName} - ${c.fullName || c.customerContactPersonName}</option>`).join('');
    dSel.innerHTML = '<option value="">Depo Filtresi (Opsiyonel)</option>' + wizardState.warehouses.map(w => `<option value="${w.id}">${w.warehouseName}</option>`).join('');
    kSel.innerHTML = '<option value="">Kategori Filtresi (Opsiyonel)</option>' + wizardState.categories.map(k => `<option value="${k.id}">${k.categoryName}</option>`).join('');
}

window.editOffer = async function (id) {
    try {
        const res = await fetch(`${API_BASE_URL}/api/offer/${id}`);
        if (!res.ok) throw new Error('Teklif detayları alınamadı');
        const offer = await res.json();

        // Faturalanmış teklif düzenlenemez (UI Kontrolü)
        if (offer.status === 4) {
            return alert('Faturalanmış veya Tamamlanmış teklifler düzenlenemez.');
        }

        // Önce Wizard'ı başlat
        await startOfferWizard();
        wizardState.offerId = id;

        // Müşteri seç
        document.getElementById('wMusteri').value = offer.customerId;
        wizardState.customerId = offer.customerId;

        // Kalemleri wizardState'e aktar
        wizardState.items = offer.items.map(i => ({
            id: i.productId,
            name: i.productName,
            code: i.productCode,
            quantity: i.quantity,
            price: i.unitPrice,
            discount: i.discountRate,
            unit: i.unitName || 'Adet',
            stock: 0,
            reserved: 0,
            currency: i.currency || 'TL',
            imageUrl: i.imageUrl,
            categoryName: i.categoryName || 'Diğer'
        }));

        // Adım 3'e atla (Fiyatlandırma) ki kullanıcı görsün
        wizardNext(3);

    } catch (e) {
        alert('Hata: ' + e.message);
        console.error(e);
    }
}

window.wizardNext = function (step) {
    if (wizardState.step === 1 && step > 1) {
        wizardState.customerId = document.getElementById('wMusteri').value;
        if (!wizardState.customerId) return alert('Lütfen müşteri seçin.');
        wizardState.warehouseId = document.getElementById('wDepo').value;
        // wizardState.categoryId is now handled per-product step
    }

    if (wizardState.step === 2 && step > 2) {
        if (wizardState.items.length === 0) return alert('Lütfen en az bir ürün ekleyin.');
    }

    if (wizardState.step === 3 && step > 3) {
        const currencies = wizardState.items.map(i => (i.currency || 'TL').toUpperCase());
        const uniqueCurrencies = new Set(currencies.map(c => c === 'TRY' ? 'TL' : c));
        if (uniqueCurrencies.size > 1) {
            return alert('Dikkat: Teklifte farklı döviz birimleri (TL, USD, EUR) bir arada bulunamaz.\n\nLütfen tüm kalemleri TL\'ye çevirerek devam edin. ⚠️');
        }
    }

    document.querySelectorAll('.wizard-pane').forEach(p => p.style.display = 'none');
    const targetPane = document.getElementById(`wizard-step-${step}`);
    if (targetPane) targetPane.style.display = 'block';

    document.querySelectorAll('.step-badge').forEach((b, idx) => {
        b.classList.toggle('active-step', (idx + 1) === step);
    });

    wizardState.step = step;

    if (step === 2) loadWizardProducts();
    if (step === 3) loadWizardPricing();
    if (step === 4) prepareOfferPreview();
}

async function loadWizardProducts() {
    const tbody = document.getElementById('wizardProductList');
    tbody.innerHTML = '<tr><td colspan="8" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const catFilter = document.getElementById('wKategori')?.value || '';
        const res = await fetch(`${API_BASE_URL}/api/product?pageSize=1000`);
        const pagedData = await res.json();
        let products = Array.isArray(pagedData) ? pagedData : (pagedData.items || []);

        if (catFilter) {
            products = products.filter(p => p.categoryId == catFilter);
        }

        wizardState.lastLoadedProducts = products;
        tbody.innerHTML = '';
        products.forEach(p => {
            const available = (p.currentStock || 0) - (p.reservedStock || 0);
            const existing = wizardState.items.find(i => i.id === p.id);
            const selectedQty = existing ? existing.quantity : 0;

            const tr = document.createElement('tr');
            tr.id = `wizard-row-${p.id}`;
            tr.innerHTML = `
                <td>${p.productName || ''} <br><small>${p.productCode || ''}</small></td>
                <td>${p.currentStock || 0}</td>
                <td style="color:var(--error);">${p.reservedStock || 0}</td>
                <td><strong style="color:${available > 0 ? 'var(--success)' : 'var(--error)'}">${available}</strong></td>
                <td>${p.unitName || 'Adet'}</td>
                <td><input type="number" id="qty-${p.id}" value="1" min="1" style="width:50px; padding:0.2rem; border:1px solid #ddd; border-radius:4px;"></td>
                <td style="text-align:right; white-space:nowrap;">
                    <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; background:var(--error); margin-right:2px;" 
                        onclick="minusFromWizardOffer(${p.id})">-</button>
                    <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto;" 
                        onclick="addToWizardOffer(${p.id})">+</button>
                </td>
                <td style="text-align:center;"><span id="selected-qty-${p.id}" class="badge" style="background:${selectedQty > 0 ? 'var(--primary)' : '#eee'}; color:${selectedQty > 0 ? 'white' : '#666'};">${selectedQty}</span></td>
            `;
            tbody.appendChild(tr);
        });
    } catch (e) {
        console.error(e);
        tbody.innerHTML = '<tr><td colspan="8" style="text-align:center; color:red;">Veriler yüklenemedi.</td></tr>';
    }
}

window.addToWizardOffer = function (id) {
    const product = wizardState.lastLoadedProducts.find(p => p.id === id);
    if (!product) return;

    const qtyInput = document.getElementById(`qty-${id}`);
    const qty = parseFloat(qtyInput.value) || 0;

    if (qty <= 0) {
        alert('Lütfen geçerli bir miktar giriniz.');
        return;
    }

    const stock = product.currentStock || 0;
    const reserved = product.reservedStock || 0;
    const available = stock - reserved;
    const unit = product.unitName || 'Adet';

    const existing = wizardState.items.find(i => i.id === id);
    const currentTotal = (existing ? existing.quantity : 0) + qty;

    if (currentTotal > available) {
        if (!confirm(`⚠️ UYARI: Stok Yetersiz!\n\nKalan (Net) Stok: ${available} ${unit}\nİstenen Toplam: ${currentTotal} ${unit}\n\nYine de eklemek istiyor musunuz?`)) {
            return;
        }
    }

    if (existing) {
        existing.quantity += qty;
    } else {
        wizardState.items.push({
            id: product.id,
            name: product.productName,
            code: product.productCode,
            quantity: qty,
            price: product.currentPrice || 0,
            discount: 0,
            unit: unit,
            stock: stock,
            reserved: reserved,
            currency: product.currency || 'TL',
            imageUrl: product.imageUrl,
            categoryName: product.categoryName || 'Diğer'
        });
    }

    const badge = document.getElementById(`selected-qty-${id}`);
    if (badge) {
        const newTotal = existing ? existing.quantity : qty;
        badge.innerText = newTotal;
        badge.style.background = 'var(--primary)';
        badge.style.color = 'white';
        // Pulse animation
        badge.style.transform = 'scale(1.2)';
        setTimeout(() => badge.style.transform = 'scale(1)', 200);
    }

    qtyInput.value = 1;
}

window.minusFromWizardOffer = function (id) {
    const existing = wizardState.items.find(i => i.id === id);
    if (!existing) return;

    const qtyInput = document.getElementById(`qty-${id}`);
    const qty = parseFloat(qtyInput.value) || 1;

    existing.quantity -= qty;

    const badge = document.getElementById(`selected-qty-${id}`);
    if (existing.quantity <= 0) {
        wizardState.items = wizardState.items.filter(i => i.id !== id);
        if (badge) {
            badge.innerText = '0';
            badge.style.background = '#eee';
            badge.style.color = '#666';
        }
    } else {
        if (badge) badge.innerText = existing.quantity;
    }

    qtyInput.value = 1;
}

window.updateWizardItemQty = function (id, qty) {
    const val = parseFloat(qty) || 0;
    if (val <= 0) {
        removeFromWizardOffer(id);
        return;
    }

    const item = wizardState.items.find(i => i.id === id);
    if (item) {
        item.quantity = val;

        // Update badge in product list if visible
        const badge = document.getElementById(`selected-qty-${id}`);
        if (badge) badge.innerText = val;

        // Refresh Steps if needed
        if (wizardState.step === 3) loadWizardPricing();
    }
}

function loadWizardPricing() {
    const tbody = document.getElementById('wizardPricingList');
    tbody.innerHTML = '';

    if (wizardState.items.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Henüz ürün eklenmedi.</td></tr>';
        return;
    }

    const currencies = wizardState.items.map(i => (i.currency || 'TL').toUpperCase());
    const uniqueCurrencies = new Set(currencies.map(c => c === 'TRY' ? 'TL' : c));
    const hasMixed = uniqueCurrencies.size > 1;
    const hasForex = currencies.some(c => c !== 'TL' && c !== 'TRY');

    const curArea = document.getElementById('wizardCurrencyArea');
    if (curArea) {
        // Dövizli ürün varsa (USD, EUR vb) kur paneli her zaman görünsün
        curArea.style.display = hasForex ? 'block' : 'none';
        const warn = document.getElementById('currencyWarning');
        if (warn) warn.style.display = hasMixed ? 'block' : 'none';
    }

    wizardState.items.forEach(i => {
        const tr = document.createElement('tr');
        const symbol = getCurrencySymbol(i.currency);
        const netPrice = (i.price * (1 - i.discount / 100)).toFixed(2);
        tr.innerHTML = `
            <td><strong>${i.name}</strong><br><small>${i.code} <span class="badge" style="background:#eee; color:#666;">${i.currency || 'TL'}</span></small></td>
            <td>${i.quantity} ${i.unit}</td>
            <td>
                <div style="font-size:0.75rem; color:var(--muted); margin-bottom:2px;">Liste: ${formatMoney(i.price)} ${symbol}</div>
                <input type="number" step="0.01" value="${i.price}" onchange="updateWizardItem(${i.id}, 'price', this.value)" style="width:90px; padding:0.25rem; border:1px solid #ddd; border-radius:4px;">
                <div style="font-size:0.75rem; color:var(--primary); margin-top:2px;">Net: <span id="net-price-${i.id}">${formatMoney(netPrice)}</span> ${symbol}</div>
            </td>
            <td><input type="number" step="1" value="${i.discount}" onchange="updateWizardItem(${i.id}, 'discount', this.value)" style="width:60px; padding:0.25rem; border:1px solid #ddd; border-radius:4px;"> %</td>
            <td id="total-${i.id}" style="font-weight:bold;">${formatMoney(i.quantity * i.price * (1 - i.discount / 100))} ${symbol}</td>
            <td style="text-align:right;"><button class="btn-primary" style="background:var(--error); width:auto; padding:0.25rem 0.5rem;" onclick="removeFromWizardOffer(${i.id})">❌</button></td>
        `;
        tbody.appendChild(tr);
    });
}

function getCurrencySymbol(cur) {
    if (!cur) return '₺';
    cur = cur.toUpperCase();
    if (cur === 'USD') return '$';
    if (cur === 'EUR') return '€';
    return '₺';
}

window.fetchExchangeRates = async function () {
    try {
        const res = await fetch(`${API_BASE_URL}/api/system/exchange-rates`);
        if (!res.ok) throw new Error('Kurlar merkezden alınamadı.');

        const data = await res.json();
        if (data.usd) document.getElementById('wRateUSD').value = data.usd;
        if (data.eur) document.getElementById('wRateEUR').value = data.eur;

        alert('Güncel kurlar başarıyla alındı. ✅');
    } catch (e) {
        console.error('Kur çekme hatası:', e);
        alert('İnternet erişimi veya CORS kısıtlaması nedeniyle kur otomatik çekilemedi. Lütfen manuel giriş yapınız. ⚠️');
    }
}

window.applyCurrencyConversion = function () {
    const rateUSD = parseFloat(document.getElementById('wRateUSD').value);
    const rateEUR = parseFloat(document.getElementById('wRateEUR').value);

    let count = 0;
    wizardState.items.forEach(item => {
        const cur = (item.currency || 'TL').toUpperCase();
        if (cur === 'USD' && rateUSD > 0) {
            item.price = item.price * rateUSD;
            item.currency = 'TL';
            count++;
        } else if (cur === 'EUR' && rateEUR > 0) {
            item.price = item.price * rateEUR;
            item.currency = 'TL';
            count++;
        }
    });

    if (count > 0) {
        alert(`${count} kalem başarıyla TL'ye çevrildi. 💱`);
        loadWizardPricing();
    } else {
        alert('Çevrilecek dövizli kalem bulunamadı veya kurlar geçersiz.');
    }
}

window.updateWizardItem = function (id, field, val) {
    const item = wizardState.items.find(i => i.id === id);
    if (item) {
        let valFloat = parseFloat(val) || 0;
        if (valFloat < 0) {
            alert('Negatif değer girilemez.');
            valFloat = 0;
        }
        item[field] = valFloat;
        const symbol = getCurrencySymbol(item.currency);
        const totalEl = document.getElementById(`total-${id}`);
        if (totalEl) {
            totalEl.innerText = formatMoney(item.quantity * item.price * (1 - item.discount / 100)) + ' ' + symbol;
        }
        const netPriceEl = document.getElementById(`net-price-${id}`);
        if (netPriceEl) {
            netPriceEl.innerText = formatMoney(item.price * (1 - item.discount / 100));
        }
    }
}
window.removeFromWizardOffer = function (id) {
    wizardState.items = wizardState.items.filter(i => i.id !== id);

    // Update Step 2 UI
    const badge = document.getElementById(`selected-qty-${id}`);
    if (badge) {
        badge.innerText = '0';
        badge.style.background = '#eee';
        badge.style.color = '#666';
    }

    // Update Step 3 UI if active
    if (wizardState.step === 3) loadWizardPricing();
}

function prepareOfferPreview() {
    const area = document.getElementById('offerPreviewArea');
    const customer = wizardState.customers.find(c => c.id == wizardState.customerId);

    // Pre-fill email if empty
    const emailToInput = document.getElementById('wizardEmailTo');
    if (emailToInput && !emailToInput.value) {
        const userJson = localStorage.getItem('user');
        if (userJson) {
            const user = JSON.parse(userJson);
            if (user.email) emailToInput.value = user.email;
        }
    }

    let subTotal = wizardState.items.reduce((acc, i) => acc + (i.quantity * i.price * (1 - i.discount / 100)), 0);
    const hasAnyDiscount = wizardState.items.some(i => i.discount > 0);
    const vat = subTotal * 0.20;
    const grandTotal = subTotal + vat;
    const firstItem = wizardState.items[0];
    const globalSymbol = firstItem ? getCurrencySymbol(firstItem.currency) : '₺';

    // Group items by category
    const itemsByCategory = wizardState.items.reduce((groups, item) => {
        const cat = item.categoryName || 'Diğer';
        if (!groups[cat]) groups[cat] = [];
        groups[cat].push(item);
        return groups;
    }, {});

    area.style.fontSize = "0.85rem"; // Global font size reduction for PDF
    area.innerHTML = `
        <div style="display:flex; justify-content:space-between; margin-bottom:2rem; font-size: 0.9rem;">
            <div>
                <h2 style="margin:0; color:var(--primary); font-size: 1.4rem;">TEKLİF FORMU</h2>
                <p><strong>Firma:</strong> ${customer ? customer.customerCompanyName : 'Seçilmedi'}</p>
                <p><strong>Yetkili:</strong> ${customer ? (customer.fullName || customer.customerContactPersonName) : '-'}</p>
            </div>
            <div style="text-align:right;">
                <p><strong>Tarih:</strong> ${new Date().toLocaleDateString()}</p>
                <p><strong>Geçerlilik:</strong> 15 Gün</p>
            </div>
        </div>
        <div id="offerItemsContainer" style="width: 100%;">
            ${Object.keys(itemsByCategory).map(catName => `
                <div style="margin-bottom: 2rem; break-inside: avoid; page-break-inside: avoid; display: block; width: 100%;">
                    <h3 style="background: #f8fafc; padding: 0.4rem 0.8rem; border-left: 4px solid var(--primary); margin-bottom: 0.5rem; font-size: 0.9rem; page-break-after: avoid;">${catName}</h3>
                    <table style="width:100%; border-collapse:collapse; margin-bottom:1rem;">
                        <thead>
                            <tr style="background:#f3f4f6; font-size: 0.8rem;">
                                <th style="padding:0.4rem; text-align:left; width: 60px;">Resim</th>
                                <th style="text-align:left; padding:0.4rem; width: 110px;">Kod</th>
                                <th style="text-align:left; padding:0.4rem;">Açıklama</th>
                                <th style="text-align:left; padding:0.4rem; width: 70px;">Miktar</th>
                                <th style="text-align:left; padding:0.4rem; width: 110px;">Birim Fiyat</th>
                                ${hasAnyDiscount ? '<th style="text-align:left; padding:0.4rem; width: 70px;">İndirim</th>' : ''}
                                <th style="text-align:left; padding:0.4rem; width: 110px;">Toplam</th>
                            </tr>
                        </thead>
                        <tbody style="font-size: 0.8rem;">
                            ${itemsByCategory[catName].map(i => {
        const lineTotal = i.quantity * i.price * (1 - i.discount / 100);
        const symbol = getCurrencySymbol(i.currency);
        const imgUrl = i.imageUrl ? (i.imageUrl.startsWith('http') ? i.imageUrl : API_BASE_URL + '/' + i.imageUrl) : '';
        return `<tr>
                                    <td style="padding:0.4rem; border-bottom:1px solid #444;">
                                        ${imgUrl ? `<img src="${imgUrl}" style="width:40px; height:40px; object-fit:contain;">` : '-'}
                                    </td>
                                    <td style="padding:0.4rem; border-bottom:1px solid #444;">${i.code}</td>
                                    <td style="padding:0.4rem; border-bottom:1px solid #444;">${i.name}</td>
                                    <td style="padding:0.4rem; border-bottom:1px solid #444;">${i.quantity} ${i.unit}</td>
                                    <td style="padding:0.4rem; border-bottom:1px solid #444;">${formatMoney(i.price)} ${symbol}</td>
                                    ${hasAnyDiscount ? `<td style="padding:0.4rem; border-bottom:1px solid #444;">%${i.discount}</td>` : ''}
                                    <td style="padding:0.4rem; border-bottom:1px solid #444;">${formatMoney(lineTotal)} ${symbol}</td>
                                </tr>`;
    }).join('')}
                        </tbody>
                    </table>
                </div>
            `).join('')}
        </div>
        <div style="display:flex; justify-content:flex-end; margin-top: 2rem; break-inside: avoid; page-break-inside: avoid; border-top: 1px solid #444; padding-top: 1rem;">
            <div style="width:320px; border: 2px solid #333; padding: 1rem; border-radius: 4px; background: #fdfdfd;">
                <div style="display:flex; justify-content:space-between; margin-bottom: 0.4rem;"><span>Ara Toplam:</span> <span>${formatMoney(subTotal)} ${globalSymbol}</span></div>
                <div style="display:flex; justify-content:space-between; margin-bottom: 0.4rem;"><span>KDV (%20):</span> <span>${formatMoney(vat)} ${globalSymbol}</span></div>
                <div style="display:flex; justify-content:space-between; font-weight:bold; margin-top:0.6rem; border-top:2px solid #333; padding-top:0.6rem; font-size: 1.1rem; color: #000;">
                    <span>GENEL TOPLAM:</span> <span>${formatMoney(grandTotal)} ${globalSymbol}</span>
                </div>
            </div>
        </div>
    `;
}

window.saveOffer = async function () {
    const dto = {
        customerId: parseInt(wizardState.customerId),
        validUntil: new Date(Date.now() + 15 * 24 * 60 * 60 * 1000).toISOString(),
        items: wizardState.items.map(i => ({
            productId: i.id,
            quantity: i.quantity,
            unitPrice: i.price,
            discountRate: i.discount,
            currency: i.currency || 'TL'
        }))
    };

    try {
        const url = wizardState.offerId ? `${API_BASE_URL}/api/offer/${wizardState.offerId}` : `${API_BASE_URL}/api/offer`;
        const method = wizardState.offerId ? 'PUT' : 'POST';

        const res = await fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });

        if (res.ok) {
            alert(wizardState.offerId ? 'Teklif başarıyla güncellendi. ✅' : 'Teklif başarıyla oluşturuldu. ✅');
            closeModal('offerWizardModal');
            loadOffers();
        } else {
            const err = await res.text();
            console.error('Save Offer Error:', err);
            alert('Teklif Kaydedilemedi! ❌\nDetay: ' + err);
        }
    } catch (e) { alert(e.message); }
}

window.downloadOffer = function (type) {
    const area = document.getElementById('offerPreviewArea');
    if (type === 'pdf') {
        const win = window.open('', '_blank');
        win.document.write(`
            <html>
            <head>
                <title>Teklif Formu</title>
                <style>
                    body { font-family: sans-serif; padding: 20px; font-size: 0.85rem; }
                    .offer-preview-content { width: 100%; }
                    table { page-break-inside: auto; }
                    tr { page-break-inside: avoid; page-break-after: auto; }
                    thead { display: table-header-group; }
                    tfoot { display: table-footer-group; }
                    @page { margin: 1.5cm; }
                </style>
            </head>
            <body>
                <div class="offer-preview-content">${area.innerHTML}</div>
            </body>
            </html>
        `);
        win.document.close();
        win.print();
    } else {
        const hasAnyDiscount = wizardState.items.some(i => i.discount > 0);
        const itemsByCategory = wizardState.items.reduce((groups, item) => {
            const cat = item.categoryName || 'Diğer';
            if (!groups[cat]) groups[cat] = [];
            groups[cat].push(item);
            return groups;
        }, {});

        let csv = `Kategori,Resim,Kod,Aciklama,Miktar,Birim Fiyat${hasAnyDiscount ? ',Indirim' : ''},Toplam\n`;
        Object.keys(itemsByCategory).forEach(catName => {
            itemsByCategory[catName].forEach(i => {
                const imgUrl = i.imageUrl ? (i.imageUrl.startsWith('http') ? i.imageUrl : API_BASE_URL + '/' + i.imageUrl) : '';
                csv += `\"${catName}\",\"${imgUrl}\",\"${i.code}\",\"${i.name}\",\"${i.quantity}\",\"${i.price}\"${hasAnyDiscount ? ',\"' + i.discount + '\"' : ''},\"${i.quantity * i.price * (1 - i.discount / 100)}\"\n`;
            });
        });
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `teklif_${new Date().getTime()}.csv`;
        a.click();
    }
}
window.sendOfferByEmail = async function () {
    const emailTo = document.getElementById('wizardEmailTo').value;
    const previewHtml = document.getElementById('offerPreviewArea').innerHTML;

    if (!emailTo) return alert('Lütfen alıcı e-posta adresini giriniz.');

    // Wrap preview in a full HTML for email
    const fullHtml = `
        <html>
        <body style="font-family: Arial, sans-serif; padding: 20px; line-height: 1.6;">
            <div style="max-width: 800px; margin: 0 auto; border: 1px solid #eee; padding: 30px; border-radius: 10px; font-size: 0.85rem;">
                ${previewHtml}
                <div style="margin-top: 30px; border-top: 1px solid #eee; padding-top: 15px; color: #666; font-size: 0.85rem;">
                    <p>Bu teklif <strong>S2O1 Sistemi</strong> üzerinden oluşturulmuştur.</p>
                </div>
            </div>
        </body>
        </html>
    `;

    try {
        const res = await fetch(`${API_BASE_URL}/api/offer/send-email`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                toEmail: emailTo,
                htmlContent: fullHtml,
                subject: 'S2O1 - Teklif Formu'
            })
        });

        if (res.ok) {
            alert('Teklif başarıyla gönderildi. ✅');
        } else {
            const data = await res.json();
            throw new Error(data.message || 'Gönderilemedi.');
        }
    } catch (e) {
        alert('Hata: ' + e.message);
    }
};

// --- WAREHOUSE & SHELF MODULES ---

window.loadShelves = async function () {
    const tbody = document.getElementById('shelfListBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="4">Yükleniyor...</td></tr>';
    try {
        const filter = window.currentStatusFilter['shelves'] || 'active';
        const res = await fetch(`${API_BASE_URL}/api/warehouse/shelves?status=${filter}`);
        const shelves = await res.json();
        tbody.innerHTML = '';
        shelves.forEach(s => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${s.warehouseName}</td>
                <td>${s.name}</td>
                <td>${s.description || '-'}</td>
                <td style="text-align:right;">
                    <div class="action-btn-container">
                        ${!s.isDeleted ? `<button class="btn-action btn-delete" onclick="deleteShelf(${s.id})">Sil</button>` : ''}
                        ${s.isDeleted && window.hasPermission('ShowDeletedItems', 'write') ? `<button class="btn-action" style="background:#10b981;" onclick="restoreEntity('shelf', ${s.id})">✅ Geri Yükle</button>` : ''}
                    </div>
                </td>
            `;
            tbody.appendChild(tr);
        });
    } catch (e) { console.error(e); }
};

window.openShelfModal = async function () {
    document.getElementById('shelfId').value = '';
    document.getElementById('shelfName').value = '';
    document.getElementById('shelfDescription').value = '';
    await loadShelfWarehouses();
    document.getElementById('shelfModal').style.display = 'flex';
};

async function loadShelfWarehouses() {
    const sel = document.getElementById('shelfWarehouseId');
    const res = await fetch(`${API_BASE_URL}/api/warehouse?pageSize=1000`);
    const paged = await res.json();
    const warehouses = Array.isArray(paged) ? paged : (paged.items || []);
    sel.innerHTML = warehouses.map(w => `<option value="${w.id}">${w.warehouseName}</option>`).join('');
}

window.saveShelf = async function () {
    const dto = {
        warehouseId: parseInt(document.getElementById('shelfWarehouseId').value),
        name: document.getElementById('shelfName').value,
        description: document.getElementById('shelfDescription').value
    };
    if (!dto.name) return alert('Raf adı gereklidir.');
    try {
        const res = await fetch(`${API_BASE_URL}/api/warehouse/shelves`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });
        if (res.ok) {
            closeModal('shelfModal');
            loadShelves();
        } else {
            alert('Kaydedilemedi.');
        }
    } catch (e) { alert(e.message); }
};

window.deleteShelf = async function (id) {
    if (!confirm('Rafı silmek istediğinize emin misiniz?')) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/warehouse/shelves/${id}`, { method: 'DELETE' });
        if (res.ok) loadShelves();
    } catch (e) { alert(e.message); }
};

// WAREHOUSE DASHBOARD

window.loadPendingDeliveries = async function () {
    const container = document.getElementById('pendingDeliveriesContainer');
    if (!container) return;
    container.innerHTML = '<p>Yükleniyor...</p>';
    try {
        const res = await fetch(`${API_BASE_URL}/api/invoice/pending-deliveries`);
        const invoices = await res.json();
        container.innerHTML = '';
        if (invoices.length === 0) {
            container.innerHTML = '<p>Bekleyen sevkiyat bulunamadı. ✅</p>';
            return;
        }
        invoices.forEach(inv => {
            const card = document.createElement('div');
            card.className = 'glass-card';
            card.innerHTML = `
                <div style="display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:1rem;">
                    <div>
                        <h4 style="margin:0;">#${inv.invoiceNumber}</h4>
                            <small style="color:var(--muted);">${inv.buyerCompanyName}</small>
                        </div>
                        <span class="badge" style="background:${inv.status === 7 ? '#fef3c7' : (inv.status === 9 ? '#fee2e2' : '#e0e7ff')}; color:${inv.status === 7 ? '#92400e' : (inv.status === 9 ? '#991b1b' : '#3730a3')};">
                            ${inv.status === 7 ? 'Hazırlanıyor' : (inv.status === 9 ? 'Yarım Kaldı (Bekliyor)' : 'Sıraya Girdi')}
                        </span>
                    </div>
                <div style="font-size:0.9rem; margin-bottom:1rem;">
                    <div><strong>Tarih:</strong> ${new Date(inv.invoiceDate).toLocaleDateString()}</div>
                    <div><strong>Ürün Sayısı:</strong> ${inv.items.length} Kalem</div>
                    ${inv.assignedDelivererUserName ? `<div><strong>Sorumlu:</strong> ${inv.assignedDelivererUserName}</div>` : ''}
                </div>
                <div style="display:flex; gap:0.5rem; flex-wrap:wrap;">
                    ${!inv.assignedDelivererUserId ? `<button class="btn-primary" onclick="assignJob(${inv.id})">📦 İşi Üstlen</button>` : ''}
                    ${(inv.assignedDelivererUserId) ? `
                        <button class="btn-primary" style="background:var(--success); flex:1;" onclick="openPrepModal(${inv.id})">🔍 Hazırla</button>
                        <button class="btn-primary" style="background:#8b5cf6; flex:1;" onclick="openTransferModal(${inv.id})">🔄 Devret</button>
                        <button class="btn-primary" style="background:#ef4444; flex:1;" onclick="unassignJob(${inv.id})">❌ Bırak</button>
                    ` : ''}
                </div>
            `;
            container.appendChild(card);
        });
    } catch (e) { console.error(e); }
};

window.assignJob = async function (id) {
    const user = JSON.parse(localStorage.getItem('user'));
    try {
        const res = await fetch(`${API_BASE_URL}/api/invoice/${id}/assign?userId=${user.id}`, { method: 'POST' });
        if (res.ok) {
            showToast('Görev atandı');
            loadPendingDeliveries();
        }
        else showToast('Atama başarısız', 'error');
    } catch (e) { showToast(e.message, 'error'); }
};

window.unassignJob = async function (id) {
    if (!confirm('Bu işi açığa çıkartmak (havuza geri bırakmak) istediğinize emin misiniz?')) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/invoice/${id}/unassign`, { method: 'POST' });
        if (res.ok) {
            showToast('İş açığa çıkarıldı', 'success');
            loadPendingDeliveries();
        }
        else {
            const err = await res.text();
            showToast('İşlem başarısız: ' + err, 'error');
        }
    } catch (e) { showToast(e.message, 'error'); }
};

window.openTransferModal = async function (id) {
    document.getElementById('transferInvoiceId').value = id;
    const targetSel = document.getElementById('transferTargetUser');
    targetSel.innerHTML = '<option value="">Yükleniyor...</option>';
    document.getElementById('transferActionModal').style.display = 'flex';

    try {
        // Personel listesini çek
        const res = await fetch(`${API_BASE_URL}/api/users?module=Warehouse&pageSize=500`);
        if (!res.ok) throw new Error('Kullanıcılar getirilemedi.');
        const pagedData = await res.json();
        const users = pagedData.items || [];
        const currentUser = JSON.parse(localStorage.getItem('user'));

        targetSel.innerHTML = '<option value="">-- Personel Seçin --</option>';
        users.forEach(u => {
            if (u.id === currentUser.id) return; // Kendine devredemez
            targetSel.innerHTML += `<option value="${u.id}">${u.firstName} ${u.lastName} (${u.role})</option>`;
        });
    } catch (e) {
        showToast(e.message, 'error');
        targetSel.innerHTML = '<option value="">Hata!</option>';
    }
};

window.confirmTransferJob = async function () {
    const invId = document.getElementById('transferInvoiceId').value;
    const toUserId = document.getElementById('transferTargetUser').value;
    if (!toUserId) return showToast('Lütfen devralacak bir personel seçiniz', 'warning');

    if (!confirm('Görevi seçili personele devretmek istediğinize emin misiniz?')) return;

    try {
        const res = await fetch(`${API_BASE_URL}/api/invoice/${invId}/transfer?toUserId=${toUserId}`, { method: 'POST' });
        if (res.ok) {
            showToast('İş başarıyla devredildi', 'success');
            closeModal('transferActionModal');
            loadPendingDeliveries();
        } else {
            const err = await res.text();
            showToast('Devretme işlemi başarısız: ' + err, 'error');
        }
    } catch (e) { showToast(e.message, 'error'); }
}

let prepState = {
    invoice: null,
    basket: []
};

window.openPrepModal = async function (id) {
    try {
        const res = await fetch(`${API_BASE_URL}/api/invoice/${id}?t=` + Date.now());
        if (res.status === 403) {
            showToast('Bu işlem için yetkiniz bulunmuyor', 'error');
            return;
        }
        if (!res.ok) {
            showToast('Fatura bilgileri yüklenemedi', 'error');
            return;
        }
        const inv = await res.json();
        if (!inv || !inv.items) {
            showToast('Fatura ürün listesi alınamadı', 'error');
            return;
        }
        prepState.invoice = inv;
        prepState.basket = inv.items.map(it => ({ ...it, picked: 0, checked: it.includeInDispatch }));

        document.getElementById('prepModalTitle').innerText = `📦 Sevkiyat Hazırlama: #${inv.invoiceNumber}`;
        document.getElementById('prepDeliverer').value = inv.assignedDelivererUserName || '';
        document.getElementById('prepReceiver').value = '';

        renderPrepBasket();
        document.getElementById('warehousePrepModal').style.display = 'flex';
        setTimeout(() => document.getElementById('prepBarcodeScan').focus(), 300);
    } catch (e) { showToast('Hazırlama başlatılamadı: ' + e.message, 'error'); }
};

function renderPrepBasket() {
    const tbody = document.getElementById('prepBasketBody');
    if (!tbody) return;
    tbody.innerHTML = '';
    prepState.basket.forEach((item, index) => {
        const isDone = item.picked >= item.quantity;
        const tr = document.createElement('tr');
        if (isDone) tr.style.background = '#f0fdf4';
        tr.innerHTML = `
            <td>
                <div><strong>${item.productName}</strong></div>
                <small style="color:var(--muted);">${item.productCode}</small>
            </td>
            <td>${item.warehouseName} / ${item.shelfName}</td>
            <td>
                <span style="font-weight:bold; color:${isDone ? 'var(--success)' : 'var(--error)'}">${item.picked}</span> / ${item.quantity} ${item.unitName || 'Adet'}
            </td>
            <td style="text-align:center;">
                <input type="checkbox" ${item.checked ? 'checked' : ''} onchange="prepState.basket[${index}].checked = this.checked">
            </td>
            <td style="text-align:right;">
                <button class="btn-action" style="background:var(--primary); min-width:40px;" onclick="prepAddItem(${index})">+</button>
            </td>
        `;
        tbody.appendChild(tr);
    });

    const dataList = document.getElementById('prepManualOptions');
    if (dataList) {
        dataList.innerHTML = '';
        prepState.basket.forEach((it) => {
            if (it.picked < it.quantity) {
                dataList.innerHTML += `<option value="${it.productCode}">${it.productName} (Kalan: ${it.quantity - it.picked})</option>`;
            }
        });
    }
}

window.prepScanItem = function () {
    const val = document.getElementById('prepBarcodeScan').value.trim();
    if (!val) return;

    // Check by product code or barcode
    const index = prepState.basket.findIndex(it => it.productCode === val);
    if (index !== -1) {
        prepAddItem(index);
        document.getElementById('prepBarcodeScan').value = '';
    } else {
        showToast('Ürün bu siparişte bulunamadı: ' + val, 'warning');
    }
};

window.prepAddManualItem = function () {
    const el = document.getElementById('prepManualSelect');
    if (!el) return;
    const val = el.value.trim().toLowerCase();
    if (!val) {
        alert('Lütfen eklenecek ürünü seçin veya arayın.');
        return;
    }

    const index = prepState.basket.findIndex(it =>
        (it.productCode && it.productCode.toLowerCase() === val) ||
        (it.productName && it.productName.toLowerCase() === val)
    );

    if (index !== -1) {
        if (prepState.basket[index].picked < prepState.basket[index].quantity) {
            prepAddItem(index);
            el.value = '';
        } else {
            alert('Bu ürünün zaten tamamı eklendi.');
            el.value = '';
        }
    } else {
        showToast('Ürün bulunamadı', 'warning');
    }
};

window.prepAddItem = function (index) {
    const item = prepState.basket[index];
    if (item.picked < item.quantity) {
        item.picked++;
        renderPrepBasket();
    } else {
        showToast('İstenen miktardan fazlasını ekleyemezsiniz', 'warning');
    }
};

window.completePrep = async function () {
    const receiver = document.getElementById('prepReceiver').value.trim();
    if (!receiver) return showToast('Lütfen teslim alan kişi adını giriniz', 'warning');

    // Check if everything is picked
    const missing = prepState.basket.filter(it => it.picked < it.quantity);
    if (missing.length > 0) {
        if (!confirm('Eksik ürün varken sevkiyatı tamamlamak istiyor musunuz? Eksik ürünler için de stok düşümü yapılacaktır.')) return;
    }

    const dto = {
        invoiceId: prepState.invoice.id,
        delivererUserId: prepState.invoice.assignedDelivererUserId,
        receiverName: receiver,
        includedItemIds: prepState.basket.filter(it => it.checked).map(it => it.id)
    };

    try {
        const res = await fetch(`${API_BASE_URL}/api/invoice/complete-delivery`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });
        if (res.ok) {
            closeModal('warehousePrepModal');
            loadPendingDeliveries();

            // Show action modal instead of just alert
            document.getElementById('dispatchActionModal').style.display = 'flex';
        } else {
            const err = await res.text();
            showToast('Tamamlanamadı: ' + err, 'error');
        }
    } catch (e) { showToast(e.message, 'error'); }
};

window.incompletePrep = async function () {
    if (!prepState.invoice) return;

    if (!confirm('Devam eden bu işi yarım (eksik) olarak beklemeye almak istiyor musunuz?')) return;

    try {
        const res = await fetch(`${API_BASE_URL}/api/invoice/${prepState.invoice.id}/incomplete`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        if (res.ok) {
            closeModal('warehousePrepModal');
            loadPendingDeliveries(); // Tabloyu yeniler, ancak bu fatura listeye bir daha düşecek mi? Sadece statüsü değişir, listeden de düşmesini istiyorsak PendingDeliveries sorgusuna 'PartiallyDelivered' durumunu kontrol eden bir filtre lazımdır. Eklendi/eklenecektir.
            showToast('Sevkiyat yarım olarak beklemeye alındı.', 'success');
        } else {
            const err = await res.text();
            showToast('Marking incomplete failed: ' + err, 'error');
        }
    } catch (e) { showToast(e.message, 'error'); }
};

window.generateDispatchHtml = function () {
    if (!prepState.invoice) return "";

    const inv = prepState.invoice;
    const delivererName = document.getElementById('prepDeliverer').value || inv.assignedDelivererUserName || 'Bilinmiyor';
    const receiverName = document.getElementById('prepReceiver').value || inv.buyerCompanyName || 'Bilinmiyor';

    // Only items that are in the prep basket and were actually included
    const pickedItems = prepState.basket.filter(item => item.checked && item.picked > 0);

    let itemsHtml = '';
    pickedItems.forEach(i => {
        itemsHtml += `
            <tr style="border-bottom: 1px solid #e2e8f0;">
                <td style="padding: 10px; text-align: left;">${i.productCode || '-'}</td>
                <td style="padding: 10px; text-align: left;">${i.productName || '-'}</td>
                <td style="padding: 10px; text-align: left;">Adet</td>
                <td style="padding: 10px; text-align: right;">${i.picked.toFixed(2)}</td>
            </tr>
        `;
    });

    return `
        <div style="font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; color: #1e293b;">
            <div style="display: flex; justify-content: space-between; margin-bottom: 2rem; border-bottom: 2px solid #e2e8f0; padding-bottom: 1rem;">
                <div>
                    <h2 style="margin: 0; color: #0f172a;">SEVK İRSALİYESİ</h2>
                    <p style="margin: 5px 0 0 0; color: #64748b;"><strong>Firma/Müşteri:</strong> ${inv.buyerCompanyName || '-'}</p>
                </div>
                <div style="text-align: right;">
                    <p style="margin: 0;"><strong>Tarih:</strong> ${new Date().toLocaleDateString('tr-TR')}</p>
                    <p style="margin: 5px 0 0 0;"><strong>Evrak No:</strong> ${inv.invoiceNumber || '-'}</p>
                </div>
            </div>
            
            <table style="width: 100%; border-collapse: collapse; margin-bottom: 3rem;">
                <thead>
                    <tr style="background-color: #f8fafc; border-bottom: 2px solid #cbd5e1;">
                        <th style="padding: 12px 10px; text-align: left; width: 20%;">Kod</th>
                        <th style="padding: 12px 10px; text-align: left; width: 50%;">Açıklama</th>
                        <th style="padding: 12px 10px; text-align: left; width: 15%;">Birim</th>
                        <th style="padding: 12px 10px; text-align: right; width: 15%;">Miktar</th>
                    </tr>
                </thead>
                <tbody>
                    ${itemsHtml}
                </tbody>
            </table>
            
            <div style="display: flex; justify-content: space-between; margin-top: 4rem; padding-top: 2rem; border-top: 1px solid #e2e8f0;">
                <div style="text-align: center; width: 40%;">
                    <h4 style="margin: 0 0 10px 0;">Teslim Eden</h4>
                    <p style="margin: 0;">${delivererName}</p>
                    <div style="margin-top: 30px; border-top: 1px solid #1e293b; width: 80%; margin-left: auto; margin-right: auto;">İmza</div>
                </div>
                <div style="text-align: center; width: 40%;">
                    <h4 style="margin: 0 0 10px 0;">Teslim Alan</h4>
                    <p style="margin: 0;">${receiverName}</p>
                    <div style="margin-top: 30px; border-top: 1px solid #1e293b; width: 80%; margin-left: auto; margin-right: auto;">İmza</div>
                </div>
            </div>
        </div>
    `;
};

// Version accepting raw data (used by downloadWaybillPdf)
window.generateDispatchHtmlFromData = function (items, buyerName, buyerAddress, sysInfo, meta) {
    const delivererName = (meta && meta.assignedUser) || '';
    const receiverName = (meta && meta.receiverName) || buyerName || 'Bilinmiyor';
    const invoiceNumber = (meta && meta.invoiceNumber) || '-';
    const date = (meta && meta.date) || new Date().toLocaleDateString('tr-TR');
    const buyerTax = (meta && meta.buyerCompanyTaxInfo) || '';
    const companyName = (sysInfo && sysInfo.companyName) || '';
    const companyAddress = (sysInfo && sysInfo.address) || '';

    let itemsHtml = '';
    (items || []).forEach((i, idx) => {
        itemsHtml += `
            <tr style="border-bottom:1px solid #e2e8f0; background:${idx % 2 === 0 ? '#fff' : '#f8fafc'}">
                <td style="padding:10px; text-align:left;">${i.productCode || '-'}</td>
                <td style="padding:10px; text-align:left;">${i.productName || '-'}</td>
                <td style="padding:10px; text-align:left;">Adet</td>
                <td style="padding:10px; text-align:right;">${parseFloat(i.quantity || 0).toFixed(2)}</td>
            </tr>
        `;
    });

    return `<!DOCTYPE html><html><head><meta charset="utf-8"><title>Sevk İrsaliyesi - ${invoiceNumber}</title>
    <style>body{font-family:Arial,sans-serif;color:#1e293b;margin:0;padding:20px;}table{border-collapse:collapse;width:100%;}@media print{body{padding:0;}}</style>
    </head><body>
        <div style="max-width:800px;margin:0 auto;">
            <div style="display:flex;justify-content:space-between;margin-bottom:1.5rem;border-bottom:2px solid #e2e8f0;padding-bottom:1rem;align-items:flex-start;">
                <div>
                    <h2 style="margin:0;color:#0f172a;font-size:1.4rem;">SEVKİYAT İRSALİYESİ</h2>
                    <p style="margin:6px 0 2px 0;font-size:0.9rem;color:#475569;"><strong>Gönderen:</strong> ${companyName}</p>
                    ${companyAddress ? `<p style="margin:2px 0;font-size:0.85rem;color:#64748b;">${companyAddress}</p>` : ''}
                </div>
                <div style="text-align:right;font-size:0.9rem;">
                    <p style="margin:0;"><strong>Evrak No:</strong> ${invoiceNumber}</p>
                    <p style="margin:5px 0 0 0;"><strong>Tarih:</strong> ${date}</p>
                </div>
            </div>
            <div style="background:#f8fafc;border:1px solid #e2e8f0;border-radius:6px;padding:1rem;margin-bottom:1.5rem;">
                <p style="margin:0;font-size:0.8rem;color:#64748b;font-weight:700;text-transform:uppercase;letter-spacing:0.05em;">Alıcı Firma</p>
                <p style="margin:4px 0 0 0;font-weight:700;">${buyerName}</p>
                ${buyerAddress ? `<p style="margin:2px 0;font-size:0.85rem;color:#475569;">${buyerAddress}</p>` : ''}
                ${buyerTax ? `<p style="margin:2px 0;font-size:0.85rem;color:#475569;">Vergi / İletişim: ${buyerTax}</p>` : ''}
            </div>
            <table>
                <thead>
                    <tr style="background-color:#1e293b;color:white;">
                        <th style="padding:10px;text-align:left;width:20%;">Ürün Kodu</th>
                        <th style="padding:10px;text-align:left;width:50%;">Ürün Adı</th>
                        <th style="padding:10px;text-align:left;width:15%;">Birim</th>
                        <th style="padding:10px;text-align:right;width:15%;">Miktar</th>
                    </tr>
                </thead>
                <tbody>${itemsHtml}</tbody>
            </table>
            <div style="display:flex;justify-content:space-between;margin-top:4rem;padding-top:1.5rem;border-top:1px solid #e2e8f0;">
                <div style="text-align:center;width:40%;">
                    <h4 style="margin:0 0 8px 0;">Teslim Eden</h4>
                    <p style="margin:0;">${delivererName}</p>
                    <div style="margin-top:40px;border-top:1px solid #1e293b;padding-top:4px;font-size:0.85rem;color:#64748b;">İmza</div>
                </div>
                <div style="text-align:center;width:40%;">
                    <h4 style="margin:0 0 8px 0;">Teslim Alan</h4>
                    <p style="margin:0;">${receiverName}</p>
                    <div style="margin-top:40px;border-top:1px solid #1e293b;padding-top:4px;font-size:0.85rem;color:#64748b;">İmza</div>
                </div>
            </div>
        </div>
    </body></html>`;
};

window.printDispatch = function () {
    const html = generateDispatchHtml();
    const win = window.open('', '_blank');
    win.document.write(`<html><head><title>Sevk İrsaliyesi</title></head><body onload="window.print()">${html}</body></html>`);
    win.document.close();
    closeModal('dispatchActionModal');
};

window.downloadDispatch = function () {
    const html = generateDispatchHtml();
    const win = window.open('', '_blank');
    win.document.write(`<html><head><title>Sevk İrsaliyesi</title></head><body onload="window.print()">${html}</body></html>`);
    win.document.close();
    closeModal('dispatchActionModal');
};

window.emailDispatch = function () {
    // Placeholder implementation for emailing dispatch note
    const receiver = prepState.invoice?.buyerCompanyName || 'Müşteri';
    showToast(receiver + ' yetkilisine irsaliye maili gönderildi');
    closeModal('dispatchActionModal');
};

// --- SCANNER MODAL LOGIC ---
let html5QrcodeScanner = null;
let currentScannerTargetId = null;
let currentScannerCallback = null;

window.openScanner = function (targetInputId, callback) {
    document.getElementById('scannerModal').style.display = 'flex';
    currentScannerTargetId = targetInputId;
    currentScannerCallback = callback;

    if (!html5QrcodeScanner) {
        html5QrcodeScanner = new Html5QrcodeScanner(
            "reader", { fps: 10, qrbox: 250 });
    }

    html5QrcodeScanner.render(onScanSuccess, onScanError);
}

window.closeScanner = function () {
    document.getElementById('scannerModal').style.display = 'none';
    if (html5QrcodeScanner) {
        html5QrcodeScanner.clear().catch(error => {
            console.error("Failed to clear html5QrcodeScanner. ", error);
        });
    }
}

function onScanSuccess(decodedText, decodedResult) {
    closeScanner();
    if (currentScannerTargetId) {
        const input = document.getElementById(currentScannerTargetId);
        if (input) {
            input.value = decodedText;
        }
    }
    if (currentScannerCallback && typeof currentScannerCallback === 'function') {
        currentScannerCallback();
    }
}

function onScanError(errorMessage) {
    // suppress errors (it fires frequently)
}

// --- WIZARD PRODUCT SEARCH & SCAN ---
window.filterWizardProducts = function () {
    const term = document.getElementById('wizardProductSearch').value.toLowerCase();
    const rows = document.querySelectorAll('#wizardProductList tr');
    rows.forEach(row => {
        if (row.id && row.id.startsWith('wizard-row-')) {
            const text = row.innerText.toLowerCase();
            row.style.display = text.includes(term) ? '' : 'none';
        }
    });
}

window.scanWizardProduct = function () {
    const term = document.getElementById('wizardProductSearch').value.trim().toLowerCase();
    if (!term) return;

    // Find product by code, system code, or exact name
    const p = wizardState.lastLoadedProducts.find(x =>
        (x.productCode && x.productCode.toLowerCase() === term) ||
        (x.systemCode && x.systemCode.toLowerCase() === term) ||
        (x.productName && x.productName.toLowerCase() === term)
    );

    if (p) {
        const qtyInput = document.getElementById(`qty-${p.id}`);
        if (qtyInput) {
            addToWizardOffer(p.id);
            document.getElementById('wizardProductSearch').value = '';
            filterWizardProducts();
        }
    } else {
        showToast("Ürün bulunamadı: " + term, 'warning');
    }
}
