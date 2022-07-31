var CustomerController = function () {
    this.initialize = function () {
        loadData();
        registerEvents();
    }

    function registerEvents() {

        $('#txt-search-keyword').keypress(function (e) {
            if (e.which === 13) {
                e.preventDefault();
                loadData(true);
            }
        });

        $("#ddl-show-page").on('change', function () {
            be.configs.pageSize = $(this).val();
            be.configs.pageIndex = 1;
            loadData(true);
        });

        $('body').on('click', '.btn-delete', function (e) {
            deleteCustomer(e, this);
        });

        $('body').on('click', '.btn-edit', function (e) {
            showSettingModal(e, this);
        });

        $('body').on('click', '.btn-reset-pass', function (e) {
            resetPassword(e, this);
        });

        $('body').on('click', '.btn-unlock', function (e) {
            unlockUser(e, this);
        });

        $('body').on('click', '.btn-lock', function (e) {
            lockUser(e, this);
        });

        $('body').on('click', '.btn-activate', function (e) {
            activateUser(e, this);
        });

        $('body').on('change', '.setting-2fa-check', function (e) {
            if (!$('.setting-2fa-check').is(":checked")) {
                let userId = $('#hidId').val();
                updateSettingUser(userId)
                this.disabled = true;
                $('.setting-2fa-status').text('Disable')
            }
        });

        $('body').on('change', '.setting-leader-check', function (e) {
            if (!$('.setting-leader-check').is(":checked")) {
                
                $('.setting-leader-status').text('Disable');
            }

            let userId = $('#hidId').val();
            var isLeader = $('.setting-leader-check').is(":checked");
            updateRoleUser(userId, isLeader);
        });


        $('body').on('click', '.btn-wallet', function (e) {
            showWalletModal(e, this);
        });

        $("#WalletType").on('change', function (e) {
            loadWalletBlanceByType($(this).val(), $('#hidId').val());
        });

        $("#btnConfirmUpdateWallet").on('click', function (e) {
            var data = {
                ActionType: parseInt($('#ActionType').val()),
                WalletType: $('#WalletType').val(),
                UserId: $('#hidId').val(),
                Amount: parseFloat($('#Amount').val().replace(/,/g, ''))
            };
            var isValid = true;

            if (data.WalletType <= 0) {
                isValid = be.notify('Please select wallet type.', 'error');
            }

            if (data.ActionType <= 0) {
                isValid = be.notify('Please select action type.', 'error');
            }

            if (!data.UserId) {
                isValid = be.notify('User is not valid', 'error');
            }

            if (data.Amount <= 0) {
                isValid = be.notify('Amount is not valid', 'error');
            }

            if (isValid) {

                $.ajax({
                    type: "POST",
                    headers: {
                        "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
                    },
                    url: '/Admin/User/UpdateWallet',
                    data: { modelJson: JSON.stringify(data) },
                    beforeSend: function () {
                        be.startLoading();
                    },
                    success: function (response) {
                        be.stopLoading();

                        if (response.Success) {

                            be.success('Update Wallet Balance', response.Message, function () {
                                window.location.reload();
                            });
                        }
                        else {
                            be.error('Update Wallet Balance', response.Message);
                        }
                    },
                    error: function (message) {
                        be.notify(`${message.responseText}`, 'error');
                        be.stopLoading();
                    }
                });
            }
        });
    };
    function showWalletModal(e, element) {
        let userId = $(element).attr('data-id');
        $('#hidId').val(userId);
        $('#ic_modal_wallet').modal('show');
    }

    function loadWalletBlanceByType(type, appUserId) {
        $.ajax({
            type: 'GET',
            url: '/Admin/User/GetWalletBlanceByType',
            dataType: 'json',
            data: { type: type, appUserId: appUserId },
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                $('#WalletBalance').val(response);
                be.stopLoading();
            },
            error: function (message) {
                be.notify(`${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function showSettingModal(e, element) {

        let userId = $(element).attr('data-id');

        $('#hidId').val(userId);

        loadSettingUser(userId);

        $('#modal-user-setting').modal('show');
    }

    function loadSettingUser(id) {
        $.ajax({
            type: "GET",
            url: `/admin/user/GetCustomerSetting?id=${id}`,
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                $('.setting-email').text(response.Email)
                $('.setting-2fa-check').attr('checked', response.TwoFactorEnabled)
                $('.setting-leader-check').attr('checked', response.IsLeader)

                let status = response.TwoFactorEnabled ? 'Enable' : 'Disable'
                $('.setting-2fa-status').text(status)

                if (response.TwoFactorEnabled) {
                    $('.setting-2fa-check').removeAttr('disabled');
                }

                be.stopLoading();

                if (response.RowCount) {
                    be.wrapPaging(response.RowCount, function () {
                        loadData();
                    }, isPageChanged);
                }
            },
            error: function (message) {
                be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function updateSettingUser(id) {

        var data = {
            Id: id,
            Enable2FA: false
        }

        $.ajax({
            type: "POST",
            url: `/admin/user/UpdateCustomerSetting`,
            data: JSON.stringify(data),
            contentType: "application/json",
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                be.stopLoading();

                be.notify('Update success!', 'success');
            },
            error: function (message) {
                be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function updateRoleUser(id,isLeader) {

        var data = {
            Id: id,
            IsLeader: isLeader
        }

        $.ajax({
            type: "POST",
            url: `/admin/user/UpdateCustomerLeaderSetting`,
            data: JSON.stringify(data),
            contentType: "application/json",
            beforeSend: function () {
                be.startLoading();
            },
            success: function (response) {
                be.stopLoading();

                be.notify('Update success!', 'success');
            },
            error: function (message) {
                be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }

    function deleteCustomer(e, element) {
        e.preventDefault();
        be.confirm('Delete member', 'You want to delete this member?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/User/DeleteCustomer",
                data: { id: $(element).data('id') },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {

                    be.stopLoading();

                    if (response.Success) {
                        be.notify(response.Message, 'success');

                        loadData(true);
                    }
                    else {
                        be.notify(response.Message, 'error');
                    }
                },
                error: function (message) {
                    be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }

    function resetPassword(e, element) {
        e.preventDefault();
        be.confirm('Reset member pasword', 'You want to reset user password to 12345678# ?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/User/ResetUserPassword",
                data: { id: $(element).data('id') },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {

                    be.stopLoading();

                    if (response.Success) {
                        be.notify(response.Message, 'success');

                        //loadData(true);
                    }
                    else {
                        be.notify(response.Message, 'error');
                    }
                },
                error: function (message) {
                    be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }

    function unlockUser(e, element) {
        e.preventDefault();
        be.confirm('Unlock user', 'You want to unlock user ?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/User/UnlockUser",
                data: { id: $(element).data('id') },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {

                    be.stopLoading();

                    if (response.Success) {
                        be.notify(response.Message, 'success');

                        loadData(true);
                    }
                    else {
                        be.notify(response.Message, 'error');
                    }
                },
                error: function (message) {
                    be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }

    function lockUser(e, element) {
        e.preventDefault();
        be.confirm('Lock user', 'You want to lock user ?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/User/lockUser",
                data: { id: $(element).data('id') },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {

                    be.stopLoading();

                    if (response.Success) {
                        be.notify(response.Message, 'success');

                        loadData(true);
                    }
                    else {
                        be.notify(response.Message, 'error');
                    }
                },
                error: function (message) {
                    be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }

    function activateUser(e, element) {
        e.preventDefault();
        be.confirm('Activate user', 'You want to activate user ?', function () {
            $.ajax({
                type: "POST",
                url: "/Admin/User/ConfirmEmail",
                data: { id: $(element).data('id') },
                beforeSend: function () {
                    be.startLoading();
                },
                success: function (response) {

                    be.stopLoading();

                    if (response.Success) {
                        be.notify(response.Message, 'success');

                        //loadData(true);
                    }
                    else {
                        be.notify(response.Message, 'error');
                    }
                },
                error: function (message) {
                    be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
                    be.stopLoading();
                }
            });
        });
    }

    function loadData(isPageChanged) {
        $.ajax({
            type: "GET",
            url: "/admin/user/GetAllCustomerPaging",
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
                        Id: item.Id,
                        Email: item.Email,
                        Sponsor: item.Sponsor,
                        BEP20PublishKey: item.BEP20PublishKey,
                        BNBBEP20PublishKey: item.BNBBEP20PublishKey,
                        BNBBalance: item.BNBBalance,
                        DateCreated: be.dateTimeFormatJson(item.DateCreated),
                        EmailConfirmed: be.getEmailConfirmed(item.EmailConfirmed),
                        RoleName: item.RoleName,
                        LockStatus: item.IsLockedOut == true ? "Locked" : "Active",
                        IsDisplayLock: item.IsLockedOut == true ? "block" : "none",
                        IsDisplayUnLock: item.IsLockedOut == true ? "none" : "block"
                    });
                });

                $("#lbl-total-records").text(response.RowCount);

                $('#tbl-content').html(render);

                be.stopLoading();

                if (response.RowCount) {
                    be.wrapPaging(response.RowCount, function () {
                        loadData();
                    }, isPageChanged);
                }
            },
            error: function (message) {
                be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
                be.stopLoading();
            }
        });
    }
}