// @ts-nocheck
// wwwroot/js/schoolMasterCredential.js
// Credentials tab: add / update the school's Admin login via AJAX.
// Needs: SweetAlert2 (Swal), ShowWarning, ShowError, and the global credentialUrls set by the view.

(function () {
    const el = (id) => document.getElementById(id);

    const tokenEl = document.querySelector('#schoolMasterForm input[name="__RequestVerificationToken"]');
    const token = tokenEl ? tokenEl.value : '';

    // show / hide password
    el('cr-toggle').addEventListener('click', function () {
        const input = el('cr-password');
        const icon = el('cr-toggle').querySelector('i');
        const show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        icon.className = show ? 'fa fa-eye-slash' : 'fa fa-eye';
    });

    el('cr-save').addEventListener('click', async function () {
        const userName = el('cr-username').value.trim();
        const email = el('cr-email').value.trim();
        const password = el('cr-password').value;

        if (!userName || !email) {
            ShowWarning('User name and email are required.');
            return;
        }

        const fd = new FormData();
        fd.append('userName', userName);
        fd.append('email', email);
        fd.append('password', password);
        fd.append('__RequestVerificationToken', token);

        try {
            const res = await fetch(credentialUrls.save, { method: 'POST', body: fd });
            const r = await res.json();

            if (r && r.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: r.message || 'Saved successfully.',
                    timer: 1200,
                    showConfirmButton: false
                }).then(function () { location.reload(); }); // active tab is restored from sessionStorage
            } else {
                ShowError((r && r.message) || 'Could not save credentials.');
                console.log(r.message)
            }
        } catch (e) {
            console.log(e);
            console.log(e.message);
            ShowError('Could not save credentials.');
        }
    });
})();