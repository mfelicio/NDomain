using NUnit.Framework;

namespace NDomain.Azure.Tests
{
    [SetUpFixture]
    public class StartStopAzureEmulator
    {
        private bool _wasUp;

        [OneTimeSetUp]
        public void StartAzureBeforeAllTestsIfNotUp()
        {
            if (!AzureStorageEmulatorManager.IsProcessStarted())
            {
                AzureStorageEmulatorManager.StartStorageEmulator();
                _wasUp = false;
            }
            else
            {
                _wasUp = true;
            }

        }

        [OneTimeTearDown]
        public void StopAzureAfterAllTestsIfWasDown()
        {
            if (!_wasUp)
            {
                AzureStorageEmulatorManager.StopStorageEmulator();
            }
            else
            {
                // Leave as it was before testing...
            }
        }
    }
}
