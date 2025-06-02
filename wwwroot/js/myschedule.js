document.addEventListener('DOMContentLoaded', function () {
    // Hàm tính số giờ còn lại đến lịch hẹn
    function getHoursDifference(appointmentTime) {
        const appointmentDate = new Date(appointmentTime);
        const now = new Date();
        return (appointmentDate - now) / (1000 * 60 * 60);
    }
    
    // Xử lý các hàng dữ liệu trong bảng
    const rows = document.querySelectorAll('.table tbody tr');
    rows.forEach(row => {
        const statusCell = row.querySelector('.status');
        const actionCell = row.querySelector('.action-cell');
        
        if (!statusCell || !actionCell) {
            console.error('Missing required elements');
            return;
        }
        
        const statusText = statusCell.textContent.trim();
        const appointmentTime = actionCell.dataset.appointmentTime;
        const appointmentId = actionCell.dataset.appointmentId;
        
        if (!appointmentTime || !appointmentId) {
            console.error('Missing appointment data');
            return;
        }
        
        // Kiểm tra trạng thái và xử lý tương ứng
        if (statusCell.querySelector('.badge.bg-warning')) {
            // Trạng thái đang chờ
            console.log('Đang chờ');
            // Không thêm nút nào, đã có nút trong HTML
        } 
        else if (statusCell.querySelector('.badge.bg-success')) {
            if (statusCell.textContent.includes('Đã hoàn thành')) {
                // Trạng thái đã hoàn thành
                checkFeedbackStatus(appointmentId, actionCell);
            } else {
                // Trạng thái đã xác nhận
                console.log('Đã xác nhận');
                // Không thêm nút nào, đã có nút trong HTML
            }
        }
        else if (statusCell.querySelector('.badge.bg-danger')) {
            // Trạng thái đã hủy
            console.log('Đã hủy');
            // Không thêm nút nào
        }
    });

    // Hàm tạo nút với chức năng tương ứng
    function createFeedbackButton(parent, appointmentId) {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'btn-action btn-feedback give-feedback';
        button.setAttribute('aria-label', 'Đánh giá');
        button.title = 'Đánh giá';
        button.innerHTML = '<i class="fas fa-star"></i>';

        button.onclick = function () {
            // Lấy thông tin lịch hẹn
            const appointmentTime = parent.dataset.appointmentTime;
            document.getElementById('appointmentId').value = appointmentId;
            
            // Tìm thông tin bác sĩ từ hàng hiện tại
            const row = parent.closest('tr');
            const doctorNameElement = row.querySelector('td:first-child .fw-medium');
            const doctorSpecialityElement = row.querySelector('td:nth-child(2) span');
            
            // Hiển thị thông tin bác sĩ
            if (doctorNameElement) {
                document.getElementById('doctorNameView').textContent = doctorNameElement.textContent;
            }
            
            if (doctorSpecialityElement) {
                document.getElementById('doctorSpecializationView').textContent = doctorSpecialityElement.textContent;
            }
            
            // Hiển thị thông tin thời gian
            const appointmentDate = new Date(appointmentTime);
            document.getElementById('appointmentDate').textContent = appointmentDate.toLocaleDateString('vi-VN', {
                weekday: 'long',
                year: 'numeric',
                month: '2-digit',
                day: '2-digit'
            });
            document.getElementById('appointmentTime').textContent = appointmentDate.toLocaleTimeString('vi-VN', {
                hour: '2-digit',
                minute: '2-digit'
            });
            
            // Hiển thị modal đánh giá
            var modal = new bootstrap.Modal(document.getElementById('feedbackModal'));
            modal.show();
        }
        
        parent.querySelector('.action-buttons').appendChild(button);
    }

    // Xử lý phần đánh giá bằng sao
    const stars = document.querySelectorAll('.rating i');
    let selectedRating = 0;

    stars.forEach(star => {
        star.addEventListener('click', function () {
            selectedRating = this.dataset.rating;
            document.getElementById('rating').value = selectedRating;
            highlightStars(selectedRating);
            
            // Cập nhật văn bản đánh giá
            const ratingText = document.querySelector('.rating-text');
            const ratingLabels = ['', 'Rất tệ', 'Tệ', 'Bình thường', 'Tốt', 'Rất tốt'];
            ratingText.textContent = ratingLabels[selectedRating] || 'Hãy chọn số sao để đánh giá';
        });
    });

    // Hàm đánh dấu sao được chọn
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

    // Xử lý gửi đánh giá
    document.getElementById('submitFeedback')?.addEventListener('click', function () {
        const form = document.getElementById('feedbackForm');
        
        // Kiểm tra biểu mẫu hợp lệ
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        const formData = new FormData(form);
        this.disabled = true;

        const modalEl = document.getElementById('feedbackModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        
        fetch(`/Patient/SubmitFeedback`, {
            method: 'POST',
            body: formData
        }).then(response => {
            console.log("Response", response);
            if (response.ok) {
                modal.hide();

                const backdrop = document.querySelector('.modal-backdrop');
                if (backdrop) {
                    backdrop.remove();
                }

                Swal.fire({
                    icon: 'success',
                    title: 'Cảm ơn bạn!',
                    text: 'Đánh giá của bạn đã được gửi thành công'
                });
                
                const appointmentId = formData.get('appointmentId');
                console.log("appointmentId ", appointmentId);
                
                if (appointmentId) {
                    const actionCell = document.querySelector(`.action-cell[data-appointment-id="${appointmentId}"]`);
                    if (actionCell) {
                        // Cập nhật giao diện sau khi đánh giá
                        const feedbackButton = actionCell.querySelector('.btn-feedback');
                        if (feedbackButton) {
                            feedbackButton.remove();
                        }
                        createViewFeedbackButton(actionCell);
                    }
                    else {
                        console.error('Không tìm thấy ô hành động cho lịch hẹn:', appointmentId);
                    }
                }
                else {
                    console.error('Không tìm thấy ID lịch hẹn trong dữ liệu biểu mẫu');
                }

            } else {
                throw new Error('Phản hồi mạng không thành công');
            }
        }).catch(error => {
            console.log('Lỗi', error);
            Swal.fire({
                icon: 'error',
                title: 'Rất tiếc...',
                text: 'Đã xảy ra lỗi! Vui lòng thử lại'
            });
        }).finally(() => {
            console.log('Hoàn tất');
            this.disabled = false;
        });
    });

    // Kiểm tra trạng thái đánh giá của lịch hẹn
    function checkFeedbackStatus(appointmentId, parent) {
        fetch(`/Patient/HasFeedback/${appointmentId}`)
        .then(response => {
            console.log(response);
            return response.json();
        })
        .then(hasFeedback => {
            if (hasFeedback) {
                createViewFeedbackButton(parent);
            }
            else {
                createFeedbackButton(parent, appointmentId);
            }
        })
        .catch(error => {
            console.error('Lỗi khi kiểm tra trạng thái đánh giá:', error);
            // Mặc định hiển thị nút đánh giá nếu có lỗi
            createFeedbackButton(parent, appointmentId);
        });
    }

    // Tạo nút xem đánh giá
    function createViewFeedbackButton(parent) {
        const viewButton = document.createElement('button');
        viewButton.type = 'button';
        viewButton.className = 'btn-action btn-view view-feedback';
        viewButton.setAttribute('aria-label', 'Xem đánh giá');
        viewButton.title = 'Xem đánh giá';
        viewButton.innerHTML = '<i class="fas fa-comment-dots"></i>';
        
        viewButton.onclick = function () {
            const appointmentId = parent.dataset.appointmentId;
            fetch(`/Patient/GetFeedback/${appointmentId}`)
                .then(response => {
                    if (!response.ok) throw new Error('Phản hồi mạng không thành công');
                    return response.json();
                }).then(data => {
                    // Cập nhật thông tin modal xem đánh giá
                    document.getElementById('doctorName').textContent = data.doctorName;
                    document.getElementById('doctorSpecialization').textContent = data.doctorSpecialization;
                    document.getElementById('overallRating').innerHTML = createStarRating(data.rating);
                    document.getElementById('professionalRating').innerHTML = createStarRating(data.professionalSkills);
                    document.getElementById('communicationRating').innerHTML = createStarRating(data.communication);
                    document.getElementById('feedbackComment').textContent = data.comment;
                    document.getElementById('feedbackDate').textContent = new Date(data.createdAt)
                        .toLocaleDateString('vi-VN', {
                            year: 'numeric',
                            month: 'long',
                            day: 'numeric',
                            hour: '2-digit',
                            minute: '2-digit'
                        });
                    
                    // Hiển thị modal xem đánh giá
                    const modal = new bootstrap.Modal(document.getElementById("viewFeedbackModal"));
                    modal.show();
                }).catch(error => {
                    console.error('Lỗi khi tải dữ liệu đánh giá', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: 'Không thể tải dữ liệu đánh giá'
                    });
                });
        };
        
        parent.querySelector('.action-buttons').appendChild(viewButton);
    }

    // Hàm tạo hiển thị đánh giá sao
    function createStarRating(rating) {
        return Array(5).fill(0)
            .map((_, index) => `<i class="fas fa-star${index < rating ? ' text-warning' : ' text-muted'}"></i>`)
            .join('');
    }

    // Xử lý sự kiện khi hiển thị modal đánh giá
    const feedbackModal = document.getElementById('feedbackModal');
    if (feedbackModal) {
        feedbackModal.addEventListener('shown.bs.modal', function () {
            const closeButton = this.querySelector('.btn-close');
            if (closeButton) {
                closeButton.focus();
            }
        });
    }

    // Xử lý nút xem chi tiết
    document.querySelectorAll('.view-appointment').forEach(button => {
        button.addEventListener('click', function() {
            const appointmentId = this.closest('.action-cell').dataset.appointmentId;
            if (appointmentId) {
                // Chuyển hướng đến trang chi tiết
                window.location.href = `/Patient/DiagnosisDetail?id=${appointmentId}`;
            }
        });
    });

    // Xử lý nút hủy lịch hẹn
    document.querySelectorAll('.cancel-appointment').forEach(button => {
        button.addEventListener('click', function() {
            const appointmentId = this.closest('.action-cell').dataset.appointmentId;
            if (appointmentId) {
                Swal.fire({
                    title: 'Xác nhận hủy lịch hẹn?',
                    text: 'Bạn có chắc chắn muốn hủy lịch hẹn này?',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    confirmButtonText: 'Đồng ý',
                    cancelButtonText: 'Không'
                }).then((result) => {
                    if (result.isConfirmed) {
                        // Sử dụng FormData thay vì JSON để tương thích với controller
                        fetch(`/Patient/CancelAppointment/${appointmentId}`, {
                            method: 'POST',
                            headers: {
                                'X-Requested-With': 'XMLHttpRequest',
                                'Content-Type': 'application/x-www-form-urlencoded'
                            }
                        })
                        .then(response => {
                            // Check content type to handle potential HTML response
                            const contentType = response.headers.get('content-type');
                            if (contentType && contentType.includes('application/json')) {
                                return response.json().then(data => {
                                    return { ok: response.ok, data };
                                });
                            } else {
                                // Handle non-JSON response
                                return response.text().then(text => {
                                    // If response is not ok and not JSON, treat as error
                                    if (!response.ok) {
                                        throw new Error('Server returned an error response');
                                    }
                                    // Successful but not JSON - assume operation successful
                                    return { ok: true, data: { success: true } };
                                });
                            }
                        })
                        .then(result => {
                            if (result.ok && result.data.success) {
                                Swal.fire(
                                    'Đã hủy!',
                                    'Lịch hẹn của bạn đã được hủy thành công.',
                                    'success'
                                ).then(() => {
                                    // Làm mới trang sau khi hủy thành công
                                    window.location.reload();
                                });
                            } else {
                                throw new Error(result.data?.message || 'Lỗi khi hủy lịch hẹn');
                            }
                        })
                        .catch(error => {
                            console.error('Error canceling appointment:', error);
                            Swal.fire(
                                'Lỗi!',
                                'Đã xảy ra lỗi khi hủy lịch hẹn. Vui lòng thử lại sau.',
                                'error'
                            );
                        });
                    }
                });
            }
        });
    });
});