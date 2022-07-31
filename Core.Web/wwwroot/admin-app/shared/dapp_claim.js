"use strict";

// Class definition
var KTDatatablesServerSide = function () {
    // Shared variables
    var table;
    var dt;
    var filterPayment;
   
    // Private functions
    var initDatatable = function () {
        dt = $("#kt_datatable_example_1").DataTable({
            searchDelay: 500,
            processing: true,
            serverSide: true,

            stateSave: true,
            select: false,
            ajax: {      
                url: '/DApp/GetHistroryClaim',
                "type": "POST",
                "data": {
                    "address": address: accounts[0],
            },
            columns: [
                {
                    data: 'AddressTo',
                    render: function (data, type, row) {
                        return row.AddressTo + '<br>' + row.AddressFrom ;
                    }
                },
                {
                    data: 'BNBAmount',,
                    render: function (data, type, row) {
                        return row.BNBAmount + '<br>' + row.TokenAmount;
                    }
                },
            {
                data: 'BNBTransactionHash',
                    render: function (data, type, row) {
                        return row.BNBTransactionHash + '<br>' + row.TokenTransactionHash;
                    }            },
                { data: 'DateCreated' },
                { data: 'WalletType' },
            ],
            // Add data-filter attribute
        });
        table = dt.$;

        // Re-init functions on every table re-draw -- more info: https://datatables.net/reference/event/draw
        dt.on('draw', function () {
            initToggleToolbar();
            toggleToolbars();
         
            KTMenu.createInstances();
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = function () {
        const filterSearch = document.querySelector('[data-kt-docs-table-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {

            dt.search(e.target.value).draw();
        });
    }

    // Filter Datatable
    var handleFilterDatatable = () => {
        // Select filter options
        filterPayment = document.querySelectorAll('[data-kt-docs-table-filter="payment_type"] [name="payment_type"]');
        const filterButton = document.querySelector('[data-kt-docs-table-filter="filter"]');

        // Filter datatable on submit
        filterButton.addEventListener('click', function () {
            // Get filter values
            let paymentValue = '';

            // Get payment value
            filterPayment.forEach(r => {
                if (r.checked) {
                    paymentValue = r.value;
                }

                // Reset payment value if "All" is selected
                if (paymentValue === 'all') {
                    paymentValue = '';
                }
            });

            // Filter datatable --- official docs reference: https://datatables.net/reference/api/search()
            dt.search(paymentValue).draw();
        });
    }


  

    // Init toggle toolbar
    var initToggleToolbar = function () {
        // Toggle selected action toolbar
        // Select all checkboxes
        const container = document.querySelector('#kt_datatable_example_1');
      //  const checkboxes = container.querySelectorAll('[type="checkbox"]');

        // Select elements
       /* const deleteSelected = document.querySelector('[data-kt-docs-table-select="delete_selected"]');
       */
        // Toggle delete selected toolbar


  
    }

    // Toggle toolbars
    var toggleToolbars = function () {
        // Define variables
        const container = document.querySelector('#kt_datatable_example_1');
        const toolbarBase = document.querySelector('[data-kt-docs-table-toolbar="base"]');
   

        // Select refreshed checkbox DOM elements
        const allCheckboxes = container.querySelectorAll('tbody [type="checkbox"]');

        // Detect checkboxes state & count
        let checkedState = false;
        let count = 0;

        // Count checked boxes
        allCheckboxes.forEach(c => {
            if (c.checked) {
                checkedState = true;
                count++;
            }
        });

        // Toggle toolbars
        if (checkedState) {
        //    selectedCount.innerHTML = count;
            toolbarBase.classList.add('d-none');
           // toolbarSelected.classList.remove('d-none');
        } else {
            toolbarBase.classList.remove('d-none');
           // toolbarSelected.classList.add('d-none');
        }
    }

    // Public methods
    return {
        init: function () {
             initDatatable();
            handleSearchDatatable();
            initToggleToolbar();
      
        }
    }
}();

// On document ready
KTUtil.onDOMContentLoaded(function () {
    KTDatatablesServerSide.init();

});