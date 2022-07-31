var StakingController = function () {
    this.initialize = function () {
        loadWalletBlance();
        registerEvents();
        registerControl();
    }
    function registerControl() {
        $(".numberFormat").each(function () {
            var numberValue = parseFloat($(this).html().replace(/,/g, ''));
            if (!numberValue) {
                $(this).html(be.formatCurrency(0));
            }
            else {
                $(this).html(be.formatCurrency(numberValue));
            }
        });

        $(".numberFormat").each(function () {
            var numberValue = parseFloat($(this).val().replace(/,/g, ''));
            if (!numberValue) {
                $(this).val(be.formatCurrency(0));
            }
            else {
                $(this).val(be.formatCurrency(numberValue));
            }
        });
    }

    var registerEvents = function () {

        $('body').on('change', "#ddl-show-page", function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadPackageData(true);
        });

        $('body').on('change', "#ddl-show-page-commission", function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadCommissionData(true);
        });

        $('body').on('change', "#ddl-show-page-affiliate", function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadCommissionData(true);
        });

        $('.numberFormat').on("keypress", function (e) {
            var keyCode = e.which ? e.which : e.keyCode;
            var ret = ((keyCode >= 48 && keyCode <= 57) || keyCode == 46);
            if (ret)
                return true;
            else
                return false;
        });

        $(".numberFormat").focusout(function () {
            var numberValue = parseFloat($(this).val().replace(/,/g, ''));
            if (!numberValue) {
                $(this).val(be.formatCurrency(0));
            }
            else {
                $(this).val(be.formatCurrency(numberValue));
            }
        });

        $("#PackageType").on('change', function (e) {
            CaculationStakingByType();
        });

        $("#walletType").on('change', function (e) {
            loadWalletBlanceByType($(this).val());
        });

        $("#btnStaking").on('click', function (e) {
            buyStaking();
        });

        $('body').on('click', '.btn-get-commission', function (e) {
            getCommission(e, this);
        });

        $("#tabPackage").on('click', function (e) {
            loadPackageData();
        });
        $("#tabCommission").on('click', function (e) {
            loadCommissionData();
        });
        $("#tabAffiliate").on('click', function (e) {
            loadAffiliateData();
        });
    }
    function buyStaking() {
        var data = {
            TimeLine: $('#TimeLineType').val(),
            Package: $('#PackageType').val(),
            Type: parseInt($('#walletType').val())
        };

        var isValid = true;

        if (!data.Type) {
            isValid = be.notify('Wallet type is required', 'error');
        }

        if (!data.TimeLine) {
            isValid = be.notify('Time Line is required', 'error');
        }

        if (!data.Package) {
            isValid = be.notify('Package is required', 'error');
        }

        if (isValid) {

            $.ajax({
                type: "POST",
                url: '/Admin/Staking/BuyStaking',
                dataType: "json",
                data: { modelJson: JSON.stringify(data) },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    be.stopLoading();

                    if (response.Success) {

                        be.success('Staking', response.Message, function () {
                            window.location.reload();
                        });
                    }
                    else {
                        be.error('Staking', response.Message);
                    }
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        }
    }

    function getCommission(e, element) {
        e.preventDefault();

        $.ajax({
            type: "POST",
            url: "/Admin/Staking/GetCommission",
            data: { id: $(element).data('id') },
            dataType: "json",
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                be.stopLoading();

                if (response.Success) {
                    be.success('Get Commission', response.Message, function () {
                        loadPackageData();
                    });
                }
                else {
                    be.error('Get Commission', response.Message);
                }
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadPackageData(isPageChanged) {
        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/staking/GetPackageAllPaging',
            dataType: 'json',
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {
                debugger;
                var template = $('#table-template').html();
                var render = "";
                $.each(response.Results, function (i, item) {
                    debugger;
                    render += Mustache.render(template, {
                        AppUserName: item.AppUserName,
                        TypeName: item.TypeName,
                        PackageName: item.PackageName,
                        InterestRate: item.InterestRate,
                        ReceiveAmount: item.ReceiveAmount,
                        ReceiveLatest: be.dateTimeFormatJson(item.ReceiveLatest),
                        ReceiveTimes: item.ReceiveTimes,
                        StakingAmount: item.StakingAmount,
                        StakingTimes: item.StakingTimes,
                        TimeLineName: item.TimeLineName,
                        Function: item.IsGetedCommission == false ? '<a data-id="' + item.Id + '" class="btn-get-commission btn btn-light-dark btn-sm me-2 mt-3 fs-8">Get Commission</a>' : '<span class="text-success fs-7 fw-bolder">Received Today</span>',
                        Sponsor: item.Sponsor,
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                    });
                });

                $('#tbl-content').html(render);

                //be.stopLoading();

                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadPackageData();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                //be.stopLoading();
            }
        });
    }

    function loadCommissionData(isPageChanged) {
        debugger;
        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/staking/GetCommissionAllPaging',
            dataType: 'json',
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {
                debugger;
                var template = $('#table-template-commission').html();
                var render = "";
                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        PackageInterestRate: item.PackageInterestRate,
                        SuddenInterestRate: item.SuddenInterestRate,
                        RealInterestRate: item.RealInterestRate,
                        Amount: item.Amount,
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                    });
                });

                $('#tbl-content-commission').html(render);

                //be.stopLoading();

                if (response.RowCount)
                    be.wrapPagingCommission(response.RowCount, function () {
                        loadCommissionData();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                //be.stopLoading();
            }
        });
    }

    function loadAffiliateData(isPageChanged) {
        debugger;
        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/staking/GetAffiliateAllPaging',
            dataType: 'json',
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {
                debugger;
                var template = $('#table-template-affiliate').html();
                var render = "";
                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        Amount: item.Amount,
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                    });
                });

                $('#tbl-content-affiliate').html(render);

                //be.stopLoading();

                if (response.RowCount)
                    be.wrapPagingAffiliate(response.RowCount, function () {
                        loadAffiliateData();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                //be.stopLoading();
            }
        });
    }

    function CaculationStakingByType() {

        var data = {
            Package: $('#PackageType').val(),
            Type: $('#walletType').val()
        };

        if (data.Package > 0) {

            $.ajax({
                type: "GET",
                url: "/Admin/Staking/CaculationStakingByType",
                dataType: "json",
                data: { modelJson: JSON.stringify(data) },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    $('#AmountPayment').val(response.AmountPayment);
                    be.stopLoading();
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });

        } else {
            $('#AmountPayment').val(0);
        }
    }

    function loadWalletBlanceByType(type) {

        $.ajax({
            type: 'GET',
            url: '/admin/Staking/GetWalletBlanceByType',
            dataType: 'json',
            data: { type: type },
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                $('#WalletBalance').val(response);

                be.stopLoading();

                CaculationStakingByType();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadWalletBlance() {
        $.ajax({
            type: 'GET',
            url: '/admin/Staking/GetWalletBlance',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                $('#WalletBalance').val(response.MainBalance);
                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}