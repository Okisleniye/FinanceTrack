window.financeCharts = {
    instances: {},

    renderDoughnut: function (canvasId, labels, data, colors) {
        var canvas = document.getElementById(canvasId);
        if (!canvas) return;

        if (this.instances[canvasId]) {
            this.instances[canvasId].destroy();
        }

        var ctx = canvas.getContext('2d');
        this.instances[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: colors,
                    borderWidth: 2,
                    borderColor: '#ffffff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            boxWidth: 12,
                            padding: 12
                        }
                    }
                },
                cutout: '65%'
            }
        });
    },

    renderBarComparison: function (canvasId, labels, incomeData, expenseData) {
        var canvas = document.getElementById(canvasId);
        if (!canvas) return;

        if (this.instances[canvasId]) {
            this.instances[canvasId].destroy();
        }

        var ctx = canvas.getContext('2d');
        this.instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Income (₸)',
                        data: incomeData,
                        backgroundColor: 'rgba(25, 135, 84, 0.85)',
                        borderColor: '#198754',
                        borderWidth: 1,
                        borderRadius: 4
                    },
                    {
                        label: 'Expenses (₸)',
                        data: expenseData,
                        backgroundColor: 'rgba(220, 53, 69, 0.85)',
                        borderColor: '#dc3545',
                        borderWidth: 1,
                        borderRadius: 4
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (val) {
                                return val.toLocaleString() + ' ₸';
                            }
                        }
                    }
                },
                plugins: {
                    legend: {
                        position: 'top'
                    }
                }
            }
        });
    }
};
