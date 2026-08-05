const supportLink = document.getElementById('supportTicketLink');
const supportModalEl = document.getElementById('supportModal');
const supportModal = supportModalEl ? new bootstrap.Modal(supportModalEl) : null;

supportLink?.addEventListener('click', (e) => {
    e.preventDefault();
    document.getElementById('supportError').classList.add('d-none');
    document.getElementById('supportSuccess').classList.add('d-none');
    document.getElementById('ticketSummary').value = '';
    supportModal.show();
});

document.getElementById('btnSubmitTicket')?.addEventListener('click', async () => {
    const summary = document.getElementById('ticketSummary').value.trim();
    const priority = document.getElementById('ticketPriority').value;
    const errorBox = document.getElementById('supportError');
    const successBox = document.getElementById('supportSuccess');

    if(!summary){
        errorBox.textContent = window.supportTexts.pleaseProvideSummary;
        errorBox.classList.remove('d-none');
        return;
    }
    const body = new URLSearchParams({
        summary, priority, returnUrl: window.location.pathname+window.location.search, inventory: window.__inventoryTitle || ''
    });
    try{
        const response = await fetch('/Support/Submit', {
            method: 'POST',
            headers: {'Content-Type': 'application/x-www-form-urlencoded'},
            body
        });
        if (!response.ok){
            const data = await response.json().catch(() => ({}));
            errorBox.textContent = data.error || window.supportTexts.couldNotSubmitTicket;
            errorBox.classList.remove('d-none');
            return;
        }
        errorBox.classList.add('d-none');
        successBox.classList.remove('d-none');
    } catch (err){
        errorBox.textContent = window.supportTexts.networkError + ': ' + err.message;
        errorBox.classList.remove('d-none');
    }
});