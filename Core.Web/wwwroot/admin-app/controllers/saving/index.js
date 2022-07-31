var SavingTransactionController = function () {
    this.initialize = function () {
        loadData();
        registerEvents();
        registerControl();
    }

    function registerControl() {
        $(".numberFormat").each(function () {
            var numberValue = parseFloat($(this).html().replace(/,/g, ''));
            if (!numberValue) {
                $(this).html(be.formatCurrency(0));
            }
            else {
                $(this).html(be.formatCurrency(numberValue));
            }
        });

        $(".numberFormat").each(function () {
            var numberValue = parseFloat($(this).html().replace(/,/g, ''));
            if (!numberValue) {
                $(this).val(be.formatCurrency(0));
            }
            else {
                $(this).val(be.formatCurrency(numberValue));
            }
        });
    }

    var registerEvents = function () {

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

        $('body').on('click', '.btn-approve', function (e) {
            approveTicket(e, this);
        });

        $('body').on('click', '.btn-reject', function (e) {
            rejectTicket(e, this);
        });

    }

    function approveTicket(e, element) {
        e.preventDefault();

        be.confirm('Approve Ticket', 'Are you sure to Approve?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/TicketTransaction/ApproveTicket",
                data: { id: $(element).data('id') },
                dataType: "json",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    be.stopLoading();
                    
                    if (response.Success) {
                        be.success('Approve Ticket', response.Message, function () {
                            loadData();
                        });
                    }
                    else {
                        be.error('Approve Ticket', response.Message);
                    }
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }

    function rejectTicket(e, element) {
        e.preventDefault();

        be.confirm('Reject Ticket', 'Are you sure to Reject?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/TicketTransaction/RejectTicket",
                data: { id: $(element).data('id') },
                dataType: "json",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {
                    be.stopLoading();

                    if (response.Success) {
                        be.success('Reject Ticket', response.Message, function () {
                            loadData();
                        });
                    }
                    else {
                        be.error('Reject Ticket', response.Message);
                    }
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
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
            url: '/admin/SavingTransaction/GetAllPaging',
            dataType: 'json',
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-template').html();
                var render = "";

                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        Id: item.Id,
                        Name: item.Name,
                        SavingAmount: item.SavingAmount,
                        TimeLine: item.TimeLine,
                        TokenName: item.TokenName,
                        TokenCode: item.TokenCode,
                        TokenImage: item.TokenImage,
                        USDAmount: item.USDAmount,
                        SavingDate: be.dateFormatJson(item.SavingDate),
                        EndDate: be.dateFormatJson(item.EndDate),
                        ExpectedInterest: item.ExpectedInterested,
                        InterestRate: item.InterestedRate,
                        Status: item.Status,
                        ValueDate: be.dateFormatJson(item.ValueDate),
                        UserName: item.UserName,
                        Sponsor: item.Sponsor
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