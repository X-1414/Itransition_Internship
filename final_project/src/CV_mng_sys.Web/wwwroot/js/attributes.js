let selectedRow = null;

const btnEdit = document.getElementById('btnEdit');
const btnDelete = document.getElementById('btnDelete');
const btnNew = document.getElementById('btnNew');
const btnSave = document.getElementById('btnSave');
const modalEl = document.getElementById('attributeModal');
const modal = new bootstrap.Modal(modalEl);

function selectRow(row) {
    document.querySelectorAll('.attribute-row').forEach(r => r.classList.remove('table-active'));
    document.querySelectorAll('.row-select').forEach(r => r.checked = false);
    row.classList.add('table-active');
    row.querySelector('.row-select').checked = true;
    selectedRow = row;
    btnEdit.disabled = false;
    btnDelete.disabled = false;
}

document.querySelectorAll('.attribute-row').forEach(row => {
    row.addEventListener('click', () => selectRow(row));
});

document.getElementById('attributeDataType').addEventListener('change', (e) => {
    document.getElementById('dropdownOptionsGroup').classList.toggle('d-none', e.target.value !== '2');
});

btnNew.addEventListener('click', () => {
    document.getElementById('modalTitle').textContent = 'New Attribute';
    document.getElementById('attributeId').value = '';
    document.getElementById('attributeVersion').value = '';
    document.getElementById('attributeName').value = '';
    document.getElementById('attributeDataType').value = '0';
    document.getElementById('attributeOptions').value = '';
    document.getElementById('dropdownOptionsGroup').classList.add('d-none');
    document.getElementById('modalError').classList.add('d-none');
    modal.show();
});

btnEdit.addEventListener('click', () => {
    if (!selectedRow) { alert('Select a row first.'); return; }
    document.getElementById('modalTitle').textContent = 'Edit Attribute';
    document.getElementById('attributeId').value = selectedRow.dataset.id;
    document.getElementById('attributeVersion').value = selectedRow.dataset.version;
    document.getElementById('attributeName').value = selectedRow.dataset.name;
    document.getElementById('attributeDataType').value = selectedRow.dataset.datatype;
    document.getElementById('attributeOptions').value = selectedRow.dataset.options || '';
    document.getElementById('dropdownOptionsGroup').classList.toggle('d-none', selectedRow.dataset.datatype !== '2');
    document.getElementById('modalError').classList.add('d-none');
    modal.show();
});

btnSave.addEventListener('click', async () => {
    const id = document.getElementById('attributeId').value;
    const version = document.getElementById('attributeVersion').value;
    const name = document.getElementById('attributeName').value;
    const dataType = document.getElementById('attributeDataType').value;
    const options = document.getElementById('attributeOptions').value;
    const errorBox = document.getElementById('modalError');

    const isEdit = !!id;
    const url = isEdit ? '/Attributes/Update' : '/Attributes/Create';
    const body = new URLSearchParams({ name, dataType, dropdownOptionsRaw: options });
    if (isEdit) {
        body.append('id', id);
        body.append('expectedVersion', version);
    }

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body
        });

        if (response.status === 409) {
            const data = await response.json();
            errorBox.textContent = data.error;
            errorBox.classList.remove('d-none');
            return;
        }
        if (!response.ok) {
            errorBox.textContent = 'Something went wrong. Please try again.';
            errorBox.classList.remove('d-none');
            return;
        }
        location.reload();
    } catch (err) {
        errorBox.textContent = 'Network error: ' + err.message;
        errorBox.classList.remove('d-none');
    }
});

btnDelete.addEventListener('click', async () => {
    if (!selectedRow) { alert('Select a row first.'); return; }
    if (!confirm(`Delete attribute "${selectedRow.dataset.name}"?`)) return;

    const body = new URLSearchParams({
        id: selectedRow.dataset.id,
        expectedVersion: selectedRow.dataset.version
    });

    try {
        const response = await fetch('/Attributes/Delete', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body
        });
        if (response.status === 409) {
            const data = await response.json();
            alert(data.error);
            return;
        }
        location.reload();
    } catch (err) {
        alert('Network error: ' + err.message);
    }
});