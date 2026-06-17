// pw visibility toggle
function togglePassword(inputId, btn){
    const input = document.getElementById(inputId);
    if (!input) return;
    const isHidden = input.type === 'password';
    input.type = isHidden ? 'text' : 'password';
    const icon = btn.querySelector('i');
    if (icon){
        icon.classList.toggle('bi-eye', !isHidden);
        icon.classList.toggle('bi-eye-slash', isHidden);
    }
}

// auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function(){
    document.querySelectorAll('.alert.alert-dismissable').forEach(function(alert){
        setTimeout(function() {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 5000);
    });
});