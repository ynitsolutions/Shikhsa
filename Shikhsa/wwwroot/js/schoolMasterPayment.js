// @ts-nocheck
// wwwroot/js/schoolMasterPayment.js
// Payment Gateway tab: add / edit / soft-delete gateways with key/value settings.
// Needs: SweetAlert2 (Swal), ShowWarning, ShowError, and the global paymentGatewayUrls set by the view.

(function () {
    const el = (id) => document.getElementById(id);

    const tokenEl = document.querySelector('#schoolMasterForm input[name="__RequestVerificationToken"]');
    const token = tokenEl ? tokenEl.value : '';

    const urls = paymentGatewayUrls;
    const rowsBody = el('pg-rows');

    // Template for Zoho (no secret values are filled in; type them in the Value column)
    const zohoTemplate = [
        { key: 'AccountId', value: '', secret: false },
        { key: 'ClientId', value: '', secret: false },
        { key: 'ClientSecret', value: '', secret: true },
        { key: 'RefreshToken', value: '', secret: true },
        { key: 'ApiKey', value: '', secret: true },
        { key: 'SigningKey', value: '', secret: true },
        { key: 'WebhookSigningKey', value: '', secret: true },
        { key: 'AccountsBaseUrl', value: 'https://accounts.zoho.in', secret: false },
        { key: 'ApiBaseUrl', value: 'https://accounts.zoho.in', secret: false },
        { key: 'CheckoutBaseUrl', value: 'https://accounts.zoho.in', secret: false },
        { key: 'SuccessUrl', value: '', secret: false },
        { key: 'FailureUrl', value: '', secret: false }
    ];

    function addRow(item) {
        item = item || { key: '', value: '', secret: false };

        const tr = document.createElement('tr');

        const tdKey = document.createElement('td');
        const key = document.createElement('input');
        key.type = 'text';
        key.className = 'form-control form-control-sm pg-key';
        key.maxLength = 100;
        key.value = item.key || '';
        tdKey.appendChild(key);

        const tdVal = document.createElement('td');
        const val = document.createElement('input');
        val.type = item.secret ? 'password' : 'text';
        val.className = 'form-control form-control-sm pg-value';
        val.autocomplete = 'new-password';
        val.value = item.value || '';
        if (item.secret && item.hasValue) { val.placeholder = '•••••• (saved)'; }
        tdVal.appendChild(val);

        const tdSecret = document.createElement('td');
        tdSecret.className = 'text-center';
        const secret = document.createElement('input');
        secret.type = 'checkbox';
        secret.className = 'form-check-input pg-secret';
        secret.checked = !!item.secret;
        secret.addEventListener('change', function () {
            val.type = secret.checked ? 'password' : 'text';
        });
        tdSecret.appendChild(secret);

        const tdDel = document.createElement('td');
        tdDel.className = 'text-center';
        const del = document.createElement('button');
        del.type = 'button';
        del.className = 'btn btn-outline-danger btn-sm';
        del.innerHTML = '<i class="fa fa-times"></i>';
        del.addEventListener('click', function () { tr.remove(); });
        tdDel.appendChild(del);

        tr.appendChild(tdKey);
        tr.appendChild(tdVal);
        tr.appendChild(tdSecret);
        tr.appendChild(tdDel);
        rowsBody.appendChild(tr);
    }

    function resetForm() {
        el('pg-id').value = '0';
        el('pg-name').value = '';
        el('pg-active').checked = true;
        el('pg-default').checked = false;
        el('pg-secret-hint').style.display = 'none';
        rowsBody.innerHTML = '';
        addRow();
    }

    async function post(url, fd) {
        fd.append('__RequestVerificationToken', token);
        const res = await fetch(url, { method: 'POST', body: fd });
        return res.json();
    }

    function successAndReload(message) {
        Swal.fire({
            icon: 'success',
            title: 'Success',
            text: message,
            timer: 1200,
            showConfirmButton: false
        }).then(function () { location.reload(); });
    }

    el('pg-add-row').addEventListener('click', function () { addRow(); });

    el('pg-load-zoho').addEventListener('click', function () {
        if (!el('pg-name').value.trim()) { el('pg-name').value = 'Zoho'; }
        rowsBody.innerHTML = '';
        zohoTemplate.forEach(addRow);
    });

    el('pg-reset').addEventListener('click', resetForm);

    el('pg-save').addEventListener('click', async function () {
        const name = el('pg-name').value.trim();
        if (!name) {
            ShowWarning('Gateway name is required.');
            return;
        }

        const fd = new FormData();
        fd.append('PaymentGatewaySettingId', el('pg-id').value);
        fd.append('GatewayName', name);
        fd.append('IsActive', String(el('pg-active').checked));
        fd.append('IsDefault', String(el('pg-default').checked));

        let index = 0;
        const rows = rowsBody.querySelectorAll('tr');
        for (const tr of rows) {
            const k = tr.querySelector('.pg-key').value.trim();
            if (!k) { continue; }
            fd.append('Values[' + index + '].SettingKey', k);
            fd.append('Values[' + index + '].SettingValue', tr.querySelector('.pg-value').value);
            fd.append('Values[' + index + '].IsSecret', String(tr.querySelector('.pg-secret').checked));
            index++;
        }

        if (index === 0) {
            ShowWarning('Add at least one setting.');
            return;
        }

        const isNew = el('pg-id').value === '0';
        try {
            const r = await post(urls.save, fd);
            if (r && r.success) {
                successAndReload(isNew ? 'Payment gateway saved successfully.' : 'Payment gateway updated successfully.');
            } else {
                ShowError((r && r.message) || 'Could not save payment gateway.');
            }
        } catch (e) {
            ShowError('Could not save payment gateway.');
        }
    });

    document.querySelectorAll('.pg-edit').forEach(function (btn) {
        btn.addEventListener('click', async function () {
            const row = btn.closest('tr');
            if (!row) { return; }
            try {
                const res = await fetch(urls.get + '?id=' + encodeURIComponent(row.dataset.id));
                const r = await res.json();
                if (!r || !r.success) {
                    ShowError((r && r.message) || 'Could not load payment gateway.');
                    return;
                }
                el('pg-id').value = r.id;
                el('pg-name').value = r.gatewayName;
                el('pg-active').checked = r.isActive;
                el('pg-default').checked = r.isDefault;
                el('pg-secret-hint').style.display = '';
                rowsBody.innerHTML = '';
                r.values.forEach(function (v) {
                    addRow({ key: v.key, value: v.value, secret: v.isSecret, hasValue: v.hasValue });
                });
                el('pg-name').scrollIntoView({ behavior: 'smooth', block: 'center' });
            } catch (e) {
                ShowError('Could not load payment gateway.');
            }
        });
    });

    document.querySelectorAll('.pg-delete').forEach(function (btn) {
        btn.addEventListener('click', async function () {
            const row = btn.closest('tr');
            if (!row) { return; }

            const confirmResult = await Swal.fire({
                title: 'Are you sure?',
                text: 'Do you want to delete this payment gateway?',
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
                const r = await post(urls.remove, fd);
                if (r && r.success) {
                    successAndReload('Payment gateway deleted successfully.');
                } else {
                    ShowError((r && r.message) || 'Could not delete.');
                }
            } catch (e) {
                ShowError('Could not delete.');
            }
        });
    });

    resetForm();
})();
