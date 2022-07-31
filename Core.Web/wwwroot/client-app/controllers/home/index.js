var HomeController = function () {
    this.initialize = function () {
        registerControls();
        registerEvents();
    }



    function registerControls() {
        chartToken();
    }

    function registerEvents() {
    }

    function chartToken() {
        am4core.useTheme(am4themes_animated);
        am4core.addLicense('ch-custom-attribution');
        am4core.options.autoSetClassName = true;

        var chart = am4core.create("chartdiv", am4charts.PieChart3D);
        chart.responsive.useDefault = false
        chart.responsive.enabled = true;
        chart.hiddenState.properties.opacity = 0; // this creates initial fade-in

        chart.data = [{
            txt: "Marketing",
            val: 10000000,
            "color": am4core.color("#ffffff")
        }, {
            txt: "Seed Round",
            val: 20000000,
            "color": am4core.color("#cddc39")
        }, {
            txt: "Angel Round",
            val: 30000000,
            "color": am4core.color("#f2a7ff")
        }, {
            txt: "Private Round",
            val: 40000000,
            "color": am4core.color("#a1e651")
        }, {
            txt: "Publish Round",
            val: 50000000,
            "color": am4core.color("#ffeb3b")
        }, {
            txt: "Saving",
            val: 50000000,
            "color": am4core.color("#ff9800")
        }, {
            txt: "Staking",
            val: 20000000,
            "color": am4core.color("#ff5722")
        }, {
            txt: "Ecosystem",
            val: 40000000,
            "color": am4core.color("#c609e6")
        }, {
            txt: "MN To Earn",
            val: 100000000,
            "color": am4core.color("#00bcd4")
        }, {
            txt: "MN Pool",
            val: 40000000,
            "color": am4core.color("#2196f3")
        }, {
            txt: "Bounty",
            val: 20000000,
            "color": am4core.color("#7534ea")
        }, {
            txt: "Team",
            val: 30000000,
            "color": am4core.color("#ff1f0f")
        }, {
            txt: "Liquidity",
            val: 40000000,
            "color": am4core.color("#00ffe7")
        }, {
            txt: "Advisor & Experts",
            val: 10000000,
            "color": am4core.color("#00ff0a")
        }];

        chart.innerRadius = am4core.percent(40);
        chart.depth = 100;

        let custom_color_arr = [
            "#ffffff",
            "#a1e651",
            "#cddc39",
            "#ffeb3b",
            "#ff9800",
            "#ff5722",
            "#00bcd4",
            "#2196f3",
            "#7534ea",
            "#c609e6",
            "#ff1f0f"
        ] //custom color arr for chart legends

        var pieSeries = chart.series.push(new am4charts.PieSeries3D());
        pieSeries.dataFields.value = "val";
        pieSeries.dataFields.depthValue = "val";
        pieSeries.dataFields.category = "txt";
        pieSeries.slices.template.cornerRadius = 5;
        pieSeries.ticks.template.disabled = true;
        pieSeries.labels.template.fill = am4core.color("white");
        pieSeries.alignLabels = false;
        pieSeries.labels.template.text = "{value.percent.formatNumber('#.')}%";

        pieSeries.slices.template.propertyFields.fill = "color";

        // Create custom legend
        chart.events.on("ready", function (event) {
            // populate our custom legend when chart renders
            chart.customLegend = document.getElementById('legend');
            pieSeries.dataItems.each(function (row, i) {
                var color = custom_color_arr[i]
                var percent = Math.round(row.values.value.percent * 100) / 100;
                var value = numberWithCommas(row.value) + " Token";
                //legend.innerHTML += '<div class="legend-item" id="legend-item-' + i + ';" onmouseover="hoverSlice(' + i + ');" onmouseout="blurSlice(' + i + ');" style="color: ' + color + ';"><div class="legend-marker" style="background: ' + color + '"></div>' + row.category + '<div class="legend-value">' + value + ' | ' + percent + '%</div></div>';
            });
        });

        function numberWithCommas(num) {
            return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
        }

        function toggleSlice(item) {
            var slice = pieSeries.dataItems.getIndex(item);
            if (slice.visible) {
                slice.hide();
            } else {
                slice.show();
            }
        }

        function hoverSlice(item) {
            var slice = pieSeries.slices.getIndex(item);
            slice.isHover = true;
        }

        function blurSlice(item) {
            var slice = pieSeries.slices.getIndex(item);
            slice.isHover = false;

        }
    }
}