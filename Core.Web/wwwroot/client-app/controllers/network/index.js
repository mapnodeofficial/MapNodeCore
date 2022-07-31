var NetworkController = function () {
    this.initialize = function () {

        //loadTreeView();

    }
    var refIndex = 1;
    function registerEvents() {

        //document.getElementById("btnCopyReferlink").addEventListener("click", function () {
        //    copyToClipboard(document.getElementById("txtReferlink"));
        //});
    };

    function copyToClipboard(elem) {
        // create hidden text element, if it doesn't already exist
        var targetId = "_hiddenCopyText_";
        var isInput = elem.tagName === "INPUT" || elem.tagName === "TEXTAREA";
        var origSelectionStart, origSelectionEnd;
        if (isInput) {
            // can just use the original source element for the selection and copy
            target = elem;
            origSelectionStart = elem.selectionStart;
            origSelectionEnd = elem.selectionEnd;
        } else {
            // must use a temporary form element for the selection and copy
            target = document.getElementById(targetId);
            if (!target) {
                var target = document.createElement("textarea");
                target.style.position = "absolute";
                target.style.left = "-9999px";
                target.style.top = "0";
                target.id = targetId;
                document.body.appendChild(target);
            }
            target.textContent = elem.textContent;
        }
        // select the content
        var currentFocus = document.activeElement;
        target.focus();
        target.setSelectionRange(0, target.value.length);

        // copy the selection
        var succeed;
        try {
            succeed = document.execCommand("copy");
        } catch (e) {
            succeed = false;
        }
        // restore original focus
        if (currentFocus && typeof currentFocus.focus === "function") {
            currentFocus.focus();
        }

        if (isInput) {
            // restore prior selection
            elem.setSelectionRange(origSelectionStart, origSelectionEnd);
        } else {
            // clear temporary content
            target.textContent = "";
        }

        be.notify('Copy to clipboard is successful', 'success');

        return succeed;
    }

    function loadTotalNetworkInfo() {
        $.ajax({
            type: "GET",
            url: "/network/GetTotalNetworkInfo",
            data: {},
            dataType: "json",
            beforeSend: function () {
            },
            success: function (response) {
                $(".TotalF1").html(response.TotalF1);
                $(".TotalF2").html(response.TotalF2);
                $(".TotalF3").html(response.TotalF3);
                $(".TotalF4").html(response.TotalF4);
                $(".TotalF5").html(response.TotalF5);
                $(".TotalF6").html(response.TotalF6);
                $(".TotalF7").html(response.TotalF7);

                $("#TotalNetwork").html(response.TotalMember);

                $("#TotalCommissionMining").html(be.formatCurrency(response.TotalCommissionMining));
                $("#TotalAffiliateReferral").html(be.formatCurrency(response.TotalAffiliateReferral));
                $("#TotalAffiliateOnProfitMining").html(be.formatCurrency(response.TotalAffiliateOnProfitMining));
                $("#TotalBuyPackage").html(be.formatCurrency(response.TotalBuyPackage));

                $("#TotalSales").html(be.formatCurrency(response.TotalSales));
                $("#TotalWeakBranchSales").html(be.formatCurrency(response.TotalWeakBranchSales));
                $("#TotalStrongBranchSales").html(be.formatCurrency(response.TotalStrongBranchSales));

                $("#StakingLevelName").html(response.StakingLevelName);
            },
            error: function (message) {
                be.notify(`jqXHR.responseText: ${message.responseText}`, 'error');
            }
        });
    };

    function loadTreeView (){
        $("div#jstree").jstree({
            plugins: ["state", "types"],
            core: {
                themes: { responsive: false },
                "check_callback": true,
                'data': {
                    'url': function (node) {
                        return '/Referral/GetMemberTreeNode?address=' + DAppHelper.CurrentAddress; // Demo API endpoint -- Replace this URL with your set endpoint
                    },
                    'data': function (node) {
                        return {
                            'parent': node.id
                        };
                    }
                }
            },
            types: {
                default: {
                    "icon": "fa fa-users text-primary"
                },
                file: {
                    "icon": "fa fa-user text-danger"
                }
            },
            state: { "key": "demo3" }
        })
    }

    
    
}