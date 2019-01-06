using Microsoft.Extensions.Logging;

namespace StratisContractTester
{
    internal class LoggerFactoryFake : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {

        }

        public ILogger CreateLogger(string categoryName)
        {
            return new LoggerFake();
        }

        public void Dispose()
        {

        }
    }
}
