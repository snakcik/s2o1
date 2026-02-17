// Automatically use the current host (works for localhost and network IP)
const API_BASE_URL = window.location.origin;

// Intercept all fetch requests to add user headers
const originalFetch = window.fetch;
window.fetch = async function (input, init) {
    init = init || {};
    init.headers = init.headers || {};

    const userJson = localStorage.getItem('user');
    if (userJson) {
        try {
            const user = JSON.parse(userJson);

            if (init.headers instanceof Headers) {
                init.headers.append('X-User-Id', user.id);
                init.headers.append('X-User-Name', user.userName);
                init.headers.append('X-User-Role', user.role);
            } else if (Array.isArray(init.headers)) {
                // If headers is array of arrays
                init.headers.push(['X-User-Id', String(user.id)]);
                init.headers.push(['X-User-Name', user.userName]);
                init.headers.push(['X-User-Role', user.role]);
            } else {
                // Plain object
                init.headers['X-User-Id'] = String(user.id);
                init.headers['X-User-Name'] = String(user.userName);
                init.headers['X-User-Role'] = String(user.role);
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

function checkAuth() {
    const user = localStorage.getItem('user');
    if (!user) window.location.href = 'index.html';
}

function loadDashboard() {
    const user = JSON.parse(localStorage.getItem('user'));
    const nameEl = document.getElementById('userNameDisplay');
    if (nameEl) nameEl.innerText = user.userName;
    const roleEl = document.getElementById('userRoleDisplay');
    if (roleEl) roleEl.innerText = user.role || 'Kullanıcı';

    // Hide user management menu for User role
    const userManagementMenu = Array.from(document.querySelectorAll('.menu-item')).find(el => el.innerText.includes('Kullanıcı'));
    if (userManagementMenu && user.role === 'User') {
        userManagementMenu.style.display = 'none';
    }

    // Default View
    switchView('dashboard');
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

window.switchView = function (viewName) {
    const currentUser = JSON.parse(localStorage.getItem('user'));

    // Prevent User role from accessing user management
    if (viewName === 'users' && currentUser.role === 'User') {
        alert('Bu sayfaya erişim yetkiniz yok!');
        return;
    }

    document.querySelectorAll('.view-section').forEach(el => el.style.display = 'none');
    const target = document.getElementById(`section-${viewName}`);
    if (target) target.style.display = 'block';

    const titleMap = {
        'dashboard': 'Dashboard',
        'users': 'Kullanıcı Yönetimi',
        'settings': 'Sistem Ayarları',
        'offers': 'Teklif Yönetimi',
        'invoices': 'Fatura Yönetimi',
        'inventory': 'Stok Yönetimi',
        'stock-entry': 'Stok Girişi',
        'reports': 'Raporlar',
        'companies': 'Şirket Yönetimi',
        'suppliers': 'Tedarikçi Yönetimi',
        'pricelists': 'Fiyat Listesi Yönetimi',
        'customer-companies': 'Müşteri Şirket Yönetimi',
        'customers': 'Müşteri Yönetimi',
        'logs': 'Denetim Kayıtları'
    };
    const titleEl = document.getElementById('pageTitle');
    if (titleEl) titleEl.innerText = titleMap[viewName] || 'Sayfa';

    if (viewName === 'users') loadUsers();
    if (viewName === 'settings') loadSystemInfo();
    if (viewName === 'offers') loadOffers();
    if (viewName === 'invoices') loadInvoices();
    if (viewName === 'inventory') switchInvTab('products'); // Default tab
    if (viewName === 'stock-entry') loadStockEntry();
    if (viewName === 'reports') initReportsView();
    if (viewName === 'companies') loadCompanies();
    if (viewName === 'suppliers') loadSuppliers();
    if (viewName === 'pricelists') loadPriceLists();
    if (viewName === 'customer-companies') loadCustomerCompanies();
    if (viewName === 'customers') loadCustomers();
    if (viewName === 'logs') loadLogs();
}

window.loadStockEntry = async function () {
    const pSel = document.getElementById('seProduct');
    const wSel = document.getElementById('seWarehouse');
    if (!pSel || !wSel) return;

    pSel.innerHTML = '<option value="">Yükleniyor...</option>';
    wSel.innerHTML = '<option value="">Yükleniyor...</option>';

    try {
        const [products, warehouses] = await Promise.all([
            fetch(`${API_BASE_URL}/api/product`).then(r => r.json()),
            fetch(`${API_BASE_URL}/api/warehouse`).then(r => r.json())
        ]);

        pSel.innerHTML = '<option value="">Ürün Seçiniz...</option>';
        products.forEach(p => {
            pSel.innerHTML += `<option value="${p.id}">${p.productCode} - ${p.productName} (Mevcut: ${p.currentStock})</option>`;
        });

        wSel.innerHTML = '<option value="">Depo Seçiniz...</option>';
        warehouses.forEach(w => {
            wSel.innerHTML += `<option value="${w.id}">${w.warehouseName}</option>`;
        });
    } catch (e) {
        pSel.innerHTML = '<option value="">Yüklenemedi!</option>';
        wSel.innerHTML = '<option value="">Yüklenemedi!</option>';
    }
}

window.submitStockEntry = async function () {
    const productId = document.getElementById('seProduct').value;
    const warehouseId = document.getElementById('seWarehouse').value;
    const quantity = document.getElementById('seQuantity').value;
    const desc = document.getElementById('seDescription').value;

    if (!productId || !warehouseId || !quantity || parseFloat(quantity) <= 0) {
        alert('Lütfen geçerli bir ürün, depo ve miktar giriniz.');
        return;
    }

    const currentUser = JSON.parse(localStorage.getItem('user'));

    try {
        const movementDto = {
            productId: parseInt(productId),
            warehouseId: parseInt(warehouseId),
            movementType: 1, // Entry
            quantity: parseFloat(quantity),
            description: desc || 'Stok Girişi Sayfası',
            userId: currentUser ? currentUser.id : 0,
            documentNo: '-'
        };

        const res = await fetch(`${API_BASE_URL}/api/stock/movement`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(movementDto)
        });

        if (res.ok) {
            alert('Stok başarıyla kaydedildi.');
            document.getElementById('seQuantity').value = '';
            document.getElementById('seDescription').value = '';
            loadStockEntry();
            // Also refresh product list if it's visible or next time it's loaded
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
        const res = await fetch(`${API_BASE_URL}/api/warehouse`);
        const warehouses = await res.json();
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
    const tbody = document.getElementById('reportListBody');
    if (!tbody) return;

    tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        let url = `${API_BASE_URL}/api/stock/report`;
        if (warehouseId) url += `?warehouseId=${warehouseId}`;

        const res = await fetch(url);
        if (!res.ok) {
            const text = await res.text();
            throw new Error(`Sunucu Hatası (${res.status}): ${text || res.statusText}`);
        }
        const data = await res.json();

        tbody.innerHTML = '';
        if (data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">Kayıt bulunamadı.</td></tr>';
            return;
        }

        data.forEach(item => {
            tbody.innerHTML += `
                <tr>
                    <td>${item.warehouseName}</td>
                    <td><b>${item.productCode}</b></td>
                    <td>${item.productName}</td>
                    <td><span class="badge" style="background:${item.currentStock > 0 ? '#d1fae5;color:#065f46;' : '#fee2e2;color:#991b1b;'}">${item.currentStock}</span></td>
                    <td>${item.unitName}</td>
                </tr>
            `;
        });
    } catch (e) {
        tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;color:red;">Hata: ' + e.message + '</td></tr>';
    }
}

async function loadSystemInfo(isShort = false) {
    try {
        const res = await fetch(`${API_BASE_URL}/api/system/info`);
        const data = await res.json();
        if (isShort) {
            const el = document.getElementById('dashboardSysInfo');
            if (el) el.innerHTML = `Veritabanı: <strong>${data.databaseStatus}</strong><br>Versiyon: ${data.appVersion}`;
        } else {
            document.getElementById('sysDbStatus').innerText = data.databaseStatus;
            document.getElementById('sysVersion').innerText = data.appVersion;
            document.getElementById('sysRuntime').innerText = data.runtime;
            document.getElementById('sysOS').innerText = data.os;
            document.getElementById('sysEnv').innerText = data.environment;
            document.getElementById('sysTime').innerText = data.serverTime;
        }
    } catch (e) { console.error(e); }
}

async function loadLogs() {
    const tbody = document.getElementById('logsListBody');
    if (!tbody) return;

    tbody.innerHTML = '<tr><td colspan="8" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const res = await fetch(`${API_BASE_URL}/api/logs`);
        if (!res.ok) throw new Error('Logs fetch failed');
        const logs = await res.json();

        tbody.innerHTML = '';
        if (logs.length === 0) {
            tbody.innerHTML = '<tr><td colspan="8" style="text-align:center;">Kayıt bulunamadı.</td></tr>';
            return;
        }

        logs.forEach(log => {
            const tr = document.createElement('tr');
            const date = new Date(log.createDate).toLocaleString();
            tr.innerHTML = `
                <td>${date}</td>
                <td>${log.actorUserName || log.actorUserId || '-'}</td>
                <td><span class="badge" style="background:#e0e7ff;">${log.actorRole}</span></td>
                <td><span class="badge" style="background:${getActionColor(log.actionType)}; color:white;">${log.actionType}</span></td>
                <td>${log.entityName}</td>
                <td>${log.entityId}</td>
                <td>${log.actionDescription || '-'}</td>
                <td style="font-size:0.8rem; color:#6b7280;">${log.source}/${log.ipAddress}</td>
            `;
            tbody.appendChild(tr);
        });
    } catch (e) {
        console.error(e);
        tbody.innerHTML = '<tr><td colspan="8" style="color:var(--error); text-align:center;">Kayıtlar yüklenemedi!</td></tr>';
    }
}

function getActionColor(action) {
    if (action === 'Added' || action === 'Create') return '#10b981'; // Green
    if (action === 'Modified' || action === 'Update') return '#f59e0b'; // Orange
    if (action === 'Deleted' || action === 'Delete') return '#ef4444'; // Red
    return '#6b7280'; // Gray
}

async function loadUsers() {
    const tbody = document.getElementById('userListBody');
    if (!tbody) return;

    const currentUser = JSON.parse(localStorage.getItem('user'));

    // User role should never access this function
    if (currentUser.role === 'User') {
        tbody.innerHTML = '<tr><td colspan="5" style="color:var(--error); text-align:center;">Erişim reddedildi!</td></tr>';
        return;
    }

    tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        let url = `${API_BASE_URL}/api/users`;

        // Root sees all users, Admin sees only users they created
        if (currentUser.role === 'Admin') {
            url += `?creatorId=${currentUser.id}`;
        }
        // Root role doesn't add creatorId parameter, so sees all users

        const res = await fetch(url);
        const users = await res.json();

        tbody.innerHTML = '';
        if (users.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">Kullanıcı bulunamadı.</td></tr>';
            return;
        }

        users.forEach(u => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><strong>${u.userName}</strong></td>
                <td>${u.firstName || '-'} ${u.lastName || '-'}</td>
                <td>${u.email}</td>
                <td><span class="badge" style="background:#f3f4f6;">${u.role || '-'}</span></td>
                <td style="text-align:right;">
                    <button class="btn-primary" style="padding:0.25rem 0.6rem; font-size:0.8rem; width:auto; margin-right:0.5rem;" onclick="openPermissionsModal(${u.id}, '${u.userName}')">Yetkiler</button>
                    <button class="btn-primary" style="padding:0.25rem 0.6rem; font-size:0.8rem; width:auto; background:var(--error);" onclick="deleteUser(${u.id}, '${u.userName}')">Sil</button>
                </td>
            `;
            tbody.appendChild(tr);
        });
    } catch (e) {
        tbody.innerHTML = '<tr><td colspan="5" style="color:var(--error); text-align:center;">Kullanıcı listesi yüklenemedi!</td></tr>';
        console.error(e);
    }
}

window.openModal = function () { document.getElementById('userModal').style.display = 'flex'; }
window.closeModal = function () { document.getElementById('userModal').style.display = 'none'; }

window.createUser = async function () {
    const currentUser = JSON.parse(localStorage.getItem('user'));
    const data = {
        userName: document.getElementById('uName').value,
        password: document.getElementById('uPass').value,
        firstName: document.getElementById('uFirst').value,
        lastName: document.getElementById('uLast').value,
        email: document.getElementById('uEmail').value,
        regNo: document.getElementById('uReg').value,
        roleId: parseInt(document.getElementById('uRole').value),
        createdByUserId: currentUser.id
    };

    if (!data.userName || !data.password || !data.firstName) { alert("Lütfen zorunlu alanları doldurun!"); return; }

    try {
        const res = await fetch(`${API_BASE_URL}/api/users`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (res.ok) {
            alert("Kullanıcı oluşturuldu!");
            closeModal();
            loadUsers(); // Refresh list
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
    tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const res = await fetch(`${API_BASE_URL}/api/users/${userId}/permissions`);
        const perms = await res.json();

        tbody.innerHTML = '';
        perms.forEach(p => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${p.moduleName}</td>
                <td style="text-align:center;"><input type="checkbox" class="perm-chk" data-mod="${p.moduleId}" data-type="read" ${p.canRead ? 'checked' : ''}></td>
                <td style="text-align:center;"><input type="checkbox" class="perm-chk" data-mod="${p.moduleId}" data-type="write" ${p.canWrite ? 'checked' : ''} onchange="permLogic(this)"></td>
                <td style="text-align:center;"><input type="checkbox" class="perm-chk" data-mod="${p.moduleId}" data-type="delete" ${p.canDelete ? 'checked' : ''} onchange="permLogic(this)"></td>
            `;
            tbody.appendChild(tr);
        });
    } catch (e) {
        tbody.innerHTML = '<tr><td colspan="4">İzinler yüklenemedi.</td></tr>';
    }
}

window.permLogic = function (el) {
    if (el.checked) {
        const row = el.closest('tr');
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
            alert("Yetkiler güncellendi.");
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
    if (!(await showConfirm('Kullanıcı Silme', `"${userName}" kullanıcısını silmek istediğinizden emin misiniz?\n\nBu işlem geri alınamaz!`))) {
        return;
    }

    try {
        const res = await fetch(`${API_BASE_URL}/api/users/${userId}`, {
            method: 'DELETE'
        });

        if (res.ok) {
            alert('Kullanıcı başarıyla silindi.');
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
        const res = await fetch(`${API_BASE_URL}/api/offer`);
        if (!res.ok) throw new Error('Teklifler alınamadı');
        const offers = await res.json();

        tbody.innerHTML = '';
        if (offers.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">Teklif bulunamadı.</td></tr>';
            return;
        }

        offers.forEach(o => {
            const tr = document.createElement('tr');
            // Status: 1=Pending, 2=Approved, 3=Rejected
            let statusBadge = '<span class="badge" style="background:#f3f4f6;">Bekliyor</span>';
            if (o.status === 2) statusBadge = '<span class="badge" style="background:var(--success); color:white;">Onaylı</span>';
            if (o.status === 3) statusBadge = '<span class="badge" style="background:var(--error); color:white;">Red</span>';

            let actions = '';
            if (o.status === 1) { // Pending
                actions += `<button class="btn-primary" style="padding:0.25rem 0.6rem; font-size:0.8rem; width:auto; margin-right:0.5rem;" onclick="approveOffer(${o.id})">Onayla</button>`;
            } else if (o.status === 2) { // Approved
                actions += `<button class="btn-primary" style="padding:0.25rem 0.6rem; font-size:0.8rem; width:auto; background:#4f46e5;" onclick="createInvoiceFromOffer(${o.id})">Faturalaştır</button>`;
            }

            tr.innerHTML = `
                <td><strong>${o.offerNumber || '-'}</strong></td>
                <td>${o.customerName || o.customerId}</td>
                <td>${new Date(o.offerDate).toLocaleDateString()}</td>
                <td>${new Date(o.validUntil).toLocaleDateString()}</td>
                <td>${o.totalAmount.toLocaleString()} ₺</td>
                <td>${statusBadge}</td>
                <td style="text-align:right;">${actions}</td>
            `;
            tbody.appendChild(tr);
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
        const res = await fetch(`${API_BASE_URL}/api/invoices`);
        if (!res.ok) throw new Error('Faturalar alınamadı');
        const invoices = await res.json();

        tbody.innerHTML = '';
        if (invoices.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Fatura bulunamadı.</td></tr>';
            return;
        }

        invoices.forEach(i => {
            const tr = document.createElement('tr');
            // Status: 1=Draft, 5=Approved
            let statusBadge = '<span class="badge" style="background:#f3f4f6;">Taslak</span>';
            if (i.status === 5) statusBadge = '<span class="badge" style="background:var(--success); color:white;">Onaylı</span>';

            let actions = '';
            if (i.status === 1) { // Draft
                actions += `<button class="btn-primary" style="padding:0.25rem 0.6rem; font-size:0.8rem; width:auto; margin-right:0.5rem;" onclick="approveInvoice(${i.id})">Onayla & Stok Düş</button>`;
            }

            tr.innerHTML = `
                <td><strong>${i.invoiceNumber || '-'}</strong></td>
                <td>${new Date(i.issueDate).toLocaleDateString()}</td>
                <td>${i.grandTotal} ₺</td>
                <td>${i.taxTotal} ₺</td>
                <td>${statusBadge}</td>
                <td style="text-align:right;">${actions}</td>
            `;
            tbody.appendChild(tr);
        });
    } catch (e) {
        tbody.innerHTML = '<tr><td colspan="6" style="color:var(--error); text-align:center;">Hata oluştu!</td></tr>';
        console.error(e);
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
            alert('Teklif onaylandı.');
            loadOffers();
        } else {
            alert('İşlem başarısız.');
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
            alert('Fatura oluşturuldu. ID: ' + invoiceId);
            switchView('invoices');
        } else {
            alert('İşlem başarısız.');
        }
    } catch (e) { alert('Hata: ' + e.message); }
}

window.approveInvoice = async function (id) {
    if (!(await showConfirm('Fatura Onayı', 'Faturayı onaylamak ve stok düşümü yapmak istiyor musunuz?', 'Onayla', 'İptal'))) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/invoices/${id}/approve`, { method: 'POST' });
        if (res.ok) {
            alert('Fatura onaylandı ve stok güncellendi.');
            loadInvoices();
        } else {
            alert('İşlem başarısız.');
        }
    } catch (e) { alert('Hata: ' + e.message); }
}

// COMPANIES & INVENTORY & MODALS

window.openModal = function (id) { // Updated to accept ID or default to userModal
    const modalId = typeof id === 'string' ? id : 'userModal';
    const el = document.getElementById(modalId);
    if (el) el.style.display = 'flex';
}

window.closeModal = function (id) { // Updated to accept ID
    const modalId = typeof id === 'string' ? id : 'userModal';
    const el = document.getElementById(modalId);
    if (el) el.style.display = 'none';

    // Reset forms if needed
    if (modalId === 'userModal') {
        document.getElementById('uName').value = '';
    }
}

// Quick Close for User Modal specifically (legacy support)
window.createUserModalClose = function () { closeModal('userModal'); }

// COMPANIES
window.loadCompanies = async function () {
    const tbody = document.getElementById('companyListBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;">Yükleniyor...</td></tr>';
    try {
        const res = await fetch(`${API_BASE_URL}/api/companies`);
        const data = await res.json();
        tbody.innerHTML = '';
        if (data.length === 0) { tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;">Kayıt yok.</td></tr>'; return; }

        data.forEach(c => {
            const tr = document.createElement('tr');
            tr.innerHTML = `<td>${c.companyName}</td><td>${c.taxNumber || '-'}</td><td>${c.allowNegativeStock ? 'Evet' : 'Hayır'}</td><td style="text-align:right;"><button class="btn-primary" style="padding:0.25rem; width:auto; background:#ef4444;" onclick="deleteCompany(${c.id}, '${c.companyName}')">Sil</button></td>`;
            tbody.appendChild(tr);
        });
    } catch (e) { console.error(e); tbody.innerHTML = '<tr><td colspan="4" style="color:var(--error);">Hata!</td></tr>'; }
}

window.openCompanyModal = function () { document.getElementById('companyModal').style.display = 'flex'; }

window.createCompany = async function () {
    const dto = {
        companyName: document.getElementById('cName').value,
        taxNumber: document.getElementById('cTax').value,
        address: document.getElementById('cAddr').value,
        allowNegativeStock: document.getElementById('cNegative').checked
    };
    if (!dto.companyName) return alert('Şirket adı zorunlu.');

    try {
        const res = await fetch(`${API_BASE_URL}/api/companies`, {
            method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(dto)
        });
        if (res.ok) { alert('Şirket eklendi.'); closeModal('companyModal'); loadCompanies(); }
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
    document.querySelectorAll('.inv-tab').forEach(el => el.style.display = 'none');
    document.getElementById(`tab-inv-${tabName}`).style.display = 'block';

    document.querySelectorAll('.tab-inv-btn').forEach(el => el.style.background = '#9ca3af');
    const btn = document.querySelector(`.tab-inv-btn[data-tab="${tabName}"]`);
    if (btn) btn.style.background = 'var(--primary)';

    if (tabName === 'products') loadProducts();
    if (tabName === 'stock-entry') loadStockEntry();
    if (tabName === 'warehouses') loadWarehouses();
    if (tabName === 'brands') loadBrands();
    if (tabName === 'categories') loadCategories();
    if (tabName === 'units') loadUnits();
}

// PRODUCTS
window.loadProducts = async function () {
    const tbody = document.getElementById('productListBody');
    if (!tbody) return;
    tbody.innerHTML = '<tr><td colspan="7">Yükleniyor...</td></tr>';
    try {
        const [products, brands, cats, units, warehouses] = await Promise.all([
            fetch(`${API_BASE_URL}/api/product`).then(r => r.json()),
            fetch(`${API_BASE_URL}/api/product/brands`).then(r => r.json()),
            fetch(`${API_BASE_URL}/api/product/categories`).then(r => r.json()),
            fetch(`${API_BASE_URL}/api/product/units`).then(r => r.json()),
            fetch(`${API_BASE_URL}/api/warehouse`).then(r => r.json())
        ]);

        tbody.innerHTML = '';
        if (products.length === 0) { tbody.innerHTML = '<tr><td colspan="7">Kayıt yok.</td></tr>'; return; }

        // Helper to find names
        const getBrandName = (id) => (brands.find(b => b.id === id) || {}).brandName || '-';
        const getCatName = (id) => (cats.find(c => c.id === id) || {}).categoryName || '-';
        const getUnitName = (id) => (units.find(u => u.id === id) || {}).unitShortName || '-';
        const getWareName = (id) => (warehouses.find(w => w.id === id) || {}).warehouseName || '-';

        products.forEach(p => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${p.productCode}</td>
                <td>${p.productName}</td>
                <td>${getCatName(p.categoryId)}</td>
                <td>${getBrandName(p.brandId)}</td>
                <td>${getWareName(p.warehouseId)}</td>
                <td>${p.currentStock} ${getUnitName(p.unitId)}</td>
                <td style="text-align:right;">
                     <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; margin-right:0.25rem;" 
                        onclick="openProductModal(${p.id})">Düzenle</button>
                    <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; background:var(--error);" 
                        onclick="deleteProduct(${p.id})">Sil</button>
                </td>
            `;
            tbody.appendChild(tr);
        });
    } catch (e) {
        console.error(e);
        tbody.innerHTML = '<tr><td colspan="7" style="color:red;">Hata: ' + e.message + '</td></tr>';
    }
}

window.openProductModal = async function (id = null) {
    document.getElementById('productModal').style.display = 'flex';
    document.querySelector('#productModal h3').innerText = id ? 'Ürün Düzenle' : 'Yeni Ürün';
    document.getElementById('pId').value = id || '';

    try {
        // Load Dropdowns FIRST
        const [cats, brands, units] = await Promise.all([
            fetch(`${API_BASE_URL}/api/product/categories`).then(r => r.json()),
            fetch(`${API_BASE_URL}/api/product/brands`).then(r => r.json()),
            fetch(`${API_BASE_URL}/api/product/units`).then(r => r.json())
        ]);

        const catSel = document.getElementById('pCategory');
        catSel.innerHTML = '<option value="">Kategori Seç...</option>';
        cats.forEach(c => catSel.innerHTML += `<option value="${c.id}">${c.categoryName}</option>`);

        const brandSel = document.getElementById('pBrand');
        brandSel.innerHTML = '<option value="">Marka Seç...</option>';
        brands.forEach(b => brandSel.innerHTML += `<option value="${b.id}">${b.brandName}</option>`);

        const unitSel = document.getElementById('pUnit');
        unitSel.innerHTML = '<option value="">Birim Seç...</option>';
        units.forEach(u => unitSel.innerHTML += `<option value="${u.id}">${u.unitName}</option>`);

        // THEN Set Values
        if (id) {
            // Edit Mode - Fetch product details
            const res = await fetch(`${API_BASE_URL}/api/product/${id}`);
            const p = await res.json();
            document.getElementById('pCode').value = p.productCode;
            document.getElementById('pName').value = p.productName;
            document.getElementById('pCategory').value = p.categoryId;
            document.getElementById('pBrand').value = p.brandId;
            document.getElementById('pUnit').value = p.unitId;
            // Warehouse logic if needed
        } else {
            // New Mode - Clear fields
            document.getElementById('pCode').value = '';
            document.getElementById('pName').value = '';
            document.getElementById('pCategory').value = '';
            document.getElementById('pBrand').value = '';
            document.getElementById('pUnit').value = '';
            // Warehouse clear if needed
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

    const dto = {
        productCode: pCode,
        productName: pName,
        categoryId: parseInt(catVal),
        brandId: parseInt(brandVal),
        unitId: parseInt(unitVal)
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
            alert(id ? 'Ürün güncellendi.' : 'Ürün başarıyla eklendi.');
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

window.deleteProduct = async function (id) {
    if (!(await showConfirm('Ürün Silme', 'Bu ürünü silmek istediğinize emin misiniz?'))) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/product/${id}`, { method: 'DELETE' });
        if (res.ok) { alert('Ürün silindi.'); loadProducts(); }
        else alert('Silinemedi.');
    } catch (e) { alert(e.message); }
}

// WAREHOUSES
window.loadWarehouses = async function () {
    const tbody = document.getElementById('warehouseListBody');
    if (!tbody) return;
    try {
        const res = await fetch(`${API_BASE_URL}/api/warehouse`);
        const data = await res.json();
        tbody.innerHTML = '';
        if (data.length === 0) { tbody.innerHTML = '<tr><td colspan="4">Kayıt yok.</td></tr>'; return; }

        data.forEach(w => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${w.warehouseName}</td>
                <td>${w.location}</td>
                <td>${w.companyId}</td>
                <td style="text-align:right;">
                    <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; margin-right:0.25rem;" 
                        onclick="openWarehouseModal(${w.id}, '${w.warehouseName}', '${w.location}', ${w.companyId})">Düzenle</button>
                    <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; background:var(--error);" 
                        onclick="deleteWarehouse(${w.id})">Sil</button>
                </td>
            `;
            tbody.appendChild(tr);
        });
    } catch (e) { console.error(e); }
}

window.openWarehouseModal = async function (id, name, loc, compId) {
    document.getElementById('warehouseModal').style.display = 'flex';
    document.querySelector('#warehouseModal h3').innerText = id ? 'Depo Düzenle' : 'Yeni Depo';

    document.getElementById('wId').value = id || '';
    document.getElementById('wName').value = name || '';
    document.getElementById('wLoc').value = loc || '';

    // Load Companies
    const res = await fetch(`${API_BASE_URL}/api/companies`);
    const data = await res.json();
    const sel = document.getElementById('wCompany');
    sel.innerHTML = '<option value="">Şirket Seç...</option>';
    data.forEach(c => {
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
            alert(id ? 'Depo güncellendi.' : 'Depo eklendi.');
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
        if (res.ok) { alert('Depo silindi.'); loadWarehouses(); }
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
        const res = await fetch(`${API_BASE_URL}/api/product/${endpoint}`);
        const data = await res.json();
        tbody.innerHTML = '';
        if (data.length === 0) {
            let colSpan = endpoint === 'units' ? 3 : 2;
            tbody.innerHTML = `<tr><td colspan="${colSpan}">Kayıt yok.</td></tr>`;
            return;
        }

        // Determine type for modal title
        let type = 'Brand';
        if (endpoint === 'categories') type = 'Category';
        if (endpoint === 'units') type = 'Unit';

        data.forEach(item => {
            let extra = '';
            let extraVal = '';
            if (endpoint === 'units') {
                extra = `<td>${item.unitShortName || ''}</td>`;
                extraVal = item.unitShortName || '';
            }

            tbody.innerHTML += `
                <tr>
                    <td>${item[nameField]}</td>
                    ${extra}
                    <td style="text-align:right;">
                        <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; margin-right:0.25rem;" 
                            onclick="openSimpleModal('${type}', ${item.id}, '${item[nameField]}', '${extraVal}')">Düzenle</button>
                        <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; background:var(--error);" 
                            onclick="deleteSimple('${endpoint}', ${item.id})">Sil</button>
                    </td>
                </tr>`;
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
            alert(id ? 'Güncellendi.' : 'Kaydedildi.');
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
    tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">Yükleniyor...</td></tr>';
    try {
        const res = await fetch(`${API_BASE_URL}/api/supplier`);
        const data = await res.json();
        tbody.innerHTML = '';
        if (data.length === 0) { tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">Kayıt yok.</td></tr>'; return; }

        data.forEach(s => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><strong>${s.supplierCompanyName}</strong></td>
                <td>${s.supplierContactName || '-'}</td>
                <td>${s.supplierContactMail || '-'}</td>
                <td>${s.supplierAddress || '-'}</td>
                <td style="text-align:right;">
                    <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; margin-right:0.25rem;" 
                        onclick="openSupplierModal(${s.id}, \`${s.supplierCompanyName}\`, \`${s.supplierContactName}\`, \`${s.supplierContactMail}\`, \`${s.supplierAddress}\`)">Düzenle</button>
                    <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; background:var(--error);" 
                        onclick="deleteSupplier(${s.id}, '${s.supplierCompanyName}')">Sil</button>
                </td>`;
            tbody.appendChild(tr);
        });
    } catch (e) { console.error(e); tbody.innerHTML = '<tr><td colspan="5" style="color:var(--error); text-align:center;">Hata oluştu!</td></tr>'; }
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
    tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">Yükleniyor...</td></tr>';
    try {
        const res = await fetch(`${API_BASE_URL}/api/pricelist`);
        const data = await res.json();
        tbody.innerHTML = '';
        if (data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">Henüz fiyat kaydı yok.</td></tr>';
            return;
        }

        data.forEach(item => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><strong>${item.productCode} - ${item.productName}</strong></td>
                <td>${item.supplierName || '-'}</td>
                <td>${item.purchasePrice}</td>
                <td>${item.salePrice}</td>
                <td>${item.currency}</td>
                <td><span class="badge" style="background:${item.isActivePrice ? '#d1fae5;color:#065f46;' : '#f3f4f6; color:#6b7280;'}">${item.isActivePrice ? 'Aktif' : 'Pasif'}</span></td>
                <td style="text-align:right;">
                    <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; margin-right:0.25rem;" 
                        onclick="openPriceListModal(${item.id})">Düzenle</button>
                    <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; background:var(--error);" 
                        onclick="deletePriceList(${item.id})">Sil</button>
                </td>`;
            tbody.appendChild(tr);
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
        const [products, suppliers] = await Promise.all([
            fetch(`${API_BASE_URL}/api/product`).then(r => r.json()),
            fetch(`${API_BASE_URL}/api/supplier`).then(r => r.json())
        ]);

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
    tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;">Yükleniyor...</td></tr>';
    try {
        const res = await fetch(`${API_BASE_URL}/api/customer/companies`);
        const data = await res.json();
        tbody.innerHTML = '';
        if (data.length === 0) { tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;">Kayıt yok.</td></tr>'; return; }

        data.forEach(item => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><strong>${item.customerCompanyName}</strong></td>
                <td>${item.customerCompanyMail || '-'}</td>
                <td>${item.customerCompanyAddress || '-'}</td>
                <td style="text-align:right;">
                    <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; margin-right:0.25rem;" 
                        onclick="openCustomerCompanyModal(${item.id}, \`${item.customerCompanyName}\`, \`${item.customerCompanyMail}\`, \`${item.customerCompanyAddress}\`)">Düzenle</button>
                    <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; background:var(--error);" 
                        onclick="deleteCustomerCompany(${item.id}, '${item.customerCompanyName}')">Sil</button>
                </td>`;
            tbody.appendChild(tr);
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
    tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">Yükleniyor...</td></tr>';
    try {
        const res = await fetch(`${API_BASE_URL}/api/customer`);
        const data = await res.json();
        tbody.innerHTML = '';
        if (data.length === 0) { tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">Kayıt yok.</td></tr>'; return; }

        data.forEach(item => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><strong>${item.customerContactPersonName} ${item.customerContactPersonLastName}</strong></td>
                <td>${item.customerCompanyName || '-'}</td>
                <td>${item.customerContactPersonMobilPhone || '-'}</td>
                <td>${item.customerContactPersonMail || '-'}</td>
                <td style="text-align:right;">
                    <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; margin-right:0.25rem;" 
                        onclick="openCustomerModal(${item.id})">Düzenle</button>
                    <button class="btn-primary" style="padding:0.25rem 0.5rem; font-size:0.8rem; width:auto; background:var(--error);" 
                        onclick="deleteCustomer(${item.id}, '${item.customerContactPersonName}')">Sil</button>
                </td>`;
            tbody.appendChild(tr);
        });
    } catch (e) { console.error(e); tbody.innerHTML = '<tr><td colspan="5" style="color:var(--error); text-align:center;">Hata oluştu!</td></tr>'; }
}

window.openCustomerModal = async function (id = null) {
    document.getElementById('customerModal').style.display = 'flex';
    document.querySelector('#customerModal h3').innerText = id ? 'Müşteri Düzenle' : 'Yeni Müşteri';
    document.getElementById('custId').value = id || '';

    const compSel = document.getElementById('custCompany');
    compSel.innerHTML = '<option value="">Yükleniyor...</option>';

    try {
        console.log("Fetching customer companies...");
        const res = await fetch(`${API_BASE_URL}/api/customer/companies`);
        if (!res.ok) throw new Error("Şirket listesi alınamadı: " + res.status);
        const companies = await res.json();
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
    categories: []
};

window.startOfferWizard = async function () {
    wizardState = { step: 1, items: [], customers: [], warehouses: [], categories: [] };
    document.getElementById('offerWizardModal').style.display = 'flex';
    wizardNext(1);

    // Load lists for Step 1
    const [resCust, resWh, resCat] = await Promise.all([
        fetch(`${API_BASE_URL}/api/customer`), // Fetch Customers (Contacts) instead of Companies
        fetch(`${API_BASE_URL}/api/warehouse`),
        fetch(`${API_BASE_URL}/api/product/categories`)
    ]);

    wizardState.customers = await resCust.json();
    wizardState.warehouses = await resWh.json();
    wizardState.categories = await resCat.json();

    const mSel = document.getElementById('wMusteri');
    const dSel = document.getElementById('wDepo');
    const kSel = document.getElementById('wKategori');

    // Display Company Name - Contact Person
    mSel.innerHTML = '<option value="">Müşteri Seçin...</option>' + wizardState.customers.map(c => `<option value="${c.id}">${c.customerCompanyName} - ${c.fullName || c.customerContactPersonName}</option>`).join('');
    dSel.innerHTML = '<option value="">Depo Filtresi (Opsiyonel)</option>' + wizardState.warehouses.map(w => `<option value="${w.id}">${w.warehouseName}</option>`).join('');
    kSel.innerHTML = '<option value="">Kategori Filtresi (Opsiyonel)</option>' + wizardState.categories.map(k => `<option value="${k.id}">${k.categoryName}</option>`).join('');
}

window.wizardNext = function (step) {
    if (wizardState.step === 1 && step > 1) {
        wizardState.customerId = document.getElementById('wMusteri').value;
        if (!wizardState.customerId) return alert('Lütfen müşteri seçin.');
        wizardState.warehouseId = document.getElementById('wDepo').value;
        wizardState.categoryId = document.getElementById('wKategori').value;
    }

    if (wizardState.step === 2 && step > 2) {
        if (wizardState.items.length === 0) return alert('Lütfen en az bir ürün ekleyin.');
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
    tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const res = await fetch(`${API_BASE_URL}/api/product`);
        let products = await res.json();
        if (wizardState.categoryId) products = products.filter(p => p.categoryId == wizardState.categoryId);

        tbody.innerHTML = '';
        products.forEach(p => {
            const tr = document.createElement('tr');
            // ProductDto fields: currentStock, currentPrice, unitName
            tr.innerHTML = `
                <td>${p.productName} <br><small>${p.productCode}</small></td>
                <td><span class="badge ${p.currentStock > 0 ? 'status-online' : 'status-offline'}" style="background:none; color:inherit;">${p.currentStock || 0}</span></td>
                <td>${p.unitName || 'Adet'}</td>
                <td><input type="number" id="qty-${p.id}" value="1" min="1" style="width:60px; padding:0.25rem; border:1px solid #ddd; border-radius:4px;"></td>
                <td style="text-align:right;">
                    <button class="btn-primary" style="padding:0.25rem 0.6rem; font-size:0.8rem; width:auto;" 
                        onclick="addToWizardOffer(${p.id}, '${p.productName.replace(/'/g, "\\'")}', '${p.productCode}', ${p.currentStock || 0}, '${p.unitName || 'Adet'}', ${p.currentPrice || 0})">+</button>
                </td>
            `;
            tbody.appendChild(tr);
        });
    } catch (e) { console.error(e); }
}

window.addToWizardOffer = function (id, name, code, stock, unit, price) {
    const qtyInput = document.getElementById(`qty-${id}`);
    const qty = parseFloat(qtyInput.value) || 1;
    const existing = wizardState.items.find(i => i.id === id);
    if (existing) {
        existing.quantity += qty;
    } else {
        wizardState.items.push({ id, name, code, quantity: qty, price: price, discount: 0, unit, stock });
    }
    alert(`${name} listeye eklendi.`);
    qtyInput.value = 1; // reset
}

function loadWizardPricing() {
    const tbody = document.getElementById('wizardPricingList');
    tbody.innerHTML = '';

    wizardState.items.forEach(i => {
        const tr = document.createElement('tr');
        const netPrice = (i.price * (1 - i.discount / 100)).toFixed(2);
        tr.innerHTML = `
            <td><strong>${i.name}</strong><br><small>${i.code}</small></td>
            <td>${i.quantity} ${i.unit}</td>
            <td>
                <div style="font-size:0.75rem; color:var(--muted); margin-bottom:2px;">Liste: ${i.price.toLocaleString()} ₺</div>
                <input type="number" step="0.01" value="${i.price}" onchange="updateWizardItem(${i.id}, 'price', this.value)" style="width:90px; padding:0.25rem; border:1px solid #ddd; border-radius:4px;">
                <div style="font-size:0.75rem; color:var(--primary); margin-top:2px;">Net: <span id="net-price-${i.id}">${parseFloat(netPrice).toLocaleString()}</span> ₺</div>
            </td>
            <td><input type="number" step="1" value="${i.discount}" onchange="updateWizardItem(${i.id}, 'discount', this.value)" style="width:60px; padding:0.25rem; border:1px solid #ddd; border-radius:4px;"> %</td>
            <td id="total-${i.id}" style="font-weight:bold;">${(i.quantity * i.price * (1 - i.discount / 100)).toLocaleString()} ₺</td>
            <td style="text-align:right;"><button class="btn-primary" style="background:var(--error); width:auto; padding:0.25rem 0.5rem;" onclick="removeFromWizardOffer(${i.id})">❌</button></td>
        `;
        tbody.appendChild(tr);
    });
}

window.updateWizardItem = function (id, field, val) {
    const item = wizardState.items.find(i => i.id === id);
    if (item) {
        item[field] = parseFloat(val) || 0;
        const totalEl = document.getElementById(`total-${id}`);
        if (totalEl) {
            totalEl.innerText = (item.quantity * item.price * (1 - item.discount / 100)).toLocaleString() + ' ₺';
        }
        const netPriceEl = document.getElementById(`net-price-${id}`);
        if (netPriceEl) {
            netPriceEl.innerText = (item.price * (1 - item.discount / 100)).toLocaleString();
        }
    }
}
window.removeFromWizardOffer = function (id) {
    wizardState.items = wizardState.items.filter(i => i.id !== id);
    loadWizardPricing();
}

function prepareOfferPreview() {
    const area = document.getElementById('offerPreviewArea');
    const customer = wizardState.customers.find(c => c.id == wizardState.customerId);

    let subTotal = 0;
    let itemsHtml = wizardState.items.map(i => {
        const lineTotal = i.quantity * i.price * (1 - i.discount / 100);
        subTotal += lineTotal;
        return `<tr>
            <td style="padding:0.5rem; border-bottom:1px solid #eee;">${i.code}</td>
            <td style="padding:0.5rem; border-bottom:1px solid #eee;">${i.name}</td>
            <td style="padding:0.5rem; border-bottom:1px solid #eee;">${i.quantity} ${i.unit}</td>
            <td style="padding:0.5rem; border-bottom:1px solid #eee;">${i.price.toLocaleString()} ₺</td>
            <td style="padding:0.5rem; border-bottom:1px solid #eee;">%${i.discount}</td>
            <td style="padding:0.5rem; border-bottom:1px solid #eee;">${lineTotal.toLocaleString()} ₺</td>
        </tr>`;
    }).join('');

    const vat = subTotal * 0.20;
    const grandTotal = subTotal + vat;

    area.innerHTML = `
        <div style="display:flex; justify-content:space-between; margin-bottom:2rem;">
            <div>
                <h2 style="margin:0; color:var(--primary);">TEKLİF FORMU</h2>
                <p><strong>Firma:</strong> ${customer ? customer.customerCompanyName : 'Seçilmedi'}</p>
                <p><strong>Yetkili:</strong> ${customer ? (customer.fullName || customer.customerContactPersonName) : '-'}</p>
            </div>
            <div style="text-align:right;">
                <p><strong>Tarih:</strong> ${new Date().toLocaleDateString()}</p>
                <p><strong>Geçerlilik:</strong> 15 Gün</p>
            </div>
        </div>
        <table style="width:100%; border-collapse:collapse; margin-bottom:2rem;">
            <thead>
                <tr style="background:#f3f4f6;">
                    <th style="text-align:left; padding:0.5rem;">Kod</th>
                    <th style="text-align:left; padding:0.5rem;">Açıklama</th>
                    <th style="text-align:left; padding:0.5rem;">Miktar</th>
                    <th style="text-align:left; padding:0.5rem;">Birim Fiyat</th>
                    <th style="text-align:left; padding:0.5rem;">İndirim</th>
                    <th style="text-align:left; padding:0.5rem;">Toplam</th>
                </tr>
            </thead>
            <tbody>
                ${itemsHtml}
            </tbody>
        </table>
        <div style="display:flex; justify-content:flex-end;">
            <div style="width:250px;">
                <div style="display:flex; justify-content:space-between;"><span>Ara Toplam:</span> <span>${subTotal.toLocaleString()} ₺</span></div>
                <div style="display:flex; justify-content:space-between;"><span>KDV (%20):</span> <span>${vat.toLocaleString()} ₺</span></div>
                <div style="display:flex; justify-content:space-between; font-weight:bold; margin-top:0.5rem; border-top:2px solid #333; padding-top:0.5rem;">
                    <span>GENEL TOPLAM:</span> <span>${grandTotal.toLocaleString()} ₺</span>
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
            discountRate: i.discount
        }))
    };

    try {
        const res = await fetch(`${API_BASE_URL}/api/offer`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        });

        if (res.ok) {
            alert('Teklif başarıyla oluşturuldu. ✅');
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
        win.document.write(`<html><head><title>Teklif</title><style>body{font-family:sans-serif; padding:20px;}</style></head><body>${area.innerHTML}</body></html>`);
        win.document.close();
        win.print();
    } else {
        let csv = "Kod,Aciklama,Miktar,Birim Fiyat,Indirim,Toplam\n";
        wizardState.items.forEach(i => {
            csv += `"${i.code}","${i.name}","${i.quantity}","${i.price}","${i.discount}","${i.quantity * i.price * (1 - i.discount / 100)}"\n`;
        });
        const blob = new Blob([csv], { type: 'text/csv' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `teklif_${new Date().getTime()}.csv`;
        a.click();
    }
}
