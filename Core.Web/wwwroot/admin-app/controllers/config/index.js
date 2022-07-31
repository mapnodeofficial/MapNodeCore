var ConfigController = function () {
    this.initialize = function () {
        loadData();
        registerEvents();
        registerControl()
    }

    class ConfigViewModel {
        constructor() {
            this.Id = $('#hidId').val();
            this.Value = $('#txtValue').val();
            this.Remarks = $('#txtRemarks').val();
            this.Name = $('#txtName').val();
        }

        Validate() {
            var isValid = true;
            if (!this.Value)
                isValid = be.notify('Value is required!!!', "", 'error');
            if (!this.Remarks)
                isValid = be.notify('Remarks is required!!!', "", 'error');
            
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

        

        $('body').on('click', '.btn-edit', function (e) { loadDetails(e, this) });

        $('#btnSave').on('click', function (e) { saveConfig(e) });

    };


    function saveConfig(e) {
        e.preventDefault();

        var model = new ConfigViewModel();
        if (model.Validate()) {
            $.ajax({
                type: "POST",
                url: "/Admin/Config/SaveEntity",
                data: { model },
                dataType: "json",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {

                    be.notify('Update config is success', 'success');

                    $('#modal-add-edit').modal('hide');


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

    

    function loadData(isPageChanged) {
        $.ajax({
            type: "GET",
            url: "/admin/Config/GetAllPaging",
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
                        Value: item.Value,
                        Remarks: item.Remarks,
                        Id: item.Id,
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
            url: "/Admin/Config/GetById",
            data: { id: $(element).data('id') },
            dataType: "json",
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                $('#hidId').val(response.Id);
                $('#txtName').val(response.Name);
                $('#txtValue').val(response.Value);
                $('#txtRemarks').val(response.Remarks);
                
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