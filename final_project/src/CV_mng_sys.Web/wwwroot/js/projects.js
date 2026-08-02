let selectedRow = null;
const btnEdit = document.getElementById('btnEdit');
const btnDelete = document.getElementById('btnDelete');
const btnNew = document.getElementById('btnNew');
const btnSave = document.getElementById('btnSave');
const modal = new bootstrap.Modal(document.getElementById('projectModal'));

document.querySelectorAll('.project-row').forEach(row => {
    row.addEventListener('click', () => {
        document.querySelectorAll('.project-row').forEach(r => r.classList.remove('table-active'));
        document.querySelectorAll('.row-select').forEach(r => r.checked = false); 
        row.classList.add('table-active');
        row.querySelector('.row-select').checked = true;
        selectedRow = row;
        btnEdit.disabled = false;
        btnDelete.disabled = false;
    });
});

let allTags = [];
fetch('/Projects/TagsJson').then(r=>r.json()).then(tags=>{
    allTags = tags;
});
const tagsInput = document.getElementById('projTags');
const suggestionsBox = document.getElementById('tagSuggestions');

function getCurrentSegment(value) {
    const parts = value.split(',');
    return parts[parts.length - 1].trim().toLowerCase();
}

function showSuggestions() {
    const segment = getCurrentSegment(tagsInput.value);
    if (!segment) {
        suggestionsBox.style.display = 'none';
        return;
    }
    const alreadyChosen = tagsInput.value.split(',').map(t => t.trim().toLowerCase());
    const matches = allTags.filter(tag =>
        tag.toLowerCase().startsWith(segment) && !alreadyChosen.includes(tag.toLowerCase())
    ).slice(0, 6);

    if (matches.length === 0) {
        suggestionsBox.style.display = 'none';
        return;
    }
    suggestionsBox.innerHTML = '';
    matches.forEach(tag => {
        const item = document.createElement('button');
        item.type = 'button';
        item.className = 'list-group-item list-group-item-action py-1';
        item.textContent = tag;
        item.addEventListener('click', () => {
            const parts = tagsInput.value.split(',');
            parts[parts.length - 1] = ' ' + tag; // replace only the in-progress segment
            tagsInput.value = parts.join(',').replace(/^,\s*/, '').trim() + ', ';
            suggestionsBox.style.display = 'none';
            tagsInput.focus();
        });
        suggestionsBox.appendChild(item);
    });
    suggestionsBox.style.display = 'block';
}
tagsInput.addEventListener('input', showSuggestions);
tagsInput.addEventListener('focus', showSuggestions);
document.addEventListener('click', (e) => {
    if (!tagsInput.contains(e.target) && !suggestionsBox.contains(e.target)) {
        suggestionsBox.style.display = 'none';
    }
});


const descriptionInput = document.getElementById('projDescription');
const previewBox = document.getElementById('descriptionPreview');
descriptionInput.addEventListener('input', ()=> { previewBox.innerHTML=marked.parse(descriptionInput.value || '');});

btnNew.addEventListener('click', () => {
    document.getElementById('modalTitle').textContent = window.projectTexts.newProject;
    document.getElementById('projId').value = '';
    document.getElementById('projVersion').value = '';
    document.getElementById('projName').value = '';
    document.getElementById('projStart').value = '';
    document.getElementById('projEnd').value = '';
    document.getElementById('projTags').value = '';
    document.getElementById('projDescription').value = '';
    previewBox.innerHTML = '';
    document.getElementById('modalError').classList.add('d-none');
    modal.show();
});

btnEdit.addEventListener('click', () => {
    if (!selectedRow) return;
    document.getElementById('modalTitle').textContent = window.projectTexts.editProject;
    document.getElementById('projId').value = selectedRow.dataset.id;
    document.getElementById('projVersion').value = selectedRow.dataset.version;
    document.getElementById('projName').value = selectedRow.dataset.name;
    document.getElementById('projStart').value = selectedRow.dataset.start !== '' ? selectedRow.dataset.start : '';
    document.getElementById('projEnd').value = selectedRow.dataset.end !== '' ? selectedRow.dataset.end : '';
    document.getElementById('projTags').value = selectedRow.dataset.tags || '';
    document.getElementById('projDescription').value = selectedRow.dataset.description || '';
    previewBox.innerHTML = marked.parse(selectedRow.dataset.description || '');
    document.getElementById('modalError').classList.add('d-none');
    modal.show();
});

btnSave.addEventListener('click', async () => {
    const id = document.getElementById('projId').value;
    const version = document.getElementById('projVersion').value;
    const name = document.getElementById('projName').value;
    const startDate = document.getElementById('projStart').value;
    const endDate = document.getElementById('projEnd').value;
    const tagsRaw = document.getElementById('projTags').value;
    const descriptionMarkdown = document.getElementById('projDescription').value;
    const errorBox = document.getElementById('modalError');

    const isEdit = !!id;
    const url = isEdit ? '/Projects/Update' : '/Projects/Create';
    const body = new URLSearchParams({ name, startDate, endDate, tagsRaw, descriptionMarkdown });
    if (isEdit) { body.append('id', id); body.append('expectedVersion', version); }
    else if (window.__targetUserId){ body.append('userId', window.__targetUserId); }

    try {
        const response = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body });
        if (response.status === 409) {
            const data = await response.json();
            errorBox.textContent = data.error;
            errorBox.classList.remove('d-none');
            return;
        }
        if (!response.ok) {
            const data = await response.json().catch(() => ({}));
            errorBox.textContent = data.error || window.projectTexts.somethingWentWrong;
            errorBox.classList.remove('d-none');
            return;
        }
        location.reload();
    } catch (err) {
        errorBox.textContent = `${window.projectTexts.networkError}: ${err.message}`;
        errorBox.classList.remove('d-none');
    }
});

btnDelete.addEventListener('click', async () => {
    if (!selectedRow) return;
    const message = window.projectTexts.deleteConfirmation.replace("{0}", selectedRow.dataset.name);
    if (!confirm(message)) return;
    const body = new URLSearchParams({ id: selectedRow.dataset.id, expectedVersion: selectedRow.dataset.version });
    const response = await fetch('/Projects/Delete', { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body });
    if (response.status === 409 || !response.ok) {
        const data = await response.json().catch(() => ({}));
        alert(data.error || window.projectTexts.couldNotDelete);
        return;
    }
    location.reload();
});