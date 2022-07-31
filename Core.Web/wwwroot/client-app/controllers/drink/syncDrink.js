var SyncController = function () {
    this.initialize = function () {

    }
    

    this.startSync = function () {
        setInterval(() => initSync(), 10000);
    };

    function initSync() {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    currentLat = position.coords.latitude;
                    currentLng = position.coords.longitude;


                    const pos = {
                        lat: position.coords.latitude,
                        lng: position.coords.longitude,
                    };
                    $.ajax({
                        type: "GET",
                        url: '/drink/SyncStatus',
                        data: pos,
                        success: function (data) {
                        }
                    });

                },
                showError
            );
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
}