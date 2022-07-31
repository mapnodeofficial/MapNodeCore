var SwapController = function () {
    this.initialize = function () {
        loadBalance();
        registerEvents();
        registerControl();
    }

    function registerControl() {

    }

    function registerEvents() {

        be.registerNumber();


        $("#txtAmount").focusout(function () {
            CalculateReceived();
        });

        $('body').on('click', '#btnConfirmSwap', function (e) {
            e.preventDefault();

            if (checkEnabled2FA()) {

                var isValid = validateSwap();
                if (!isValid)
                    return;

                be.verifyCodeAndPassword(confirmSwap);
            }
        });

        $('body').on('change', '#ddlWallet', function (e) {
            loadBalance();
        });
    }

    function CalculateReceived() {
        var balance = parseFloat($('#txtBalance').val().replace(/,/g, ''));

        var amount = parseFloat($("#txtAmount").val().replace(/,/g, ''));

        var feeAmount = amount * (swapFee / 100);

        var receiveAmount = (amount - feeAmount) / tokenPrice;

        if (amount > balance) {
            $(".lblErrorInsufficient").html("Insufficient account balance");
        }
        else {
            $(".lblErrorInsufficient").html("");
        }

        $('#txtFeeAmount').val(be.formatCurrency(feeAmount));

        $('#txtAmountReceive').val(be.formatCurrency(receiveAmount));
    }

    function checkEnabled2FA() {
        var isEnabled2FA = $("#Enabled2FA").val();
        if (isEnabled2FA) {
            return true;
        }
        else {
            be.error("Two-factor authentication (2FA)", "Your account has not enabled 2FA, please go to the profile page to enable.");
            return false;
        }
    }

    var SwapVM = null;

    function validateSwap() {

        SwapVM = {
            Balance: parseFloat($('#txtBalance').val().replace(/,/g, '')),
            Amount: parseFloat($('#txtAmount').val().replace(/,/g, '')),
            TokenCode: tokenCode
        };

        var isValid = true;

        if (SwapVM.Amount <= 0) {
            isValid = be.notify('Amount is required', 'error');
        }
        else {
            if (SwapVM.Amount < minSwap) {
                isValid = be.notify('Minimum transfer ' + minSwap + ' MNO', 'error');
            }
        }

        if (SwapVM.Amount > SwapVM.Balance) {
            isValid = be.notify('Insufficient account balance', 'error');
        }

        return isValid;
    }

    function confirmSwap() {


        SwapVM.Password = $('#be-hidden-password').val();

        var code = $('#be-hidden-2faCode').val();

        var url = '/Swap/******?authenticatorCode=' + code;

        $.ajax({
            type: "POST",
            headers: {
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            url: url,
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(SwapVM),
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                be.stopLoading();

                if (response.Success) {
                    be.success('Swap', response.Message, function () {
                        window.location.reload();
                    });
                }
                else {
                    be.error('Swap', response.Message);
                }
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    var minSwap = 0.0;
    var swapFee = 0.0;
    var tokenCode = "";
    var tokenPrice = 0.0;

    function loadBalance() {

        tokenCode = $('#ddlWallet').val();

        $.ajax({
            type: 'GET',
            data: {
            },
            url: '/Swap/GetBalance?tokenCode=' + tokenCode,
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                $('#txtBalance').val(be.formatNumber(response.Balance, 4));

                $('.lblMinSwap').text(response.MinSwap);

                minSwap = parseFloat(response.MinSwap);

                balance = parseFloat(response.Balance);

                swapFee = parseFloat(response.SwapFee);

                tokenPrice = parseFloat(response.TokenPrice);

                CalculateReceived();

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}