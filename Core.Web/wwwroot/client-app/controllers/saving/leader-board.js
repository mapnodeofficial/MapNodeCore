var LeaderBoardController = function () {
    this.initialize = function () {

        setTimeout(loadData, 2000);

        registerEvents();
        registerControl();
    }

    function registerControl() {
        be.registerNumber();
    }

    function registerEvents() {
        $('#txt-search-keyword').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                loadData(true);
            }
        });

        $('body').on('change', "#ddl-show-page", function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadData(true);
        });
    }

    function loadData(isPageChanged) {
        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/Saving/GetAllLeaderBoardPaging',
            dataType: 'json',
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {
                var template = $('#table-template').html();
                var render = "";
                $.each(response.Results, function (i, item) {
                    //debugger;

                    
                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        USDAmount: be.formatCurrency(item.USDAmount),
                        SavingAmount: be.formatCurrency(item.SavingAmount),
                        TokenName: item.TokenName,
                        TokenCode: item.TokenCode,
                        TokenImage: item.TokenImage,
                        TransactionStateName: item.TransactionStateName,
                        TokenTransactionHash: item.TokenTransactionHash,
                        InterestedRate: item.InterestedRate,
                        Timeline: item.Timeline,
                        SavingDate: be.dateTimeFormatJson(item.SavingDate),
                        EndDate: be.dateTimeFormatJson(item.EndDate),
                    });
                });

                $('#lbl-total-records').text(response.RowCount);

                $('#tbl-content').html(render);

                //be.stopLoading();

                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadData();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                //be.stopLoading();
            }
        });
    }
}