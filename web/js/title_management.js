
// ============================================
// TITLE / DEPARTMENT MANAGEMENT
// ============================================

window.openTitleModal = async function (id) {
    const modal = document.getElementById('titleModal');
    if (!modal) return;

    // Reset Form
    document.getElementById('tId').value = '';
    document.getElementById('tName').value = '';
    const companySelect = document.getElementById('tCompany');
    if (companySelect) companySelect.value = '';

    // Load Companies
    if (companySelect) {
        companySelect.innerHTML = '<option value="">Yükleniyor...</option>';
        try {
            const res = await fetch(`${API_BASE_URL}/api/companies`);
            if (res.ok) {
                const companies = await res.json();
                companySelect.innerHTML = '<option value="">Şirket Seçin...</option>';
                companies.forEach(c => {
                    companySelect.innerHTML += `<option value="${c.id}">${c.companyName}</option>`;
                });

                // If editing, fetch single title
                if (id) {
                    try {
                        const tRes = await fetch(`${API_BASE_URL}/api/users/titles/${id}`);
                        if (tRes.ok) {
                            const t = await tRes.json();
                            document.getElementById('tId').value = t.id;
                            document.getElementById('tName').value = t.titleName;
                            document.getElementById('tCompany').value = t.companyId;
                        }
                    } catch (e) { console.error("Title load failed", e); }
                }
            } else {
                companySelect.innerHTML = '<option value="">Hata!</option>';
            }
        } catch (e) {
            console.error(e);
            companySelect.innerHTML = '<option value="">Bağlantı hatası!</option>';
        }
    }

    modal.style.display = 'flex';
}

window.saveTitle = async function () {
    const id = document.getElementById('tId').value;
    const name = document.getElementById('tName').value;
    const companyId = document.getElementById('tCompany').value;

    if (!name || !companyId) {
        alert('Lütfen Ünvan/Bölüm Adı ve Şirket seçiniz.');
        return;
    }

    const payload = {
        titleName: name,
        companyId: parseInt(companyId)
    };

    try {
        const url = id ? `${API_BASE_URL}/api/users/titles/${id}` : `${API_BASE_URL}/api/users/titles`;
        const method = id ? 'PUT' : 'POST';

        const res = await fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            alert(id ? 'Ünvan başarıyla güncellendi.' : 'Ünvan/Bölüm başarıyla kaydedildi.');
            closeModal('titleModal');
            loadTitles(); // Refresh list
        } else {
            const err = await res.json();
            alert('Hata: ' + (err.message || 'Kaydedilemedi.'));
        }
    } catch (e) {
        alert('Bir hata oluştu: ' + e.message);
    }
}

window.loadTitles = async function () {
    const tbody = document.getElementById('titleListBody');
    if (!tbody) return;

    tbody.innerHTML = '<tr><td colspan="3" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const [titlesRes, companiesRes] = await Promise.all([
            fetch(`${API_BASE_URL}/api/users/titles`),
            fetch(`${API_BASE_URL}/api/companies`)
        ]);

        if (titlesRes.ok && companiesRes.ok) {
            const titles = await titlesRes.json();
            const companies = await companiesRes.json();
            const companyMap = {};
            companies.forEach(c => companyMap[c.id] = c.companyName);

            if (titles.length === 0) {
                tbody.innerHTML = '<tr><td colspan="3" style="text-align:center;">Kayıt bulunamadı.</td></tr>';
                return;
            }

            tbody.innerHTML = '';
            titles.forEach(t => {
                const companyName = companyMap[t.companyId] || 'Bilinmiyor';
                tbody.innerHTML += `
                    <tr>
                        <td>${t.titleName}</td>
                        <td>${companyName}</td>
                        <td style="text-align:right;">
                            <div class="action-btn-container">
                                <button class="btn-action btn-edit" onclick="openTitleModal(${t.id})">✏️ Düzenle</button>
                                <button class="btn-action" style="background:var(--primary); color:white;" onclick="openTitlePermissionsModal(${t.id}, '${t.titleName}')">🔑 Yetkiler</button>
                                <button class="btn-action btn-delete" onclick="deleteTitle(${t.id})">🗑️ Sil</button>
                            </div>
                        </td>
                    </tr>
                `;
            });
        } else {
            tbody.innerHTML = '<tr><td colspan="3" style="text-align:center; color:red;">Veriler yüklenemedi.</td></tr>';
        }
    } catch (e) {
        console.error(e);
        tbody.innerHTML = `<tr><td colspan="3" style="text-align:center; color:red;">Hata: ${e.message}</td></tr>`;
    }
}

window.deleteTitle = async function (id) {
    if (!await showConfirm('Silmek İstiyor musunuz?', 'Bu ünvan/bölüm silinecektir.')) return;

    try {
        const res = await fetch(`${API_BASE_URL}/api/users/titles/${id}`, {
            method: 'DELETE'
        });

        if (res.ok) {
            alert('Silindi.');
            loadTitles();
        } else {
            alert('Silinemedi.');
        }
    } catch (e) {
        alert('Hata: ' + e.message);
    }
}

// Title Permissions
window.openTitlePermissionsModal = async function (id, name) {
    const modal = document.getElementById('titlePermissionsModal');
    if (!modal) return;

    document.getElementById('tpTitleId').value = id;
    document.getElementById('tpTitleName').innerText = name;

    const tbody = document.getElementById('tpListBody');
    tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;">Yükleniyor...</td></tr>';

    modal.style.display = 'flex';

    try {
        const res = await fetch(`${API_BASE_URL}/api/users/titles/${id}/permissions`);
        if (res.ok) {
            const perms = await res.json();
            tbody.innerHTML = '';
            perms.forEach((p, index) => {
                tbody.innerHTML += `
                    <tr>
                        <td style="font-weight:600;">${p.moduleName}</td>
                        <td style="text-align:center;"><input type="checkbox" class="tp-full" data-idx="${index}" ${p.isFull ? 'checked' : ''}></td>
                        <td style="text-align:center;"><input type="checkbox" class="tp-read" data-idx="${index}" ${p.canRead ? 'checked' : ''}></td>
                        <td style="text-align:center;"><input type="checkbox" class="tp-write" data-idx="${index}" ${p.canWrite ? 'checked' : ''}></td>
                        <td style="text-align:center;"><input type="checkbox" class="tp-delete" data-idx="${index}" ${p.canDelete ? 'checked' : ''}>
                            <input type="hidden" class="tp-mod-id" value="${p.moduleId}">
                        </td>
                    </tr>
                `;
            });

            // Add event listeners for auto-checking dependencies
            tbody.querySelectorAll('.tp-full').forEach(chk => {
                chk.addEventListener('change', function () {
                    if (this.checked) {
                        const idx = this.getAttribute('data-idx');
                        tbody.querySelector(`.tp-read[data-idx="${idx}"]`).checked = true;
                        tbody.querySelector(`.tp-write[data-idx="${idx}"]`).checked = true;
                        tbody.querySelector(`.tp-delete[data-idx="${idx}"]`).checked = true;
                    }
                });
            });

            tbody.querySelectorAll('.tp-write, .tp-delete').forEach(chk => {
                chk.addEventListener('change', function () {
                    if (this.checked) {
                        const idx = this.getAttribute('data-idx');
                        tbody.querySelector(`.tp-read[data-idx="${idx}"]`).checked = true;
                    }
                });
            });

        } else {
            tbody.innerHTML = '<tr><td colspan="5" style="text-align:center; color:red;">Yetkiler yüklenemedi.</td></tr>';
        }
    } catch (e) {
        tbody.innerHTML = `<tr><td colspan="5" style="text-align:center; color:red;">Hata: ${e.message}</td></tr>`;
    }
}

window.saveTitlePermissions = async function () {
    const titleId = document.getElementById('tpTitleId').value;
    const tbody = document.getElementById('tpListBody');
    const rows = tbody.querySelectorAll('tr');

    const permissions = [];
    rows.forEach((row, index) => {
        const modId = row.querySelector('.tp-mod-id').value;
        const full = row.querySelector('.tp-full').checked;
        const read = row.querySelector('.tp-read').checked;
        const write = row.querySelector('.tp-write').checked;
        const del = row.querySelector('.tp-delete').checked;

        permissions.push({
            moduleId: parseInt(modId),
            isFull: full,
            canRead: read,
            canWrite: write,
            canDelete: del
        });
    });

    try {
        const res = await fetch(`${API_BASE_URL}/api/users/titles/${titleId}/permissions`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(permissions)
        });

        if (res.ok) {
            alert('Ünvan yetkileri başarıyla güncellendi.');
            closeModal('titlePermissionsModal');
        } else {
            const err = await res.json();
            alert('Hata: ' + (err.message || 'Güncellenemedi.'));
        }
    } catch (e) {
        alert('Hata: ' + e.message);
    }
}
