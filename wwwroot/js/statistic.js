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
        // const parts = dateFilter.dateFilter.split("|");
        // console.log('Parts', parts);
        // const startDate = parts[0];
        // const endDate = parts[1];
        // const customDate = `${startDate}%7C${endDate}`
        url = `${url}dateRange=${encodeURIComponent(dateFilter.dateFilter)}`
    }
    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`Http error! Status ${response.status}`)
        }
        var data = await response.json();
        if (dateFilter.filterType == 'day') {
            const dateObj = new Date(dateFilter.dateFilter);
            const month = String(dateObj.getMonth() + 1).padStart(2, '0');
            const day = String(dateObj.getDate()).padStart(2, '0');
            dateFilter.dateFilter = `${month}/${day}`;
            console.log(`DateFilter ${dateFilter.dateFilter}`);
        }
        if (data.amountAppointment && amountAppointmentChart) {
            amountAppointmentChart.data.datasets[0].data = [data.amountAppointment[dateFilter.dateFilter]]
            amountAppointmentChart.update();
        }
        if (data.oldAndNewUser && oldAndNewUserChart) {
            oldAndNewUserChart.data.datasets[0].data = [data.oldAndNewUser[dateFilter.dateFilter][1]];
            oldAndNewUserChart.data.datasets[1].data = [data.oldAndNewUser[dateFilter.dateFilter][0]];
            oldAndNewUserChart.update();
        }
        if (data.topDoctorAppointment && topDoctorAmountAppointment) {
            const doctorData = data.topDoctorAppointment[dateFilter.dateFilter];
            topDoctorAmountAppointment.data.labels = doctorData.map(doctor => doctor.specialization);
            topDoctorAmountAppointment.data.datasets[0].data = doctorData.map(doctor => doctor.appointmentAmount);
            topDoctorAmountAppointment.update();
        }
        if (data.compareAmountAppointment && compareAmountAppointment) {
            compareAmountAppointment.data.labels = Object.keys(data.compareAmountAppointment);
            compareAmountAppointment.data.datasets[0].data = Object.values(data.compareAmountAppointment);
            compareAmountAppointment.update();
        }
        if (data.compareAmountOldAndNewUser && compareOldAndNewUser) {
            const compareData = data.compareAmountOldAndNewUser;
            compareOldAndNewUser.data.datasets[0].data = Object.values(compareData).map(d => d[0]);
            compareOldAndNewUser.data.datasets[1].data = Object.values(compareData).map(d => d[1]);
            compareOldAndNewUser.update();
        }

    }
    catch (error) {
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
        const oldAndNewUserData = data.oldAndNewUser[dateFilter];
        console.log('Value amount old and new user', oldAndNewUserData);
        var topDoctorAmountAppointmentData = data.topDoctorAppointment[dateFilter];
        console.log('Value amount of top doctor amount appointment', topDoctorAmountAppointmentData);
        var compareAmountAppointmentData = data.compareAmountAppointment;
        console.log('Value compare amount appointment', compareAmountAppointmentData);
        var compareOldAndNewUserData = data.compareAmountOldAndNewUser;
        console.log('Value compare new and old user', compareOldAndNewUserData);
        if (oldAndNewUserChart || amountAppointmentChart || topDoctorAmountAppointment || compareAmountAppointment || compareOldAndNewUser) {
            oldAndNewUserChart.destroy();
            amountAppointmentChart.destroy();
            topDoctorAmountAppointment.destroy();
            compareAmountAppointment.destroy();
            compareOldAndNewUser.destroy();
        }

        var doctorName = topDoctorAmountAppointmentData.map(doctor => doctor.doctorName);
        console.log("Doctor Name", doctorName);
        var specialization = topDoctorAmountAppointmentData.map(doctor => doctor.specialization);
        console.log("Specialization", specialization);
        var appointmentAmount = topDoctorAmountAppointmentData.map(doctor => doctor.appointmentAmount);
        console.log("AppointmentAmount", appointmentAmount);
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
                        max: 100,
                        title: {
                            display: true,
                            text: 'Number of Appointments'
                        }
                    }
                }
            }
        });
        compareAmountAppointment = new Chart(document.getElementById("compareAmountAppointment"), {
            type: 'line',
            data: {
                labels: Object.keys(compareAmountAppointmentData),
                datasets: [{
                    label: 'Appointments',
                    data: Object.values(compareAmountAppointmentData),
                    backgroundColor: 'rgba(255, 99, 132, 0.6)',
                    borderColor: 'rgba(255, 99, 132, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: 'Appointments Comparison',
                        font: { size: 16 }
                    },
                    legend: {
                        position: 'bottom'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 50,
                        title: {
                            display: true,
                            text: 'Number of Appointments'
                        }
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
                labels: Object.keys(compareOldAndNewUserData),
                datasets: [{
                    label: "New User ",
                    data: Object.values(compareOldAndNewUserData).map(item => item[0]),
                    borderColor: 'rgba(75, 192, 192, 1)',
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    tension: 0.4,
                    fill: true
                }, {
                    label: 'Old User',
                    data: Object.values(compareOldAndNewUserData).map(item => item[1]),
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
                            text: 'Month'
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