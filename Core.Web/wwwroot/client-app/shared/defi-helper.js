var DeFiHelper = {

    Web3Instant: function fn() {
        return new Web3(Web3.givenProvider)
    }(),

    ChainId: 56,
    Chain: '0x38',
    ChainName: 'Binance Smart Chain',
    ChainTokenName:'Binance Coin',
    CurrencyName: 'BNB',
    RpcUrls: 'https://quiet-weathered-voice.bsc.quiknode.pro/d3e2*****f22baad17a0ba707bd/',
    CurrentAddress: '',
    CurrentSponsorId: '',
    BalanceInEth: 0.00,
    ContractAddress: '',
    Decimals: 18,
    IsBNB: false,
    SelectedTokenCode: '',
    ABIConfig: '',
    DappRefCode:'',
    ChainExplorer: 'https://bscscan.com',
    ChainRpc:'https://rpc.ankr.com/bsc',
    ReferralLink: window.location.origin + '/reflink/',

    initialize: function () {

        if (!window.Web3Instant) {
            window.Web3Instant = new Web3(Web3.givenProvider);
        }

        if (!(ethereum && ethereum.isMetaMask)) {
            //show popup with link to metamask extension install
            window.location.replace('https://metamask.io/');
        }

        if (!(ethereum && ethereum.isTrust)) {
            //window.location.replace('https://metamask.io/');
        }
    },

    CheckNetWork: async function fn() {


        try {
            let currentChain = await DeFiHelper.Web3Instant.eth.getChainId();

            if (currentChain === DeFiHelper.ChainId) {
                return true;
            }

            return false

        } catch (e) {
            be.error('DAPP Notification', e.message)
            return false
        }
    },

    AnyConnectedAccounts: async function fn() {
        try {
            var accounts = await DeFiHelper.Web3Instant.eth.getAccounts();

            return accounts && accounts.length > 0;
        } catch (e) {
            console.log('DAPP Notification', e.message);

            return false;
        }
    },

    AddBSCChain: async function fn() {
        window.ethereum.request({
            method: 'wallet_addEthereumChain',
            params: [{
                chainId: '0x38',
                chainName: 'Binance Smart Chain',
                nativeCurrency: {
                    name: 'Binance Coin',
                    symbol: 'BNB',
                    decimals: 18
                },
                rpcUrls: [RpcUrls],
                blockExplorerUrls: ['https://bscscan.com']
            }]
        }).catch((error) => {
            console.log(error)
        });
    },

    SwitchChain: async function fn() {


        if (ethereum.isTrust)
            return true

        try {

            await ethereum.request({
                method: 'wallet_switchEthereumChain',
                params: [{
                    chainId: DeFiHelper.Chain
                }],
            });

            return true;
        } catch (switchError) {
            debugger;
            // This error code indicates that the chain has not been added to DAPP.
            if (switchError.code === 4902) {
                try {
                    window.ethereum.request({
                        method: 'wallet_addEthereumChain',
                        params: [{
                            chainId: DeFiHelper.Chain,
                            chainName: DeFiHelper.ChainName,
                            nativeCurrency: {
                                name: DeFiHelper.ChainTokenName,
                                symbol: DeFiHelper.CurrencyName,
                                decimals: DeFiHelper.Decimals
                            },
                            rpcUrls: [DeFiHelper.ChainRpc],
                            blockExplorerUrls: [DeFiHelper.ChainExplorer]
                        }]
                    })
                        .catch((error) => {
                            console.log(error)
                        });
                } catch (addError) {
                    return false
                }
            }
            
            return false
        }
    },

    RegisterEthereumEvents: function fn(accountChangeHandler) {
        ethereum.on('accountsChanged', accountChangeHandler);
    },

    RegisterEthereumNetworkChangeEvents: function fn(networkChangeHandler) {
        ethereum.on('chainChanged', networkChangeHandler);
    },

    HasInstalledMetaMask: function fn() {
        try {
            return window.ethereum && window.ethereum.isMetaMask
        } catch (e) {
            return false;
        }
    },

    HasInstalledTrust: function fn() {
        try {
            return window.ethereum && window.ethereum.isTrust
        } catch (e) {
            return false;
        }
    },

    DApp: function () {
        this.currentAccount = '';

        this.init = async function fn() {

            registerEvents()


            //console.log("Initializing example");
            //console.log("WalletConnectProvider is", WalletConnectProvider);

            // Tell Web3modal what providers we have available.
            // Built-in web browser provider (only one can exist as a time)
            // like MetaMask, Brave or Opera is added automatically by Web3modal


            //console.log("Initializing example");
            //console.log("WalletConnectProvider is", WalletConnectProvider);
            if (!window.ethereum) {
                //ShowConnectButton()
                //$('#connect-wallet-modal').modal('show');
                //$('.dapp_add-asset').hide()
                return
            }

            if (window.ethereum.isTrust) {
                //$('.dapp_add-asset').hide()
            }

            DeFiHelper.RegisterEthereumEvents(handleAccountsChanged)

            DeFiHelper.RegisterEthereumNetworkChangeEvents(handleNetworkChanged)

            var hasAccounts = await DeFiHelper.AnyConnectedAccounts()

            if (hasAccounts) {
                var isValidNet = await DeFiHelper.CheckNetWork();
                if (isValidNet) {
                    await handleRequestAccounts();
                    return;
                } else {
                    HandleDisconnected();
                    await DeFiHelper.DAppDisconnect();
                }
            } else {
                await DeFiHelper.DAppDisconnect();
                HandleDisconnected();
            }
        }

        function registerEvents() {

            $('#wallet-connect-metamask').on('click', async function (e) {
                e.preventDefault();
                if (!DeFiHelper.HasInstalledMetaMask()) {
                    if (be.isDevice()) {
                        var url = 'https://metamask.app.link/dapp/fbsdefi.com/reflink/' + DeFiHelper.DappRefCode;
                        window.location.replace(url);
                        return;
                    }
                }

                if (DeFiHelper.HasInstalledMetaMask()) {
                    await ConnectMetaMask();

                    $('#connect-wallet-modal').modal('hide');
                } else {
                    $('#walletInformation').modal('show');
                    $('#WalletConnect').modal('hide');
                    $("#linkProvider").attr("href", "https://metamask.io/download/")
                    /*window.open('https://metamask.io/', '_blank');*/
                    return;
                }
            });

            $('#wallet-connect-trust').on('click', async function (e) {
                e.preventDefault()

                if (DeFiHelper.HasInstalledTrust()) {
                    await ConnectTrustWallet();

                    $('#connect-wallet-modal').modal('hide');
                } else {
                    $('#walletInformation').modal('show');
                    $('#WalletConnect').modal('hide');
                    $("#linkProvider").attr("href", "https://link.trustwallet.com/")

                    return;
                }
            });

            
        }



        async function ConnectTrustWallet() {
            await handleRequestAccounts();
        }

        async function ConnectMetaMask() {
            if (!DeFiHelper.HasInstalledMetaMask()) {
                if (be.isDevice()) {
                    window.location.replace('https://metamask.app.link/dapp/fbsdefi.com');
                    return;
                }
            }

            var isValidNet = await DeFiHelper.CheckNetWork();

            if (!isValidNet) {
                await DeFiHelper.SwitchChain();
            } else {
                await handleRequestAccounts();
            }
        }

        async function CheckBalance(address) {

            try {

                if (DeFiHelper.IsBNB == "True") {

                    DeFiHelper.Web3Instant.eth.getBalance(address).then((balanceInWei) => {

                        DeFiHelper.BalanceInEth = parseFloat(DeFiHelper.Web3Instant.utils.fromWei(balanceInWei));

                        HandleSavingDetailBalance(DeFiHelper.BalanceInEth);

                        HandleExchangeDetailBalance(DeFiHelper.BalanceInEth);
                    });

                } else {
                    if (!DeFiHelper.ContractAddress) {
                        return;
                    }
                    const abiJsonArr = [];
                    DeFiHelper.ABIConfig = DeFiHelper.ABIConfig.replace(/\\/g, "");;
                    var abiJsonObj = JSON.parse(DeFiHelper.ABIConfig);
                    abiJsonArr.push(abiJsonObj);

                    const contract = new DeFiHelper.Web3Instant.eth.Contract(abiJsonArr, DeFiHelper.ContractAddress);
                    const balance = await contract.methods.balanceOf(address).call();
                    // note that this number includes the decimal places (in case of BUSD, that's 18 decimal places)
                    var adjustedBalance = balance * 10 ** -DeFiHelper.Decimals;

                    DeFiHelper.BalanceInEth = parseFloat(adjustedBalance);
                    HandleSavingDetailBalance(DeFiHelper.BalanceInEth);
                }

            } catch (error) {
                //console.log(error);
            }
        }

        //request to metamask if not connected, will show a prompt from metamask
        async function handleRequestAccounts() {
            try {

                var isValidNet = await DeFiHelper.CheckNetWork();

                if (!isValidNet) {
                    return;
                }

                let accounts = await DeFiHelper.Web3Instant.eth.requestAccounts();

                if (accounts && accounts.length > 0) {
                    this.CurrentAddress = accounts[0];

                    await DeFiHelper.DAppConnect(CurrentAddress);
                    await CheckBalance(CurrentAddress);

                    HandleConnected();
                    ConnectWalletPage();
                    return accounts

                }
            } catch (e) {
                //ShowConnectButton()
                DisconnectWalletPage();
                HandleDisconnected();
                await DeFiHelper.DAppDisconnect();
            }
        }

        async function handleAccountsChanged(accounts) {

            if (accounts.length === 0) {
                console.log("handleAccountsChanged - accounts = 0");
                // DAPP is locked or the user has not connected any accounts
                //ShowConnectButton()
                HandleDisconnected();
                await DeFiHelper.DAppDisconnect();

            } else if (accounts[0]) {
                // Do any other work!
                var isValidNet = await DeFiHelper.CheckNetWork();
                if (isValidNet) {
                    CurrentAddress = accounts[0];

                    await CheckBalance(CurrentAddress);
                    await DeFiHelper.DAppConnect(CurrentAddress);
                    HandleConnected();
                    ConnectWalletPage();

                    return;
                }

                HandleDisconnected();
                DisconnectWalletPage();
                await DeFiHelper.DAppDisconnect();
            }
        }

        async function handleNetworkChanged(networkId) {
            if (networkId !== DeFiHelper.ChainId) {
                HandleDisconnected();
            }
        }

        function HandleConnected() {
            if (CurrentAddress=='') {
                return;
            }
            var btnConnect = $(".btnConnectWallet");
            var btnBuyToken = $(".btnBuyToken");
            var lbWalletAddress = $(".lbWalletAddress");
            btnConnect.hide();
            btnBuyToken.show();
            $('#WalletConnect').modal('hide');

            lbWalletAddress.text(FormatAddress(CurrentAddress));
        }

        function FormatAddress(source) {

            var dest = '';
            dest = source.slice(0, 2) + '....' + source.slice(source.length - 4);

            return dest;
        }

        function HandleDisconnected() {
            CurrentAddress = '';
            currentAccount = '';
            BalanceInEth = 0.00;
            var btnConnect = $(".btnConnectWallet");
            var btnBuyToken = $(".btnBuyToken");
            var lbWalletAddress = $(".lbWalletAddress");
            btnConnect.show();
            btnBuyToken.hide();

            lbWalletAddress.text('');

            DeFiHelper.SetReferralInfo("Not connected wallet yet");

            DisconnectWalletPage();

            HandleSavingDetailBalance(0);
        }

        function DisconnectWalletPage() {

            var isWallet = $('#isWallet').val();

            if (isWallet === undefined) {
                return;
            }

            walletObj.ReloadWallet('');
        }

        function ConnectWalletPage() {

            var isWallet = $('#isWallet').val();
            if (isWallet === undefined) {
                return;
            }

            walletObj.ReloadWallet(DeFiHelper.CurrentAddress);
        }

        function HandleExchangeDetailBalance(balance) {


            if (!IsExchange()) {
                return;
            }
            var exchangeObj = new ExchangeController();
            exchangeObj.ReloadBalance(balance);
        }

        

        function HandleSavingDetailBalance(balance) {

            if (!IsSavingDetail()) {
                return;
            }

            var savingDetailObj = new SavingDetailController();
            savingDetailObj.ReloadBalance(balance);

        }

        function IsSavingDetail() {
            var isWallet = $('#isSavingDetail').val();
            if (isWallet === undefined) {
                return false;
            }
            return true;
        }

        function IsExchange() {
            var isWallet = $('#isExchange').val();
            if (isWallet === undefined) {
                return false;
            }
            return true;
        }

        //function UpdateErrorMetaMask(transactionHex, errorCode) {
        //    let data = {
        //        TransactionHex: transactionHex,
        //        ErrorCode: errorCode.toString()
        //    }
        //    return DeFiHelper.PostLegacyAsync('/DAPP/UpdateErrorMetaMask', data);
        //}

    },

    VerifyMetaMaskRequest: function (transactionHash, dappTxnHash) {
        debugger;
        let data = {
            TransactionHash: transactionHash,
            DappTxnHash: dappTxnHash,
            IsBNB: DeFiHelper.IsBNB == "True"
        }

        return DeFiHelper.PostAsync('/SavingDefi/VerifyTransactionRequest', data)
    },

    VerifySaleRequest: function fn(transactionHash, dappTxnHash) {
        debugger;

        let data = {
            TransactionHash: transactionHash,
            DappTxnHash: dappTxnHash
        }

        return DeFiHelper.PostAsync('/SaleDefi/VerifyTransactionRequest', data)
    },

    UpdateErrorMetaMask: function fn(transactionHex, errorCode) {

        var errorMsg = "Undefine error";
        if (errorCode !== undefined) {
            errorMsg = errorCode.toString()
        }
        let data = {
            TransactionHex: transactionHex,
            ErrorCode: errorMsg
        }
        return DeFiHelper.PostLegacyAsync('/SavingDefi/UpdateErrorMetaMask', data);
    },

    UpdateSaleError: function fn(transactionHex, errorCode) {
        debugger;

        var errorMsg = "Undefine error";

        if (errorCode !== undefined) {
            errorMsg = errorCode.toString()
        }
        let data = {
            TransactionHex: transactionHex,
            ErrorCode: errorMsg
        }

        return DeFiHelper.PostLegacyAsync('/SaleDefi/UpdateErrorMetaMask', data);
    },

    InitializeSavingProgress: function (amount, rateId) {

        let data = {
            Address: DeFiHelper.CurrentAddress,
            IsDevice: DeFiHelper.isDevice,
            Amount: amount,
            Timeline: rateId,
            TokenCode: DeFiHelper.SelectedTokenCode
        }

        if (ethereum.isMetaMask) {
            data.WalletType = "Metamask"
        }

        if (ethereum.isTrust) {
            data.WalletType = "Trust"
        }

        return DeFiHelper
            .PostAsync('/SavingDefi/InitializeTransactionProgress/', data);
    },

    InitializeSaleTokenProgress: function fn(amount, typeId) {

        let data = {
            Address: DeFiHelper.CurrentAddress,
            IsDevice: DeFiHelper.isDevice,
            BNBAmount: amount,
            TypeId: typeId
        }

        if (ethereum.isMetaMask) {
            data.WalletType = "Metamask"
        }

        if (ethereum.isTrust) {
            data.WalletType = "Trust"
        }

        return DeFiHelper
            .PostAsync('/SaleDefi/InitializeTransactionProgress/', data);
    },

    SendTransaction: async function fn(data) {

        return await ethereum.request({
            method: 'eth_sendTransaction',
            params: [
                data,
            ]
        });
    },

    SendSmartContractTransaction: async function fn(data) {

        const TRANSFER_FUNCTION_ABI = [{ "constant": false, "inputs": [{ "name": "_to", "type": "address" }, { "name": "_value", "type": "uint256" }], "name": "transfer", "outputs": [], "payable": false, "stateMutability": "nonpayable", "type": "function" }];

        const contract = new DeFiHelper.Web3Instant.eth
            .Contract(TRANSFER_FUNCTION_ABI, DeFiHelper.ContractAddress);

        return contract.methods.transfer(data.to, data.value)
            .send({
                from: data.from,
                //gas: data.gas,
                //gasPrice: data.gasPrice,
                data: data.data
            });
    },

    ConfirmProcessingTransaction: async function (savingAmount, rateId) {

        //switch net
        var isSwitchSucess = await DeFiHelper.SwitchChain()

        if (!isSwitchSucess) {
            return;
        }

        be.startLoading();

        DeFiHelper
            .InitializeSavingProgress(savingAmount, rateId)
            .then(async res => {


                if (!res.Success) {

                    be.stopLoading()

                    if (!res.Message) {
                        be.error('DAPP Notification', 'Can not process transaction.')
                        return;
                    }

                    be.error('DAPP Notification', res.Message)

                    return
                }

                var amountConverted = res.Data.Value * ("1e" + res.Data.Decimals);

                //var amountConverted = Web3.utils.toWei(res.Data.Value.toString(), "ether");

                var hexAmount = Web3.utils.numberToHex(amountConverted);

                let data = {
                    from: res.Data.From,
                    to: res.Data.To,
                    value: hexAmount,
                    //gasPrice: '0x55f0',
                    //gas: '0x2540be400',
                    data: res.Data.TransactionHex
                };

                let transactionHash = '';

                if (DeFiHelper.IsBNB == "False") {

                    let temptransactionHash = await DeFiHelper
                        .SendSmartContractTransaction(data)
                        .then(txh => txh)
                        .catch(error => {
                            if (window.ethereum.isMetaMask) {

                                DeFiHelper.UpdateErrorMetaMask(data.data, error.code);

                                if (error.code === 4001)
                                    be.error('DAPP Notification', 'Transaction was Rejected')
                                else {
                                    be.error('DAPP Notification', 'Something went wrong! Please contact administrator for support. Code: ' + error.code)
                                }
                            }

                            if (window.ethereum.isTrust) {
                                DeFiHelper.UpdateErrorMetaMask(data.data, 4001);
                                be.error('DAPP Notification', 'Transaction was Rejected')
                            }

                            be.stopLoading();
                        })

                    if (!temptransactionHash) {
                        return;
                    }

                    //smart contract return txn difference bnb
                    transactionHash = temptransactionHash.transactionHash;
                }
                else {
                    debugger;

                    transactionHash = await DeFiHelper
                        .SendTransaction(data)
                        .then(txh => txh)
                        .catch(error => {
                            if (window.ethereum.isMetaMask) {

                                DeFiHelper.UpdateErrorMetaMask(data.data, error.code);

                                if (error.code === 4001)
                                    be.error('DAPP Notification', 'Transaction was Rejected')
                                else {
                                    be.error('DAPP Notification', 'Something went wrong! Please contact administrator for support. Code: ' + error.code)
                                }
                            }

                            if (window.ethereum.isTrust) {

                                DeFiHelper.UpdateErrorMetaMask(data.data, 4001);

                                be.error('DAPP Notification', 'Transaction was Rejected');
                            }

                            be.stopLoading();
                        })

                    if (!transactionHash) {
                        return;
                    }
                }

                be.stopLoading();

                be.startLoading('<b>We are processing your transaction.</b>' +
                    '<b> Kindly wait for a moment ultil the process completed...</b>');

                DeFiHelper
                    .VerifyMetaMaskRequest(transactionHash, data.data)
                    .then(res => {

                        be.stopLoading();

                        if (!res.Success) {

                            be.error('DAPP Notification', res.Message)
                            return
                        }

                        be.success('DAPP Notification', res.Message, function () {
                            window.location.reload();
                        })

                    });
            })
            .catch(error => {
                //console.log(error);
                be.error('DAPP Notification',
                    'Something went wrong! please, contact administrator.');
                be.stopLoading();
            })
    },

    ConfirmSaleTokenTransaction: async function fn(amount, typeId) {
        debugger;

        //switch net
        var isSwitchSucess = await DeFiHelper.SwitchChain()

        if (!isSwitchSucess) {
            return;
        }

        be.startLoading();

        DeFiHelper
            .InitializeSaleTokenProgress(amount, typeId)
            .then(async res => {
                debugger;

                if (!res.Success) {

                    be.stopLoading();

                    if (!res.Message) {
                        be.error('DAPP Notification', 'Can not process transaction.')
                        return;
                    }

                    be.error(res.Message)
                    return
                }

                var amountConverted = Web3.utils.toWei(res.Data.Value.toString());

                var hexAmount = Web3.utils.numberToHex(amountConverted, 'ether');

                let data = {
                    from: res.Data.From,
                    to: res.Data.To,
                    value: hexAmount,
                    //gasPrice: '0x55f0',
                    //gas: '0x2540be400',
                    data: res.Data.TransactionHex
                };

                let transactionHash = await DeFiHelper.SendTransaction(data)
                    .then(txh => txh)
                    .catch(error => {
                        if (window.ethereum.isMetaMask) {

                            DeFiHelper.UpdateSaleError(data.data, error.code);

                            if (error.code === 4001) {
                                be.error('DAPP Notification', 'Transaction was Rejected')
                            }
                            else {
                                be.error('DAPP Notification',
                                    'Something went wrong! Please contact administrator for support. Code: '
                                    + error.code)
                            }
                        }

                        if (window.ethereum.isTrust) {

                            DeFiHelper.UpdateSaleError(data.data, 4001);

                            be.error('DAPP Notification', 'Transaction was Rejected')
                        }

                        be.stopLoading();
                    })

                if (!transactionHash) {
                    return;
                }

                be.stopLoading();

                be.startLoading('<b>We are processing your transaction.</b>' +
                    '<b> Kindly wait for a moment ultil the process completed...</b>');

                DeFiHelper
                    .VerifySaleRequest(transactionHash, data.data)
                    .then(res => {

                        debugger;

                        be.stopLoading();

                        if (!res.Success) {

                            be.error('DAPP Notification', res.Message)
                            return
                        }
                        be.success('DAPP Notification', res.Message, function () {
                            window.location.reload();
                        })
                    });
            })
            .catch(error => {

                //console.log(error);
                be.error('DAPP Notification', 'Something went wrong! please, contact administrator.')
                be.stopLoading();
            })
    },

    PostLegacyAsync: async function fn(url = '', data = {}) {
        $.ajax({
            type: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify(data),
            url: url,
            dataType: 'JSON',
            beforeSend: function () { },
            success: function (response) {

                if (response.status === 401) {
                    be.error('DAPP Notification',
                        'Please, Disconnect and connect your wallet again!')
                }

                return response.json()
            },
            error: function (message) {
                be.error(`${message.responseText}`, 'error');

            },
        });
    },

    PostAsync: async function fn(url = '', data = {}) {

        //var accounts = await DeFiHelper.Web3Instant.eth.getAccounts();
        //var address = accounts[0];

        return await fetch(url, {
            method: 'POST',
            dataType: 'json',
            mode: 'cors',
            headers: {
                'Content-Type': 'application/json',
                'ConnectedAddress': DeFiHelper.CurrentAddress,
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            body: JSON.stringify(data),
        })
            .then(response => {
                if (response.status === 401) {
                    be.error('DAPP Notification', 'Please, Disconnect and connect your wallet again!')
                }
                return response.json()
            })
    },

    GetAsync: async function fn(url = '') {
        let accounts = await DeFiHelper.Web3Instant.eth.getAccounts();

        let address = accounts[0];

        return await fetch(url, {
            method: 'GET',
            dataType: 'JSON',
            headers: {
                'Content-Type': 'application/json',
                'ConnectedAddress': address,
                "XSRF-TOKEN": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
        }).then(response => {
            if (response.status === 401) {
                be.error('DAPP Notification', 'Please, Disconnect and connect your wallet again!')
            }
            return response.json()
        })
    },

    DAppConnect: async function DAppConnect(address) {

        let result = await DeFiHelper.GetAsync(`/SavingDefi/DAppConnect?address=${address}&referral=${DeFiHelper.DappRefCode}`);

        if (result.Data === undefined) {
            return;
        }

        if (result.Data.Referral) {
            this.CurrentSponsorId = result.Data.Referral;
            this.CurrentAddress = address;
            DeFiHelper.SetReferralInfo(this.ReferralLink + this.CurrentSponsorId);
        } else {
            if (referralHidden && referralHidden.length > 0) {
                DeFiHelper.SetReferralInfo("Not connected wallet yet");
            }
        }
    },
    DAppDisconnect: async function fn() {
        await DeFiHelper.GetAsync(`/SavingDefi/DAppDisconnect`);
    },

    SetReferralInfo: function SetReferralInfo(refUrl) {
        if ($('#txtReferralLink') !== 'undefined') {
            $('#txtReferralLink').val(refUrl);
        }
    },

    CheckoutAssert: async function (checkoutAmount, rateId) {

        var isValidNet = await DeFiHelper.CheckNetWork();

        if (isValidNet) {
            await DeFiHelper.ConfirmProcessingTransaction(checkoutAmount, rateId);
        }
    },

    SaleToken: async function fn(checkoutAmount, typeId) {

        var isValidNet = await DeFiHelper.CheckNetWork();

        if (isValidNet) {
            await DeFiHelper.ConfirmSaleTokenTransaction(checkoutAmount, typeId);
        }
    }
}


if (typeof window.ethereum !== 'undefined') {
    console.log('MetaMask is installed!');
}