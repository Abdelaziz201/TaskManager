$('#loginBtn').on('click', function () {
    const email = $('#loginEmail').val().trim();
    const password = $('#loginPassword').val().trim();

    $('#loginError').hide().text('');
    $('#emailError').text('');
    $('#passwordError').text('');

    let valid = true;
    if (!email) { $('#emailError').text('Email is required'); valid = false; }
    if (!password) { $('#passwordError').text('Password is required'); valid = false; }
    if (!valid) return;

    $.ajax({
        url: '/Account/Login',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ email, password }),
        success: function (response) {
            window.location.href = response.redirectUrl;
        },
        error: function (xhr) {
            const msg = xhr.responseJSON?.message || 'Invalid email or password';
            $('#loginError').text(msg).show();
        }
    });
});