var LeaderBoardController = function () {
    this.initialize = function () {
        loadData();

        setTimeout(loadData, 10000);

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
            url: '/Exchange/GetAllLeaderBoardPaging',
            dataType: 'json',
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {
                var template = $('#table-template').html();
                var render = "";
                $.each(response.Results, function (i, item) {

                    //var bnbTransactionHashShort = item.BNBTransactionHash.substring(0, 10)
                    //    + "....." + item.BNBTransactionHash.substring(item.BNBTransactionHash.length - 10);

                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        BNBAmount: be.formatNumber(item.BNBAmount, 4),
                        Amount: be.formatCurrency(item.Amount),
                        TypeName: item.TypeName,
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                        AppUserName: item.AppUserName
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