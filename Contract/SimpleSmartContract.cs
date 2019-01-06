using System;
using Stratis.SmartContracts;

public class SimpleSmartContract : SmartContract
{
    public SimpleSmartContract(ISmartContractState smartContractState)
    : base(smartContractState)
    {

    }

    public string SayHello(string nameToSayHelloTo)
    {
        return $"Hello {nameToSayHelloTo}";
    }
}
