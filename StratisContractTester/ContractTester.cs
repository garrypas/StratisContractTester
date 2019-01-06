using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Features.SmartContracts.Networks;
using Stratis.Patricia;
using Stratis.SmartContracts;
using Stratis.SmartContracts.CLR;
using Stratis.SmartContracts.CLR.Compilation;
using Stratis.SmartContracts.CLR.Loader;
using Stratis.SmartContracts.CLR.ResultProcessors;
using Stratis.SmartContracts.CLR.Serialization;
using Stratis.SmartContracts.CLR.Validation;
using Stratis.SmartContracts.Core;
using Stratis.SmartContracts.Core.State;

namespace StratisContractTester
{
    public partial class ContractTester
    {
        private const ulong BlockHeight = 0;
        private uint160 CoinbaseAddress = 0;
        private Money MempoolFee = new Money(1_000_000);

        private static readonly uint160 SenderAddress = 2;

        private readonly IKeyEncodingStrategy keyEncodingStrategy;
        private readonly ILoggerFactory loggerFactory;
        private readonly Network network;
        private readonly IContractRefundProcessor refundProcessor;
        private readonly IStateRepositoryRoot state;
        private readonly IContractTransferProcessor transferProcessor;
        private readonly SmartContractValidator validator;
        private IInternalExecutorFactory internalTxExecutorFactory;
        private IVirtualMachine vm;
        private readonly ICallDataSerializer callDataSerializer;
        private readonly StateFactory stateFactory;
        private readonly IAddressGenerator addressGenerator;
        private readonly ILoader assemblyLoader;
        private readonly IContractModuleDefinitionReader moduleDefinitionReader;
        private readonly IContractPrimitiveSerializer contractPrimitiveSerializer;
        private readonly IStateProcessor stateProcessor;
        private readonly ISmartContractStateFactory smartContractStateFactory;
        private readonly ISerializer serializer;
        private ContractExecutor contractExecutor;

        public ContractTester()
        {
            keyEncodingStrategy = BasicKeyEncodingStrategy.Default;
            loggerFactory = new LoggerFactoryFake();
            network = new SmartContractsRegTest();
            refundProcessor = new ContractRefundProcessor(loggerFactory);
            state = new StateRepositoryRoot(new NoDeleteSource<byte[], byte[]>(new MemoryDictionarySource()));
            transferProcessor = new ContractTransferProcessor(loggerFactory, network);
            validator = new SmartContractValidator();
            addressGenerator = new AddressGenerator();
            assemblyLoader = new ContractAssemblyLoader();
            moduleDefinitionReader = new ContractModuleDefinitionReader();
            contractPrimitiveSerializer = new ContractPrimitiveSerializer(network);
            serializer = new Serializer(contractPrimitiveSerializer);
            vm = new ReflectionVirtualMachine(validator, loggerFactory, assemblyLoader, moduleDefinitionReader);
            stateProcessor = new StateProcessor(vm, addressGenerator);
            internalTxExecutorFactory = new InternalExecutorFactory(loggerFactory, stateProcessor);
            smartContractStateFactory = new SmartContractStateFactory(contractPrimitiveSerializer, internalTxExecutorFactory, serializer);

            callDataSerializer = new CallDataSerializer(contractPrimitiveSerializer);

            stateFactory = new StateFactory(smartContractStateFactory);

            contractExecutor = new ContractExecutor(loggerFactory,
                callDataSerializer,
                state,
                refundProcessor,
                transferProcessor,
                stateFactory,
                stateProcessor,
                contractPrimitiveSerializer);
        }

        public ContractCompilationResult Compile(string fileName)
        {
            return ContractCompiler.CompileFile(fileName);
        }

        public string PublishContract(ContractCompilationResult compiledContract)
        {
            var transactionValue = (Money)100;

            byte[] contractExecutionCode = compiledContract.Compilation;

            var contractTxData = new ContractTxData(1, (Gas)1, (Gas)500_000, contractExecutionCode);

            var transaction = new Transaction();
            TxOut txOut = transaction.AddOutput(0, new Script(callDataSerializer.Serialize(contractTxData)));
            txOut.Value = transactionValue;
            var transactionContext = new ContractTransactionContext(BlockHeight, CoinbaseAddress, MempoolFee, SenderAddress, transaction);

            IContractExecutionResult result = contractExecutor.Execute(transactionContext);
            return result.NewContractAddress?.ToString();
        }

        public IContractExecutionResult ExecuteMethod(string contractAddressStr, string methodName, object[] parameters)
        {
            if (!uint160.TryParse(contractAddressStr, out uint160 contractAddress))
            {
                throw new System.ArgumentException($"Could not part the contract address '{contractAddressStr}'");
            }
            var contractTxData = new ContractTxData(1, (Gas)1, (Gas)500_000, contractAddress, methodName, parameters);

            var transaction = new Transaction();
            var txOut = transaction.AddOutput(0, new Script(callDataSerializer.Serialize(contractTxData)));
            var transactionContext = new ContractTransactionContext(BlockHeight, CoinbaseAddress, MempoolFee, SenderAddress, transaction);

            return contractExecutor.Execute(transactionContext);
        }
    }
}
