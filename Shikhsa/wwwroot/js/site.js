// ============================================================
//  site.js — Shikhsa Common JavaScript
// ============================================================

const Shikhsa = (() => {

    // ── Sidebar Toggle ─────────────────────────────────────
    function initSidebar() {
        const sidebar  = document.getElementById('sidebar');
        const overlay  = document.getElementById('sidebarOverlay');
        const toggleBtn = document.getElementById('sidebarToggle');
        if (!sidebar) return;

        function openSidebar() {
            sidebar.classList.add('open');
            overlay?.classList.add('open');
            document.body.style.overflow = 'hidden';
        }

        function closeSidebar() {
            sidebar.classList.remove('open');
            overlay?.classList.remove('open');
            document.body.style.overflow = '';
        }

        toggleBtn?.addEventListener('click', () => {
            sidebar.classList.contains('open') ? closeSidebar() : openSidebar();
        });

        overlay?.addEventListener('click', closeSidebar);

        // Close on resize to desktop
        window.addEventListener('resize', () => {
            if (window.innerWidth > 991) closeSidebar();
        });
    }

    // ── Submenu Accordion ──────────────────────────────────
    function initSubmenus() {
        document.querySelectorAll('.nav-link[data-submenu]').forEach(link => {
            link.addEventListener('click', function (e) {
                e.preventDefault();
                const targetId = this.dataset.submenu;
                const submenu  = document.getElementById(targetId);
                const arrow    = this.querySelector('.nav-arrow');
                if (!submenu) return;

                const isOpen = submenu.classList.contains('open');

                // Close all others
                document.querySelectorAll('.nav-submenu.open').forEach(sm => {
                    sm.classList.remove('open');
                });
                document.querySelectorAll('.nav-arrow').forEach(a => {
                    a.style.transform = '';
                });

                if (!isOpen) {
                    submenu.classList.add('open');
                    if (arrow) arrow.style.transform = 'rotate(90deg)';
                }
            });
        });

        // Auto-open active submenu on page load
        document.querySelectorAll('.nav-submenu .nav-link.active').forEach(link => {
            const submenu = link.closest('.nav-submenu');
            if (submenu) {
                submenu.classList.add('open');
                const parentLink = document.querySelector(`[data-submenu="${submenu.id}"]`);
                const arrow = parentLink?.querySelector('.nav-arrow');
                if (arrow) arrow.style.transform = 'rotate(90deg)';
            }
        });
    }

    // ── Password Toggle ────────────────────────────────────
    function initPasswordToggle() {
        document.querySelectorAll('.input-toggle[data-target]').forEach(btn => {
            btn.addEventListener('click', function () {
                const input = document.getElementById(this.dataset.target);
                if (!input) return;
                const isPassword = input.type === 'password';
                input.type = isPassword ? 'text' : 'password';
                const icon = this.querySelector('i');
                if (icon) {
                    icon.className = isPassword ? 'ti ti-eye-off' : 'ti ti-eye';
                }
            });
        });
    }

    // ── Alert Auto-dismiss ─────────────────────────────────
    function initAlerts() {
        document.querySelectorAll('.alert[data-autohide]').forEach(alert => {
            const ms = parseInt(alert.dataset.autohide) || 4000;
            setTimeout(() => {
                alert.style.transition = 'opacity 0.4s ease';
                alert.style.opacity    = '0';
                setTimeout(() => alert.remove(), 400);
            }, ms);
        });

        document.querySelectorAll('.alert .alert-close').forEach(btn => {
            btn.addEventListener('click', function () {
                const alert = this.closest('.alert');
                alert.style.transition = 'opacity 0.3s ease';
                alert.style.opacity    = '0';
                setTimeout(() => alert.remove(), 300);
            });
        });
    }

    // ── Button Loading State ───────────────────────────────
    function initButtonLoading() {
        document.querySelectorAll('form').forEach(form => {
            form.addEventListener('submit', function () {
                const btn = this.querySelector('button[type="submit"]');
                if (!btn || btn.dataset.noloading) return;
                btn.classList.add('loading');
                btn.disabled = true;
                // Re-enable after 8s as fallback
                setTimeout(() => {
                    btn.classList.remove('loading');
                    btn.disabled = false;
                }, 8000);
            });
        });
    }

    // ── Active Nav Link ────────────────────────────────────
    function initActiveNav() {
        const path = window.location.pathname.toLowerCase();
        document.querySelectorAll('.nav-link[href]').forEach(link => {
            const href = link.getAttribute('href').toLowerCase();
            if (href !== '#' && path.startsWith(href) && href !== '/') {
                link.classList.add('active');
            }
            if (href === '/' && path === '/') {
                link.classList.add('active');
            }
        });
    }

    // ── Toast Notifications ────────────────────────────────
    function toast(message, type = 'info', duration = 3500) {
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.style.cssText = `
                position: fixed; bottom: 1.5rem; right: 1.5rem;
                z-index: 9999; display: flex; flex-direction: column; gap: 8px;
            `;
            document.body.appendChild(container);
        }

        const icons = { success: 'ti-check', danger: 'ti-x', warning: 'ti-alert-triangle', info: 'ti-info-circle' };

        const el = document.createElement('div');
        el.className = `alert alert-${type} animate-fade`;
        el.style.cssText = 'min-width: 260px; max-width: 360px; box-shadow: var(--shadow-md);';
        el.innerHTML = `<i class="ti ${icons[type] || 'ti-info-circle'}" aria-hidden="true"></i> ${message}`;

        container.appendChild(el);

        setTimeout(() => {
            el.style.transition = 'opacity 0.35s ease';
            el.style.opacity    = '0';
            setTimeout(() => el.remove(), 350);
        }, duration);
    }

    // ── Confirm Delete ─────────────────────────────────────
    function initConfirmDelete() {
        document.querySelectorAll('[data-confirm]').forEach(el => {
            el.addEventListener('click', function (e) {
                const msg = this.dataset.confirm || 'Are you sure?';
                if (!confirm(msg)) e.preventDefault();
            });
        });
    }

    // ── Init All ───────────────────────────────────────────
    function init() {
        initSidebar();
        initSubmenus();
        initPasswordToggle();
        initAlerts();
        initButtonLoading();
        initActiveNav();
        initConfirmDelete();
    }

    document.addEventListener('DOMContentLoaded', init);

    return { toast, init };

})();
