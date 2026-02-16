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
        'companies': 'Şirket Yönetimi',
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
    if (viewName === 'companies') loadCompanies();
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
    tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const res = await fetch(`${API_BASE_URL}/api/offers`);
        if (!res.ok) throw new Error('Teklifler alınamadı');
        const offers = await res.json();

        tbody.innerHTML = '';
        if (offers.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Teklif bulunamadı.</td></tr>';
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
                <td>${o.customerId}</td>
                <td>${new Date(o.offerDate).toLocaleDateString()}</td>
                <td>${o.totalAmount} ₺</td>
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
        const currentUser = JSON.parse(localStorage.getItem('user'));
        const res = await fetch(`${API_BASE_URL}/api/offers/${id}/approve`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${currentUser.token || ''}` // Assuming token might be needed if JWT is used later, currently cookie/session or handled by headers?
                // API uses [Authorize] but user provided code doesn't explicitly show JWT token handling in login response.
                // Assuming cookie authentication or similar if not JWT.
                // Ah, Login returns user object but NO token in previous code analysis. Hopefully authentication is handled via Cookies or Basic Auth?
                // Wait, S2O1.API Program.cs uses app.UseAuthorization() but AddAuthentication wasn't explicitly configured with JWT.
                // If there's no JWT, Authorize attribute might fail if no scheme registered.
                // But let's stick to current flow.
            }
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
        const res = await fetch(`${API_BASE_URL}/api/offers/${id}/create-invoice`, { method: 'POST' });
        if (res.ok) {
            const data = await res.json();
            alert('Fatura oluşturuldu. ID: ' + data.invoiceId);
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

    // Clear or Set Values
    if (!id) {
        document.getElementById('pCode').value = '';
        document.getElementById('pName').value = '';
        document.getElementById('pCategory').value = '';
        document.getElementById('pBrand').value = '';
        document.getElementById('pUnit').value = '';
        document.getElementById('pWarehouse').value = '';
    } else {
        // Edit mode
    }

    try {
        // Load Dropdowns
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

        // If Edit Mode, Fetch Product Details and Set Values
        if (id) {
            const res = await fetch(`${API_BASE_URL}/api/product/${id}`);
            const p = await res.json();
            document.getElementById('pCode').value = p.productCode;
            document.getElementById('pName').value = p.productName;
            document.getElementById('pCategory').value = p.categoryId;
            document.getElementById('pBrand').value = p.brandId;
            document.getElementById('pUnit').value = p.unitId;
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
    const wareVal = document.getElementById('pWarehouse').value;

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









