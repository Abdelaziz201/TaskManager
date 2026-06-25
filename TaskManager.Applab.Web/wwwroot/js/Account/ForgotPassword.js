$('#forgotBtn').on('click', function () {
    const email = $('#forgotEmail').val().trim();

    $('#forgotError').hide().text('');
    $('#forgotSuccess').hide().text('');
    $('#forgotEmailError').text('');

    if (!email) {
        $('#forgotEmailError').text('Email is required');
        return;
    }

    $.ajax({
        url: '/Account/ForgotPassword',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ email }),
        success: function (response) {
            $('#forgotSuccess').text(response.message).show();
            $('#forgotEmail').val('');
        },
        error: function (xhr) {
            const msg = xhr.responseJSON?.message || 'Something went wrong. Please try again.';
            $('#forgotError').text(msg).show();
        }
    });
});