var SavingController = function () {
    this.initialize = function () {
        loadData();
        registerEvents();
        registerControl();
    }

    function registerControl() {

    }


    function registerEvents() {
        be.registerNumber();

        $('#txt-search-keyword').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                loadData(true);
            }
        });

        $("#btnSearch").on('click', function (e) {
            loadData(true);
        });

        $('body').on('change', "#ddl-show-page", function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadData(true);
        });

        $('body').on('click', ".StakeTablinks", function (e) {
            e.preventDefault();
            openTab(this);
        });
    }

    function CalculateInterest() {

        var tokenPrice = parseFloat($("#tokenPrice").val());

        var txtAmount = parseFloat($("#txtAmount").val());

        var totalAmount = txtAmount * tokenPrice;

        $(".lblSubTotal").text("= $" + be.formatCurrency(totalAmount));

        var rateAmount = parseFloat(totalAmount * rateValue / 100);

        $("#lblRateExpected").text("$" + be.formatCurrency(rateAmount));
    }

    var rateId = 360;
    var rateValue = 35;

    function openTab(element) {

        var id = $(element).data('id');

        var savingPeriodType = $(element).data('period');

        rateId = parseInt(savingPeriodType);

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
    function loadData(isPageChanged) {

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
            url: '/Saving/GetAllTokenConfigPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-template').html();

                var render = "";

                $.each(response.Results, function (i, item) {

                    let timeLines = "";

                    $.each(item.TimeLines, function () {
                        timeLines += '<li class="StakeTablinks StakeTablinks' + item.Id + '" data-id="' + item.Id + '" data-Period="' + this.Value + '">' +
                            '<button class="btn-Period" data-id="' + item.Id + '">' + this.Value + '</button></li>'
                    })

                    render += Mustache.render(template, {
                        Id: item.Id,
                        Name: item.Name,
                        TokenCode: item.TokenCode,
                        TokenImageUrl: item.TokenImageUrl,
                        TotalSupply: be.formatCurrency(item.TotalSupply),
                        TotalSaving: be.formatCurrency(item.TotalSaving),
                        PerTotalSaving: item.PerTotalSaving,
                        TypeName: item.TypeName,
                        InterestRate: item.MaxInterestRate,
                        //CreatedOn: be.dateFormatJson(item.CreatedOn),
                        TimeLines: timeLines
                    });
                });

                $('#lbl-total-records').text(response.RowCount);

                $('#lbl-page-size').text(be.configs.pageSize);

                $('#tbl-content').html(render);

                be.stopLoading();

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