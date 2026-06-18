$('#registerBtn').on('click', function () {
    const username = $('#regUsername').val().trim();
    const email = $('#regEmail').val().trim();
    const password = $('#regPassword').val().trim();

    $('#registerError').hide().text('');
    $('#usernameError').text('');
    $('#regEmailError').text('');
    $('#regPasswordError').text('');

    let valid = true;
    if (!username) { $('#usernameError').text('Username is required'); valid = false; }
    if (!email) { $('#regEmailError').text('Email is required'); valid = false; }
    if (!password) { $('#regPasswordError').text('Password is required'); valid = false; }
    if (!valid) return;

    $.ajax({
        url: '/Account/Register',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ username, email, password }),
        success: function (response) {
            window.location.href = response.redirectUrl;
        },
        error: function (xhr) {
            const msg = xhr.responseJSON?.message || 'Registration failed';
            $('#registerError').text(msg).show();
        }
    });
});