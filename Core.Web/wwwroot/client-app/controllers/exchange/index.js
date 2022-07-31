var ExchangeController = function () {
    this.initialize = function () {
        loadDataExchange();
        registerEvents();
        registerControl();
    }

    var bnbBalance = 0;


    function registerControl() {
    }

    function registerEvents() {

        be.registerNumber();

        $('.btnMax').on('click', function (e) {

            if (bnbBalance > 0) {

                $('#txtAmount').val(be.formatNumber(bnbBalance, 4));

                CalculateInterest();
            }
        });

        $('#txtAmount').keyup(function (e) {

            debugger;

            var amount = parseFloat($(this).val().replace(/,/g, ''));

            if (isNaN(amount)) {
                amount = 0;
            }

            if (amount > bnbBalance) {
                $(this).val(be.formatNumber(bnbBalance, 4));
            }

            CalculateInterest();
        });

        $("#btnConfirm").on('click', async function (e) {

            debugger;
            var amount = parseFloat($('#txtAmount').val().replace(/,/g, ''));

            if (amount < 0.1) {
                be.notify(`Min payment must 0.1 BNB`, 'warning');
                return;
            }

            var postData = {
                OrderBNB: amount
            };

            be.confirm('Confirm to buy token', 'Are you sure to buy amount ' + amount + ' BNB', function () {
                $.ajax({
                    type: "POST",
                    url: "/Exchange/ConfirmExchangeToken",
                    headers: {
                        "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                    },
                    data: JSON.stringify(postData),
                    dataType: "json",
                    contentType: "application/json",
                    beforeSend: function () {
                        be.startLoading();
                    },
                    success: function (response) {
                        if (response.Success) {
                            be.success('Payment', response.Message, function () {
                                window.location.reload();
                            });

                        } else {
                            be.error('Payment', response.Message);
                        }

                        be.stopLoading();
                    },
                    error: function (message) {
                        be.notify(`${message.responseText}`, 'error');
                        be.stopLoading();
                    }
                });
            });
        });
    }

    function CalculateInterest() {

        debugger;

        var tokenPrice = parseFloat($("#tokenPrice").val());

        var bnbPrice = parseFloat($("#bnbPrice").val());

        var txtAmount = parseFloat($("#txtAmount").val().replace(/,/g, ''));

        var totalUSD = bnbPrice * txtAmount;

        $(".lblTotalUSD").text("$" + be.formatCurrency(totalUSD));

        var totalToken = totalUSD / tokenPrice;

        $(".lblTotalToken").text(be.formatCurrency(totalToken) + " MNI");
    }

    function loadDataExchange() {

        $.ajax({
            type: 'GET',
            data: {
            },
            url: '/Exchange/GetData',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                bnbBalance = response.BNBBalance;

                $('.CurrentBalance').html(be.formatNumber(response.BNBBalance, 4));

                $('#tokenPrice').val(response.TokenPrice);

                $('.tokenPrice').html(response.TokenPrice);

                $('#bnbPrice').val(response.BNBPrice);

                $('#transactionType').val(response.Type);

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}