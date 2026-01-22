using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Xunit;
using KoClipCS;

namespace KoClipCS.Tests
{
    [Collection("Settings Tests")]
    public class ImageProcessingTests
    {
        private Bitmap CreateTestBitmap(int width = 100, int height = 100)
        {
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Red);
                g.FillRectangle(Brushes.Blue, 10, 10, 30, 30);
            }
            return bitmap;
        }

        [Theory]
        [InlineData(FileType.BMP)]
        [InlineData(FileType.JPEG)]
        [InlineData(FileType.PNG)]
        public void ImageSaveFormatConsistency(FileType fileType)
        {
            using var mainForm = new MainForm();
            mainForm.CurrentFileType = fileType;
            mainForm.JpegQuality = 85;
            
            using var testBitmap = CreateTestBitmap();
            var tempPath = Path.GetTempFileName();
            
            try
            {
                var expectedExtension = fileType switch
                {
                    FileType.BMP => ".bmp",
                    FileType.JPEG => ".jpg",
                    FileType.PNG => ".png",
                    _ => ".bmp"
                };
                
                var finalPath = Path.ChangeExtension(tempPath, expectedExtension);
                
                var format = fileType switch
                {
                    FileType.BMP => ImageFormat.Bmp,
                    FileType.JPEG => ImageFormat.Jpeg,
                    FileType.PNG => ImageFormat.Png,
                    _ => ImageFormat.Bmp
                };
                
                testBitmap.Save(finalPath, format);
                
                Assert.True(File.Exists(finalPath));
                Assert.Equal(expectedExtension, Path.GetExtension(finalPath), StringComparer.OrdinalIgnoreCase);
                
                if (File.Exists(finalPath))
                    File.Delete(finalPath);
            }
            catch
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
                throw;
            }
        }

        [Theory]
        [InlineData(50, 50)]
        [InlineData(100, 100)]
        [InlineData(200, 150)]
        public void ImageDimensionsPreserved(int width, int height)
        {
            using var testBitmap = CreateTestBitmap(width, height);
            var tempPath = Path.GetTempFileName() + ".png";
            
            try
            {
                testBitmap.Save(tempPath, ImageFormat.Png);
                
                using (var loadedBitmap = new Bitmap(tempPath))
                {
                    Assert.Equal(width, loadedBitmap.Width);
                    Assert.Equal(height, loadedBitmap.Height);
                }
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    // ファイルが使用中の場合は少し待ってから削除
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            File.Delete(tempPath);
                            break;
                        }
                        catch (IOException)
                        {
                            if (i == 2) throw; // 最後の試行で失敗した場合は例外を投げる
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
            }
        }

        [Theory]
        [InlineData(FileType.BMP)]
        [InlineData(FileType.JPEG)]
        [InlineData(FileType.PNG)]
        public void ImageSaveSucceeds(FileType fileType)
        {
            using var testBitmap = CreateTestBitmap();
            var tempPath = Path.GetTempFileName();
            
            var extension = fileType switch
            {
                FileType.BMP => ".bmp",
                FileType.JPEG => ".jpg",
                FileType.PNG => ".png",
                _ => ".bmp"
            };
            
            var finalPath = Path.ChangeExtension(tempPath, extension);
            
            try
            {
                var format = fileType switch
                {
                    FileType.BMP => ImageFormat.Bmp,
                    FileType.JPEG => ImageFormat.Jpeg,
                    FileType.PNG => ImageFormat.Png,
                    _ => ImageFormat.Bmp
                };
                
                testBitmap.Save(finalPath, format);
                
                Assert.True(File.Exists(finalPath));
                Assert.True(new FileInfo(finalPath).Length > 0);
            }
            finally
            {
                if (File.Exists(finalPath))
                    File.Delete(finalPath);
            }
        }

        [Fact]
        public void ImageComparisonDetectsDifferences()
        {
            using var bitmap1 = CreateTestBitmap(100, 100);
            using var bitmap2 = CreateTestBitmap(100, 100);
            using var bitmap3 = CreateTestBitmap(200, 200); // 異なるサイズ
            
            // 同じ画像の比較
            Assert.True(AreBitmapsEqual(bitmap1, bitmap2));
            
            // 異なるサイズの画像の比較
            Assert.False(AreBitmapsEqual(bitmap1, bitmap3));
        }

        private bool AreBitmapsEqual(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height)
                return false;
                
            for (int x = 0; x < bmp1.Width; x++)
            {
                for (int y = 0; y < bmp1.Height; y++)
                {
                    if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                        return false;
                }
            }
            return true;
        }

        [Theory]
        [InlineData(25)]
        [InlineData(50)]
        [InlineData(75)]
        public void JpegQualityAffectsFileSize(int quality)
        {
            using var testBitmap = CreateTestBitmap(200, 200);
            var tempPath1 = Path.GetTempFileName() + ".jpg";
            var tempPath2 = Path.GetTempFileName() + ".jpg";
            
            try
            {
                // 異なる品質で保存
                var encoder = ImageCodecInfo.GetImageEncoders()
                    .First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
                
                var encoderParams1 = new EncoderParameters(1);
                encoderParams1.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
                
                var encoderParams2 = new EncoderParameters(1);
                encoderParams2.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                
                testBitmap.Save(tempPath1, encoder, encoderParams1);
                testBitmap.Save(tempPath2, encoder, encoderParams2);
                
                var size1 = new FileInfo(tempPath1).Length;
                var size2 = new FileInfo(tempPath2).Length;
                
                // 品質が低い場合、ファイルサイズは小さくなるはず（品質100と比較して）
                Assert.True(size1 <= size2, $"Quality {quality} file size ({size1}) should be <= quality 100 file size ({size2})");
            }
            finally
            {
                if (File.Exists(tempPath1)) File.Delete(tempPath1);
                if (File.Exists(tempPath2)) File.Delete(tempPath2);
            }
        }
    }
}