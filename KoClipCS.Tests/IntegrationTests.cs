using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Xunit;
using KoClipCS;

namespace KoClipCS.Tests
{
    [Collection("Settings Tests")]
    public class IntegrationTests : IDisposable
    {
        private readonly string testSettingsPath;
        
        public IntegrationTests()
        {
            // Create unique test settings file
            testSettingsPath = Path.Combine(Path.GetTempPath(), $"KoClipIntegrationTest_{Guid.NewGuid()}.json");
            
            // Set custom settings path for testing
            SettingsManager.SetCustomSettingsPath(testSettingsPath);
        }
        
        public void Dispose()
        {
            // Reset custom settings path
            SettingsManager.SetCustomSettingsPath(null);
            
            // Clean up test file
            if (File.Exists(testSettingsPath))
            {
                File.Delete(testSettingsPath);
            }
        }
        [Theory]
        [InlineData(FileType.BMP, SaveLocationMode.MyPictures, SoundMode.Beep, 85, true)]
        [InlineData(FileType.JPEG, SaveLocationMode.CurrentDirectory, SoundMode.DefaultWav, 100, false)]
        [InlineData(FileType.PNG, SaveLocationMode.MyPictures, SoundMode.NoSound, 50, true)]
        public void SpecificSettingsCombinationsWork(FileType fileType, SaveLocationMode saveMode, 
            SoundMode soundMode, int jpegQuality, bool useTimestamp)
        {
            using var mainForm = new MainForm();
            
            // 設定を適用
            mainForm.CurrentFileType = fileType;
            mainForm.SaveLocationMode = saveMode;
            mainForm.SoundMode = soundMode;
            mainForm.JpegQuality = jpegQuality;
            mainForm.UseTimestampNaming = useTimestamp;
            
            // 設定フォームを開いて設定が反映されているか確認
            using var settingsForm = new SettingsForm(mainForm);
            
            Assert.Equal(fileType, mainForm.CurrentFileType);
            Assert.Equal(saveMode, mainForm.SaveLocationMode);
            Assert.Equal(soundMode, mainForm.SoundMode);
            Assert.Equal(jpegQuality, mainForm.JpegQuality);
            Assert.Equal(useTimestamp, mainForm.UseTimestampNaming);
        }

        [Fact]
        public void ApplicationStartsWithDefaultSettings()
        {
            // テスト用の一意な設定ファイルパスを設定
            var testSettingsPath = Path.Combine(Path.GetTempPath(), $"KoClipDefaultTest_{Guid.NewGuid()}.json");
            SettingsManager.SetCustomSettingsPath(testSettingsPath);
            
            try
            {
                using var mainForm = new MainForm();
                
                // デフォルト設定の確認
                Assert.Equal(FileType.PNG, mainForm.CurrentFileType); // デフォルトはPNG
                Assert.Equal(SaveLocationMode.MyPictures, mainForm.SaveLocationMode); // デフォルトはマイピクチャ
                Assert.Equal(SoundMode.DefaultWav, mainForm.SoundMode); // デフォルトはデフォルトWAV
                Assert.Equal(100, mainForm.JpegQuality); // デフォルト品質は100
                Assert.True(mainForm.UseTimestampNaming); // デフォルトはタイムスタンプ
            }
            finally
            {
                // テスト後のクリーンアップ
                SettingsManager.SetCustomSettingsPath(null);
                if (File.Exists(testSettingsPath))
                {
                    File.Delete(testSettingsPath);
                }
            }
        }

        [Fact]
        public void SettingsFormCanBeOpenedMultipleTimes()
        {
            using var mainForm = new MainForm();
            
            // 複数回設定フォームを開く
            for (int i = 0; i < 5; i++)
            {
                using var settingsForm = new SettingsForm(mainForm);
                Assert.NotNull(settingsForm);
                Assert.Equal("設定", settingsForm.Text);
            }
        }

        [Theory]
        [InlineData(FileType.BMP, SaveLocationMode.CurrentDirectory, SoundMode.Beep, 85, true)]
        [InlineData(FileType.JPEG, SaveLocationMode.MyPictures, SoundMode.DefaultWav, 100, false)]
        [InlineData(FileType.PNG, SaveLocationMode.Custom, SoundMode.NoSound, 50, true)]
        public void SettingsFormHandlesValidCombinations(FileType fileType, SaveLocationMode saveMode, 
            SoundMode soundMode, int jpegQuality, bool useTimestamp)
        {
            try
            {
                using var mainForm = new MainForm();
                
                mainForm.CurrentFileType = fileType;
                mainForm.SaveLocationMode = saveMode;
                mainForm.SoundMode = soundMode;
                mainForm.JpegQuality = jpegQuality;
                mainForm.UseTimestampNaming = useTimestamp;
                
                if (saveMode == SaveLocationMode.Custom)
                {
                    mainForm.CustomSaveDirectory = @"C:\TestPath";
                }
                
                using var settingsForm = new SettingsForm(mainForm);
                
                // フォームが正常に作成され、設定が保持されているか確認
                Assert.NotNull(settingsForm);
                Assert.Equal(fileType, mainForm.CurrentFileType);
                Assert.Equal(saveMode, mainForm.SaveLocationMode);
                Assert.Equal(soundMode, mainForm.SoundMode);
                Assert.Equal(jpegQuality, mainForm.JpegQuality);
                Assert.Equal(useTimestamp, mainForm.UseTimestampNaming);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Settings combination failed: {ex.Message}");
            }
        }

        [Fact]
        public void MainFormDisposesCleanly()
        {
            MainForm? mainForm = null;
            
            try
            {
                mainForm = new MainForm();
                Assert.NotNull(mainForm);
            }
            finally
            {
                mainForm?.Dispose();
            }
            
            // 例外が発生しないことを確認
            Assert.True(true);
        }

        [Fact]
        public void SettingsFormDisposesCleanly()
        {
            using var mainForm = new MainForm();
            SettingsForm? settingsForm = null;
            
            try
            {
                settingsForm = new SettingsForm(mainForm);
                Assert.NotNull(settingsForm);
            }
            finally
            {
                settingsForm?.Dispose();
            }
            
            // 例外が発生しないことを確認
            Assert.True(true);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(100)]
        public void JpegQualityBoundaryValues(int quality)
        {
            using var mainForm = new MainForm();
            
            mainForm.JpegQuality = quality;
            
            using var settingsForm = new SettingsForm(mainForm);
            
            Assert.Equal(quality, mainForm.JpegQuality);
            Assert.InRange(mainForm.JpegQuality, 1, 100);
        }

        [Fact]
        public void AllFileTypesCanBeSet()
        {
            using var mainForm = new MainForm();
            
            var fileTypes = new[] { FileType.BMP, FileType.JPEG, FileType.PNG };
            
            foreach (var fileType in fileTypes)
            {
                mainForm.CurrentFileType = fileType;
                Assert.Equal(fileType, mainForm.CurrentFileType);
            }
        }

        [Fact]
        public void AllSaveLocationModesCanBeSet()
        {
            using var mainForm = new MainForm();
            
            var saveModes = new[] { SaveLocationMode.CurrentDirectory, SaveLocationMode.MyPictures, SaveLocationMode.Custom };
            
            foreach (var saveMode in saveModes)
            {
                mainForm.SaveLocationMode = saveMode;
                Assert.Equal(saveMode, mainForm.SaveLocationMode);
            }
        }

        [Fact]
        public void AllSoundModesCanBeSet()
        {
            using var mainForm = new MainForm();
            
            var soundModes = new[] { SoundMode.Beep, SoundMode.DefaultWav, SoundMode.NoSound };
            
            foreach (var soundMode in soundModes)
            {
                mainForm.SoundMode = soundMode;
                Assert.Equal(soundMode, mainForm.SoundMode);
            }
        }
    }
}