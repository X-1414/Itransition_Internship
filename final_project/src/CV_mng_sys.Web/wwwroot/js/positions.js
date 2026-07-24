let selectedRow = null;
const btnEdit = document.getElementById('btnEdit');
const btnDelete = document.getElementById('btnDelete');
const btnDuplicate = document.getElementById('btnDuplicate');
const btnNew = document.getElementById('btnNew');
const btnSave = document.getElementById('btnSave');
const modal = new bootstrap.Modal(document.getElementById('positionModal'));

document.querySelectorAll('.position-row').forEach(row => {
    row.querySelector('.row-select')?.addEventListener('click', (e) => {
        e.stopPropagation();
        document.querySelectorAll('.position-row').forEach(r => r.classList.remove('table-active'));
        row.classList.add('table-active');
        selectedRow = row;
        btnEdit.disabled = false;
        btnDelete.disabled = false;
        btnDuplicate.disabled = false;
    });
});

btnNew.addEventListener('click', () => {
    document.getElementById('modalTitle').textContent = 'New Position';
    document.getElementById('posId').value = '';
    document.getElementById('posVersion').value = '';
    document.getElementById('posTitle').value = '';
    document.getElementById('posDescription').value = '';
    document.getElementById('modalError').classList.add('d-none');
    modal.show();
});

btnEdit.addEventListener('click', () => {
    if (!selectedRow) return;
    document.getElementById('modalTitle').textContent = 'Edit Position';
    document.getElementById('posId').value = selectedRow.dataset.id;
    document.getElementById('posVersion').value = selectedRow.dataset.version;
    document.getElementById('posTitle').value = selectedRow.dataset.title;
    document.getElementById('posDescription').value = selectedRow.dataset.description || '';
    document.getElementById('modalError').classList.add('d-none');
    modal.show();
});

btnSave.addEventListener('click', async () => {
    const id = document.getElementById('posId').value;
    const version = document.getElementById('posVersion').value;
    const title = document.getElementById('posTitle').value;
    const description = document.getElementById('posDescription').value;
    const errorBox = document.getElementById('modalError');

    const isEdit = !!id;
    const url = isEdit ? '/Positions/Update' : '/Positions/Create';
    const body = new URLSearchParams({ title, description });
    if (isEdit) { body.append('id', id); body.append('expectedVersion', version); }

    const response = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body });
    if (response.status === 409) {
        const data = await response.json();
        errorBox.textContent = data.error;
        errorBox.classList.remove('d-none');
        return;
    }
    if (!response.ok) { errorBox.textContent = 'Error saving position.'; errorBox.classList.remove('d-none'); return; }
    location.reload();
});

btnDuplicate.addEventListener('click', async () => {
    if (!selectedRow) return;
    const body = new URLSearchParams({ id: selectedRow.dataset.id });
    await fetch('/Positions/Duplicate', { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body });
    location.reload();
});

btnDelete.addEventListener('click', async () => {
    if (!selectedRow) return;
    if (!confirm(`Delete position "${selectedRow.dataset.title}"?`)) return;
    const body = new URLSearchParams({ id: selectedRow.dataset.id, expectedVersion: selectedRow.dataset.version });
    const response = await fetch('/Positions/Delete', { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body });
    if (response.status === 409) { const data = await response.json(); alert(data.error); return; }
    location.reload();
});