var WalletController = function () {
    this.initialize = function (publishKeyRaw, receiveAddressRaw) {
        publishKey = publishKeyRaw;
        receiveAddress = receiveAddressRaw;

        setTimeout(loadWallets, 3000);

        registerEvents();
        registerControl();
    }

    var publishKey = "";
    var receiveAddress = "";
    var feeWithdraw = "";
    var tokenCode = "";
    var minWithdraw = 1;
    var tokenConfigId = 0;

    function registerControl() {
        be.registerNumber();

        jQuery('#qrcodePublishKey').qrcode({
            text: publishKey
        });
    }

    var registerEvents = function () {

        $('body').on('click', '#btnDeposit', function (e) {
            e.preventDefault();

            var tokenCode = $(this).data('tokencode');
            $(".tokenCode").html(tokenCode);

            var minDeposit = $(this).data('mindeposit');
            $(".minDeposit").html(minDeposit);

            $("#txtPublishKey").val(publishKey)

            showModalDeposit()
        });

        $('body').on('click', '#btnCopyPublishKey', function (e) {
            e.preventDefault();
            copyPublishKey();
        });

        $("#txtAmount").focusout(function () {

            var balance = parseFloat($('.balance').val().replace(/,/g, ''));

            var amount = parseFloat($(this).val().replace(/,/g, ''));

            var feeAmount = amount * (feeWithdraw / 100);

            var receiveAmount = amount - feeAmount;

            if (amount > balance) {
                $(".lblErrorInsufficient").html("Insufficient account balance");
            }
            else {
                $(".lblErrorInsufficient").html("");
            }

            $('#txtFeeAmount').val(be.formatCurrency(feeAmount));

            $('#txtAmountReceive').val(be.formatCurrency(receiveAmount));
        });

        $('body').on('click', '#btnWithdraw', function (e) {
            e.preventDefault();
            debugger;
            if (checkEnabled2FA()) {

                tokenCode = $(this).data('tokencode');
                $(".tokenCode").html(tokenCode);

                minWithdraw = $(this).data('minwithdraw');
                $(".minWithdraw").html(minWithdraw);

                var balance = $(this).data('balance');
                $(".balance").val(balance);

                feeWithdraw = parseFloat($(this).data('fee'));

                tokenConfigId = parseInt($(this).data('tokenid'));

                $("#txtReceiveAddress").val(receiveAddress)

                showModalWithdraw()
            }
        });

        $('body').on('click', '#btnConfirmWithdraw', function (e) {
            e.preventDefault();

            var isValid = validateWithdraw();
            if (!isValid)
                return;

            hideModalWithdraw();

            be.verifyCodeAndPassword(confirmWithdraw);
        });
    }


    var WithdrawVM = null;
    function validateWithdraw() {

        WithdrawVM = {
            Balance: parseFloat($('.balance').val().replace(/,/g, '')),
            Amount: parseFloat($('#txtAmount').val().replace(/,/g, '')),
            TokenConfigId: tokenConfigId
        };


        var isValid = true;

        if (WithdrawVM.Amount <= 0) {
            isValid = be.notify('Amount is required', 'error');
        }
        else {
            if (WithdrawVM.Amount < minWithdraw) {
                isValid = be.notify('Minimum withdraw ' + minWithdraw + ' ' + tokenCode, 'error');
            }
        }

        if (WithdrawVM.Amount > WithdrawVM.Balance) {
            isValid = be.notify('Insufficient account balance', 'error');
        }

        return isValid;
    }

    function confirmWithdraw() {

        debugger;

        WithdrawVM.Password = $('#be-hidden-password').val()

        var code = $('#be-hidden-2faCode').val();

        var url = '/Wallet/******?authenticatorCode=' + code;

        $.ajax({
            type: "POST",
            headers: {
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            url: url,
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify(WithdrawVM),
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                debugger;

                be.stopLoading();

                if (response.Success) {
                    be.success('Withdraw', response.Message, function () {
                        window.location.reload();
                    });
                }
                else {
                    be.error('Withdraw', response.Message);
                }
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function showModalDeposit() {
        $("#modal_deposit").modal("show");
    }

    function copyPublishKey() {
        var copyText = $("#txtPublishKey");
        copyText.select();
        document.execCommand("copy");
        be.notify('Copy to clipboard is successful', 'success');
    }

    function showModalWithdraw() {
        $("#modal_withdraw").modal("show");
    }
    function hideModalWithdraw() {
        $("#modal_withdraw").modal("hide");
    }

    function checkEnabled2FA() {
        var isEnabled2FA =  $("#Enabled2FA").val() == "True";
        if (isEnabled2FA) {
            return true;
        }
        else {
            be.error("Two-factor authentication (2FA)", "Your account has not enabled 2FA, please go to the profile page to enable.");
            return false;
        }
    }

    function loadWallets() {
        $.ajax({
            type: 'GET',
            url: '/Wallet/GetWallets',
            dataType: 'json',
            data: {},
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-template').html();
                var render = "";

                $.each(response.Data, function (i, item) {
                    render += Mustache.render(template, {
                        TokenConfigId: item.TokenConfigId,
                        ContractAddress: item.ContractAddress,
                        TokenImageUrl: item.TokenImageUrl,
                        TokenCode: item.TokenCode,
                        Name: item.Name,
                        Amount: be.formatNumber(item.Amount, 4),
                        MaxDeposit: be.formatCurrency(item.MaxDeposit),
                        MinDeposit: be.formatCurrency(item.MinDeposit),
                        MinWithdraw: be.formatCurrency(item.MinWithdraw),
                        MaxWithdraw: be.formatCurrency(item.MaxWithdraw),
                        FeeWithdraw: be.formatCurrency(item.FeeWithdraw),
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
}