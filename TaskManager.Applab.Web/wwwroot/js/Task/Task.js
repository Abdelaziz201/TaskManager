// live search
$('#searchInput').on('input', function () {
    const query = $(this).val().toLowerCase();
    $('.task-card').each(function () {
        const title = $(this).data('title') || '';
        $(this).toggle(title.includes(query));
    });
});

// update status
function updateStatus(select) {
    const taskId = $(select).data('task-id');
    const newStatus = $(select).val();

    $.ajax({
        url: '/Task/UpdateStatus',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ id: parseInt(taskId), status: newStatus }),
        success: function () {
            $(select).removeClass('badge-pending badge-inprogress badge-done');
            if (newStatus === 'Pending') $(select).addClass('badge-pending');
            if (newStatus === 'InProgress') $(select).addClass('badge-inprogress');
            if (newStatus === 'Done') $(select).addClass('badge-done');
            refreshStats();
        },
        error: function () {
            alert('Failed to update status');
            location.reload();
        }
    });
}

// delete task
function deleteTask(taskId, btn) {
    if (!confirm('Delete this task?')) return;

    $.ajax({
        url: '/Task/Delete/' + taskId,
        type: 'POST',
        success: function () {
            $(btn).closest('.task-card').remove();
            refreshStats();
        },
        error: function () {
            alert('Failed to delete task');
        }
    });
}

// create task
$('#createTaskBtn').on('click', function () {
    const title = $('#taskTitle').val().trim();
    const description = $('#taskDescription').val().trim();
    const dueDate = $('#taskDueDate').val();

    $('#taskTitleError').text('');
    $('#taskDateError').text('');

    let valid = true;
    if (!title) { $('#taskTitleError').text('Title is required'); valid = false; }
    if (!dueDate) { $('#taskDateError').text('Due date is required'); valid = false; }
    if (!valid) return;

    $.ajax({
        url: '/Task/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ title, description, dueDate }),
        success: function (task) {
            bootstrap.Modal.getInstance(document.getElementById('createTaskModal')).hide();

            $('#taskTitle').val('');
            $('#taskDescription').val('');
            $('#taskDueDate').val('');

            const card = `
                <div class="task-card" data-title="${task.title.toLowerCase()}">
                    <h3>${task.title}</h3>
                    ${task.description ? `<p>${task.description}</p>` : ''}
                    <div class="task-card-footer">
                        <div class="d-flex gap-2 align-items-center flex-wrap">
                            <select class="status-select badge-pending"
                                    data-task-id="${task.id}"
                                    onchange="updateStatus(this)">
                                <option value="Pending" selected>Pending</option>
                                <option value="InProgress">In Progress</option>
                                <option value="Done">Done</option>
                            </select>
                            <span class="task-date">
                                <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                                    <rect x="3" y="4" width="18" height="18" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/>
                                </svg>
                                ${new Date(task.dueDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}
                            </span>
                        </div>
                        <div class="task-card-actions">
                            <button class="btn-delete" onclick="deleteTask(${task.id}, this)">Delete</button>
                        </div>
                    </div>
                </div>`;

            $('.empty-state').remove();
            $('#taskGrid').prepend(card);
            refreshStats();
        },
        error: function () {
            alert('Failed to create task');
        }
    });
});

// recalculate stat cards
function refreshStats() {
    let total = 0, pending = 0, inProgress = 0, done = 0;
    $('.task-card').each(function () {
        total++;
        const status = $(this).find('.status-select').val();
        if (status === 'Pending') pending++;
        if (status === 'InProgress') inProgress++;
        if (status === 'Done') done++;
    });
    $('#statTotal').text(total);
    $('#statPending').text(pending);
    $('#statInProgress').text(inProgress);
    $('#statDone').text(done);
}