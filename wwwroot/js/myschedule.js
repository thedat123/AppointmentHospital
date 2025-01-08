document.addEventListener('DOMContentLoaded', function () {
    console.log("Run myschedule.js");
    function getHoursDifference(appointmentTime) {
        const appointmentDate = new Date(appointmentTime);
        const now = new Date();
        return (appointmentDate - now) / (1000 * 60 * 60);
    }
    const rows = document.querySelectorAll('.table tbody tr');
    rows.forEach(row => {
        const statusSpan = row.querySelector('.status span');
        const actionCell = row.querySelector('.action');
        if (!statusSpan || !actionCell) {
            console.error('Missing required elements');
            return;
        }
        const appointmentTime = actionCell.dataset.appointmentTime;
        const appointmentId = actionCell.dataset.appointmentId;
        if (!appointmentTime || !appointmentId) {
            console.error('Missing appointment data');
            return;
        }
        switch (statusSpan.textContent.trim()) {
            case 'Pending':
                console.log('Pending start');
                var hoursDiff = getHoursDifference(appointmentTime);
                if (hoursDiff < 24 && hoursDiff > 0) {
                    const form = document.createElement('form');
                    form.method = 'post';
                    form.action = `/Patient/CancelAppointment/${actionCell.dataset.appointmentId}`;
                    form.innerHTML = ` <button type="submit" class="btn btn-danger btn-sm">
                                                                    <i class="fas fa-times-circle"></i> Cancel
                                                                </button>`;
                    actionCell.appendChild(form);
                }
                else {
                    createButton(actionCell, "Cancel");
                }
                break;
            case 'Confirmed':
                createButton(actionCell, "Cancel");
                break;

            case 'Cancelled':
                createButton(actionCell, "Cancelled");
                break;
            case 'Completed':
                checkFeedbackStatus(appointmentId, actionCell);
                break;

        }

    });
    function createButton(parent, text) {
        const button = document.createElement('button');
        button.className = text === "Feedback" ? "btn btn-info btn-sm" : "btn btn-danger btn-sm";
        button.type = 'button';
        button.disabled = text == "Feedback" ? false : true;
        button.style.opacity = text == "Feedback" ? '1' : '0.5';

        // Add proper ARIA labels
        button.setAttribute('aria-label', text === 'Feedback' ? 'Give feedback' : text);

        // Use role="presentation" for icons
        button.innerHTML = text == 'Feedback' ?
            `<i class="fa fa-commenting" role="presentation"></i> ${text}` :
            `<i class="fas fa-times-circle" role="presentation"></i> ${text}`;

        if (text === 'Feedback') {
            button.onclick = function () {
                const appointmentId = parent.dataset.appointmentId;
                const appointmentTime = parent.dataset.appointmentTime;
                document.getElementById('appointmentId').value = appointmentId;
                const appointmentDate = new Date(appointmentTime);
                document.getElementById('appointmentDate').textContent = appointmentDate.toLocaleDateString();
                document.getElementById('appointmentTime').textContent = appointmentDate.toLocaleTimeString();
                var modal = new bootstrap.Modal(document.getElementById('feedbackModal'));
                modal.show();
            }
        }
        parent.appendChild(button);
    }
    const stars = document.querySelectorAll('.rating i');
    let selectedRating = 0;

    stars.forEach(star => {
        star.addEventListener('click', function () {
            selectedRating = this.dataset.rating;
            document.getElementById('rating').value = selectedRating;
            highlightStars(selectedRating);
        });
    });

    function highlightStars(rating) {
        stars.forEach(star => {
            const starRating = star.dataset.rating;
            if (starRating <= rating) {
                star.classList.remove('far');
                star.classList.add('fas');
                star.style.color = '#ffc107';
            } else {
                star.classList.remove('fas');
                star.classList.add('far');
                star.style.color = '#ccc';
            }
        });
    };
    document.getElementById('submitFeedback').addEventListener('click', function () {
        const form = document.getElementById('feedbackForm');
        const formData = new FormData(form);
        this.disabled = true;

        // Store reference to modal
        const modalEl = document.getElementById('feedbackModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        console.log("a");
        console.log("b");
        console.log("Call API");
        fetch(`/Patient/SubmitFeedback`, {
            method: 'POST',
            body: formData
        }).then(response => {
            console.log("Response", response)
            if (response.ok) {
                // Properly hide modal
                modal.hide();

                // Remove modal backdrop if present
                const backdrop = document.querySelector('.modal-backdrop');
                if (backdrop) {
                    backdrop.remove();
                }

                Swal.fire({
                    icon: 'success',
                    title: 'Thank you!',
                    text: 'Your feedback has been submitted successfully'
                });
                const appointmentId = formData.get('appointmentId');
                console.log("appointmentId ", appointmentId)
                if (appointmentId) {
                    const actionCell = document.querySelector(`.action[data-appointment-id="${appointmentId}"]`);
                    if (actionCell) {
                        actionCell.innerHTML = '';
                        console.log(actionCell);
                        createViewFeedbackButton(actionCell);
                    }
                    else {
                        console.error('Action cell not found for appointment:', appointmentId);
                    }
                }
                else {
                    console.error('No appointment ID found in form data');
                }

            } else {
                throw new Error('Network response was not ok');
            }
        }).catch(error => {
            console.log('Error', error);
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Something went wrong! Please try again'
            });
        }).finally(() => {
            console.log('Finally');
            this.disabled = false;
        });
    });

    function checkFeedbackStatus(appointmentId, parent) {
        console.log("AppointmentIDDDDD", appointmentId);
        fetch(`/Patient/HasFeedback/${appointmentId}`)
        .then(response => {
            console.log(response);
            return response.json();
        })
        .then(hasFeedback => {
            console.log(hasFeedback);
            if(hasFeedback) {
                createViewFeedbackButton(parent);
            }
            else {
                createButton(parent, 'Feedback');
            }
        })
        .catch(error => {
            console.error('Error checking feedback status:', error);
            createFeedbackButton(parent); // Default to feedback button
        });
    }

    function createViewFeedbackButton(parent) {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'btn btn-success btn-sm';
        button.setAttribute('aria-label', 'View feedback details');
        button.innerHTML = `<i class="fas fa-eye" role="presentation"></i> View Feedback`;
        button.onclick = function () {
            const appointmentId = parent.dataset.appointmentId;
            fetch(`/Patient/GetFeedback/${appointmentId}`)
                .then(response => {
                    if (!response.ok) throw new Error('Network response was not ok');
                    return response.json();
                }).then(data => {
                    const doctorName = document.getElementById('doctorNameView');
                    const doctorSpec = document.getElementById('doctorSpecializationView');

                    if (!doctorName || !doctorSpec) {
                        console.error('Doctor info elements not found');
                        return;
                    }

                    // Update modal content
                    doctorName.textContent = data.doctorName;
                    doctorSpec.textContent = data.doctorSpecialization;
                    document.getElementById('overallRating').innerHTML = createStarRating(data.rating);
                    document.getElementById('professionalRating').innerHTML = createStarRating(data.professionalSkills);
                    document.getElementById('communicationRating').innerHTML = createStarRating(data.communication);
                    document.getElementById('feedbackComment').textContent = data.comment;
                    document.getElementById('feedbackDate').textContent = new Date(data.createdAt)
                        .toLocaleDateString('en-US', {
                            year: 'numeric',
                            month: 'long',
                            day: 'numeric',
                            hour: '2-digit',
                            minute: '2-digit'
                        });
                    const modal = new bootstrap.Modal(document.getElementById("viewFeedbackModal"));
                    modal.show();
                }).catch(error => {
                    console.error('Error fetching feedback', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'Could not load feedback data'
                    });
                });

        };
        parent.appendChild(button);
    }

    function createStarRating(rating) {
        return Array(5).fill(0)
            .map((_, index) => `<i class="fas fa-star${index < rating ? ' text-warning' : ' text-muted'}"></i>`)
            .join('');
    }

    const feedbackModal = document.getElementById('feedbackModal');
    if (feedbackModal) {
        feedbackModal.addEventListener('shown.bs.modal', function () {
            const closeButton = this.querySelector('.btn-close');
            if (closeButton) {
                closeButton.focus();
            }
        });
    }
})
