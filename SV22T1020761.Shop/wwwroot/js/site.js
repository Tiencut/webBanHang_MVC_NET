// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Common JS helpers for Shop
// Adds RequestVerificationToken header for fetch when antiforgery input exists
window.getRequestVerificationToken = function() {
    var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) return tokenInput.value;
    // fallback to antiForgeryForm
    var form = document.getElementById('antiForgeryForm');
    if (form) {
        var inp = form.querySelector('input[name="__RequestVerificationToken"]');
        return inp ? inp.value : null;
    }
    return null;
};

window.fetchWithAntiforgery = async function(url, options) {
    options = options || {};
    options.headers = options.headers || {};
    var token = window.getRequestVerificationToken();
    if (token) options.headers['RequestVerificationToken'] = token;
    options.headers['X-Requested-With'] = options.headers['X-Requested-With'] || 'XMLHttpRequest';
    return fetch(url, options);
};

// Simple helper to submit a form via AJAX and replace target container
window.submitFormAjax = async function(form, targetSelector, onSuccess) {
    var fd = new FormData(form);
    var res = await window.fetchWithAntiforgery(form.action, { method: form.method || 'POST', body: fd });
    if (res.ok) {
        var html = await res.text();
        if (targetSelector) document.querySelector(targetSelector).innerHTML = html;
        if (onSuccess) onSuccess(html, res);
        return true;
    }
    return false;
};

// Focus first invalid input if present
window.focusFirstInvalid = function() {
    // find client-side validation error inputs
    var invalid = document.querySelector('.input-validation-error, .text-danger:empty ~ input, .field-validation-error');
    if (!invalid) {
        // fallback: find first element with validation message
        var msg = document.querySelector('.text-danger:empty');
    }
    // better approach: find elements with aria-invalid or with validation error span
    var field = document.querySelector('.input-validation-error');
    if (!field) {
        var span = document.querySelector('.field-validation-error, .text-danger');
        if (span) {
            // try to focus associated input
            var id = span.getAttribute('data-valmsg-for');
            if (id) {
                var el = document.getElementById(id) || document.querySelector('[name="' + id + '"]');
                if (el) { el.focus(); return; }
            }
            // fallback: focus next input
            var next = span.parentElement && span.parentElement.querySelector('input,select,textarea');
            if (next) { next.focus(); return; }
        }
    } else {
        field.focus();
    }
};

// Initialize bootstrap tooltips in a container (or whole document if no container)
window.initTooltips = function(container) {
    try {
        var root = container ? (typeof container === 'string' ? document.querySelector(container) : container) : document;
        if (!root) return;
        var els = root.querySelectorAll('[data-bs-toggle="tooltip"]');
        els.forEach(function(el) {
            // Avoid double initialization: store instance
            if (!el._bs_tooltip) {
                el._bs_tooltip = new bootstrap.Tooltip(el);
            }
        });
    } catch (e) {
        console.error('initTooltips error', e);
    }
};

// Observe modal content changes to focus first invalid input inside modal
(function observeModal() {
    var modalContent = document.querySelector('#dialogModal .modal-content');
    if (!modalContent) return;
    var mo = new MutationObserver(function(mutations) {
        mutations.forEach(function(m) {
            if (m.addedNodes && m.addedNodes.length > 0) {
                window.focusFirstInvalid && window.focusFirstInvalid();
                window.initTooltips && window.initTooltips(modalContent);
            }
        });
    });
    mo.observe(modalContent, { childList: true, subtree: true });
})();

// Delegated handlers for product container: pagination clicks, add-to-cart buttons, quickview
document.addEventListener('DOMContentLoaded', function() {
    // init tooltips on load
    window.initTooltips && window.initTooltips();

    var container = document.getElementById('productTableContainer') || document.getElementById('homeFeatured');
    if (!container) return;

    container.addEventListener('click', async function(e) {
        // pagination
        var pageLink = e.target.closest('a.page-link');
        if (pageLink && pageLink.dataset.page) {
            e.preventDefault();
            var page = pageLink.dataset.page;
            // reuse search form if exists
            var form = document.getElementById('formSearch');
            if (form) {
                var inputPage = form.querySelector('input[name="Page"]');
                if (!inputPage) {
                    inputPage = document.createElement('input');
                    inputPage.type = 'hidden';
                    inputPage.name = 'Page';
                    form.appendChild(inputPage);
                }
                inputPage.value = page;
                // submit via AJAX helper
                window.submitFormAjax(form, '#productTableContainer', function(html) {
                    window.focusFirstInvalid && window.focusFirstInvalid();
                    window.initTooltips && window.initTooltips('#productTableContainer');
                });
            } else {
                // fallback: request Products Index with query param
                var params = new URLSearchParams(window.location.search);
                params.set('page', page);
                window.location.search = params.toString();
            }
            return;
        }

        // add to cart
        var addBtn = e.target.closest('.btn-add-to-cart');
        if (addBtn) {
            e.preventDefault();
            var id = addBtn.getAttribute('data-product-id');
            if (!id) return;
            var url = '/Cart/Add';
            var fd = new FormData();
            fd.append('productId', id);
            fd.append('qty', '1');
            // include antiforgery token if present
            var token = window.getRequestVerificationToken();
            if (token) fd.append('__RequestVerificationToken', token);

            try {
                var res = await fetch(url, { method: 'POST', body: fd, headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                if (res.ok) {
                    var text = await res.text();
                    try {
                        // Try to parse as JSON first
                        var data = JSON.parse(text);
                        if (data.requiresLogin === true) {
                            showToast('Vui lòng đăng nhập để thêm sản phẩm vào giỏ', 'info');
                            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.href);
                            return;
                        }
                    } catch (e) {
                        // Not JSON, treat as HTML (cart summary partial)
                    }
                    // Update cart summary with HTML
                    var cs = document.getElementById('cartSummary');
                    if (cs) cs.innerHTML = text;
                    showToast('Đã thêm vào giỏ', 'success');
                } else {
                    showToast('Lỗi khi thêm vào giỏ', 'error');
                }
            } catch (err) { showToast('Lỗi: ' + err.message, 'error'); }
            return;
        }

        // quick view handled elsewhere
    });
});