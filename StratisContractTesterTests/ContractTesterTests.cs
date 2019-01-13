using Microsoft.VisualStudio.TestTools.UnitTesting;
using StratisContractTester;
using System.Linq;

namespace StratisContractTesterTests
{
    [TestClass]
    public class ContractTesterTests
    {
        private static string ContractFile => @"..\..\..\..\Contract\SimpleSmartContract.cs".Replace(@"\", System.IO.Path.DirectorySeparatorChar.ToString());

        private ContractTester contractTester;

        [TestInitialize]
        public void Initialize()
        {
            contractTester = new ContractTester();
        }

        [TestMethod]
        public void Compiles()
        {
            var compilationResult = contractTester.Compile(ContractFile);
            Assert.IsTrue(compilationResult.Success);
        }

        [TestMethod]
        public void ValidatesSmartContract()
        {
            var compilationResult = contractTester.Compile(ContractFile);
            if (!compilationResult.Success)
            {
                Assert.Inconclusive("Failed to compile smart contract. Probably a bug earlier in the flow.");
                return;
            }
            Assert.IsFalse(contractTester.Validate(compilationResult).Any());
        }

        [TestMethod]
        public void PublishesSmartContract()
        {
            var compilationResult = contractTester.Compile(ContractFile);
            if (!compilationResult.Success)
            {
                Assert.Inconclusive("Failed to compile smart contract. Probably a bug earlier in the flow.");
                return;
            }
            if (contractTester.Validate(compilationResult).Any())
            {
                Assert.Inconclusive("Failed to validate smart contract.");
                return;
            }
            var contractAddress = contractTester.PublishContract(compilationResult);
            Assert.IsNotNull(contractAddress);
        }

        [TestMethod]
        public void ExecutesMethod()
        {
            var compilationResult = contractTester.Compile(ContractFile);
            if (!compilationResult.Success)
            {
                Assert.Inconclusive("Failed to compile smart contract. Probably a bug earlier in the flow.");
                return;
            }
            if (contractTester.Validate(compilationResult).Any())
            {
                Assert.Inconclusive("Failed to validate smart contract.");
                return;
            }
            var contractAddress = contractTester.PublishContract(compilationResult);
            if(contractAddress == null)
            {
                Assert.Inconclusive("Failed to publish smart contract. Probably a bug earlier in the flow.");
                return;
            }
            var result = contractTester.ExecuteMethod(contractAddress, "SayHello", new[] { "Garry" });
            Assert.AreEqual("Hello Garry", result.Return);
        }
    }
}
