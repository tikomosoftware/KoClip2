using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Xunit;
using FsCheck;
using FsCheck.Xunit;
using KoClipCS;

namespace KoClipCS.Tests
{
    [Collection("Settings Tests")]
    public class SettingsTests
    {
        [Fact]
        public void FileTypeSettingRoundTrip_BMP()
        {
            using var form = new MainForm();
            form.CurrentFileType = FileType.BMP;
            Assert.Equal(FileType.BMP, form.CurrentFileType);
        }

        [Fact]
        public void FileTypeSettingRoundTrip_JPEG()
        {
            using var form = new MainForm();
            form.CurrentFileType = FileType.JPEG;
            form.JpegQuality = 85;
            Assert.Equal(FileType.JPEG, form.CurrentFileType);
            Assert.Equal(85, form.JpegQuality);
        }

        [Fact]
        public void FileTypeSettingRoundTrip_PNG()
        {
            using var form = new MainForm();
            form.CurrentFileType = FileType.PNG;
            Assert.Equal(FileType.PNG, form.CurrentFileType);
        }

        [Theory]
        [InlineData(SaveLocationMode.CurrentDirectory)]
        [InlineData(SaveLocationMode.MyPictures)]
        [InlineData(SaveLocationMode.Custom)]
        public void SaveLocationModeRoundTrip(SaveLocationMode saveMode)
        {
            using var form = new MainForm();
            form.SaveLocationMode = saveMode;
            if (saveMode == SaveLocationMode.Custom)
            {
                form.CustomSaveDirectory = @"C:\TestPath";
            }
            Assert.Equal(saveMode, form.SaveLocationMode);
        }

        [Theory]
        [InlineData(SoundMode.Beep)]
        [InlineData(SoundMode.DefaultWav)]
        [InlineData(SoundMode.NoSound)]
        public void SoundModeRoundTrip(SoundMode soundMode)
        {
            using var form = new MainForm();
            form.SoundMode = soundMode;
            Assert.Equal(soundMode, form.SoundMode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FileNamingConsistency(bool useTimestamp)
        {
            using var form = new MainForm();
            form.UseTimestampNaming = useTimestamp;
            Assert.Equal(useTimestamp, form.UseTimestampNaming);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(85)]
        [InlineData(100)]
        public void JpegQualityBounds(int quality)
        {
            using var form = new MainForm();
            form.JpegQuality = quality;
            Assert.InRange(form.JpegQuality, 1, 100);
            Assert.Equal(quality, form.JpegQuality);
        }

        [Fact]
        public void SettingsFormOpensWithoutError()
        {
            using var mainForm = new MainForm();
            using var settingsForm = new SettingsForm(mainForm);
            
            Assert.NotNull(settingsForm);
            Assert.Equal("設定", settingsForm.Text);
        }

        [Theory]
        [InlineData(FileType.BMP)]
        [InlineData(FileType.JPEG)]
        [InlineData(FileType.PNG)]
        public void FileTypeSettingsPersist(FileType fileType)
        {
            using var mainForm = new MainForm();
            using var settingsForm = new SettingsForm(mainForm);
            
            // 設定を変更
            mainForm.CurrentFileType = fileType;
            
            // 設定フォームを開いて確認
            using var newSettingsForm = new SettingsForm(mainForm);
            
            Assert.Equal(fileType, mainForm.CurrentFileType);
        }

        [Theory]
        [InlineData(SaveLocationMode.CurrentDirectory)]
        [InlineData(SaveLocationMode.MyPictures)]
        [InlineData(SaveLocationMode.Custom)]
        public void SaveLocationSettingsPersist(SaveLocationMode saveMode)
        {
            using var mainForm = new MainForm();
            
            mainForm.SaveLocationMode = saveMode;
            if (saveMode == SaveLocationMode.Custom)
            {
                mainForm.CustomSaveDirectory = @"C:\TestPath";
            }
            
            using var settingsForm = new SettingsForm(mainForm);
            
            Assert.Equal(saveMode, mainForm.SaveLocationMode);
        }

        [Theory]
        [InlineData(SoundMode.Beep)]
        [InlineData(SoundMode.DefaultWav)]
        [InlineData(SoundMode.NoSound)]
        public void SoundModeSettingsPersist(SoundMode soundMode)
        {
            using var mainForm = new MainForm();
            
            mainForm.SoundMode = soundMode;
            
            using var settingsForm = new SettingsForm(mainForm);
            
            Assert.Equal(soundMode, mainForm.SoundMode);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(85)]
        [InlineData(100)]
        public void JpegQualitySettingsPersist(int quality)
        {
            using var mainForm = new MainForm();
            
            mainForm.JpegQuality = quality;
            
            using var settingsForm = new SettingsForm(mainForm);
            
            Assert.Equal(quality, mainForm.JpegQuality);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TimestampNamingSettingsPersist(bool useTimestamp)
        {
            using var mainForm = new MainForm();
            
            mainForm.UseTimestampNaming = useTimestamp;
            
            using var settingsForm = new SettingsForm(mainForm);
            
            Assert.Equal(useTimestamp, mainForm.UseTimestampNaming);
        }
    }
}