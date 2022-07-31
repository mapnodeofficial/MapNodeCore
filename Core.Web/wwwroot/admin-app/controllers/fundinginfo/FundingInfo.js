var initController = function () {

    this.initialize = function () {
        
        registerEvents();
    }
    function registerEvents() {

        $('#btnCreate').off('click').on('click', function () {
            
            $('#frmFundingInfo').trigger("reset");
            $('#kt_modal_ViewFundingInfo').modal('show');
        });

        //$('body').on('click', '.btn-viewInfo', function (e) {
        //    var that = $(this).data('id');
        //    console.log('loadDetails');
        //});

        //$('body').on('click', '.btn-delete', function (e) {
        //    var that = $(this).data('id');

        //    be.confirm('Delete configs', 'Are you sure to delete?', function () {
        //        deleteBot(that);
        //    });
        //});

        $('#btnSave').on('click', function (e) {
            
            saveFundingInfo(e);
        });
    }

    function loadData(isPageChanged) {
        var template = $("#table-template").html();
        var render = "";

        $.ajax({
            type: "GET",
            data: {
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },

            url: "/Admin/FundingInfo/GetAllPaging",
            dataType: "json",
            success: function (response) {
                $.each(response.Results, function (i, item) {
                    console.log('item', item);
                    render += Mustache.render(template, {
                        UserId: item.UserId,
                        Amount: item.Amount,
                        Description: item.Description,
                        CreatedOn: be.dateTimeFormatJson(item.CreatedOn),

                    });
                });
                $("#tbl-content").html(render);
            },
            error: function (status) {
                be.notify('Cannot loading data', 'error')
            }
        })
    }

    
    function saveFundingInfo(e) {
        e.preventDefault();
        var amount = $('#in_Amount').val();
        

        if (parseFloat(amount)>2000000) {
            be.notify('Maximum amount can add is 2,000,000', 'error');
            return;
        }


        $.ajax({
            type: "POST",
            url: "/Admin/Game/saveFundingInfo",
            data: {
                
                Amount: amount,
            },
            dataType: "json",
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                be.notify('Add Funding Info Successful', 'success');
                $('#kt_modal_ViewFundingInfo').modal('hide');
                $('#frmFundingInfo').trigger("reset");
                
                be.stopLoading();
            },
            error: function () {
                be.notify('Has an error in save product progress', 'error');
                be.stopLoading();
            }
        })
    }

}

