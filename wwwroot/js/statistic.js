// Select DOM elements
const radioButton = document.querySelectorAll('input[name="statisticType"]');
const statisticTable = document.getElementById('statistic-table');
const weekInput = document.getElementById('weekInput');
const dayInput = document.getElementById('dayInput');
const monthInput = document.getElementById('monthInput');
const yearInput = document.getElementById('yearInput');
const rangeInput = document.getElementById('rangeInput');

// Chart variables
let oldAndNewUserChart;
let amountAppointmentChart;
let topDoctorAmountAppointment;
let compareAmountAppointment;
let compareOldAndNewUser;

// Initialize SignalR and load all data by default
initializeSignalR();
fetchAndDisplayAllData();

// SignalR setup for real-time updates
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

// Fetch weeks for year selection
document.getElementById('yearSelect').addEventListener('change', async function () {
    const year = this.value;
    try {
        const response = await fetch(`/Statistic/GetWeeksByYear?year=${year}`);
        if (!response.ok) throw new Error('Network response was not ok');
        const weeks = await response.json();
        console.log('weeks', weeks);
        const weekSelect = document.getElementById('weekSelect');
        weekSelect.innerHTML = '<option disabled selected value="">Choose week</option>';
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

// Get current date filter (optional, defaults to "all")
function getCurrentDateFilter() {
    const singleDate = document.getElementById("singleDate").value;
    const monthSelect = document.getElementById("monthSelect").value;
    const weekSelect = document.getElementById("weekSelect").value;
    const startDate = document.getElementById("startDate").value;
    const endDate = document.getElementById("endDate").value;
    const yearSelect = document.getElementById("yearSelect").value;

    let dateFilter = null;
    let filterType = 'all'; // Default to all data
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

// Fetch and display all appointment data
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
    } // No params for 'all' - assume API handles "all data" by default

    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
        const data = await response.json();
        console.log('Fetched data:', data);

        // Remove $id from objects
        const removeIdKey = (obj) => {
            if (!obj || typeof obj !== 'object') return obj;
            return Object.entries(obj)
                .filter(([key]) => key !== '$id')
                .reduce((acc, [key, value]) => {
                    acc[key] = value;
                    return acc;
                }, {});
        };

        // Process data for charts
        const amountData = removeIdKey(data.amountAppointment || {});
        const oldAndNewUserData = removeIdKey(data.oldAndNewUser || {});
        const topDoctorData = removeIdKey(data.topDoctorAppointment || {});
        const compareAmountData = removeIdKey(data.compareAmountAppointment || {});
        const compareOldNewData = removeIdKey(data.compareAmountOldAndNewUser || {});

        // Destroy existing charts
        [oldAndNewUserChart, amountAppointmentChart, topDoctorAmountAppointment, 
         compareAmountAppointment, compareOldAndNewUser].forEach(chart => chart?.destroy());

        // Prepare data for all appointments
        const allAmountLabels = Object.keys(amountData).length > 0 ? Object.keys(amountData) : ['No Data'];
        const allAmountValues = Object.values(amountData).length > 0 ? Object.values(amountData) : [0];

        // Prepare old vs new users data
        const allOldNewLabels = Object.keys(oldAndNewUserData).length > 0 ? Object.keys(oldAndNewUserData) : ['No Data'];
        const oldUserValues = allOldNewLabels.map(key => oldAndNewUserData[key]?.$values?.[0] || 0);
        const newUserValues = allOldNewLabels.map(key => oldAndNewUserData[key]?.$values?.[1] || 0);

        // Prepare top doctors data
        const allDoctorData = topDoctorData['All Data']?.$values || Object.values(topDoctorData).flatMap(v => v.$values || []);
        const doctorLabels = allDoctorData.length > 0 
            ? allDoctorData.map(d => d.specialityName || d.specialization || 'N/A')
            : ['No Data'];
        const doctorNames = allDoctorData.length > 0 
            ? allDoctorData.map(d => d.doctorName || 'N/A')
            : ['No Data'];
        const doctorAmounts = allDoctorData.length > 0 
            ? allDoctorData.map(d => d.appointmentAmount || 0)
            : [0];

        // Prepare comparison data
        const compareAmountLabels = Object.keys(compareAmountData).length > 0 
            ? Object.keys(compareAmountData) : ['No Data'];
        const compareAmountValues = Object.values(compareAmountData).length > 0 
            ? Object.values(compareAmountData) : [0];

        // Prepare old vs new user comparison
        const compareEntries = Object.entries(compareOldNewData).filter(([key]) => key !== '$id' && key);
        let compareLabels = ['No Data'];
        let compareOldUsers = [0];
        let compareNewUsers = [0];
        if (compareEntries.length > 0) {
            compareEntries.sort(([keyA], [keyB]) => {
                const numA = Number(keyA);
                const numB = Number(keyB);
                return !isNaN(numA) && !isNaN(numB) ? numA - numB : keyA.localeCompare(keyB);
            });
            compareLabels = compareEntries.map(([key]) => 
                !isNaN(Number(key)) && Number(key) >= 1 && Number(key) <= 12 ? `Month ${key}` : key
            );
            compareOldUsers = compareEntries.map(([_, value]) => value.$values?.[0] || 0);
            compareNewUsers = compareEntries.map(([_, value]) => value.$values?.[1] || 0);
        }

        // Create charts
        amountAppointmentChart = createAmountAppointmentChart(allAmountLabels, allAmountValues);
        oldAndNewUserChart = createOldAndNewUserChart(allOldNewLabels, oldUserValues, newUserValues);
        topDoctorAmountAppointment = createTopDoctorChart(doctorLabels, doctorNames, doctorAmounts);
        compareAmountAppointment = createCompareAmountChart(compareAmountLabels, compareAmountValues);
        compareOldAndNewUser = createCompareOldNewChart(compareLabels, compareOldUsers, compareNewUsers);

    } catch (error) {
        console.error('Error fetching statistics:', error);
        statisticTable.innerHTML = '<p>Error loading data. Please try again.</p>';
    }
}

// Chart creation functions
function createAmountAppointmentChart(labels, data) {
    return new Chart(document.getElementById("amountAppointment"), {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: "Appointments",
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
                title: { display: true, text: 'Total Appointments Over Time', font: { size: 16, weight: 'bold' } },
                legend: { position: 'bottom' },
                tooltip: { mode: 'index', intersect: false }
            },
            scales: {
                y: { beginAtZero: true, title: { display: true, text: 'Number of Appointments' } },
                x: { title: { display: true, text: 'Period' } }
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
                    label: "Old Users",
                    backgroundColor: 'rgba(255, 99, 132, 0.6)',
                    borderColor: 'rgba(255, 99, 132, 1)',
                    borderWidth: 1,
                    data: oldData,
                    barPercentage: 0.7
                },
                {
                    label: "New Users",
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
                title: { display: true, text: 'New vs Old Users Over Time', font: { size: 16, weight: 'bold' } },
                legend: { position: 'bottom' },
                tooltip: { mode: 'index', intersect: false }
            },
            scales: {
                y: { beginAtZero: true, title: { display: true, text: 'Number of Users' } },
                x: { title: { display: true, text: 'Period' } }
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
                label: 'Appointments',
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
                            return `${doctorNames[index]}: ${data[index]} appointments`;
                        }
                    }
                },
                title: { display: true, text: 'Top Doctors by Appointments', font: { size: 16, weight: 'bold' } },
                legend: { position: 'bottom' }
            },
            scales: {
                x: { beginAtZero: true, title: { display: true, text: 'Number of Appointments' } },
                y: { title: { display: true, text: 'Specialization' } }
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
                label: 'Appointments',
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
                title: { display: true, text: 'Appointments Trend Comparison', font: { size: 16, weight: 'bold' } },
                legend: { position: 'bottom' },
                tooltip: { mode: 'index', intersect: false }
            },
            scales: {
                y: { beginAtZero: true, title: { display: true, text: 'Number of Appointments' } },
                x: { title: { display: true, text: 'Period' } }
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
                    label: "New Users",
                    data: newData,
                    borderColor: 'rgba(75, 192, 192, 1)',
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    tension: 0.4,
                    fill: true
                },
                {
                    label: "Old Users",
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
                title: { display: true, text: 'New vs Old Users Trend Comparison', font: { size: 16, weight: 'bold' } },
                legend: { position: 'bottom' },
                tooltip: { mode: 'index', intersect: false }
            },
            scales: {
                y: { beginAtZero: true, title: { display: true, text: 'Number of Users' } },
                x: { title: { display: true, text: 'Period' } }
            },
            animations: { tension: { duration: 1000, easing: 'linear' } }
        }
    });
}

// Radio button logic for filter selection
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

// Submit button to fetch filtered or all data
async function submitStatistic() {
    const periodComparison = document.getElementById('periodComparison');
    const userTrends = document.getElementById('userTrends');
    const amountChartContainer = document.getElementById('amountAppointment').parentElement;
    const oldNewChartContainer = document.getElementById('oldAndNewUser').parentElement;

    // Reset layout
    periodComparison.style.display = 'block';
    userTrends.style.display = 'block';
    amountChartContainer.classList.remove('center-chart');
    oldNewChartContainer.classList.remove('center-chart');

    await fetchAndDisplayAllData();
}