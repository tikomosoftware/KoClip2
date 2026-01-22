using Xunit;

namespace KoClipCS.Tests
{
    [CollectionDefinition("Settings Tests")]
    public class SettingsTestCollection : ICollectionFixture<SettingsTestFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public class SettingsTestFixture
    {
        // Shared fixture for settings tests
    }
}