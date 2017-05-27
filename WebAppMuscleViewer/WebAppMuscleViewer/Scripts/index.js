$(document).ready(function () {
    var timeData = [],
        muscleData = [];
    var data = {
        labels: timeData,
        datasets: [
            {
                fill: false,
                label: 'Muscle',
                yAxisID: 'Muscle',
                borderColor: "rgba(255, 204, 0, 1)",
                pointBoarderColor: "rgba(255, 204, 0, 1)",
                backgroundColor: "rgba(255, 204, 0, 0.4)",
                pointHoverBackgroundColor: "rgba(255, 204, 0, 1)",
                pointHoverBorderColor: "rgba(255, 204, 0, 1)",
                data: muscleData
            }
        ]
    }

    var basicOption = {
        title: {
            display: true,
            text: 'Muscle Real-time Data',
            fontSize: 36
        },
        scales: {
            yAxes: [{
                id: 'Muscle',
                type: 'linear',
                scaleLabel: {
                    labelString: 'Muscle',
                    display: true
                },
                position: 'left',
            }]
        }
    }

    //Get the context of the canvas element we want to select
    var ctx = document.getElementById("myChart").getContext("2d");
    var optionsNoAnimation = { animation: false }
    var myLineChart = new Chart(ctx, {
        type: 'line',
        data: data,
        options: basicOption
    });
    var hub = $.connection.muscleHub;
    hub.on("Update", function (packet) {
        if (!packet.MeasuredTime || !packet.Muscle) {
            return;
        }
        timeData.push(packet.MeasuredTime);
        muscleData.push(packet.Muscle);

        // only keep no more than 50 points in the line chart
        var len = timeData.length;
        if (len > 50) {
            timeData.shift();
            muscleData.shift();
        }
        myLineChart.update();
    });
    $.connection.hub.start();
});
