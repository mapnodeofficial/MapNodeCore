var SavingDetailController = function () {
    this.initialize = function (
        tokenBalanceRaw,
        tokenConfigIdRaw,
        defaultTimeline,
        defaltInterest) {

        debugger;

        tokenBalance = tokenBalanceRaw;
        tokenConfigId = tokenConfigIdRaw;
        timeline = defaultTimeline;
        rateValue = defaltInterest;

        $('#CurrentBalance').text(be.formatNumber(tokenBalanceRaw, 4));

        loadData();

        registerEvents();
        registerControl();
    }

    var tokenBalance = 0;
    var tokenConfigId = 0;
    var tokenConfigCode = "";
    var timeline = 720;
    var rateValue = 0.4;

    function registerControl() {

    }

    function loadData() {
        
        tokenConfigCode = $('#tokenCode').val();

        var minSaving = parseFloat($('#hideMinSaving').val().replace(',', '.'));

        var maxSaving = parseFloat($('#hideMaxSaving').val().replace(',', '.'));

        var totalSupply = parseFloat($('#hideTotalSupply').val().replace(',', '.'));

        var maxInterestRate = parseFloat($('#hideMaxInterestRate').val().replace(',', '.'));

        $(".lblMinSaving").html(be.formatCurrency(minSaving));

        $(".lblMaxSaving").html(be.formatCurrency(maxSaving));

        $(".lblTotalSupply").html(be.formatCurrency(totalSupply));

        $(".lblInterestRate").html(be.formatCurrency(maxInterestRate) + '%');
    }

    function registerEvents() {
        be.registerNumber();

        $('.btnMax').on('click', function (e) {
            debugger;
            if (tokenBalance > 0) {

                $('#txtAmount').val(be.formatNumber(tokenBalance, 4));

                CalculateInterest();
            }
        });

        $('#txtAmount').keyup(function (e) {

            var amount = parseFloat($(this).val());

            if (isNaN(amount)) {
                amount = 0;
            }

            if (amount > tokenBalance) {
                $(this).val(be.formatNumber(tokenBalance, 4));
            }

            CalculateInterest();
        });

        $("#btnConfirm").on('click', async function (e) {

            var postData = {
                TokenConfigId: tokenConfigId,
                Amount: parseFloat($('#txtAmount').val().replace(/,/g, '')),
                Timeline: timeline
            };

            if (postData.Amount > 0) {
                be.confirm('Confirm to saving', 'Are you sure to saving ' + postData.Amount + ' ' + tokenConfigCode, function () {
                    $.ajax({
                        type: "POST",
                        url: "/saving/******",
                        headers: {
                            "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                        },
                        data: JSON.stringify(postData),
                        dataType: "JSON",
                        contentType: "application/json",
                        beforeSend: function () {
                            be.startLoading();
                        },
                        success: function (response) {
                            if (response.Success) {
                                be.success('Saving', response.Message, function () {
                                    window.location.reload();
                                });

                            } else {
                                be.error('Saving', response.Message);
                            }

                            be.stopLoading();
                        },
                        error: function (message) {
                            be.notify(`${message.responseText}`, 'error');
                            be.stopLoading();
                        }
                    });
                });
            }
        });

        $('body').on('click', ".StakeTablinks", function (e) {
            e.preventDefault();
            openTab(this);
        });
    }

    function CalculateInterest() {
        debugger;

        var tokenPrice = parseFloat($("#tokenPrice").val().replace(',', '.'));

        var txtAmount = parseFloat($("#txtAmount").val().replace(/,/g, ''));

        var totalAmount = txtAmount * tokenPrice;

        $(".lblSubTotal").text("= $" + be.formatCurrency(totalAmount, 2));

        var rateAmount = parseFloat(timeline * totalAmount * rateValue / 100);

        $("#lblRateExpected").text("$" + be.formatCurrency(rateAmount));
    }

    function openTab(element) {

        var id = $(element).data('id');

        var savingPeriodType = $(element).data('period');

        timeline = parseInt(savingPeriodType);

        $(".lockPeriod").html(savingPeriodType + ' days');

        var i, tablinks;

        tablinks = document.getElementsByClassName("StakeTablinks" + id);

        for (i = 0; i < tablinks.length; i++) {
            tablinks[i].className = tablinks[i].className.replace(" active", "");
        }

        element.className += " active";

        getInterestRate(savingPeriodType, id);
    }

    function getInterestRate(savingPeriodType, tokenConfigId) {

        $.ajax({
            type: 'GET',
            data: {
                timeline: savingPeriodType,
                id: tokenConfigId,
            },
            url: '/Saving/GetInterestRate',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                rateValue = response.RateValue;

                $('#lblEndDate').text(response.EndDate);

                $('.InterestRate' + tokenConfigId).html(response.RateValue + '%');

                CalculateInterest();

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}