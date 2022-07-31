var TokenConfigController = function () {
    this.initialize = function () {
        loadData();
        registerEvents();
        registerControl()
    }

    class TokenConfigViewModel {
        constructor() {
            this.Id = +($('#hidId').val());
            this.Name = $('#txtName').val();
            this.TokenCode = $('#txtTokenCode').val();
            this.ContractAddress = $('#txtContractAddress').val();
            this.TokenImageUrl = $('#txtTokenImageUrl').val();
            this.TotalSupply = $('#txtTotalSupply').val();
            this.Decimals = $('#txtDecimals').val();
            this.MinSaving = $('#txtMinSaving').val();
            this.MaxSaving = $('#txtMaxSaving').val();
            this.MinWithdraw = $('#txtMinWithdraw').val();
            this.MaxWithdraw = $('#txtMaxWithdraw').val();
            this.MinDeposit = $('#txtMinDeposit').val();
            this.MaxDeposit = $('#txtMaxDeposit').val();
            this.Type = $("#TypeId").val();
            this.Interest180Day = $('#txt180Rate').val();
            this.Interest270Day = $('#txt270Rate').val();
            this.Interest360 = $('#txt360Rate').val();
            this.Interest720 = $('#txt720Rate').val();
            this.FeeWithdraw = $('#txtFeeWithdraw').val();
        }

        Validate() {
            var isValid = true;
            if (!this.Name)
                isValid = be.notify('Name is required!!!', "", 'error');
            if (!this.TokenCode)
                isValid = be.notify('Token code is required!!!', "", 'error');
            if (!this.ContractAddress)
                isValid = be.notify('Contract Address is required!!!', "", 'error');
            if (!this.TokenImageUrl)
                isValid = be.notify('Token image is required!!!', "", 'error');

            if (!this.TotalSupply)
                isValid = be.notify('Total supply is required!!!', "", 'error');

            if (!this.Decimals)
                isValid = be.notify('Decimals is required!!!', "", 'error');

            if (!this.MinSaving)
                isValid = be.notify('Min Saving is required!!!', "", 'error');

            if (!this.MaxSaving)
                isValid = be.notify('Max Saving is required!!!', "", 'error');

            if (!this.Type)
                isValid = be.notify('Type is required!!!', "", 'error');

            return isValid;
        }
    }

    function registerControl() {
        //$('#RoleId,#SearchRoleId').select2({
        //    placeholder: "Chọn vai trò",
        //    allowClear: true,
        //});
    }



    function registerEvents() {

        $('#txt-search-keyword').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                loadData(true);
            }
        });

        $("#SearchRoleId").on('change', function (e) {
            e.preventDefault();
            loadData(true);
        });

        $("#ddl-show-page").on('change', function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadData(true);
        });

        $("#btn-create").on('click', function () {
            resetFormMaintainance();
            $('#modal-add-edit').modal('show');
        });

        $('body').on('click', '.btn-edit', function (e) { loadDetails(e, this) });

        $('#btnSave').on('click', function (e) { saveToken(e) });

        $('body').on('click', '.btn-delete', function (e) {
            deleteToken(e, this)
        });
    };


    function saveToken(e) {
        e.preventDefault();

        var model = new TokenConfigViewModel();
        if (model.Validate()) {
            $.ajax({
                type: "POST",
                url: "/Admin/Token/SaveEntity",
                data: { model },
                dataType: "json",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {

                    be.notify('Update token config is success', 'success');

                    $('#modal-add-edit').modal('hide');

                    resetFormMaintainance();

                    be.stopLoading();

                    loadData(true);
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, `Status code: ${message.status}`, 'error');
                    be.stopLoading();
                },
            });
        }
    };

    function deleteToken(e, element) {
        e.preventDefault();
        be.confirm('Delete token', 'Are you sure to delete?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/Token/Delete",
                data: { id: $(element).data('id') },
                dataType: "json",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function () {
                    be.notify('Remove token is success', 'success');
                    be.stopLoading();
                    loadData();
                },
                error: function (message) {
                    be.notify(`${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    };

    function resetFormMaintainance() {
        $('#hidId').val(0);
        $('#txtName').val('');
        $('#txtTokenCode').val('');
        $('#txtContractAddress').val('');
        $('#txtTokenImageUrl').val('');
        $('#txtTotalSupply').val('');
        $('#txtDecimals').val('');
        $('#txtMinSaving').val('');
        $('#txtMaxSaving').val('');
    };

    function loadData(isPageChanged) {
        $.ajax({
            type: "GET",
            url: "/admin/Token/GetAllPaging",
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            dataType: "json",
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {

                var template = $('#table-template').html();
                var render = "";

                $.each(response.Results, function (i, item) {
                    render += Mustache.render(template, {
                        Name: item.Name,
                        TokenCode: item.TokenCode,
                        ContractAddress: item.ContractAddress,
                        TokenImageUrl: item.TokenImageUrl,
                        TotalSupply: item.TotalSupply,
                        Decimals: item.Decimals,
                        MinSaving: item.MinSaving,
                        MaxSaving: item.MaxSaving,
                        TotalSaving: item.TotalSaving,
                        TypeName: item.TypeName,
                        Id: item.Id,
                        CreatedOn: item.CreatedOn
                    });
                });

                $("#lbl-total-records").text(response.RowCount);

                $('#tbl-content').html(render);
                be.stopLoading();
                if (response.RowCount)
                    be.wrapPaging(response.RowCount, function () {
                        loadData()
                    }, isPageChanged);

                
            },
            error: function (message) {
                be.notify(`${message.responseText}`, `Status code: ${message.status}`, 'error');
                be.stopLoading();
            }
        });
    };

    function loadDetails(e, element) {
        e.preventDefault();
        $.ajax({
            type: "GET",
            url: "/Admin/Token/GetById",
            data: { id: $(element).data('id') },
            dataType: "json",
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                $('#hidId').val(response.Id);
                $('#txtName').val(response.Name);
                $('#txtTokenCode').val(response.TokenCode);
                $('#txtContractAddress').val(response.ContractAddress);
                $('#txtTokenImageUrl').val(response.TokenImageUrl);
                $('#txtTotalSupply').val(response.TotalSupply);
                $('#txtDecimals').val(response.Decimals);
                $('#txtMinSaving').val(response.MinSaving);
                $('#txtMaxSaving').val(response.MaxSaving);
                $('#txtMinWithdraw').val(response.MinWithdraw);
                $('#txtMaxWithdraw').val(response.MaxWithdraw);
                $('#txtMinDeposit').val(response.MinDeposit);
                $('#txtMaxDeposit').val(response.MaxDeposit);

                $('#TypeId').val(response.Type).trigger('change');

             
                $('#txt180Rate').val(response.Interest180Day);
                $('#txt270Rate').val(response.Interest270Day);
                $('#txt360Rate').val(response.Interest360Day);
                $('#txt720Rate').val(response.Interest720Day);
                $('#txtFeeWithdraw').val(response.FeeWithdraw);
                

                $('#modal-add-edit').modal('show');

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, `Status code: ${message.status}`, 'error');
                be.stopLoading();
            }
        });
    };
}

function closeModal() {
    $('#modal-add-edit').modal('hide');
}