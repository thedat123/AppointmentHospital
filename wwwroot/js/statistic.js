const radioButton = document.querySelectorAll('input[name="statisticType"]');
document.getElementById('statistic-table').style.display = 'none'
const weekInput = document.getElementById('weekInput');
const dayInput = document.getElementById('dayInput');
const monthInput = document.getElementById('monthInput');
let oldAndNewUserChart;
let amountAppointmentChart;
let topDoctorAmountAppointment;
let compareAmountAppointment;
let compareOldAndNewUser;
initializeSignalR();

function initializeSignalR() {
    try {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/scheduleHub")
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Debug)
            .build();

        connection.onreconnecting((error) => {
            console.log('Reconnecting:', error);
        });

        connection.onreconnected((connectionId) => {
            console.log('Reconnected:', connectionId);
        });

        connection.onclose((error) => {
            console.log('Connection closed:', error);
        });

        connection.on('UpdateStatistics', async () => {
            console.log("Received update statistic event from server");
            const dateFilter = getCurrentDateFilter();
            await updateChart(dateFilter);
        });

        connection.start();
        console.log("SignalR Connected successfully");
    } catch (error) {
        console.error("SignalR Connection Error:", error);
    }
}

document.getElementById('yearSelect').addEventListener('change', async function () {
    const year = this.value;
    try {
        const response = await fetch(`/Statistic/GetWeeksByYear?year=${year}`);
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        var weeks = await response.json();
        console.log('week', weeks);
        var weekSelect = document.getElementById('weekSelect');
        weekSelect.innerHTML = '<option disabled selected value="">Choose week</option>';
        weeks.forEach(week => {
            const option = document.createElement('option');
            option.value = week.value;
            option.textContent = week.text;
            weekSelect.appendChild(option);
        });
    }
    catch (error) {
        console.log('Error fetching weeks', error);
    }
});


function getCurrentDateFilter() {
    const singleDate = document.getElementById("singleDate").value;
    const monthSelect = document.getElementById("monthSelect").value;
    const weekSelect = document.getElementById("weekSelect").value;
    const startDate = document.getElementById("startDate").value;
    const endDate = document.getElementById("endDate").value;

    console.log(`startDate ${startDate}, endDate ${endDate}`);
    console.log('SingleDate', singleDate);
    let dateFilter = null;
    let filterType = '';
    if (startDate && endDate) {
        console.log(`startDate ${startDate}, endDate ${endDate}`);
        dateFilter = `${startDate}|${endDate}`;
        filterType = 'custom'
    }
    if (singleDate) {
        console.log('SingleDate', singleDate);
        dateFilter = singleDate;
        filterType = 'day';
    } else if (monthSelect) {
        dateFilter = monthSelect;
        filterType = 'month';
    } else if (weekSelect) {
        dateFilter = weekSelect;
        filterType = 'week';
    }
    console.log(`dateFilter ${dateFilter}, filterType ${filterType}`);

    return { dateFilter, filterType };
}

async function updateChart(dateFilter) {
    let yearSelect = document.getElementById("yearSelect").value;
    document.getElementById('statistic-table').style.display = 'block';
    let url = '/Statistic/Statistic?';
    console.log(dateFilter);
    if (dateFilter.filterType == 'day') {
        url = `${url}singleDate=${encodeURIComponent(dateFilter.dateFilter)}`;
    } else if (dateFilter.filterType === 'week') {
        url = `${url}week=${encodeURIComponent(dateFilter.dateFilter)}&&year=${yearSelect}`;
    } else if (dateFilter.filterType === 'month') {
        url = `${url}month=${encodeURIComponent(dateFilter.dateFilter)}&&year=${yearSelect}`;
    } else if (dateFilter.filterType === 'custom') {
        url = `${url}dateRange=${encodeURIComponent(dateFilter.dateFilter)}`
    }
    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`Http error! Status ${response.status}`)
        }
        var data = await response.json();

        // Hàm lọc bỏ key $id trong dữ liệu
        const removeIdKey = (obj) => {
            return Object.entries(obj)
                .filter(([key]) => key !== '$id')  // Loại bỏ key $id
                .reduce((acc, [key, value]) => {
                    acc[key] = value;
                    return acc;
                }, {});
        }

        if (dateFilter.filterType == 'day') {
            const dateObj = new Date(dateFilter.dateFilter);
            const month = String(dateObj.getMonth() + 1).padStart(2, '0');
            const day = String(dateObj.getDate()).padStart(2, '0');
            dateFilter.dateFilter = `${month}/${day}`;
            console.log(`DateFilter ${dateFilter.dateFilter}`);
        }

        if (data.amountAppointment && amountAppointmentChart) {
            const amountData = removeIdKey(data.amountAppointment);  // Loại bỏ $id
            amountAppointmentChart.data.datasets[0].data = [amountData[dateFilter.dateFilter]];
            amountAppointmentChart.update();
        }

        if (data.oldAndNewUser && oldAndNewUserChart) {
            const oldAndNewUserData = removeIdKey(data.oldAndNewUser);  // Loại bỏ $id

            // Lấy giá trị từ mảng $values trong dữ liệu
            const values = oldAndNewUserData[dateFilter.dateFilter]?.$values;

            // Kiểm tra nếu values tồn tại và cập nhật chart
            if (values) {
                oldAndNewUserChart.data.datasets[0].data = [values[1]];  // Người dùng mới
                oldAndNewUserChart.data.datasets[1].data = [values[0]];  // Người dùng cũ
                oldAndNewUserChart.update();
            } else {
                console.error("Không tìm thấy dữ liệu cho ngày này.");
            }
        }

        if (data.topDoctorAppointment && topDoctorAmountAppointment) {
            const doctorData = removeIdKey(data.topDoctorAppointment[dateFilter.dateFilter]);
            console.log("DoctorData", doctorData)
            const doctorAppointments = doctorData.$values.map(doctor => ({
                doctorName: doctor.doctorName,
                specialization: doctor.specialization,
                appointmentAmount: doctor.appointmentAmount
            }));
            topDoctorAmountAppointment.data.labels = doctorAppointments.map(doctor => doctor.specialization);
            topDoctorAmountAppointment.data.datasets[0].data = doctorAppointments.map(doctor => doctor.appointmentAmount);
            topDoctorAmountAppointment.update();
        }

        if (data.compareAmountAppointment && compareAmountAppointment) {
            const compareAmountData = removeIdKey(data.compareAmountAppointment);  // Loại bỏ $id
            compareAmountAppointment.data.labels = Object.keys(compareAmountData);
            compareAmountAppointment.data.datasets[0].data = Object.values(compareAmountData);
            compareAmountAppointment.update();
        }

        if (data.compareAmountOldAndNewUser && compareOldAndNewUser) {
            // Loại bỏ $id và lấy dữ liệu từ mảng $values
            const compareAmountOldAndNewUserData = removeIdKey(data.compareAmountOldAndNewUser);  // Loại bỏ $id

            // Chuyển đổi dữ liệu thành mảng để lấy giá trị người dùng mới và người dùng cũ
            const oldUserData = Object.values(compareAmountOldAndNewUserData).map(d => d.$values[0]);  // Người dùng cũ
            const newUserData = Object.values(compareAmountOldAndNewUserData).map(d => d.$values[1]);  // Người dùng mới

            // Cập nhật dữ liệu cho biểu đồ
            compareOldAndNewUser.data.datasets[0].data = oldUserData;  // Dữ liệu người dùng cũ
            compareOldAndNewUser.data.datasets[1].data = newUserData;  // Dữ liệu người dùng mới
            compareOldAndNewUser.update();  // Cập nhật biểu đồ
        }

    } catch (error) {
        console.error('Error: ', error)
    }
}

radioButton.forEach(radio => {
    radio.addEventListener('change', function () {

        let singleDate = document.getElementById("singleDate");
        let monthSelect = document.getElementById("monthSelect");
        let weekSelect = document.getElementById("weekSelect");
        let yearInput = document.getElementById("yearInput");

        let rangeInput = document.getElementById("rangeInput");
        let startDate = document.getElementById('startDate');
        let endDate = document.getElementById('endDate');

        const periodComparison = document.getElementById("period-comparison");
        const userTrends = document.getElementById("user-trends");
        document.getElementById('statistic-table').style.display = 'none'

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
            rangeInput.style.display = 'block'
            singleDate.value = null;
            monthSelect.value = null;
            weekSelect.value = null;
        }
    })
})

async function submitStatistic() {
    document.getElementById('statistic-table').style.display = 'block'

    let dateFilter;
    let singleDate = document.getElementById("singleDate").value;
    let date;
    if (singleDate) {
        // Convert YYYY-MM-DD to MM/DD
        const dateObj = new Date(singleDate);
        const month = String(dateObj.getMonth() + 1).padStart(2, '0');
        const day = String(dateObj.getDate()).padStart(2, '0');
        date = `${month}/${day}`;
    }
    let startDate = document.getElementById('startDate').value
    console.log('startDate', startDate);
    let endDate = document.getElementById('endDate').value;
    console.log('endDate', endDate);

    let monthSelect = document.getElementById("monthSelect").value;
    console.log('Month', monthSelect);
    let weekSelect = document.getElementById("weekSelect").value;
    console.log('Week', weekSelect);
    let yearSelect = document.getElementById("yearSelect").value;

    singleDate ? dateFilter = date : monthSelect ? dateFilter = monthSelect : weekSelect ? dateFilter = weekSelect : dateFilter = `${startDate}|${endDate}`;
    console.log("dateFilter", dateFilter);
    let url = '/Statistic/Statistic?';
    if (startDate && endDate) {
        customRange = `${startDate}%7C${endDate}`;
        url = `${url}dateRange=${customRange}`;
        const periodComparison = document.getElementById('periodComparison');
        const userTrends = document.getElementById('userTrends');

        periodComparison.style.display = 'none';
        userTrends.style.display = 'none';
        document.getElementById('amountAppointment').parentElement.classList.add('center-chart');
        document.getElementById('oldAndNewUser').parentElement.classList.add('center-chart');
    }
    if (singleDate) {
        url = `${url}singleDate=${encodeURIComponent(singleDate)}`;
        periodComparison.style.display = 'block';
        userTrends.style.display = 'block';
    }
    if (monthSelect) {
        url = `${url}month=${encodeURIComponent(monthSelect)}&&year=${yearSelect}`;
        periodComparison.style.display = 'block';
        userTrends.style.display = 'block';
    }
    if (weekSelect) {
        url = `${url}week=${encodeURIComponent(weekSelect)}&&year=${yearSelect}`;
        periodComparison.style.display = 'block';
        userTrends.style.display = 'block';
    }
    try {

        const response = await fetch(url, {
            method: 'GET'
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`)
        }
        const data = await response.json();
        console.log(data);
        const amountAppointmentData = data.amountAppointment[dateFilter];
        console.log('Value amount appointment', amountAppointmentData);
        const oldAndNewUserData = data.oldAndNewUser[dateFilter].$values || [];
        console.log('Value amount old and new user', oldAndNewUserData);
        const topDoctorAmountAppointmentData = data.topDoctorAppointment[dateFilter]?.$values || [];
        console.log('Value amount of top doctor amount appointment', topDoctorAmountAppointmentData);
        var compareAmountAppointmentData = data.compareAmountAppointment;
        console.log('Value compare amount appointment', compareAmountAppointmentData);
        const compareOldAndNewUserData = data.compareAmountOldAndNewUser;
        console.log('Value compare new and old user:', compareOldAndNewUserData);

        // Tạo mảng để lưu trữ dữ liệu cho từng người dùng (New User và Old User)

        if (oldAndNewUserChart || amountAppointmentChart || topDoctorAmountAppointment || compareAmountAppointment || compareOldAndNewUser) {
            oldAndNewUserChart.destroy();
            amountAppointmentChart.destroy();
            topDoctorAmountAppointment.destroy();
            compareAmountAppointment.destroy();
            compareOldAndNewUser.destroy();
        }



        const compareDataEntries = Object.entries(compareOldAndNewUserData).filter(([key, value]) => key !== '$id');

        // Sắp xếp các khóa nếu cần thiết (ví dụ: theo thứ tự tăng dần)
        compareDataEntries.sort(([keyA], [keyB]) => Number(keyA) - Number(keyB));

        // Tạo các mảng labels, newUserData và oldUserData
        const labels = compareDataEntries.map(([key]) => `Month ${key}`); // Bạn có thể thay đổi định dạng nhãn theo nhu cầu
        const newUserData = compareDataEntries.map(([_, value]) => value.$values[0] || 0);
        const oldUserData = compareDataEntries.map(([_, value]) => value.$values[1] || 0);

        console.log('Labels:', labels);
        console.log('New User Data:', newUserData);
        console.log('Old User Data:', oldUserData);

        if (Array.isArray(topDoctorAmountAppointmentData)) {
            var doctorName = topDoctorAmountAppointmentData.map(doctor => doctor.doctorName || 'N/A');
            console.log("Doctor Name", doctorName);

            var specialization = topDoctorAmountAppointmentData.map(doctor => doctor.specialization || 'N/A');
            console.log("Specialization", specialization);

            var appointmentAmount = topDoctorAmountAppointmentData.map(doctor => doctor.appointmentAmount || 0);
            console.log("AppointmentAmount", appointmentAmount);
        } else {
            console.error('topDoctorAmountAppointmentData is not a valid array.');
        }
        topDoctorAmountAppointment = new Chart(document.getElementById("topDoctorAmountAppointment"), {
            type: 'bar',
            data: {
                labels: specialization,
                datasets: [{
                    label: 'Appointments',
                    data: appointmentAmount,
                    backgroundColor: 'rgba(54, 162, 235, 0.6)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                scales: {
                    x: {
                        beginAtZero: true,
                        max: 100
                    }
                },
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                const index = context.dataIndex;
                                return `${doctorName[index]}: ${appointmentAmount[index]} appointments`;
                            }
                        }
                    },

                    title: {
                        display: true,
                        text: "Doctor Appointments by Specialization",
                        font: { size: 16 }
                    },
                    legend: {
                        position: 'bottom'
                    }

                }
            }
        });
        amountAppointmentChart = new Chart(document.getElementById("amountAppointment"), {
            type: 'bar',
            data: {
                labels: [dateFilter],
                datasets: [{
                    label: "Appointments",
                    data: [amountAppointmentData],
                    backgroundColor: 'rgba(75, 192, 192, 0.6)',
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 1,
                    barPercentage: 0.7
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 1000,
                    easing: 'easeInOutQuart'
                },
                plugins: {
                    title: {
                        display: true,
                        text: 'Appointments',
                        font: { size: 16, weight: 'bold' }
                    },
                    legend: {
                        position: 'bottom'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 150,
                        title: {
                            display: true,
                            text: 'Number of Appointments'
                        }
                    }
                }
            }
        });

        const filteredCompareAmountAppointmentData = Object.entries(compareAmountAppointmentData)
            .filter(([key]) => key !== '$id') 
            .reduce((acc, [key, value]) => {
                acc[key] = value; 
                return acc;
            }, {});

        
        compareAmountAppointment = new Chart(document.getElementById("compareAmountAppointment"), {
            type: 'line',
            data: {
                labels: Object.keys(filteredCompareAmountAppointmentData), 
                datasets: [{
                    label: 'Appointments',
                    data: Object.values(filteredCompareAmountAppointmentData), 
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
                    title: {
                        display: true,
                        text: 'Appointments Comparison',
                        font: { size: 16, weight: 'bold' }
                    },
                    legend: {
                        position: 'bottom'
                    }
                },
                scales: {
                    y: {
                        max: 150,
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Number of Appointments'
                        }
                    },
                    x: {
                        title: {
                            display: true,
                        }
                    }
                },
                animations: {
                    tension: {
                        duration: 1000,
                        easing: 'easeOutQuad'
                    }
                }
            }
        });

        oldAndNewUserChart = new Chart(document.getElementById("oldAndNewUser"), {
            type: 'bar',
            data: {
                labels: [dateFilter],
                datasets: [
                    {
                        label: "Old Users",
                        backgroundColor: 'rgba(255, 99, 132, 0.6)',
                        borderColor: 'rgba(255, 99, 132, 1)',
                        borderWidth: 1,
                        data: [oldAndNewUserData[1]],
                        barPercentage: 0.7
                    },
                    {
                        label: "New Users",
                        backgroundColor: 'rgba(54, 162, 235, 0.6)',
                        borderColor: 'rgba(54, 162, 235, 1)',
                        borderWidth: 1,
                        data: [oldAndNewUserData[0]],
                        barPercentage: 0.7
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: {
                    duration: 1000,
                    easing: 'easeInOutQuart'
                },
                plugins: {
                    title: {
                        display: true,
                        text: 'New And Old Users',
                        font: { size: 16, weight: 'bold' }
                    },
                    legend: {
                        position: 'bottom'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 100,
                        title: {
                            display: true,
                            text: 'Number of Users'
                        }
                    }
                }
            }
        });
        compareOldAndNewUser = new Chart(document.getElementById("compareOldAndNewUser"), {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: "New User ",
                    data: newUserData,
                    borderColor: 'rgba(75, 192, 192, 1)',
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    tension: 0.4,
                    fill: true
                }, {
                    label: 'Old User',
                    data: oldUserData,
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
                    title: {
                        display: true,
                        text: 'New And Old User Comparison',
                        font: { size: 16, weight: 'bold' }
                    },
                    legend: {
                        position: 'bottom'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Number of Users'
                        }
                    },
                    x: {
                        title: {
                            display: true,
                        }
                    }
                },
                animations: {
                    tension: {
                        duration: 1000,
                        easing: 'linear'
                    }
                }
            }

        });
    }
    catch (error) {
        console.error('Error fetching statistic', error);
    }




}