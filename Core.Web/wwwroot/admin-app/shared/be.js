var be = {

    configs: {
        pageSize: 10,
        pageUserSize: 150,
        pageIndex: 1
    },

    notify: function (message, type) {
        toastr.options = {
            "closeButton": true,
            "debug": false,
            "newestOnTop": false,
            "progressBar": true,
            "positionClass": "toast-top-right",
            "preventDuplicates": false,
            "onclick": null,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "5000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        };

        if (type.search("success") >= 0) {
            toastr.success(message);
            return true;
        }
        else {
            toastr.error(message);
            return false;
        }
    },

    confirm: function (title, message, okCallback) {
        Swal.fire({
            title: title,
            text: message,
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: "Yes, do it",
            cancelButtonText: 'No, cancel',
        }).then((result) => {
            if (result.isConfirmed) {
                okCallback();
            }
        })
    },

    error: function (title, message) {
        Swal.fire({
            title: title,
            text: message,
            icon: "error",
            buttonsStyling: !1,
            confirmButtonText: "Ok, got it!",
            customClass: {
                confirmButton: "btn btn-primary"
            }
        })
    },

    congratulations: function (title, message, okCallback) {

        Swal.fire({
            title: title,
            text: message,
            padding: '3em',
            icon: "success",
            buttonsStyling: !1,
            confirmButtonText: "Ok, got it!",
            customClass: {
                confirmButton: "btn btn-primary"
            },
            backdrop: `#2d312e8c 
                        url("/images/games/images/nyan-cat.gif")
                        left top no-repeat`
        })
    },

    success: function (title, message, okCallback) {
        Swal.fire({
            title: title,
            text: message,
            icon: "success",
            buttonsStyling: !1,
            confirmButtonText: "Ok, got it!",
            customClass: {
                confirmButton: "btn btn-primary"
            }
        }).then((result) => {
            if (okCallback) {
                okCallback()
            }
        })
    },

    dateFormatJson: function (datetime) {
        if (datetime == null || datetime == '')
            return '';
        var newdate = new Date(datetime);
        var month = newdate.getMonth() + 1;
        var day = newdate.getDate();
        var year = newdate.getFullYear();
        var hh = newdate.getHours();
        var mm = newdate.getMinutes();
        if (month < 10)
            month = "0" + month;
        if (day < 10)
            day = "0" + day;
        if (hh < 10)
            hh = "0" + hh;
        if (mm < 10)
            mm = "0" + mm;
        return day + "/" + month + "/" + year;
    },

    dateTimeFormatJson: function (datetime) {
        if (datetime == null || datetime == '')
            return '';
        var newdate = new Date(datetime);
        var month = newdate.getMonth() + 1;
        var day = newdate.getDate();
        var year = newdate.getFullYear();
        var hh = newdate.getHours();
        var mm = newdate.getMinutes();
        var ss = newdate.getSeconds();
        if (month < 10)
            month = "0" + month;
        if (day < 10)
            day = "0" + day;
        if (hh < 10)
            hh = "0" + hh;
        if (mm < 10)
            mm = "0" + mm;
        if (ss < 10)
            ss = "0" + ss;
        return month + "/" + day + "/" + year + " " + hh + ":" + mm + ":" + ss;
    },

    startLoading: function (message) {
        blockUI.block();
        if (message)
            $('.blockui-message').html('<div><span class="spinner-border text-primary"></span></div><div>' + message + '</div>')
    },

    stopLoading: function () {
        blockUI.release();
    },

    getStatus: function (status) {
        if (status == 1)
            return '<span class="badge badge-light-success fw-bolder px-4 py-3">Yes</span>';
        else
            return '<span class="badge badge-light-danger fw-bolder px-4 py-3">No</span>';
    },

    getEmailConfirmed: function (status) {
        if (status == true)
            return '<span class="badge badge-light-success fw-bolder px-4 py-3">Yes</span>';
        else
            return '<span class="badge badge-light-danger fw-bolder px-4 py-3">No</span>';
    },

    getActivated: function (status) {
        if (status == true)
            return '<span class="badge badge-light-success fw-bolder px-4 py-3">Yes</span>';
        else
            return '<span class="badge badge-light-danger fw-bolder px-4 py-3">No</span>';
    },

    getUpdatedKYC: function (status) {
        if (status == true)
            return '<span class="badge badge-light-success fw-bolder px-4 py-3">Yes</span>';
        else
            return '<span class="badge badge-light-danger fw-bolder px-4 py-3">No</span>';
    },

    getType: function (type) {
        if (type == 1)
            return '<span class="badge badge-light-success fw-bolder px-4 py-3">New</span>';
        else if (type == 2)
            return '<span class="badge badge-light-secondary fw-bolder px-4 py-3">Watched</span>';
        else
            return '<span class="badge badge-light-primary fw-bolder px-4 py-3">Responded</span>';
    },

    getTransactionType: function (type) {
        if (type == 1)
            return '<span class="badge badge-light-success fw-bolder px-4 py-3">New</span>';
        else if (type == 2)
            return '<span class="badge badge-light-primary fw-bolder px-4 py-3">Approve</span>';
        else
            return '<span class="badge badge-light-danger fw-bolder px-4 py-3">Reject</span>';
    },

    getSupportType: function (type) {
        if (type == 1)
            return '<span class="badge badge-light-success fw-bolder px-4 py-3">New</span>';
        else
            return '<span class="badge badge-light-primary fw-bolder px-4 py-3">Responded</span>';
    },

    getKYCType: function (type) {
        if (type == 1)
            return '<span class="badge badge-primary-light">Pending</span>';
        else if (type == 2)
            return '<span class="badge badge-danger-light">Reject</span>';
        else if (type == 3)
            return '<span class="badge badge-success-light">Accept</span>';
        else
            return '<span class="badge badge-secondary-light">Locked</span>';
    },

    getTicketStatus: function (type) {
        if (type == 1)
            return '<span class="badge badge-light-warning">Pending</span>';
        else if (type == 2)
            return '<span class="badge badge-light-danger">Rejected</span>';
        else if (type == 3)
            return '<span class="badge badge-light-success">Approved</span>';
    },

    formatNumber: function (number, precision) {
        if (!isFinite(number)) {
            return number.toString();
        }

        var a = number.toFixed(precision).split('.');
        a[0] = a[0].replace(/\d(?=(\d{3})+$)/g, '$&,');
        return a.join('.');
    },

    formatCurrency: function (num) {
        num = num.toString().replace(/\$|\,/g, '');
        if (isNaN(num))
            num = "0";
        sign = (num == (num = Math.abs(num)));
        num = Math.floor(num * 100 + 0.50000000001);
        cents = num % 100;
        num = Math.floor(num / 100).toString();
        if (cents < 10)
            cents = "0" + cents;
        for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3); i++)
            num = num.substring(0, num.length - (4 * i + 3)) + ',' +
                num.substring(num.length - (4 * i + 3));
        return (((sign) ? '' : '-') + num + '.' + cents);
    },

    //formatCurrency: function (num) {
    //    num = num.toString().replace(/\$|\,/g, '');
    //    if (isNaN(num))
    //        num = "0";
    //    sign = (num == (num = Math.abs(num)));
    //    num = Math.floor(num * 100 + 0.50000000001);
    //    cents = num % 100;
    //    num = Math.floor(num / 100).toString();
    //    if (cents < 10)
    //        cents = "0" + cents;
    //    for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3); i++)
    //        num = num.substring(0, num.length - (4 * i + 3)) + ',' +
    //            num.substring(num.length - (4 * i + 3));
    //    return (((sign) ? '' : '-') + num + '.' + cents);
    //},

    unflattern: function (arr) {
        var map = {};
        var roots = [];
        for (var i = 0; i < arr.length; i += 1) {
            var node = arr[i];
            node.children = [];
            map[node.id] = i; // use map to look-up the parents
            if (node.parentId !== null) {
                arr[map[node.parentId]].children.push(node);
            } else {
                roots.push(node);
            }
        }
        return roots;
    },

    wrapUserPaging: function (recordCount, callBack, changePageSize) {
        var totalsize = Math.ceil(recordCount / be.configs.pageUserSize);
        //Unbind pagination if it existed or click change pagesize
        if ($('#paginationUL a').length === 0 || changePageSize === true) {
            $('#paginationUL').empty();
            $('#paginationUL').removeData("twbs-pagination");
            $('#paginationUL').unbind("page");
        }
        //Bind Pagination Event
        $('#paginationUL').twbsPagination({
            totalPages: totalsize,
            visiblePages: 7,
            first: '<<',
            prev: '<',
            next: '>',
            last: '>>',
            onPageClick: function (event, p) {
                //if (be.configs.pageIndex !== p) {
                be.configs.pageIndex = p;
                callBack();
                //}
            }
        });
    },

    wrapPaging: function (recordCount, callBack, changePageSize) {
        var totalsize = Math.ceil(recordCount / be.configs.pageSize);
        //Unbind pagination if it existed or click change pagesize
        if ($('#paginationUL a').length === 0 || changePageSize === true) {
            $('#paginationUL').empty();
            $('#paginationUL').removeData("twbs-pagination");
            $('#paginationUL').unbind("page");
        }
        //Bind Pagination Event
        $('#paginationUL').twbsPagination({
            totalPages: totalsize,
            visiblePages: 7,
            first: '<<',
            prev: '<',
            next: '>',
            last: '>>',
            onPageClick: function (event, p) {
                //if (be.configs.pageIndex !== p) {
                be.configs.pageIndex = p;
                callBack();
                //}
            }
        });
    },

    wrapPagingCommission: function (recordCount, callBack, changePageSize) {
        var totalsize = Math.ceil(recordCount / be.configs.pageSize);
        //Unbind pagination if it existed or click change pagesize
        if ($('#paginationULCommission a').length === 0 || changePageSize === true) {
            $('#paginationULCommission').empty();
            $('#paginationULCommission').removeData("twbs-pagination");
            $('#paginationULCommission').unbind("page");
        }
        //Bind Pagination Event
        $('#paginationULCommission').twbsPagination({
            totalPages: totalsize,
            visiblePages: 7,
            first: '<<',
            prev: '<',
            next: '>',
            last: '>>',
            onPageClick: function (event, p) {
                //if (be.configs.pageIndex !== p) {
                be.configs.pageIndex = p;
                callBack();
                //}
            }
        });
    },

    wrapPagingAffiliate: function (recordCount, callBack, changePageSize) {
        var totalsize = Math.ceil(recordCount / be.configs.pageSize);
        //Unbind pagination if it existed or click change pagesize
        if ($('#paginationULAffiliate a').length === 0 || changePageSize === true) {
            $('#paginationULAffiliate').empty();
            $('#paginationULAffiliate').removeData("twbs-pagination");
            $('#paginationULAffiliate').unbind("page");
        }
        //Bind Pagination Event
        $('#paginationULAffiliate').twbsPagination({
            totalPages: totalsize,
            visiblePages: 7,
            first: '<<',
            prev: '<',
            next: '>',
            last: '>>',
            onPageClick: function (event, p) {
                //if (be.configs.pageIndex !== p) {
                be.configs.pageIndex = p;
                callBack();
                //}
            }
        });
    },

    verifyCodeAndPassword: function fncp(callBack) {
        Swal.fire({
            title: 'Enter your authenticator code and password',
            html: '<input placeholder="Enter password" type="password" id="swal-password" class="swal2-input">' +
                '<input placeholder="Enter code" id="swal-code" class="swal2-input">',
            confirmButtonText:
                'Confirm',
            showCancelButton: true,
            showLoaderOnConfirm: true,
            preConfirm: () => {

                let password = $('#swal-password').val();
                let code = $('#swal-code').val();

                if (!password || !code) {
                    Swal.showValidationMessage(
                        "Please enter password and authenticator code"
                    );

                    return;
                }

                let url = `/Authentication/VerifyAuthenticatorCode/${code}`;

                return fetch(url, {
                    method: 'POST', // or 'PUT'
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(password)
                })
                    .then(response => {
                        $('#be-hidden-password').val(password);
                        $('#be-hidden-2faCode').val(code);
                        return response.json()
                    })
                    .then(result => {
                        if (!result.Success) throw new Error(result.Message)
                    })
                    .catch(error => {
                        Swal.showValidationMessage(
                            `${error}`
                        )
                    })
            },
            allowOutsideClick: () => !Swal.isLoading()
        }).then((result) => {
            if (result.isConfirmed) {
                if (callBack) {
                    callBack()
                }
            }

            $('#be-hidden-2faCode').val("");
            $('#be-hidden-password').val("");
        })
    },

    VerifyCode: function fn(callBack) {
        Swal.fire({
            title: 'Enter your authenticator code and password',
            input: 'text',
            inputAttributes: {
                autocapitalize: 'off'
            },
            showCancelButton: true,
            confirmButtonText: 'Verify',
            showLoaderOnConfirm: true,
            preConfirm: (value) => {
                return fetch(`/Authentication/VerifyAuthenticatorCode/${value}`)
                    .then(response => {

                        $('#be-hidden-2faCode').val(value);
                        return response.json()
                    })
                    .then(result => {
                        if (!result.Success) throw new Error(result.Message)
                    })
                    .catch(error => {
                        Swal.showValidationMessage(
                            `${error}`
                        )
                    })
            },
            allowOutsideClick: () => !Swal.isLoading()
        }).then((result) => {
            if (result.isConfirmed) {
                if (callBack) {
                    callBack()
                }
            }

            $('#be-hidden-2faCode').val("");
        })
    },

    isDevice: function () {
        return /(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|ipad|iris|kindle|Android|Silk|lge |maemo|midp|mmp|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino/i.test(navigator.userAgent)
            || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(navigator.userAgent.substr(0, 4))
    }
}

var target = document.querySelector("#kt_body");

var blockUI = new KTBlockUI(target, {
    message: '<div class="blockui-message"><span class="spinner-border text-primary"></span> Loading...</div>',
});

$(document).ajaxSend(function (e, xhr, options) {
    if (options.type.toUpperCase() === "POST" || options.type.toUpperCase() === "PUT") {
        var token = $('form').find("input[name='__RequestVerificationToken']").val();
        xhr.setRequestHeader("RequestVerificationToken", token);
    }
});