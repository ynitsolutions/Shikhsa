// @ts-nocheck
// wwwroot/js/schoolMasterEmail.js
// Email Configuration tab: save / edit / delete via AJAX.
// Plain .js file, so no Razor escaping (@@) is needed here.
// URLs come from the global emailSettingUrls, which the view sets.
// Uses ShowError / ShowWarning (SweetAlert2 helpers) and Swal directly.

(function () {
    const el = (id) => document.getElementById(id);

    const tokenEl = document.querySelector('#schoolMasterForm input[name="__RequestVerificationToken"]');
    const token = tokenEl ? tokenEl.value : '';

    const schoolIdEl = el('SchoolId');
    const schoolId = schoolIdEl ? String(schoolIdEl.value) : '0';

    const saveUrl = emailSettingUrls.save;
    const deleteUrl = emailSettingUrls.remove;

    function resetForm() {
        el('es-id').value = '0';
        el('es-provider').value = '';
        el('es-priority').value = '1';
        el('es-host').value = '';
        el('es-port').value = '587';
        el('es-username').value = '';
        el('es-password').value = '';
        el('es-fromemail').value = '';
        el('es-fromname').value = '';
        el('es-ssl').checked = true;
        el('es-active').checked = true;
        el('es-password-hint').style.display = 'none';
    }

    async function post(url, fd) {
        fd.append('__RequestVerificationToken', token);
        const res = await fetch(url, { method: 'POST', body: fd });
        return res.json();
    }

    // Show a success popup briefly, then reload (the active tab is restored from sessionStorage)
    function successAndReload(message) {
        Swal.fire({
            icon: 'success',
            title: 'Success',
            text: message,
            timer: 1200,
            showConfirmButton: false
        }).then(function () { location.reload(); });
    }

    el('es-reset').addEventListener('click', resetForm);

    el('es-save').addEventListener('click', async function () {
        const provider = el('es-provider').value.trim();
        const host = el('es-host').value.trim();
        const user = el('es-username').value.trim();
        const from = el('es-fromemail').value.trim();
        const isNew = el('es-id').value === '0';

        if (!provider || !host || !user || !from || (el('es-password').value === '' && isNew)) {
            ShowWarning('Provider, Host, User Name, Password and From Email are required.');
            return;
        }
        if (!schoolId || schoolId === '0') {
            ShowWarning('Please save School Information first, then add email settings.');
            return;
        }

        const fd = new FormData();
        fd.append('EmailSettingId', el('es-id').value);
        fd.append('SchoolId', schoolId);
        fd.append('ProviderName', provider);
        fd.append('Priority', el('es-priority').value);
        fd.append('Host', host);
        fd.append('Port', el('es-port').value);
        fd.append('UserName', user);
        fd.append('Password', el('es-password').value);
        fd.append('FromEmail', from);
        fd.append('FromName', el('es-fromname').value.trim());
        fd.append('EnableSsl', String(el('es-ssl').checked));
        fd.append('IsEmailActive', String(el('es-active').checked));

        try {
            const r = await post(saveUrl, fd);
            if (r && r.success) {
                successAndReload(isNew ? 'Email setting saved successfully.' : 'Email setting updated successfully.');
            } else {
                ShowError((r && r.message) || 'Could not save email setting.');
            }
        } catch (e) {
            ShowError('Could not save email setting.');
        }
    });

    document.querySelectorAll('.es-edit').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const row = btn.closest('tr');
            if (!row) { return; }
            const d = row.dataset;
            el('es-id').value = d.id || '0';
            el('es-provider').value = d.provider || '';
            el('es-priority').value = d.priority || '1';
            el('es-host').value = d.host || '';
            el('es-port').value = d.port || '587';
            el('es-username').value = d.username || '';
            el('es-password').value = '';
            el('es-fromemail').value = d.fromemail || '';
            el('es-fromname').value = d.fromname || '';
            el('es-ssl').checked = d.ssl === 'true';
            el('es-active').checked = d.active === 'true';
            el('es-password-hint').style.display = '';
            el('es-provider').scrollIntoView({ behavior: 'smooth', block: 'center' });
        });
    });

    document.querySelectorAll('.es-delete').forEach(function (btn) {
        btn.addEventListener('click', async function () {
            const row = btn.closest('tr');
            if (!row) { return; }

            const confirmResult = await Swal.fire({
                title: 'Are you sure?',
                text: 'Do you want to delete this email setting?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Yes, delete',
                cancelButtonText: 'No'
            });
            if (!confirmResult.isConfirmed) { return; }

            const fd = new FormData();
            fd.append('id', row.dataset.id || '0');
            try {
                const r = await post(deleteUrl, fd);
                if (r && r.success) {
                    successAndReload('Email setting deleted successfully.');
                } else {
                    ShowError((r && r.message) || 'Could not delete.');
                }
            } catch (e) {
                ShowError('Could not delete.');
            }
        });
    });
})();