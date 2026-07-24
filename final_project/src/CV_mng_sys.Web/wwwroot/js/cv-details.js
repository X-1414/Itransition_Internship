const debounceTimers = new Map();
const cvId = document.getElementById('cvActions')?.dataset.cvId;

function scheduleSave(row) {
    const attrId = row.dataset.attrId;
    clearTimeout(debounceTimers.get(attrId));
    const timer = setTimeout(() => saveRow(row), 1500);
    debounceTimers.set(attrId, timer);
}

async function saveRow(row) {
    const attrId = row.dataset.attrId;
    const version = row.dataset.version;
    const input = row.querySelector('.cv-value');
    const statusCell = row.querySelector('.save-status');
    let value = input.type === 'checkbox' ? String(input.checked) : input.value;

    statusCell.textContent = 'Saving...';

    const body = new URLSearchParams({ cvId, attributeDefinitionId: attrId, value: value ?? '', expectedVersion: version });

    try {
        const response = await fetch('/Cv/SetValue', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body
        });

        if (response.status === 409) {
            const data = await response.json();
            alert(data.error + ' Reloading...');
            location.reload();
            return;
        }
        if (!response.ok) {
            let msg = 'Error';
            try { const data = await response.json(); if (data.error) msg = data.error; } catch {}
            statusCell.textContent = msg;
            statusCell.classList.add('text-danger');
            return;
        }

        const data = await response.json();
        row.dataset.version = data.newVersion;
        row.classList.remove('table-danger'); 
        statusCell.textContent = 'Saved';
        setTimeout(() => location.reload(), 600); 
    } catch (err) {
        statusCell.textContent = 'Network error';
        statusCell.classList.add('text-danger');
    }
}

document.querySelectorAll('.cv-value').forEach(input => {
    const row = input.closest('tr');
    const eventName = (input.tagName === 'SELECT' || input.type === 'checkbox') ? 'change' : 'input';
    input.addEventListener(eventName, () => scheduleSave(row));
});

document.getElementById('btnPublish')?.addEventListener('click', async () => {
    const version = document.getElementById('cvActions').dataset.version;
    const response = await fetch('/Cv/Publish', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ id: cvId, expectedVersion: version })
    });
    if (response.status === 409) { const data = await response.json(); alert(data.error); return; }
    if (!response.ok) { alert('Could not publish - check all required fields are filled.'); return; }
    location.reload();
});

document.getElementById('btnUnpublish')?.addEventListener('click', async () => {
    const version = document.getElementById('cvActions').dataset.version;
    const response = await fetch('/Cv/Unpublish', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ id: cvId, expectedVersion: version })
    });
    if (response.status === 409) { const data = await response.json(); alert(data.error); return; }
    location.reload();
});