let selectedUserId = null;
let selectedBlocked = false;

const btnBlock = document.getElementById('btnBlock');
const btnUnblock = document.getElementById('btnUnblock');
const btnDelete = document.getElementById('btnDelete');
const btnAssignRole = document.getElementById('btnAssignRole');
const btnRemoveRole = document.getElementById('btnRemoveRole');
const roleSelect = document.getElementById('roleSelect');
const errorBox = document.getElementById('actionError');

document.querySelectorAll('.user-row').forEach(row => {
    row.addEventListener('click', () => {
        document.querySelectorAll('.user-row').forEach(r => r.classList.remove('table-active'));
        row.classList.add('table-active');
        row.querySelector('.row-select').checked = true;

        selectedUserId = row.dataset.id;
        selectedBlocked = row.dataset.blocked === 'true';

        btnBlock.disabled = selectedBlocked;
        btnUnblock.disabled = !selectedBlocked;
        btnDelete.disabled = false;
        roleSelect.disabled = false;
        btnAssignRole.disabled = false;
        btnRemoveRole.disabled = false;
        errorBox.classList.add('d-none');
    });
});

async function postAction(url, body) {
    const response = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body
    });
    if (!response.ok) {
        const data = await response.json().catch(() => ({}));
        errorBox.textContent = data.error || window.userTexts.actionFailed;
        errorBox.classList.remove('d-none');
        return false;
    }
    return true;
}

btnBlock.addEventListener('click', async () => {
    if (await postAction('/Admin/Block', new URLSearchParams({ userId: selectedUserId }))) location.reload();
});

btnUnblock.addEventListener('click', async () => {
    if (await postAction('/Admin/Unblock', new URLSearchParams({ userId: selectedUserId }))) location.reload();
});

btnDelete.addEventListener('click', async () => {
    if (!confirm(window.userTexts.confirmDelete)) return;
    if (await postAction('/Admin/Delete', new URLSearchParams({ userId: selectedUserId }))) location.reload();
});

btnAssignRole.addEventListener('click', async () => {
    const roleName = roleSelect.value;
    if (await postAction('/Admin/AssignRole', new URLSearchParams({ userId: selectedUserId, roleName }))) location.reload();
});

btnRemoveRole.addEventListener('click', async () => {
    const roleValue = roleSelect.value;
    const roleText = roleSelect.options[roleSelect.selectedIndex].text;
    const message = window.userTexts.confirmRemoveRole.replace("{0}", roleText);
    if (!confirm(message)) return;
    if (await postAction('/Admin/RemoveRole', new URLSearchParams({ userId: selectedUserId, roleName: roleValue }))) location.reload();
});