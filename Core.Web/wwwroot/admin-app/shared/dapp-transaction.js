

var DAppTransactionController = function fn() {
    this.initialize = function () {
        loadData();
        registerEvents();
        registerControl();
    }

    function registerControl() {

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
        });

        $('body').on('change', '.dapp_email', function () {
            loadData(true);
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
    }

    async function loadData(isPageChanged) {

        var email = $('.dapp_email').val()

        if (!email) {
            return;
        }

        var type = $('.dapp_transactions').attr('data-type')

        var data = {
            key: $('#txt-search-keyword').val(),
            page: be.configs.pageIndex,
            pageSize: be.configs.pageSize,
            type: type
        }

        $.ajax({
            type: 'GET',
            headers: {
                'Content-Type': 'application/json; charset=utf-8',
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val(),
                'email': email
            },
            data: data,
            url: '/DApp/GetTransactions',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                var template = $('#table-template').html();
                var render = "";
                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        AddressFrom: item.AddressFrom,
                        AddressTo: item.AddressTo,
                        BNBAmount: item.BNBAmount,
                        TokenAmount: item.TokenAmount,
                        BNBTransactionHash: item.BNBTransactionHash,
                        TokenTransactionHash: item.TokenTransactionHash,
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                        WalletType: item.WalletType,
                        ReferralAddress: item.ReferralAddress,
                    });
                });

                $('#lbl-total-records').text(response.RowCount);

                $('#tbl-content').html(render);

                be.stopLoading();

                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadData();
                    }, isPageChanged);
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}
