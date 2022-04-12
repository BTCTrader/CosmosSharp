using System;
using System.Threading;
using System.Threading.Tasks;
using CosmosSharp.Api;
using CosmosSharp.StargateApiModel;
using CosmosSharp.Types;
using BlockInfo = CosmosSharp.StargateApiModel.BlockInfo;
using NewTransaction = CosmosSharp.StargateApiModel.NewTransaction;

namespace CosmosSharp
{
    public class StargateApi
    {
        private CosmosConfigurator _config { get; set; }
        private IHttpHandler _httpHandler { get; set; }

        public StargateApi(CosmosConfigurator config)
        {
            _config = config;
            _httpHandler = new HttpHandler();
        }

        public StargateApi(CosmosConfigurator config,
            IHttpHandler httpHandler)
        {
            _config = config;
            _httpHandler = httpHandler;
        }

        public async Task<AccountModel> GetAccount(string accountName, CancellationToken cancellationToken)
        {
            var url = $"{_config.HttpEndpoint}/cosmos/auth/v1beta1/accounts/{accountName}";
            return await _httpHandler.GetJsonAsync<AccountModel>(url, _config.HeaderKeyValues, cancellationToken);
        }

        /// <summary>
        /// Receives account balance via cosmos-sdk <b>before v0.44.4</b>. See more details about change in <a href="https://github.com/cosmos/cosmos-sdk/pull/10394#discussion_r734448254"><i>query account balance endpoint fix</i></a> and <a href="https://github.com/cosmos/cosmos-sdk/releases/tag/v0.44.4"><i>release notes</i></a>.
        /// </summary>
        /// <param name="accountName">address</param>
        /// <param name="denom">denom or IBC denom</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<BalanceResponse> GetAccountBalanceLegacy(string accountName, string denom, CancellationToken cancellationToken)
        {
            var url = $"{_config.HttpEndpoint}/cosmos/bank/v1beta1/balances/{accountName}/{denom}";
            return await _httpHandler.GetJsonAsync<BalanceResponse>(url, _config.HeaderKeyValues, cancellationToken);
        }
        
        /// <summary>
        /// Receives account balance via cosmos-sdk <b>as from v0.44.4</b>. See more details about <a href="https://github.com/cosmos/cosmos-sdk/pull/10394#discussion_r734448254"><i>query account balance endpoint fix</i></a> and <a href="https://github.com/cosmos/cosmos-sdk/releases/tag/v0.44.4"><i>release notes</i></a>.
        /// </summary>
        /// <param name="accountName">address</param>
        /// <param name="denom">denom or IBC denom</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<BalanceResponse> GetAccountBalance(string accountName, string denom, CancellationToken cancellationToken)
        {
            // grpc-gateway query account balance by IBC denom had an incorrect endpoint
            var url = $"{_config.HttpEndpoint}/cosmos/bank/v1beta1/balances/{accountName}/by_denom?denom={denom}";
            return await _httpHandler.GetJsonAsync<BalanceResponse>(url, _config.HeaderKeyValues, cancellationToken);
        }

        public async Task<BlockInfo> GetLatestBlock(CancellationToken cancellationToken)
        {
            var url = $"{_config.HttpEndpoint}/cosmos/base/tendermint/v1beta1/blocks/latest";
            var response = await _httpHandler.GetJsonAsync<BlockInfo>(url, _config.HeaderKeyValues, cancellationToken);
            return response;
        }

        public async Task<BlockDetail> GetBlock(ulong blockHeight, CancellationToken cancellationToken)
        {
            var url = $"{_config.HttpEndpoint}/cosmos/tx/v1beta1/txs?events=tx.height={blockHeight}";
            var response = await _httpHandler.GetJsonAsync<BlockDetail>(url, _config.HeaderKeyValues, cancellationToken);
            return response;
        }

        public async Task<NewTransaction> GetTx(string txHash, CancellationToken cancellationToken)
        {
            var url = $"{_config.HttpEndpoint}/cosmos/tx/v1beta1/txs/{txHash}";
            var response = await _httpHandler.GetJsonAsync<NewTransaction>(url, _config.HeaderKeyValues, cancellationToken);
            return response;
        }

        public Task<BroadcastTxResponse> BroadCastTx<TMsg>(StdTx<TMsg> signedTx, BroadcastMode mode, CancellationToken cancellationToken)
        {
            // TODO: Parameters and endpoint will be updated.
            // Details in "feature/cosmos-grpc-research" branch.
            throw new Exception("Not ready, yet.");
        }
    }
}