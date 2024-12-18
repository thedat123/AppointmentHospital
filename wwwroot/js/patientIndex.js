// document.addEventListener('DOMContentLoaded', function () {
//     const carouselInner = document.getElementById('carouselInner');
//     const items = Array.from(carouselInner.getElementsByClassName('carousel-item'));
//     let currentIndex = 0;

//     // Hiển thị 4 bác sĩ
//     function showDoctors() {
//         items.forEach((item, index) => {
//             item.style.display = 'none'; // Ẩn tất cả bác sĩ
//         });

//         // Hiển thị 4 bác sĩ bắt đầu từ currentIndex
//         for (let i = 0; i < 4; i++) {
//             const index = (currentIndex + i) % items.length; // Lặp lại nếu vượt quá số lượng bác sĩ
//             items[index].style.display = 'block'; // Hiển thị bác sĩ
//         }
//     }

//     // Cập nhật chỉ số và hiển thị bác sĩ
//     function updateCarousel(direction) {
//         if (direction === 'next') {
//             currentIndex = (currentIndex + 1) % items.length; // Tăng chỉ số
//         } else {
//             currentIndex = (currentIndex - 1 + items.length) % items.length; // Giảm chỉ số
//         }
//         showDoctors(); // Cập nhật hiển thị bác sĩ
//     }

//     document.querySelector('.carousel-control-next').addEventListener('click', function () {
//         updateCarousel('next');
//     });

//     document.querySelector('.carousel-control-prev').addEventListener('click', function () {
//         updateCarousel('prev');
//     });

//         // Hiển thị lần đầu tiên
//         showDoctors();

//         // Bắt sự kiện khi nhấn nút "Next"
//         document.querySelector('.carousel-control-next').addEventListener('click', function () {
//             updateCarousel('next');
//         });
    
//         // Bắt sự kiện khi nhấn nút "Previous"
//         document.querySelector('.carousel-control-prev').addEventListener('click', function () {
//             updateCarousel('prev');
//         });
//     });