var SavingHistoryController = function () {
    this.initialize = function () {

        loadRewardHistory();
        

        registerEvents();
        registerControl();
    }

    function registerControl() {

    }

    function registerEvents() {
        $('#txt-search-keyword').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                loadRewardHistory(true);
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
            url: '/admin/SavingTransaction/GetAllSavingRewardPaging',
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
                        Amount: item.Amount,
                        Remarks: item.Remarks,
                        TokenName: item.TokenName,
                        TokenCode: item.TokenCode,
                        TokenImage: item.TokenImage,
                        ReferralName: item.ReferralName,
                        InterestedRate: item.InterestedRate,
                        CreatedOn: be.dateTimeFormatJson(item.CreatedOn),
                        AppUserName: item.AppUserName,
                        Sponsor: item.Sponsor
                    });
                });

                $('#lbl-total-records').text(response.RowCount);

                $('#lbl-page-size').text(be.configs.pageSize);

                $('#tbl-content').html(render);

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