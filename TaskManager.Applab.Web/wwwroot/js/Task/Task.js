let currentMode = 'create'; // 'create' or 'edit'
let searchTimeout;



//search
$('#searchInput').on('input', function () {
    clearTimeout(searchTimeout);
    const query = $(this).val();

    searchTimeout = setTimeout(function () {
        $.ajax({
            url: '/Task/Search',
            type: 'GET',
            data: { search: query },
            success: function (tasks) {
                renderTaskGrid(tasks);
            },
            error: function () {
                alert('Search failed');
            }
        });
    }, 300);
});

function renderTaskGrid(tasks) {
    if (!tasks || tasks.length === 0) {
        showEmptyState();
        return;
    }
    const html = tasks.map(t => buildCardHtml(t)).join('');
    $('#taskGrid').html(html);
    sortTasksByDueDate($('#sortSelect').val() || 'dueDateAsc');
}

function toLocalDateTimeString(date) {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
}


// open modal for creating
function openCreateModal() {
    currentMode = 'create';
    $('#taskModalTitle').text('New Task');
    $('#saveTaskBtnText').text('Create Task');
    $('#taskId').val('');
    $('#taskTitle').val('');
    $('#taskDescription').val('');

    const tomorrow = new Date(Date.now() + 86400000);
    dueDatePicker.set('minDate', new Date());
    dueDatePicker.setDate(tomorrow, true);

    $('#taskTitleError').text('');
    $('#taskDateError').text('');
    new bootstrap.Modal(document.getElementById('taskModal')).show();
}

// open modal for editing
function openEditModal(btn) {
    currentMode = 'edit';
    const card = $(btn).closest('.task-card');

    $('#taskModalTitle').text('Edit Task');
    $('#saveTaskBtnText').text('Save Changes');
    $('#taskId').val(card.data('id'));
    $('#taskTitle').val(card.data('task-title'));
    $('#taskDescription').val(card.data('task-description'));
    dueDatePicker.set('minDate', null); // allow past dates when editing
    dueDatePicker.setDate(card.data('task-duedate'), true);

    $('#taskTitleError').text('');
    $('#taskDateError').text('');

    new bootstrap.Modal(document.getElementById('taskModal')).show();
}

// ───── custom status dropdown ─────

// toggle dropdown open/close
function toggleStatusMenu(btn) {
    const menu = $(btn).siblings('.status-menu');
    $('.status-menu').not(menu).removeClass('show'); // close any other open menu
    menu.toggleClass('show');
}

// close dropdown when clicking outside
$(document).on('click', function (e) {
    if (!$(e.target).closest('.status-dropdown').length) {
        $('.status-menu').removeClass('show');
    }
});

// pick a status from the custom dropdown
function selectStatus(option) {
    const newStatus = $(option).data('value');
    const dropdown = $(option).closest('.status-dropdown');
    const taskId = dropdown.data('task-id');
    const trigger = dropdown.find('.status-trigger');

    $.ajax({
        url: '/Task/UpdateStatus',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ id: parseInt(taskId), status: newStatus }),
        success: function () {
            trigger.removeClass('badge-pending badge-inprogress badge-done');
            const badgeClass = newStatus === 'InProgress' ? 'badge-inprogress'
                : newStatus === 'Done' ? 'badge-done' : 'badge-pending';
            trigger.addClass(badgeClass);
            trigger.find('.status-label').text(newStatus === 'InProgress' ? 'In Progress' : newStatus);

            dropdown.closest('.task-card').attr('data-task-status', newStatus);

            $('.status-menu').removeClass('show');
            refreshStats();
        },
        error: function () {
            alert('Failed to update status');
            location.reload();
        }
    });
}


//24 hs date picker

let dueDatePicker;

$(function () {
    dueDatePicker = flatpickr("#taskDueDate", {
        enableTime: true,
        dateFormat: "Y-m-d\\TH:i", // matches datetime-local format internally
        altInput: true,
        altFormat: "M j, Y H:i",   // what the user sees, 24-hour format
        time_24hr: true,           // forces 24-hour picker UI
        minDate: new Date()
    });
});

// ───── delete task ─────

let taskIdToDelete = null;
let btnToRemove = null;

function deleteTask(taskId, btn) {
    taskIdToDelete = taskId;
    btnToRemove = btn;
    new bootstrap.Modal(document.getElementById('deleteConfirmModal')).show();
}
$('#confirmDeleteBtn').on('click', function () {
    const btn = $(this);
    if (btn.prop('disabled')) return; // prevent double-click

    btn.prop('disabled', true);

    $.ajax({
        url: '/Task/Delete/' + taskIdToDelete,
        type: 'POST',
        success: function () {
            $(btnToRemove).closest('.task-card').remove();
            refreshStats();
            if ($('.task-card').length === 0) showEmptyState();
            bootstrap.Modal.getInstance(document.getElementById('deleteConfirmModal'))?.hide();
        },
        error: function () {
            alert('Failed to delete task');
        },
        complete: function () {
            btn.prop('disabled', false);
        }
    });
});

// ───── build a task card's HTML (used after create/edit) ─────

function buildCardHtml(task) {
    const badgeClass = task.status === 'InProgress' ? 'badge-inprogress'
        : task.status === 'Done' ? 'badge-done' : 'badge-pending';
    const statusLabel = task.status === 'InProgress' ? 'In Progress' : task.status;

    const dateFormatted = new Date(task.dueDate).toLocaleString('en-US', {
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
    });
    const dateIso = toLocalDateTimeString(task.dueDate);

    return `
        <div class="task-card"
             data-title="${task.title.toLowerCase()}"
             data-id="${task.id}"
             data-task-title="${task.title}"
             data-task-description="${task.description || ''}"
             data-task-duedate="${dateIso}"
             data-task-status="${task.status}">
            <h3>${task.title}</h3>
            ${task.description ? `<p>${task.description}</p>` : ''}
            <div class="task-card-footer">
                <div class="d-flex gap-2 align-items-center flex-wrap">
                    <div class="status-dropdown" data-task-id="${task.id}">
                        <button type="button" class="status-trigger ${badgeClass}" onclick="toggleStatusMenu(this)">
                            <span class="status-label">${statusLabel}</span>
                            <svg width="10" height="10" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7"/>
                            </svg>
                        </button>
                        <div class="status-menu">
                            <div class="status-option badge-pending" data-value="Pending" onclick="selectStatus(this)">Pending</div>
                            <div class="status-option badge-inprogress" data-value="InProgress" onclick="selectStatus(this)">In Progress</div>
                            <div class="status-option badge-done" data-value="Done" onclick="selectStatus(this)">Done</div>
                        </div>
                    </div>
                    <span class="task-date">
                        <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                            <rect x="3" y="4" width="18" height="18" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/>
                        </svg>
                        ${dateFormatted}
                    </span>
                </div>
                <div class="task-card-actions">
                    <button class="btn-edit" onclick="openEditModal(this)">Edit</button>
                    <button class="btn-delete" onclick="deleteTask(${task.id}, this)">Delete</button>
                </div>
            </div>
        </div>`;
}

function showEmptyState() {
    $('#taskGrid').html(`
        <div class="empty-state">
            <svg fill="none" stroke="currentColor" stroke-width="1.5" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
            </svg>
            <p>No tasks yet. Create your first one!</p>
        </div>`);
}

// ───── create OR edit (shared save button) ─────

$('#saveTaskBtn').on('click', function () {
    const title = $('#taskTitle').val().trim();
    const description = $('#taskDescription').val().trim();
    const dueDate = $('#taskDueDate').val();

    $('#taskTitleError').text('');
    $('#taskDateError').text('');

    let valid = true;
    if (!title) {
        $('#taskTitleError').text('Title is required');
        valid = false;
    }
    if (!dueDate) {
        $('#taskDateError').text('Due date is required');
        valid = false;
    } else if (currentMode === 'create' && new Date(dueDate) <= new Date()) {
        $('#taskDateError').text('Due date must be in the future');
        valid = false;
    }
    if (!valid) return;

    // check for duplicate title (only when creating, or editing to a different title)
    const currentId = $('#taskId').val();
    const isDuplicate = $('.task-card').toArray().some(card => {
        const cardId = $(card).data('id').toString();
        const cardTitle = $(card).data('task-title').toLowerCase();
        return cardTitle === title.toLowerCase() && cardId !== currentId;
    });

    if (isDuplicate) {
        $('#taskModal').modal('hide');
        $('#duplicateTitleText').text(`A task named "${title}" already exists.`);
        new bootstrap.Modal(document.getElementById('duplicateConfirmModal')).show();
        return; // wait for user confirmation
    }

    proceedWithSave(title, description, dueDate);
});

function proceedWithSave(title, description, dueDate) {
    $('#saveTaskBtn').prop('disabled', true);
    $('#saveTaskSpinner').show();

    if (currentMode === 'create') {
        console.log('sending create request...');
        $.ajax({
            url: '/Task/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ title, description, dueDate }),
            success: function (task) {
                console.log('create success', task);
                bootstrap.Modal.getInstance(document.getElementById('taskModal'))?.hide();
                $('.empty-state').remove();
                $('#taskGrid').prepend(buildCardHtml(task));
                sortTasksByDueDate($('#sortSelect').val() || 'dueDateAsc');
                refreshStats();
            },
            error: function (xhr) {
                console.log('create error', xhr.status, xhr.responseText);
                alert('Failed to create task');
            },
            complete: function () {
                $('#saveTaskBtn').prop('disabled', false);
                $('#saveTaskSpinner').hide();
            }
        });
    } else {
        const id = $('#taskId').val();
        const status = $(`.task-card[data-id="${id}"]`).data('task-status');

        $.ajax({
            url: '/Task/Edit',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ id: parseInt(id), title, description, dueDate, status }),
            success: function (task) {
                bootstrap.Modal.getInstance(document.getElementById('taskModal'))?.hide();
                $(`.task-card[data-id="${id}"]`).replaceWith(buildCardHtml(task));
                refreshStats();
            },
            error: function () {
                alert('Failed to update task');
            },
            complete: function () {
                $('#saveTaskBtn').prop('disabled', false);
                $('#saveTaskSpinner').hide();
            }
        });
    }
}

$('#confirmDuplicateBtn').on('click', function () {
    const title = $('#taskTitle').val().trim();
    const description = $('#taskDescription').val().trim();
    const dueDate = $('#taskDueDate').val();

    const modalEl = document.getElementById('duplicateConfirmModal');
    const modalInstance = bootstrap.Modal.getInstance(modalEl);
    if (modalInstance) modalInstance.hide();

    proceedWithSave(title, description, dueDate);
});

function reopenTaskModal() {
    new bootstrap.Modal(document.getElementById('taskModal')).show();
}

// ───── recalculate stat cards ─────

function refreshStats() {
    let total = 0, pending = 0, inProgress = 0, done = 0;
    $('.task-card').each(function () {
        total++;
        const status = $(this).data('task-status');
        if (status === 'Pending') pending++;
        if (status === 'InProgress') inProgress++;
        if (status === 'Done') done++;
    });
    $('#statTotal').text(total);
    $('#statPending').text(pending);
    $('#statInProgress').text(inProgress);
    $('#statDone').text(done);
}


// -------sort --------
function sortTasksByDueDate(sortBy) {
    const cards = $('.task-card').get();

    cards.sort(function (a, b) {
        const dateA = new Date($(a).data('task-duedate'));
        const dateB = new Date($(b).data('task-duedate'));
        return sortBy === 'dueDateAsc' ? dateA - dateB : dateB - dateA;
    });

    $('#taskGrid').empty().append(cards);
}

$('#sortSelect').on('change', function () {
    const sortBy = $(this).val();
    if (!sortBy) return;
    sortTasksByDueDate(sortBy);
});

// sort by nearest due date on page load
$(function () {
    sortTasksByDueDate('dueDateAsc');
    $('#sortSelect').val('dueDateAsc');
});