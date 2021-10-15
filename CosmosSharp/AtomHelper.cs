using System;
using System.Collections.Generic;
using CosmosSharp.Models;
using CosmosSharp.Api;
using CosmosSharp.StargateApiModel;
using CosmosSharp.Types;

namespace CosmosSharp
{
    public static class DenomTypes
    {
        public static string TestnetCoin = "uphoton";
        public static string MainnetCoin = "uatom";
    }
    
    public enum Network 
    {
        Mainnet = 0,
        Testnet = 1
    }

    public class AtomHelper
    {
        public static StdTx<MsgSend> CreateTx(string fromAddress, string toAddress, string memo, long amountUAtom, ushort feeUAtom, Network network = Network.Mainnet)
        {   
            string denom = 
                network == Network.Mainnet ? DenomTypes.MainnetCoin : 
                network == Network.Testnet ? DenomTypes.TestnetCoin : 
                throw new NotSupportedException($"Network type is not supported.");

            StdTx<MsgSend> newTx = new StdTx<MsgSend>(){
                Msg = new List<Msg<MsgSend>>(){
                    new Msg<MsgSend>(){
                        Type = "cosmos-sdk/MsgSend",
                        Value = new MsgSend(){
                            FromAddress = fromAddress ?? throw new ArgumentNullException(nameof(fromAddress)),
                            ToAddress = toAddress ?? throw new ArgumentNullException(nameof(toAddress)),
                            Amount = new List<Coin>(){
                                new Coin(){
                                    Amount = amountUAtom.ToString(),
                                    Denom = denom,
                                }
                            }
                        },
                    }
                },
                Fee = new StdFee(){
                    Gas = "200000"
                },
                Memo = memo
            };

            if(feeUAtom > 0) newTx.Fee.Amount.Add(new Coin(){ Amount = feeUAtom.ToString(), Denom = denom });

            return newTx;
        }

        public static StdTx<MsgSend> SignTx(StdTx<MsgSend> atomTransfer, AccountModel account, Wallet wallet, string chainId)
        {
            SignMeta meta = new SignMeta(){ AccountNumber = account.Account.AccountNumber, Sequence = account.Account.Sequence, ChainId = chainId };
            StdSignMsg<MsgSend> signMsg = new StdSignMsg<MsgSend>(atomTransfer, meta);
            StdTx<MsgSend> signed = wallet.Sign<MsgSend>(signMsg);
            return signed;
        }
    }
}