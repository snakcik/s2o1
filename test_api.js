const fetch = require('node-fetch');
async function test() {
    try {
        const res = await fetch('http://localhost:5267/api/customer/companies');
        const data = await res.json();
        console.log(JSON.stringify(data, null, 2));
    } catch (e) {
        console.error(e);
    }
}
test();
