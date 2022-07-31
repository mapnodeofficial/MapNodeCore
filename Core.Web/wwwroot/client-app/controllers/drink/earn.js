var EarnController = function () {
    this.initialize = function () {
        getLocation();
    }

    var currentLat = 0;
    var currentLng = 0;

    function getLocation() {

        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    currentLat = position.coords.latitude;
                    currentLng = position.coords.longitude;


                    const pos = {
                        lat: position.coords.latitude,
                        lng: position.coords.longitude,
                    };
                    GetStoreInfo(pos)

                },
                showError
            );
        } else {
            // Browser doesn't support Geolocation
            handleLocationError(false, infoWindow, map.getCenter());
        }
    }

    function showError(error) {
        switch (error.code) {
            case error.PERMISSION_DENIED:
                console.log("User denied the request for Geolocation.");
                break;
            case error.POSITION_UNAVAILABLE:
                console.log("Location information is unavailable.");
                break;
            case error.TIMEOUT:
                console.log("The request to get user location timed out.");
                break;
            case error.UNKNOWN_ERROR:
                console.log("An unknown error occurred.");
                break;
        }
    }

    function GetStoreInfo(dataParams) {
        $.ajax({
            type: 'GET',
            data: dataParams,
            url: '/Drink/GetActiveStoreInformation',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var lbStoreInfo = $(".storeInformation");

                if (response.IsSuccess) {
                    lbStoreInfo.text(response.Result.StoreName);

                    MapStoreInfo(response.Result);
                } else {
                    lbStoreInfo.text(response.Msg);
                }

                be.stopLoading();

            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function MapStoreInfo(storeInfo) {
        var render = "";
        var template = $('#table-template').html();
        render += Mustache.render(template, {
            Id: storeInfo.Id,
            StoreName: storeInfo.StoreName,
            StoreAddress: storeInfo.StoreAddress,
            ImgUrl: storeInfo.ImgUrl,

        });

        $('#tbl-content').html(render);
    }
}