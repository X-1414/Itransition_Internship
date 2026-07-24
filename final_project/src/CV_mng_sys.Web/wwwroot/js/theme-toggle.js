const themeToggle = document.getElementById('themeToggle');
const themeIcon = document.getElementById('themeIcon');

function updateIcon() {
    const current = document.documentElement.getAttribute('data-bs-theme');
    themeIcon.textContent = current === 'dark' ? '☀️' : '🌙';
}

updateIcon();

themeToggle.addEventListener('click', () => {
    const current = document.documentElement.getAttribute('data-bs-theme');
    const next = current === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-bs-theme', next);
    localStorage.setItem('theme', next);
    updateIcon();
});