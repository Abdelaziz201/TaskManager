let currentMode = 'create'; // 'create' or 'edit'
let searchTimeout;
let pendingAttachments = [];


function showToast(message, type = 'error') {
    const toast = $(`
        <div class="app-toast toast-${type}">
            <svg class="app-toast-icon" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                ${type === 'error'
            ? '<circle cx="12" cy="12" r="10"/><path stroke-linecap="round" d="M12 8v4m0 4h.01"/>'
            : '<path stroke-linecap="round" stroke-linejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>'
        }
            </svg>
            <span>${message}</span>
        </div>
    `);

    $('#toastContainer').append(toast);
    requestAnimationFrame(() => toast.addClass('show'));

    setTimeout(() => {
        toast.removeClass('show');
        setTimeout(() => toast.remove(), 200);
    }, 3500);
}


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
                showToast('Search failed');
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

    const activeCard = $('.stat-card.active')[0];
    if (activeCard) filterByStatus(activeCard);
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
    pendingAttachments = [];
    $('#taskModalTitle').text('New Task');
    $('#saveTaskBtnText').text('Create Task');
    $('#taskId').val('');
    $('#taskTitle').val('');
    $('#taskDescription').val('');
    $('#uploadError').hide();

    const tomorrow = new Date(Date.now() + 86400000);
    dueDatePicker.set('minDate', new Date());
    dueDatePicker.setDate(tomorrow, true);

    $('#taskTitleError').text('');
    $('#taskDateError').text('');

    // show attachments section for create as well — allow creating task implicitly when user uploads
    $('#attachmentsSection').show();
    $('#attachmentsList').empty();

    new bootstrap.Modal(document.getElementById('taskModal')).show();
}

function renderPendingAttachments() {
    const html = pendingAttachments.map((file, index) => `
        <div class="attachment-item" data-pending-index="${index}">
            <span class="attachment-item-name" style="cursor:default;">
                ${fileIconSvg()} ${file.name}
            </span>
            <div class="attachment-item-actions">
                <button class="attachment-delete-btn" onclick="removePendingAttachment(${index})">Remove</button>
            </div>
        </div>
    `).join('');
    $('#attachmentsList').html(html);
}

function removePendingAttachment(index) {
    pendingAttachments.splice(index, 1);
    renderPendingAttachments();
}

function updateCardAttachmentBadge(taskId, count) {
    const card = $(`.task-card[data-id="${taskId}"]`);
    card.find('.attachment-badge')
        .attr('title', `${count} attachment(s)`)
        .find('.attachment-count').text(count);
}

// open modal for editing
function openEditModal(btn) {
    currentMode = 'edit';
    const card = $(btn).closest('.task-card');
    const taskId = card.data('id');

    $('#taskModalTitle').text('Edit Task');
    $('#saveTaskBtnText').text('Save Changes');
    $('#taskId').val(taskId);
    $('#taskTitle').val(card.data('task-title'));
    $('#taskDescription').val(card.data('task-description'));
    $('#uploadError').hide();
    dueDatePicker.set('minDate', null); // allow past dates when editing
    dueDatePicker.setDate(card.data('task-duedate'), true);

    $('#taskTitleError').text('');
    $('#taskDateError').text('');

    $('#attachmentsSection').show();
    loadAttachments(taskId);


    new bootstrap.Modal(document.getElementById('taskModal')).show();
}

$('#taskModal').on('hidden.bs.modal', function () {
    if (currentMode === 'create') {
        pendingAttachments = [];
    }
});

// function restoreUploadZone() {
//     $('#uploadZone').html(`
//         <input type="file" id="fileInput" hidden />
//         <svg width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.5" viewBox="0 0 24 24">
//             <path stroke-linecap="round" stroke-linejoin="round" d="M12 16v-8m0 0l-3 3m3-3l3 3M3 16.2V18a2 2 0 002 2h14a2 2 0 002-2v-1.8" />
//         </svg>
//         <span>Click to upload a file</span>
//     `);
// }

function fileIconSvg() {
    return `<svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/>
        <path stroke-linecap="round" stroke-linejoin="round" d="M14 2v6h6"/>
    </svg>`;
}

function loadAttachments(taskId) {
    $.ajax({
        url: '/Task/GetAttachments',
        type: 'GET',
        data: { taskId },
        success: function (attachments) {
            renderAttachmentsList(taskId, attachments);
        },
        error: function () {
            $('#attachmentsList').html('<span class="text-danger small">Failed to load attachments</span>');
        }
    });
}

function renderAttachmentsList(taskId, attachments) {
    if (!attachments || attachments.length === 0) {
        $('#attachmentsList').empty();
        return;
    }

    const html = attachments.map(a => {
        const canPreview = a.contentType && (
            a.contentType.startsWith('image/') || a.contentType === 'application/pdf'
        );

        const previewBtn = canPreview
            ? `<button class="attachment-preview-btn" onclick="previewAttachment(${taskId}, ${a.id})">Preview</button>`
            : '';

        return `
            <div class="attachment-item" data-attachment-id="${a.id}">
                <span class="attachment-item-name" onclick="downloadAttachment(${taskId}, ${a.id})">
                    ${fileIconSvg()} ${a.fileName}
                </span>
                <div class="attachment-item-actions">
                    ${previewBtn}
                    <button class="attachment-delete-btn" onclick="deleteAttachment(${taskId}, ${a.id}, this)">Remove</button>
                </div>
            </div>
        `;
    }).join('');

    $('#attachmentsList').html(html);
}

function previewAttachment(taskId, attachmentId) {
    window.open(`/Task/PreviewAttachment?taskId=${taskId}&attachmentId=${attachmentId}`, '_blank');
}

function downloadAttachment(taskId, attachmentId) {
    window.open(`/Task/DownloadAttachment?taskId=${taskId}&attachmentId=${attachmentId}`, '_blank');
}

let attachmentIdToDelete = null;
let taskIdForAttachmentDelete = null;
let attachmentBtnToRemove = null;

function deleteAttachment(taskId, attachmentId, btn) {
    taskIdForAttachmentDelete = taskId;
    attachmentIdToDelete = attachmentId;
    attachmentBtnToRemove = btn;
    new bootstrap.Modal(document.getElementById('deleteAttachmentModal')).show();
}

$('#confirmDeleteAttachmentBtn').on('click', function () {
    const btn = $(this);
    if (btn.prop('disabled')) return;
    btn.prop('disabled', true);

    $.ajax({
        url: `/Task/DeleteAttachment?taskId=${taskIdForAttachmentDelete}&attachmentId=${attachmentIdToDelete}`,
        type: 'POST',
        success: function () {
            $(attachmentBtnToRemove).closest('.attachment-item').remove();
            bootstrap.Modal.getInstance(document.getElementById('deleteAttachmentModal'))?.hide();
        },
        error: function () {
            showToast('Failed to remove attachment');
        },
        complete: function () {
            btn.prop('disabled', false);
        }
    });
});
    
// upload zone click → trigger file input (delegated so it survives re-render)
$('#attachmentsSection').on('click', '#uploadZone', function (e) {
    // Prevent handling clicks that originated from the file input itself
    if ($(e.target).is('#fileInput')) return;
    $('#fileInput').click();
});

// Prevent the file input's click event from bubbling back to the upload zone
$('#attachmentsSection').on('click', '#fileInput', function (e) {
    e.stopPropagation();
});

$('#attachmentsSection').on('change', '#fileInput', function () {
    const file = this.files[0];
    if (!file) return;

    if (file.size > 5 * 1024 * 1024) {
        $('#uploadError').text('File exceeds the 5MB limit').show();

        $('#fileInput').val('');
        return;
    }

    $('#uploadError').hide();

    if (currentMode === 'create') {
        pendingAttachments.push(file);
        renderPendingAttachments();
        $('#fileInput').val('');
        return;
    }

    const taskId = $('#taskId').val();

    const formData = new FormData();
    formData.append('file', file);
    formData.append('taskId', taskId);

    $.ajax({
        url: '/Task/UploadAttachment?taskId=' + taskId,
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function () {
            loadAttachments(taskId);
            $.get('/Task/GetAttachments', { taskId }, function (attachments) {
                updateCardAttachmentBadge(taskId, (attachments || []).length);
            });
        },
        error: function (xhr) {
            $('#uploadError').text(xhr.responseJSON?.message || 'Upload failed').show();
        },
        complete: function () {
            $('#fileInput').val('');
        }
    });
});

    

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

            //  re-apply whatever filter is currently active
            const activeCard = $('.stat-card.active')[0];
            if (activeCard) filterByStatus(activeCard);
        },
        error: function () {
            showToast('Failed to update status');
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

            const activeCard = $('.stat-card.active')[0];
            if (activeCard) filterByStatus(activeCard);
        },
        error: function () {
            showToast('Failed to delete task');
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
                <div class="task-card-footer-row">
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

                    <div class="task-card-actions">
                        <button class="btn-edit" onclick="openEditModal(this)" title="Edit">
                            <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7" />
                                <path stroke-linecap="round" stroke-linejoin="round" d="M18.5 2.5a2.121 2.121 0 113 3L12 15l-4 1 1-4 9.5-9.5z" />
                            </svg>
                        </button>
                        <button class="btn-delete" onclick="deleteTask(${task.id}, this)" title="Delete">
                            <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M3 6h18M8 6V4a2 2 0 012-2h4a2 2 0 012 2v2m3 0l-1 14a2 2 0 01-2 2H7a2 2 0 01-2-2L4 6h16z" />
                            </svg>
                        </button>
                    </div>
                </div>

                <div class="task-card-footer-row task-card-meta-row">
                    <span class="task-date">
                        <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                            <rect x="3" y="4" width="18" height="18" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/>
                        </svg>
                        ${dateFormatted}
                    </span>
                    <span class="attachment-badge" title="${task.attachmentCount} attachment(s)">
                        <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M21.44 11.05l-9.19 9.19a6 6 0 01-8.49-8.49l9.19-9.19a4 4 0 015.66 5.66l-9.2 9.19a2 2 0 01-2.83-2.83l8.49-8.48" />
                        </svg>
                        <span class="attachment-count">${task.attachmentCount}</span>
                    </span>
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


// ───── Quick add ─────

let quickAddPendingData = null;

$('#quickAddInput').on('keypress', function (e) {
    if (e.which !== 13) return; // Enter key only

    const title = $(this).val().trim();
    if (!title) return;

    const isDuplicate = $('.task-card').toArray().some(card => {
        const cardTitle = $(card).attr('data-task-title').toLowerCase();
        return cardTitle === title.toLowerCase();
    });

    if (isDuplicate) {
        quickAddPendingData = { title };
        $('#duplicateTitleText').text(`A task named "${title}" already exists.`);
        new bootstrap.Modal(document.getElementById('duplicateConfirmModal')).show();
        return;
    }

    createQuickTask(title);
});

function createQuickTask(title) {
    const input = $('#quickAddInput');
    input.prop('disabled', true);

    $.ajax({
        url: '/Task/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            title: title,
            description: '',
            dueDate: toLocalDateTimeString(new Date())
        }),
        success: function (task) {
            $('.empty-state').remove();
            $('#taskGrid').prepend(buildCardHtml(task));
            sortTasksByDueDate($('#sortSelect').val() || 'dueDateAsc');
            refreshStats();
            input.val('');

            const activeCard = $('.stat-card.active')[0];
            if (activeCard) filterByStatus(activeCard);
        },
        error: function () {
            showToast('Failed to create task');
        },
        complete: function () {
            input.prop('disabled', false).focus();
        }
    });
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
        $.ajax({
            url: '/Task/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ title, description, dueDate }),
            success: function (task) {
                $('.empty-state').remove();
                $('#taskGrid').prepend(buildCardHtml(task));
                sortTasksByDueDate($('#sortSelect').val() || 'dueDateAsc');
                refreshStats();

                const activeCard = $('.stat-card.active')[0];
                if (activeCard) filterByStatus(activeCard);

                // now upload any pending files, in order, sequentially
                uploadPendingAttachmentsSequentially(task.id, 0, function () {
                    bootstrap.Modal.getInstance(document.getElementById('taskModal'))?.hide();
                    $('#saveTaskBtn').prop('disabled', false);
                    $('#saveTaskSpinner').hide();

                    updateCardAttachmentBadge(task.id, pendingAttachments.length); // ← count from what we just uploaded
                    pendingAttachments = [];
                });
            },
            error: function (xhr) {
                showToast('Failed to create task');
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

                const activeCard = $('.stat-card.active')[0];
                if (activeCard) filterByStatus(activeCard);
            },
            error: function () {
                showToast('Failed to update task');
            },
            complete: function () {
                $('#saveTaskBtn').prop('disabled', false);
                $('#saveTaskSpinner').hide();
            }
        });
    }
}

//uploads pending files one at a time, in array order
function uploadPendingAttachmentsSequentially(taskId, index, onComplete) {
    if (index >= pendingAttachments.length) {
        onComplete();
        return;
    }

    const file = pendingAttachments[index];
    const formData = new FormData();
    formData.append('file', file);
    formData.append('taskId', taskId);

    $.ajax({
        url: '/Task/UploadAttachment?taskId=' + taskId,
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function () {
            uploadPendingAttachmentsSequentially(taskId, index + 1, onComplete);
        },
        error: function (xhr) {
            showToast(`Failed to upload "${file.name}": ` + (xhr.responseJSON?.message || 'Upload failed'));
            // continue with the rest even if one fails, so one bad file doesn't block the others
            uploadPendingAttachmentsSequentially(taskId, index + 1, onComplete);
        }
    });
}

$('#confirmDuplicateBtn').on('click', function () {
    const modalEl = document.getElementById('duplicateConfirmModal');
    const modalInstance = bootstrap.Modal.getInstance(modalEl);
    if (modalInstance) modalInstance.hide();

    if (quickAddPendingData) {
        createQuickTask(quickAddPendingData.title);
        quickAddPendingData = null;
        return;
    }

    const title = $('#taskTitle').val().trim();
    const description = $('#taskDescription').val().trim();
    const dueDate = $('#taskDueDate').val();

    proceedWithSave(title, description, dueDate);
});

$('#duplicateConfirmModal').on('shown.bs.modal', function () {
    $('#confirmDuplicateBtn').focus(); // so Enter triggers it by default
});

$('#duplicateConfirmModal').on('keydown', function (e) {
    if (e.key === 'Enter') {
        e.preventDefault();
        $('#confirmDuplicateBtn').click();
    }
});

$('#duplicateConfirmModal').on('hidden.bs.modal', function () {
    if (quickAddPendingData) {
        quickAddPendingData = null; // user cancelled, clear pending quick-add
    }
});

function reopenTaskModal() {
    new bootstrap.Modal(document.getElementById('taskModal')).show();
}


// ───── recalculate stat cards ─────

function refreshStats() {
    let total = 0, pending = 0, inProgress = 0, done = 0;
    $('.task-card').each(function () {
        total++;
        const status = $(this).attr('data-task-status');
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


// ───── filter by status ─────
function filterByStatus(card) {
    const filter = $(card).data('filter');

    $('.stat-card').removeClass('active');
    $(card).addClass('active');

    if (!filter) {
        $('.task-card').show();
    } else {
        $('.task-card').each(function () {
            const status = $(this).attr('data-task-status');
            $(this).toggle(status === filter);
        });
    }
}