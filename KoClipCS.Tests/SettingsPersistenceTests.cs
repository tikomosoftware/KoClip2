using System;
using System.IO;
using Xunit;
using KoClipCS;

namespace KoClipCS.Tests
{
    [Collection("Settings Tests")]
    public class SettingsPersistenceTests : IDisposable
    {
        private readonly string testIniPath;
        private readonly string testSettingsPath;
        private readonly MainForm mainForm;

        public SettingsPersistenceTests()
        {
            // Create a temporary INI file for testing
            testIniPath = Path.Combine(Path.GetTempPath(), "KoClipTest.ini");
            testSettingsPath = Path.Combine(Path.GetTempPath(), $"KoClipTest_{Guid.NewGuid()}.json");
            
            // Clean up any existing test file
            if (File.Exists(testIniPath))
            {
                File.Delete(testIniPath);
            }
            
            if (File.Exists(testSettingsPath))
            {
                File.Delete(testSettingsPath);
            }
            
            // Set custom settings path for testing
            SettingsManager.SetCustomSettingsPath(testSettingsPath);
            
            mainForm = new MainForm();
        }

        public void Dispose()
        {
            // Reset custom settings path
            SettingsManager.SetCustomSettingsPath(null);
            
            // Clean up test files
            if (File.Exists(testIniPath))
            {
                File.Delete(testIniPath);
            }
            
            if (File.Exists(testSettingsPath))
            {
                File.Delete(testSettingsPath);
            }
            
            mainForm?.Dispose();
        }

        [Fact]
        public void CustomFilenameSettings_ShouldBeSavedAndLoaded()
        {
            // Arrange
            mainForm.UseTimestampNaming = false;
            mainForm.FilenamePrefix = "test_";
            mainForm.FilenameSuffix = "_image";
            mainForm.FilenameSequenceNumber = 5;
            mainForm.FilenameDigits = 4;

            // Act - Save settings
            SettingsManager.SaveSettings(mainForm);

            // Create a new MainForm to test loading (it will automatically load settings in constructor)
            var newMainForm = new MainForm();

            // Assert
            Assert.False(newMainForm.UseTimestampNaming);
            Assert.Equal("test_", newMainForm.FilenamePrefix);
            Assert.Equal("_image", newMainForm.FilenameSuffix);
            Assert.Equal(5, newMainForm.FilenameSequenceNumber);
            Assert.Equal(4, newMainForm.FilenameDigits);

            newMainForm.Dispose();
        }

        [Fact]
        public void GenerateFileName_WithCustomSettings_ShouldUseCorrectFormat()
        {
            // Arrange
            mainForm.UseTimestampNaming = false;
            mainForm.FilenamePrefix = "img_";
            mainForm.FilenameSuffix = "_test";
            mainForm.FilenameSequenceNumber = 1;
            mainForm.FilenameDigits = 3;
            mainForm.CurrentFileType = FileType.PNG;

            // Act
            var fileName = mainForm.GenerateFileName();

            // Assert
            Assert.Equal("img_001_test.png", fileName);
            Assert.Equal(2, mainForm.FilenameSequenceNumber); // Should increment after generation
        }

        [Fact]
        public void GenerateFileName_WithTimestamp_ShouldUseTimestampFormat()
        {
            // Arrange
            mainForm.UseTimestampNaming = true;
            mainForm.CurrentFileType = FileType.JPEG;

            // Act
            var fileName = mainForm.GenerateFileName();

            // Assert
            Assert.EndsWith(".jpg", fileName);
            Assert.Equal(14 + 4, fileName.Length); // YYYYMMDDHHMMSS + .jpg = 18 characters
        }

        [Fact]
        public void AllSettings_ShouldBeSavedAndLoaded()
        {
            // Arrange
            mainForm.SaveLocationMode = SaveLocationMode.Custom;
            mainForm.CustomSaveDirectory = @"C:\TestFolder";
            mainForm.CurrentFileType = FileType.JPEG;
            mainForm.JpegQuality = 85;
            mainForm.UseTimestampNaming = false;
            mainForm.FilenamePrefix = "prefix_";
            mainForm.FilenameSuffix = "_suffix";
            mainForm.FilenameSequenceNumber = 10;
            mainForm.FilenameDigits = 5;
            mainForm.SoundMode = SoundMode.Beep;

            // Act - Save settings
            SettingsManager.SaveSettings(mainForm);
            
            // Verify settings file was created
            Assert.True(File.Exists(testSettingsPath), "Settings file should exist after saving");
            
            // Debug: Check file contents
            var fileContent = File.ReadAllText(testSettingsPath);
            System.Diagnostics.Debug.WriteLine($"Settings file content: {fileContent}");
            
            // Create a new MainForm to test loading (it will automatically load settings in constructor)
            var newMainForm = new MainForm();
            
            // Debug: Check what was actually loaded
            System.Diagnostics.Debug.WriteLine($"Loaded SaveLocationMode: {newMainForm.SaveLocationMode}");
            System.Diagnostics.Debug.WriteLine($"Loaded CustomSaveDirectory: {newMainForm.CustomSaveDirectory}");

            // Assert
            Assert.Equal(SaveLocationMode.Custom, newMainForm.SaveLocationMode);
            Assert.Equal(@"C:\TestFolder", newMainForm.CustomSaveDirectory);
            Assert.Equal(FileType.JPEG, newMainForm.CurrentFileType);
            Assert.Equal(85, newMainForm.JpegQuality);
            Assert.False(newMainForm.UseTimestampNaming);
            Assert.Equal("prefix_", newMainForm.FilenamePrefix);
            Assert.Equal("_suffix", newMainForm.FilenameSuffix);
            Assert.Equal(10, newMainForm.FilenameSequenceNumber);
            Assert.Equal(5, newMainForm.FilenameDigits);
            Assert.Equal(SoundMode.Beep, newMainForm.SoundMode);

            newMainForm.Dispose();
        }
    }
}