const https = require('https');
// If using Fetch API (Node 18+)
// Ignore self-signed certificate errors for local dev
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const BASE_URL = 'http://localhost:5267/api/auth';

async function testApi() {
    console.log('--- API Test Başlatılıyor ---');

    // 1. Register User
    const newUser = {
        userName: `user_${Date.now()}`,
        password: 'Password123!',
        firstName: 'Test',
        lastName: 'User',
        email: 'test@example.com',
        roleId: 2 // Assuming 2 is User role
    };

    console.log(`\n1. Kayıt Olunuyor: ${newUser.userName}`);
    try {
        const registerRes = await fetch(`${BASE_URL}/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newUser)
        });

        if (!registerRes.ok) {
            const err = await registerRes.text();
            throw new Error(`Register Failed: ${err}`);
        }

        const registeredUser = await registerRes.json();
        console.log('✅ Kayıt Başarılı:', registeredUser);

        // 2. Login User
        console.log(`\n2. Giriş Yapılıyor: ${newUser.userName}`);
        const loginRes = await fetch(`${BASE_URL}/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userName: newUser.userName, password: newUser.password })
        });

        if (!loginRes.ok) {
            const err = await loginRes.text();
            throw new Error(`Login Failed: ${err}`);
        }

        const loginData = await loginRes.json();
        console.log('✅ Giriş Başarılı:', loginData);

        // 3. Assign Role (Simulating Admin action)
        // Note: In real world, this endpoint should be protected and only accessible by Admin
        console.log('\n3. Rol Atanıyor (Admin Action)...');
        // Let's try assigning role 1 (Admin/Root) to this new user
        const assignRoleRes = await fetch(`${BASE_URL}/role`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId: registeredUser.id, roleId: 1 })
        });

        if (assignRoleRes.ok) {
            console.log('✅ Rol Ataması Başarılı!');
        } else {
            console.log('❌ Rol Ataması Başarısız:', await assignRoleRes.text());
        }

    } catch (error) {
        console.error('🛑 HATA:', error.message);
    }
}

testApi();
