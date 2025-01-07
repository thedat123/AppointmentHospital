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
        console.log('Status', statusSpan.textContent);
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
                console.log('Hour diff', hoursDiff);
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
               button.className  = text === "Feedback" ? "btn btn-info btn-sm" : "btn btn-danger btn-sm"
               button.type = 'button';
               button.disabled = text == "Feedback" ? false : true;
               button.style.opacity = text == "Feedback" ?  '1' : '0.5';
               button.innerHTML = text == 'Feedback' ? `<i class="fa fa-commenting" aria-hidden="true"></i>${text}` :`<i class="fas fa-times-circle"></i>${text}`; 
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
    }
})
