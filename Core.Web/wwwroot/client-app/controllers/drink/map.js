var MapController = function () {

    var intervalHash = 1000;

    var intervalStatus = 60 * 2 * 1000;

    this.initialize = function () {

        getLocation();
        loadCups();



        setInterval(() => runHash(), intervalHash);

        setInterval(() => checkStatus(), intervalStatus);

        registerControl();
    }


    function runHash() {
        $('.is-on-y').each(function (i, obj) {
            var currenthashValue = parseFloat($(this).attr("current-hash"));
            var hashValue = parseFloat($(this).attr("hash-val"));

            var partVal = 1000 / intervalHash;

            var hashMint = hashValue / partVal;
            currenthashValue += hashMint;

            $(this).attr("current-hash", currenthashValue);

            $(this).text(be.formatCurrency(currenthashValue));
        });
    }

    function loadCups() {
        var template = $('#cup-template').html();

        $.ajax({
            type: 'GET',
            url: "/Drink/GetUserCups",
            dataType: 'json',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                var render = "";
                $.each(response.Results, function (i, item) {

                    render += Mustache.render(template, {

                        Id: item.Id,
                        ImageUrl: item.ImageUrl.replace('~', ''),
                        Name: item.Name,
                        HashRate: item.HashRate,
                        HashRateDisplay: be.formatCurrency(item.HashRate),
                        MaxOut: be.formatCurrency(item.MaxOut),
                        TimeToUse: be.formatCurrency(item.TimeToUse),
                        Price: be.formatCurrency(item.Price),
                        Code: item.Code,
                        displayDrink: item.IsInDrink ? "none" : "block",
                        displayStop: item.IsInDrink ? "block" : "none",
                        isDrink: item.IsInDrink ? "y" : "n",
                        CurrentHashRate: item.CurrentHashRate,
                        CurrentHashRateDisplay: be.formatCurrency(item.CurrentHashRate)
                    });
                });

                $('#cup-content').html(render);

                checkStatus();

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function SyncLocation() {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    currentLat = position.coords.latitude;
                    currentLng = position.coords.longitude;
                })
        } else {
            currentLat = 0;
            currentLng = 0;

        }
    }

    function checkStatus() {


        var isDrink = false;

        $('.is-on-y').each(function (i, obj) {
            isDrink = true;
        });
        if (isDrink) {
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(
                    (position) => {
                        const pos = {
                            lat: position.coords.latitude,
                            lng: position.coords.longitude,
                        };
                        $.ajax({
                            type: 'GET',
                            data: pos,
                            headers: {
                                'Accept': 'application/json',
                                'Content-Type': 'application/json',
                                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                            },
                            url: '/drink/SyncStatus',
                            dataType: 'json',
                            beforeSend: function () {
                            },
                            success: function (response) {
                            },
                            error: function (message) {
                            }
                        });
                    },
                    showError
                );
            } else {
                be.notify(`Browser doesn't support Geolocation`, 'error');
                // Browser doesn't support Geolocation
                //handleLocationError(false, infoWindow, map.getCenter());
            }
        }

    }

    function registerControl() {
        $("#btn_history").on('click', function (e) {
            loadDrinkHistory();
        });
    }

    function loadDrinkHistory(isPageChanged) {
        var template = $('#table-history-template').html();

        var page = be.configs.pageIndex;
        var pageSize = be.configs.pageSize;

        $.ajax({
            type: 'GET',
            data: {
                page: page,
                pageSize: pageSize
            },
            url: "/Drink/GetAllDrinkHistoryPaging",
            dataType: 'json',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                var render = "";
                $.each(response.Results, function (i, item) {

                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        Id: item.Id,
                        HashRate: item.HashRate,
                        CupName: item.CupName,
                        StoreName: item.StoreName,
                        HashRate: item.HashRate,
                        StartOn: be.dateTimeFormatJson(item.StartOn),
                        LeaveOn: be.dateTimeFormatJson(item.LeaveOn),
                        HashEarn: item.HashEarn
                    });
                });


                $('#tbl-content-history').html(render);


                be.stopLoading();

                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadDrinkHistory();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }


    var currentLat = 0;
    var currentLng = 0;
    var arrCors = [];

    function getLocation() {


        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    debugger;
                    currentLat = position.coords.latitude;
                    currentLng = position.coords.longitude;
                    const pos = {
                        lat: currentLat,
                        lng: currentLng,
                    };
                    SendAjax('/Drink/GetStoreNearby', pos);
                },
                showError
            );
        } else {
            be.notify(`Browser doesn't support Geolocation`, 'error');
            // Browser doesn't support Geolocation
            //handleLocationError(false, infoWindow, map.getCenter());
        }
    }

    function SendAjax(url, dataParams) {
        $.ajax({
            type: 'GET',
            data: dataParams,
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            url: url,
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                loadData(response);
                be.stopLoading();

            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function showError(error) {
        switch (error.code) {
            case error.PERMISSION_DENIED:
                be.notify('User denied the request for Geolocation.', 'error');
                console.log("User denied the request for Geolocation.");
                break;
            case error.POSITION_UNAVAILABLE:
                be.notify('Location information is unavailable.', 'error');
                console.log("Location information is unavailable.");
                break;
            case error.TIMEOUT:
                be.notify('The request to get user location timed out.', 'error');
                console.log("The request to get user location timed out.");
                break;
            case error.UNKNOWN_ERROR:
                be.notify('An unknown error occurred.', 'error');
                console.log("An unknown error occurred.");
                break;
        }
    }

    function loadData(response) {

        var template = $('#table-template').html();

        var render = "";
        arrCors = [];

        $.each(response.Results, function (i, item) {

            let timeLines = "";

            arrCors.push({
                lat: item.Lat,
                lng: item.Lng,
                shopName: item.StoreName,
                address: item.StoreAddress
            });

            render += Mustache.render(template, {
                bgName: i % 2 == 0 ? "dark" : "",
                Id: item.Id,
                StoreName: item.StoreName,
                Increase: 0,
                Distance: item.Distance,
                StoreAddress: item.StoreAddress,
                ImgUrl: item.ImgUrl,
                Website: item.Website,
                PhoneNumber: item.Phone
            });

            $('#tbl-content').html(render);

        });

        initializeMapBox();
    }


    this.Drink = function (id) {

        SyncLocation();

        const pos = {
            lat: currentLat,
            lng: currentLng,
        };

        var response = GetActiveStoreInfo(pos);

        if (response.IsSuccess) {

            var postData = {
                Id: id,
                Lat: currentLat,
                Lng: currentLng
            };


            be.confirm('Drink to earn', 'Are you sure to drink at ' + response.Result.StoreName + '?', function () {
                $.ajax({
                    type: "GET",
                    url: "/Drink/******",
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json',
                        "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                    },
                    data: postData,
                    dataType: "JSON",
                    beforeSend: function () {
                        be.startLoading();
                    },
                    success: function (response) {
                        if (response.Success) {
                            be.notify('Drink to earn success', 'success');

                        } else {
                            be.notify(`${response.Message}`, 'warning');
                        }
                        loadCups();
                        be.stopLoading();
                    },
                    error: function (message) {
                        be.notify(`${message.responseText}`, 'error');
                        be.stopLoading();
                    }
                });
            });
        } else {
            be.notify(`$You are not inside any store`, 'warning');
            be.stopLoading();
        }


    }

    this.StopDrink = function (id) {

        SyncLocation();

        const pos = {
            lat: currentLat,
            lng: currentLng,
        };

        var response = GetActiveStoreInfo(pos);

        if (response.IsSuccess) {

            var postData = {
                Id: id,
                Lat: currentLat,
                Lng: currentLng
            };

            be.confirm('Stop drink', 'Are you sure to stop drink at ' + response.Result.StoreName + '?', function () {
                $('#item-li-' + id).attr("class", "is-on-n");
                $.ajax({
                    type: "GET",
                    url: "/Drink/******",
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json',
                        "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                    },
                    data: postData,
                    dataType: "JSON",
                    beforeSend: function () {
                        be.startLoading();
                    },
                    success: function (response) {
                        if (response.Success) {
                            be.notify('Stop Drink to earn success', 'success');
                            loadCups();
                        } else {
                            be.notify(`${response.Message}`, 'warning');
                        }

                        be.stopLoading();
                    },
                    error: function (message) {
                        be.notify(`${message.responseText}`, 'error');
                        be.stopLoading();
                    }
                });
            });
        } else {
            be.notify(`$You are not inside any store`, 'warning');
            be.stopLoading();
        }


    }

    function GetActiveStoreInfo(dataParams) {
        var storeInfo = null;
        $.ajax({
            type: 'GET',
            data: dataParams,
            url: '/Drink/GetActiveStoreInformation',
            dataType: 'json',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            async: false,
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                storeInfo = response;

            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
        return storeInfo;
    }


    this.SwapHashToMNO = function (id) {

        be.confirm('Swap Hashrate', 'Are you sure swap hashrate to MNO token?', function () {
            $.ajax({
                type: "POST",
                url: "/Drink/******",
                headers: {
                    "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                },
                data: { id: id },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    

                    if (response.Success) {
                        be.success("Swap Hashrate", response.Message);
                    }
                    else {
                        be.error("Swap Hashrate", response.Message);
                    }

                    be.stopLoading();
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }

    function initializeMapBox() {


        const map = new mapboxgl.Map({
            container: 'map',
            style: 'mapbox://styles/mapbox/streets-v11',
            center: [currentLng, currentLat],
            zoom: 14
        });

        map.addControl(
            new mapboxgl.GeolocateControl({
                positionOptions: {
                    enableHighAccuracy: true
                },
                // When active the map will receive updates to the device's location as it changes.
                trackUserLocation: true,
                // Draw an arrow next to the location dot to indicate which direction the device is heading.
                showUserHeading: true
            })
        );

        const marker = new mapboxgl.Marker()
            .setLngLat([currentLng, currentLat])
            .addTo(map);

        arrCors.forEach((position, i) => {
            //var myLatlng = new google.maps.LatLng(position.lat, position.lng);

            const popup = new mapboxgl.Popup({ offset: 25 }).setText(
                position.shopName
            );

            // create DOM element for the marker
            const el = document.createElement('div');
            el.id = 'marker';

            const marker1 = new mapboxgl.Marker(el)
                .setLngLat([position.lng, position.lat])
                .setPopup(popup)
                .addTo(map);
        });

        // Create a default Marker and add it to the map.

    }

    this.GetLocation = function () {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    currentLat = position.coords.latitude;
                    currentLng = position.coords.longitude;

                    $("#lbLat").text(currentLat);
                    $("#lbLng").text(currentLng);

                },
                showError
            );
        } else {
            // Browser doesn't support Geolocation
            //handleLocationError(false, infoWindow, map.getCenter());
        }
    }
}