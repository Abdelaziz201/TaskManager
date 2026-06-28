$('#resetBtn').on('click', function () {
    const token = $('#resetToken').val();
    const newPassword = $('#newPassword').val().trim();
    const confirmPassword = $('#confirmPassword').val().trim();

    $('#resetError').hide().text('');
    $('#newPasswordError').text('');
    $('#confirmPasswordError').text('');

    let valid = true;
    if (!newPassword) { $('#newPasswordError').text('Password is required'); valid = false; }
    if (newPassword !== confirmPassword) { $('#confirmPasswordError').text('Passwords do not match'); valid = false; }
    if (!valid) return;

    $.ajax({
        url: '/Account/ResetPassword',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ token, newPassword, confirmPassword}),
        success: function (response) {
            window.location.href = response.redirectUrl;
        },
        error: function (xhr) {
            const msg = xhr.responseJSON?.message || 'Could not reset password.';
            $('#resetError').text(msg).show();
        }
    });
});