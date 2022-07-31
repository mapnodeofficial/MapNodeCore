var SavingHistoryController = function () {
    this.initialize = function () {
        loadHistory();
        loadRewardHistory();
        loadCommissionHistory();

        registerEvents();
        registerControl();
    }

    function registerControl() {

    }

    function registerEvents() {
        be.registerNumber();

        $('.ProjectSummeryTabBtn1').click(function () {
            $(this).addClass('active');

            $('.ProjectSummeryTabBtn2').each(function (a) {
                $(this).removeClass('active')
            });
        });

        $('.ProjectSummeryTabBtn2').click(function () {
            $(this).addClass('active');

            $('.ProjectSummeryTabBtn1').each(function (a) {
                $(this).removeClass('active')
            });
        });
    }

    function loadHistory(isPageChanged) {

        var keyword = $('#txt-search-keyword').val();
        var page = be.configs.pageIndex;
        var pageSize = be.configs.pageSize;

        $.ajax({
            type: 'GET',
            data: {
                keyword: keyword,
                page: page,
                pageSize: pageSize
            },
            url: '/Saving/GetAllSavingHistoriesPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-template').html();

                var render = "";

                $.each(response.Results, function (i, item) {

                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        Id: item.Id,
                        Name: item.Name,
                        SavingAmount: item.SavingAmount,
                        TimeLine: item.TimeLine,
                        TokenName: item.TokenName,
                        TokenCode: item.TokenCode,
                        TokenImage: item.TokenImage,
                        USDAmount: item.USDAmount,
                        SavingDate: be.dateFormatJson(item.SavingDate),
                        EndDate: be.dateFormatJson(item.EndDate),
                        ExpectedInterest: item.ExpectedInterested,
                        InterestRate: item.InterestedRate,
                        Status: item.Status,
                        ValueDate: be.dateFormatJson(item.ValueDate)
                    });
                });

                $('#tbl-content').html(render);

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadRewardHistory(isPageChanged) {

        var keyword = $('#txt-search-keyword').val();
        var page = be.configs.pageIndex;
        var pageSize = be.configs.pageSize;

        $.ajax({
            type: 'GET',
            data: {
                keyword: keyword,
                page: page,
                pageSize: pageSize
            },
            url: '/Saving/GetAllSavingRewardPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-template-interest').html();

                var render = "";

                $.each(response.Results, function (i, item) {

                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        Amount: item.Amount,
                        Remarks: item.Remarks,
                        TokenName: item.TokenName,
                        TokenCode: item.TokenCode,
                        TokenImage: item.TokenImage,
                        ReferralName: item.ReferralName,
                        InterestedRate: item.InterestedRate,
                        CreatedOn: be.dateTimeFormatJson(item.CreatedOn)
                    });
                });

                $('#lbl-total-records').text(response.RowCount);

                $('#lbl-page-size').text(be.configs.pageSize);

                $('#tbl-interest-rate-content').html(render);

                be.stopLoading();

                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadRewardHistory();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function loadCommissionHistory(isPageChanged) {

        var keyword = $('#txt-search-keyword').val();
        var page = be.configs.pageIndex;
        var pageSize = be.configs.pageSize;

        $.ajax({
            type: 'GET',
            data: {
                keyword: keyword,
                page: page,
                pageSize: pageSize
            },
            url: '/Saving/GetAllSavingCommissionPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-template-commission').html();

                var render = "";

                $.each(response.Results, function (i, item) {

                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        Amount: item.Amount,
                        Remarks: item.Remarks,
                        TokenName: item.TokenName,
                        TokenCode: item.TokenCode,
                        TokenImage: item.TokenImage,
                        ReferralName: item.ReferralName,
                        InterestedRate: item.InterestedRate,
                        CreatedOn: be.dateTimeFormatJson(item.CreatedOn)
                    });
                });

                $('#lbl-total-records').text(response.RowCount);

                $('#lbl-page-size').text(be.configs.pageSize);

                $('#tbl-commission-content').html(render);

                be.stopLoading();

                if (response.RowCount)
                    be.wrapPagingCommission(response.RowCount, function () {
                        loadCommissionHistory();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}