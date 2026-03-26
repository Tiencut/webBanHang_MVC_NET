// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Common JS helpers for Shop
// Adds RequestVerificationToken header for fetch when antiforgery input exists
window.getRequestVerificationToken = function() {
    var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : null;
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
