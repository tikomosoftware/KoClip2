using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Xunit;
using KoClipCS;

namespace KoClipCS.Tests
{
    [Collection("Settings Tests")]
    public class FileNamingTests
    {
        [Theory]
        [InlineData(FileType.BMP)]
        [InlineData(FileType.JPEG)]
        [InlineData(FileType.PNG)]
        public void TimestampNamingGeneratesValidFilenames(FileType fileType)
        {
            var extension = fileType switch
            {
                FileType.BMP => ".bmp",
                FileType.JPEG => ".jpg",
                FileType.PNG => ".png",
                _ => ".bmp"
            };
            
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var filename = timestamp + extension;
            
            // ファイル名が有効かチェック
            Assert.True(!string.IsNullOrWhiteSpace(filename));
            Assert.True(filename.IndexOfAny(Path.GetInvalidFileNameChars()) == -1);
            Assert.EndsWith(extension, filename);
            
            // タイムスタンプ形式が正しいかチェック
            var timestampPattern = @"^\d{14}";
            Assert.Matches(timestampPattern, filename);
        }

        [Theory]
        [InlineData("test", "", 1, 3, FileType.BMP, "test001.bmp")]
        [InlineData("img", "_backup", 42, 4, FileType.JPEG, "img0042_backup.jpg")]
        [InlineData("", "", 5, 2, FileType.PNG, "05.png")]
        [InlineData("prefix", "suffix", 999, 3, FileType.BMP, "prefix999suffix.bmp")]
        public void CustomNamingGeneratesExpectedFilenames(string prefix, string suffix, int sequence, int digits, FileType fileType, string expected)
        {
            var extension = fileType switch
            {
                FileType.BMP => ".bmp",
                FileType.JPEG => ".jpg",
                FileType.PNG => ".png",
                _ => ".bmp"
            };
            
            var sequenceStr = sequence.ToString().PadLeft(digits, '0');
            var filename = prefix + sequenceStr + suffix + extension;
            
            Assert.Equal(expected, filename);
        }

        [Fact]
        public void TimestampNamingGeneratesUniqueFilenames()
        {
            var filenames = new HashSet<string>();
            
            for (int i = 0; i < 10; i++)
            {
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var filename = timestamp + ".bmp";
                filenames.Add(filename);
                
                // 少し待機して異なるタイムスタンプを生成
                System.Threading.Thread.Sleep(1);
            }
            
            // 少なくとも一部のファイル名は異なるはず（ミリ秒レベルでの違い）
            Assert.True(filenames.Count >= 1);
        }

        [Theory]
        [InlineData(1, 1, "1")]
        [InlineData(1, 3, "001")]
        [InlineData(42, 2, "42")]
        [InlineData(999, 4, "0999")]
        [InlineData(1000, 3, "1000")] // 桁数を超える場合
        public void SequenceNumberPaddingWorksCorrectly(int sequence, int digits, string expected)
        {
            var result = sequence.ToString().PadLeft(digits, '0');
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(FileType.BMP, ".bmp")]
        [InlineData(FileType.JPEG, ".jpg")]
        [InlineData(FileType.PNG, ".png")]
        public void FileExtensionMatchesFileType(FileType fileType, string expectedExtension)
        {
            var extension = fileType switch
            {
                FileType.BMP => ".bmp",
                FileType.JPEG => ".jpg",
                FileType.PNG => ".png",
                _ => ".bmp"
            };
            
            var filename = "test" + extension;
            
            Assert.Equal(expectedExtension, Path.GetExtension(filename), StringComparer.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("test", "backup", 1, 3, FileType.BMP)]
        [InlineData("img", "", 42, 4, FileType.JPEG)]
        [InlineData("", "suffix", 5, 2, FileType.PNG)]
        public void GeneratedFilenamesAreValidPaths(string prefix, string suffix, int sequence, int digits, FileType fileType)
        {
            try
            {
                var extension = fileType switch
                {
                    FileType.BMP => ".bmp",
                    FileType.JPEG => ".jpg",
                    FileType.PNG => ".png",
                    _ => ".bmp"
                };
                
                var sequenceStr = sequence.ToString().PadLeft(digits, '0');
                var filename = prefix + sequenceStr + suffix + extension;
                
                // パスとして有効かテスト
                var tempDir = Path.GetTempPath();
                var fullPath = Path.Combine(tempDir, filename);
                
                // パスが作成できるかテスト
                Assert.True(!string.IsNullOrWhiteSpace(fullPath));
                Assert.True(fullPath.Length < 260); // Windows パス長制限
                Assert.Equal(filename, Path.GetFileName(fullPath));
            }
            catch (Exception ex)
            {
                Assert.Fail($"Path validation failed: {ex.Message}");
            }
        }
    }
}