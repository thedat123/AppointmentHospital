// Chọn các phần tử DOM
const radioButton = document.querySelectorAll('input[name="statisticType"]');
const statisticTable = document.getElementById('statistic-table');
const weekInput = document.getElementById('weekInput');
const dayInput = document.getElementById('dayInput');
const monthInput = document.getElementById('monthInput');
const yearInput = document.getElementById('yearInput');
const rangeInput = document.getElementById('rangeInput');

// Biến biểu đồ
let oldAndNewUserChart;
let amountAppointmentChart;
let topDoctorAmountAppointment;
let compareAmountAppointment;
let compareOldAndNewUser;
// Khởi tạo SignalR và tải toàn bộ dữ liệu mặc định
initializeSignalR();
fetchAndDisplayAllData();

// Thiết lập SignalR cho cập nhật thời gian thực
function initializeSignalR() {
    try {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/scheduleHub")
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Debug)
            .build();

        connection.onreconnecting((error) => console.log('Reconnecting:', error));
        connection.onreconnected((connectionId) => console.log('Reconnected:', connectionId));
        connection.onclose((error) => console.log('Connection closed:', error));

        connection.on('UpdateStatistics', async () => {
            console.log("Received update statistic event from server");
            await fetchAndDisplayAllData();
        });

        connection.start().then(() => console.log("SignalR Connected successfully"));
    } catch (error) {
        console.error("SignalR Connection Error:", error);
    }
}

// Lấy danh sách tuần theo năm được chọn
document.getElementById('yearSelect').addEventListener('change', async function () {
    const year = this.value;
    try {
        const response = await fetch(`/Statistic/GetWeeksByYear?year=${year}`);
        if (!response.ok) throw new Error('Phản hồi mạng không thành công');
        const weeks = await response.json();
        console.log('weeks', weeks);
        const weekSelect = document.getElementById('weekSelect');
        weekSelect.innerHTML = '<option disabled selected value="">Chọn tuần</option>';
        weeks.forEach(week => {
            const option = document.createElement('option');
            option.value = week.value;
            option.textContent = week.text;
            weekSelect.appendChild(option);
        });
    } catch (error) {
        console.log('Error fetching weeks', error);
    }
});

// Lấy bộ lọc ngày hiện tại (mặc định là "tất cả")
function getCurrentDateFilter() {
    const singleDate = document.getElementById("singleDate").value;
    const monthSelect = document.getElementById("monthSelect").value;
    const weekSelect = document.getElementById("weekSelect").value;
    const startDate = document.getElementById("startDate").value;
    const endDate = document.getElementById("endDate").value;
    const yearSelect = document.getElementById("yearSelect").value;

    let dateFilter = null;
    let filterType = 'all'; // Mặc định là tất cả dữ liệu
    if (startDate && endDate) {
        dateFilter = `${startDate}|${endDate}`;
        filterType = 'custom';
    } else if (singleDate) {
        dateFilter = singleDate;
        filterType = 'day';
    } else if (monthSelect) {
        dateFilter = monthSelect;
        filterType = 'month';
    } else if (weekSelect) {
        dateFilter = weekSelect;
        filterType = 'week';
    }
    console.log(`dateFilter: ${dateFilter}, filterType: ${filterType}`);
    return { dateFilter, filterType, yearSelect };
}

// Lấy và hiển thị toàn bộ dữ liệu lịch hẹn
async function fetchAndDisplayAllData() {
    statisticTable.style.display = 'block';
    const { dateFilter, filterType, yearSelect } = getCurrentDateFilter();
    let url = '/Statistic/Statistic?';
    if (filterType === 'day') {
        url = `${url}singleDate=${encodeURIComponent(dateFilter)}`;
    } else if (filterType === 'week') {
        url = `${url}week=${encodeURIComponent(dateFilter)}&year=${yearSelect}`;
    } else if (filterType === 'month') {
        url = `${url}month=${encodeURIComponent(dateFilter)}&year=${yearSelect}`;
    } else if (filterType === 'custom') {
        url = `${url}dateRange=${encodeURIComponent(dateFilter)}`;
    }

    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
        const data = await response.json();
        console.log('Fetched data:', data);

        // Xóa khóa $id khỏi các đối tượng
        const removeIdKey = (obj) => {
            if (!obj || typeof obj !== 'object') return obj;
            return Object.entries(obj)
                .filter(([key]) => key !== '$id')
                .reduce((acc, [key, value]) => {
                    acc[key] = value;
                    return acc;
                }, {});
        };

        // Xử lý dữ liệu cho biểu đồ
        const amountData = removeIdKey(data.amountAppointment || {});
        const oldAndNewUserData = removeIdKey(data.oldAndNewUser || {});
        const topDoctorData = removeIdKey(data.topDoctorAppointment || {});
        const compareAmountData = removeIdKey(data.compareAmountAppointment || {});
        const compareOldNewData = removeIdKey(data.compareAmountOldAndNewUser || {});

        // Hủy các biểu đồ hiện có
        [oldAndNewUserChart, amountAppointmentChart, topDoctorAmountAppointment, 
         compareAmountAppointment, compareOldAndNewUser].forEach(chart => chart?.destroy());

        // Chuẩn bị dữ liệu cho tất cả lịch hẹn
        const allAmountLabels = Object.keys(amountData).length > 0 ? Object.keys(amountData) : ['Không có dữ liệu'];
        const allAmountValues = Object.values(amountData).length > 0 ? Object.values(amountData) : [0];

        // Chuẩn bị dữ liệu người dùng cũ và mới
        const allOldNewLabels = Object.keys(oldAndNewUserData).length > 0 ? Object.keys(oldAndNewUserData) : ['Không có dữ liệu'];
        const oldUserValues = allOldNewLabels.map(key => oldAndNewUserData[key]?.$values?.[0] || 0);
        const newUserValues = allOldNewLabels.map(key => oldAndNewUserData[key]?.$values?.[1] || 0);

        // Chuẩn bị dữ liệu bác sĩ hàng đầu
        const allDoctorData = topDoctorData['All Data']?.$values || Object.values(topDoctorData).flatMap(v => v.$values || []);
        const doctorLabels = allDoctorData.length > 0 
            ? allDoctorData.map(d => d.specialityName || d.specialization || 'Không xác định')
            : ['Không có dữ liệu'];
        const doctorNames = allDoctorData.length > 0 
            ? allDoctorData.map(d => d.doctorName || 'Không xác định')
            : ['Không có dữ liệu'];
        const doctorAmounts = allDoctorData.length > 0 
            ? allDoctorData.map(d => d.appointmentAmount || 0)
            : [0];

        // Chuẩn bị dữ liệu so sánh
        const compareAmountLabels = Object.keys(compareAmountData).length > 0 
            ? Object.keys(compareAmountData) : ['Không có dữ liệu'];
        const compareAmountValues = Object.values(compareAmountData).length > 0 
            ? Object.values(compareAmountData) : [0];

        // Chuẩn bị so sánh người dùng cũ và mới
        const compareEntries = Object.entries(compareOldNewData).filter(([key]) => key !== '$id' && key);
        let compareLabels = ['Không có dữ liệu'];
        let compareOldUsers = [0];
        let compareNewUsers = [0];
        if (compareEntries.length > 0) {
            compareEntries.sort(([keyA], [keyB]) => {
                const numA = Number(keyA);
                const numB = Number(keyB);
                return !isNaN(numA) && !isNaN(numB) ? numA - numB : keyA.localeCompare(keyB);
            });
            compareLabels = compareEntries.map(([key]) => 
                !isNaN(Number(key)) && Number(key) >= 1 && Number(key) <= 12 ? `Tháng ${key}` : key
            );
            compareOldUsers = compareEntries.map(([_, value]) => value.$values?.[0] || 0);
            compareNewUsers = compareEntries.map(([_, value]) => value.$values?.[1] || 0);
        }

        // Tạo biểu đồ
        amountAppointmentChart = createAmountAppointmentChart(allAmountLabels, allAmountValues);
        oldAndNewUserChart = createOldAndNewUserChart(allOldNewLabels, oldUserValues, newUserValues);
        topDoctorAmountAppointment = createTopDoctorChart(doctorLabels, doctorNames, doctorAmounts);
        compareAmountAppointment = createCompareAmountChart(compareAmountLabels, compareAmountValues);
        compareOldAndNewUser = createCompareOldNewChart(compareLabels, compareOldUsers, compareNewUsers);

    } catch (error) {
        console.error('Error fetching statistics:', error);
        statisticTable.innerHTML = '<p>Lỗi tải dữ liệu. Vui lòng thử lại.</p>';
    }
}

// Hàm tạo biểu đồ
function createAmountAppointmentChart(labels, data) {
    return new Chart(document.getElementById("amountAppointment"), {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: "Lịch hẹn",
                data: data,
                backgroundColor: 'rgba(75, 192, 192, 0.6)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1,
                barPercentage: 0.7
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: { duration: 1000, easing: 'easeInOutQuart' },
            plugins: {
                title: { display: true, text: 'Tổng số lịch hẹn theo thời gian', font: { size: 16, weight: 'bold' } },
                legend: { position: 'bottom' },
                tooltip: { mode: 'index', intersect: false }
            },
            scales: {
                y: { beginAtZero: true, title: { display: true, text: 'Số lượng lịch hẹn' } },
                x: { title: { display: true, text: 'Thời kỳ' } }
            }
        }
    });
}

function createOldAndNewUserChart(labels, oldData, newData) {
    return new Chart(document.getElementById("oldAndNewUser"), {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: "Người dùng cũ",
                    backgroundColor: 'rgba(255, 99, 132, 0.6)',
                    borderColor: 'rgba(255, 99, 132, 1)',
                    borderWidth: 1,
                    data: oldData,
                    barPercentage: 0.7
                },
                {
                    label: "Người dùng mới",
                    backgroundColor: 'rgba(54, 162, 235, 0.6)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1,
                    data: newData,
                    barPercentage: 0.7
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: { duration: 1000, easing: 'easeInOutQuart' },
            plugins: {
                title: { display: true, text: 'Người dùng mới và cũ theo thời gian', font: { size: 16, weight: 'bold' } },
                legend: { position: 'bottom' },
                tooltip: { mode: 'index', intersect: false }
            },
            scales: {
                y: { beginAtZero: true, title: { display: true, text: 'Số lượng người dùng' } },
                x: { title: { display: true, text: 'Thời kỳ' } }
            }
        }
    });
}

function createTopDoctorChart(labels, doctorNames, data) {
    return new Chart(document.getElementById("topDoctorAmountAppointment"), {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Lịch hẹn',
                data: data,
                backgroundColor: 'rgba(54, 162, 235, 0.6)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 1
            }]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const index = context.dataIndex;
                            return `${doctorNames[index]}: ${data[index]} lịch hẹn`;
                        }
                    }
                },
                title: { display: true, text: 'Bác sĩ hàng đầu theo số lịch hẹn', font: { size: 16, weight: 'bold' } },
                legend: { position: 'bottom' }
            },
            scales: {
                x: { beginAtZero: true, title: { display: true, text: 'Số lượng lịch hẹn' } },
                y: { title: { display: true, text: 'Chuyên môn' } }
            }
        }
    });
}

function createCompareAmountChart(labels, data) {
    return new Chart(document.getElementById("compareAmountAppointment"), {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Lịch hẹn',
                data: data,
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 2,
                tension: 0.4,
                fill: true
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                title: { display: true, text: 'Xu hướng so sánh số lượng lịch hẹn', font: { size: 16, weight: 'bold' } },
                legend: { position: 'bottom' },
                tooltip: { mode: 'index', intersect: false }
            },
            scales: {
                y: { beginAtZero: true, title: { display: true, text: 'Số lượng lịch hẹn' } },
                x: { title: { display: true, text: 'Thời kỳ' } }
            },
            animations: { tension: { duration: 1000, easing: 'easeOutQuad' } }
        }
    });
}

function createCompareOldNewChart(labels, oldData, newData) {
    return new Chart(document.getElementById("compareOldAndNewUser"), {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: "Người dùng mới",
                    data: newData,
                    borderColor: 'rgba(75, 192, 192, 1)',
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    tension: 0.4,
                    fill: true
                },
                {
                    label: "Người dùng cũ",
                    data: oldData,
                    borderColor: 'rgba(255, 99, 132, 1)',
                    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                    tension: 0.4,
                    fill: true
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                title: { display: true, text: 'So sánh xu hướng người dùng mới và cũ', font: { size: 16, weight: 'bold' } },
                legend: { position: 'bottom' },
                tooltip: { mode: 'index', intersect: false }
            },
            scales: {
                y: { beginAtZero: true, title: { display: true, text: 'Số lượng người dùng' } },
                x: { title: { display: true, text: 'Thời kỳ' } }
            },
            animations: { tension: { duration: 1000, easing: 'linear' } }
        }
    });
}

// Logic cho nút radio để chọn bộ lọc
radioButton.forEach(radio => {
    radio.addEventListener('change', function () {
        const singleDate = document.getElementById("singleDate");
        const monthSelect = document.getElementById("monthSelect");
        const weekSelect = document.getElementById("weekSelect");
        const startDate = document.getElementById('startDate');
        const endDate = document.getElementById('endDate');

        statisticTable.style.display = 'none';
        dayInput.style.display = 'none';
        weekInput.style.display = 'none';
        monthInput.style.display = 'none';
        yearInput.style.display = 'none';
        rangeInput.style.display = 'none';

        if (this.value === 'day') {
            dayInput.style.display = 'block';
            monthSelect.value = null;
            weekSelect.value = null;
            startDate.value = null;
            endDate.value = null;
        } else if (this.value === 'week') {
            weekInput.style.display = 'block';
            yearInput.style.display = 'block';
            singleDate.value = null;
            monthSelect.value = null;
            startDate.value = null;
            endDate.value = null;
        } else if (this.value === 'month') {
            monthInput.style.display = 'block';
            yearInput.style.display = 'block';
            singleDate.value = null;
            weekSelect.value = null;
            startDate.value = null;
            endDate.value = null;
        } else if (this.value === 'range') {
            rangeInput.style.display = 'block';
            singleDate.value = null;
            monthSelect.value = null;
            weekSelect.value = null;
        }
    });
});

// Nút gửi để lấy dữ liệu đã lọc hoặc tất cả
async function submitStatistic() {
    const periodComparison = document.getElementById('periodComparison');
    const userTrends = document.getElementById('userTrends');
    const amountChartContainer = document.getElementById('amountAppointment').parentElement;
    const oldNewChartContainer = document.getElementById('oldAndNewUser').parentElement;

    // Đặt lại bố cục
    periodComparison.style.display = 'block';
    userTrends.style.display = 'block';
    amountChartContainer.classList.remove('center-chart');
    oldNewChartContainer.classList.remove('center-chart');

    await fetchAndDisplayAllData();
}