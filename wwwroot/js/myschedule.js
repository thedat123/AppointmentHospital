document.addEventListener('DOMContentLoaded', function(){
    console.log("Run myschedule.js");
    function getHoursDifference(appointmentTime) {
        const appointmentDate = new Date(appointmentTime);
        const now = new Date();
        return (appointmentDate-now) / (1000*60*60);
    }
    const rows = document.querySelectorAll('.table tbody tr');
    rows.forEach(row => {
        const statusSpan = row.querySelector('.status span');
        const actionCell = row.querySelector('.action');
        if(!statusSpan || !actionCell) {
            console.error('Missing required elements');
            return;
        }
        const appointmentTime = actionCell.dataset.appointmentTime;
        const appointmentId = actionCell.dataset.appointmentId;
        if(!appointmentTime || !appointmentId ){
            console.error('Missing appointment data');
            return;
        }
        switch(statusSpan.textContent.trim()){
            case 'Pending': 
                console.log('Pending start');
                var hoursDiff = getHoursDifference(appointmentTime);
                if(hoursDiff < 24 && hoursDiff > 0){
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
                createButton(actionCell, "Feedback");
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
    
        if(text === 'Feedback') {
            button.onclick = function (){
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
        star.addEventListener('click', function() {
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
    document.getElementById('submitFeedback').addEventListener('click', function() {
        const form = document.getElementById('feedbackForm');
        const formData = new FormData(form);
        this.disabled = true;

        // Store reference to modal
        const modalEl = document.getElementById('feedbackModal');
        const modal = bootstrap.Modal.getInstance(modalEl);

        fetch(`/Patient/SubmitFeedback`, {
            method: 'POST',
            body: formData
        }).then(response => {
            console.log("Response",response.ok())
            if(response.ok) {
                // Properly hide modal
                modal.hide();
                
                // Remove modal backdrop if present
                const backdrop = document.querySelector('.modal-backdrop');
                if(backdrop) {
                    backdrop.remove();
                }

                Swal.fire({
                    icon: 'success',
                    title: 'Thank you!',
                    text: 'Your feedback has been submitted successfully'
                });
                const actionCell = document.querySelector(`.action[data-appointment-id=${formData.get('appointmentId')}]`);
                actionCell.innerHTML = '';
                createViewFeedbackButton(actionCell);
            } else {
                throw new Error('Network response was not ok');
            }
        }).catch(error => {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Something went wrong! Please try again'
            });
        }).finally(() => {
            this.disabled = false;
        });
    });

    function createViewFeedbackButton(parent) {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'btn btn-success btn-sm';
        button.setAttribute('aria-label', 'View feedback details');
        button.innerHTML = `<i class="fas fa-eye" role="presentation"></i> View Feedback`;
        button.onclick = function () {
            var appointmentId = parent.dataset.appointmentId;
            fetch(`/Patient/GetFeedback/${appointmentId}`)
            .then(response => response.json())
            .then(data => {
                displayFeedback(data);
                const modal = new bootstrap.Modal(document.getElementById('viewFeedbackModal'));
                modal.show();
            }).catch(error => {
                Swal.fire({
                    icon: 'error',
                    title: 'Oops...',
                    text: 'Could not load feedback'
                });
            });
        }
        parent.appendChild(button);
    }
    function displayFeedback(data) {
        document.getElementById('doctorName').textContent = data.doctorName;
        document.getElementById('doctorSpecialization').textContent = data.doctorSpecialization;
    
        document.getElementById('overallRating').innerHTML = createStarRating(data.rating);
        document.getElementById('professionalRating').innerHTML = createStarRating(data.professionalSkills);
        document.getElementById('communicationRating').innerHTML = createStarRating(data.communication);
    
        document.getElementById('feedbackComment').textContent = data.comment;
        document.getElementById('feedbackDate').textContent = new Date(data.createdAt).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }
    function createStarRating(rating) {
        return Array(5).fill(0).map((_, index) => 
            `<i class="fas fa-star${index < rating ? ' text-warning' : ' text-muted'}"></i>`
        ).join('');
    }
    const feedbackModal = document.getElementById('feedbackModal');
    if (feedbackModal) {
        feedbackModal.addEventListener('shown.bs.modal', function() {
            const closeButton = this.querySelector('.btn-close');
            if (closeButton) {
                closeButton.focus();
            }
        });
    }
})
