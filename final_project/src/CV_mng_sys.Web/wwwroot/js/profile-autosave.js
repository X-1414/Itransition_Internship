const debounceTimers = new Map();

function scheduleSave(row) {
    const attrId = row.dataset.attrId;
    clearTimeout(debounceTimers.get(attrId));
    const timer = setTimeout(() => saveRow(row), 1500);
    debounceTimers.set(attrId, timer);
}

async function saveRow(row) {
    const attrId = row.dataset.attrId;
    const version = row.dataset.version;
    const input = row.querySelector('.profile-value');
    const statusCell = row.querySelector('.save-status');
    let value = input.type === 'checkbox' ? String(input.checked) : input.value;

    statusCell.textContent = 'Saving...';

    const body = new URLSearchParams({
        attributeDefinitionId: attrId,
        value: value ?? '',
        expectedVersion: version
    });

    try {
        const response = await fetch('/Profile/SetValue', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body
        });

        if (response.status === 409) {
            const data = await response.json();
            statusCell.textContent = 'Conflict!';
            statusCell.classList.add('text-danger');
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
        row.dataset.version = data.newVersion; // critical: update local version for next save
        statusCell.textContent = 'Saved';
        statusCell.classList.remove('text-danger');
        setTimeout(() => { statusCell.textContent = ''; }, 2000);
    } catch (err) {
        statusCell.textContent = 'Network error';
        statusCell.classList.add('text-danger');
    }
}

document.querySelectorAll('.profile-value').forEach(input => {
    const row = input.closest('tr');
    const eventName = (input.tagName === 'SELECT' || input.type === 'checkbox') ? 'change' : 'input';
    input.addEventListener(eventName, () => scheduleSave(row));
});