// ============================================================
//  login.js — Login Page JavaScript
// ============================================================

document.addEventListener('DOMContentLoaded', () => {

    // ── Password visibility toggle ─────────────────────────
    const toggleBtn = document.getElementById('togglePassword');
    const passInput = document.getElementById('Password');

    if (toggleBtn && passInput) {
        toggleBtn.addEventListener('click', () => {
            const isHidden = passInput.type === 'password';
            passInput.type = isHidden ? 'text' : 'password';
            const icon = toggleBtn.querySelector('i');
            if (icon) icon.className = isHidden ? 'ti ti-eye-off' : 'ti ti-eye';
            toggleBtn.setAttribute('aria-label', isHidden ? 'Hide password' : 'Show password');
        });
    }

    // ── Input focus animation ──────────────────────────────
    document.querySelectorAll('.form-control').forEach(input => {
        const group = input.closest('.input-group');
        if (!group) return;

        input.addEventListener('focus', () => {
            group.style.transform = 'translateY(-1px)';
        });

        input.addEventListener('blur', () => {
            group.style.transform = '';
        });
    });

    // ── Form validation feedback ───────────────────────────
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', function (e) {
            let valid = true;

            const email    = document.getElementById('Input_Email') || document.getElementById('Email');
            const password = document.getElementById('Password');

            if (email && !email.value.trim()) {
                showError(email, 'Email is required');
                valid = false;
            } else if (email) {
                clearError(email);
            }

            if (password && !password.value.trim()) {
                showError(password, 'Password is required');
                valid = false;
            } else if (password) {
                clearError(password);
            }

            if (!valid) e.preventDefault();
        });
    }

    function showError(input, message) {
        input.classList.add('is-invalid');
        let fb = input.parentElement.querySelector('.invalid-feedback');
        if (!fb) {
            fb = document.createElement('div');
            fb.className = 'invalid-feedback';
            input.parentElement.after(fb);
        }
        fb.textContent = message;
    }

    function clearError(input) {
        input.classList.remove('is-invalid');
        const fb = input.parentElement.querySelector('.invalid-feedback');
        if (fb) fb.textContent = '';
    }

    // ── Animate cards in on load ───────────────────────────
    document.querySelectorAll('.login-feature').forEach((el, i) => {
        el.style.animationDelay = `${0.2 + i * 0.1}s`;
        el.style.animation      = 'fadeIn 0.5s ease both';
    });

});
