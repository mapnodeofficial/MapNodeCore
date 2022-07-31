var CupItemController = function () {
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

        $('#btnSave').on('click', function (e) { saveToken(e) });

        $('body').on('click', '.btn-edit', function (e) { loadDetails(e, this) });
    }

    class CupItemViewModel {
        constructor() {
            this.Id = +($('#txcId').val());
            this.Name = $('#txtName').val();
            this.Code = $('#txtCode').val();
            this.HashRate = $('#txtHashRate').val();
            this.ImageUrl = $('#txtImageUrl').val();
            this.TimeToUse = $('#txtTimeToUse').val();
            this.MaxOut = $('#txtMaxOut').val();
            this.Price = parseFloat($('#txtPrice').val());
        }

        Validate() {
            var isValid = true;
            if (!this.Name)
                isValid = be.notify('Name is required!!!', "", 'error');
            if (!this.Code)
                isValid = be.notify('Code is required!!!', "", 'error');
            if (!this.HashRate)
                isValid = be.notify('Hash Rate is required!!!', "", 'error');
            if (!this.ImageUrl)
                isValid = be.notify('Image is required!!!', "", 'error');
            if (!this.TimeToUse)
                isValid = be.notify('Time To Use is required!!!', "", 'error');
            if (!this.MaxOut)
                isValid = be.notify('Max Out is required!!!', "", 'error');
            if (!this.Price)
                isValid = be.notify('Price is required!!!', "", 'error');
            return isValid;
        }
    }

    function loadDetails(e, element) {
        e.preventDefault();
        $.ajax({
            type: "GET",
            url: "/Admin/CupItem/GetById",
            data: { id: $(element).data('id') },
            dataType: "json",
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                $('#txcId').val(response.Id);
                $('#txtName').val(response.Name);
                $('#txtCode').val(response.Code);
                $('#txtHashRate').val(response.HashRate);
                $('#txtPrice').val(response.Price);
                $('#txtTimeToUse').val(response.TimeToUse);
                $('#txtMaxOut').val(response.MaxOut);
                $('#txtImageUrl').val(response.ImageUrl);
                $('#modal-add-edit').modal('show');

                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, `Status code: ${message.status}`, 'error');
                be.stopLoading();
            }
        });
    };

    function loadData(isPageChanged) {

        $.ajax({
            type: 'GET',
            data: {
                keyword: $('#txt-search-keyword').val(),
                page: be.configs.pageIndex,
                pageSize: be.configs.pageSize
            },
            url: '/admin/CupItem/GetAllPaging',
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
                        ImageUrl: item.ImageUrl.replace('~', ''),
                        Name: item.Name,
                        HashRate: be.formatCurrency(item.HashRate),
                        VolumeOfMining: be.formatCurrency((60 * 60 * item.HashRate) / 36000000),
                        MaxOut: be.formatCurrency(item.MaxOut),
                        TimeToUse: be.formatCurrency(item.TimeToUse),
                        Price: be.formatCurrency(item.Price),
                        Code: item.Code,
                        DateCreated: be.dateTimeFormatJson(item.DateCreated)
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

    function saveToken(e) {
        e.preventDefault();

        var model = new CupItemViewModel();
        if (model.Validate()) {
            $.ajax({
                type: "POST",
                url: "/Admin/CupItem/SaveEntity",
                data: { model },
                dataType: "json",
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {

                    be.notify('Update cup item is success', 'success');

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


}

function closeModal() {
    $('#modal-add-edit').modal('hide');
}