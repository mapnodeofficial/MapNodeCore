var BlockController = function () {
    this.initialize = function () {
        loadData();
        registerEvents();
        registerControl();
    }

    function registerControl() {
    }

    function registerEvents() {
        be.registerNumber();
    }

    function loadData(isPageChanged) {
        debugger;
        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/exchange/GetAllBlockPaging',
            dataType: 'json',
            beforeSend: function () {
                //be.startLoading();
            },
            success: function (response) {
                var template = $('#table-template').html();
                var render = "";
                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        bgName: i % 2 == 0 ? "dark" : "",
                        TypeName: item.TypeName,
                        RoundTypeName: item.RoundTypeName,
                        TransactionHash: item.TransactionHash,
                        Amount: be.formatCurrency(item.Amount),
                        UnlockDate: be.dateFormatJson(item.StartOn),
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                        Sponsor: item.Sponsor,
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