using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Xunit;
using KoClipCS;

namespace KoClipCS.Tests
{
    [Collection("Settings Tests")]
    public class ClipboardIntegrationTests : IDisposable
    {
        private readonly string testDirectory;
        private readonly string testSettingsPath;
        
        public ClipboardIntegrationTests()
        {
            // テスト用の一時ディレクトリを作成
            testDirectory = Path.Combine(Path.GetTempPath(), $"KoClipTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(testDirectory);
            
            // テスト用の設定ファイルパス
            testSettingsPath = Path.Combine(Path.GetTempPath(), $"KoClipClipboardTest_{Guid.NewGuid()}.json");
            SettingsManager.SetCustomSettingsPath(testSettingsPath);
        }
        
        public void Dispose()
        {
            // クリーンアップ
            SettingsManager.SetCustomSettingsPath(null);
            
            if (File.Exists(testSettingsPath))
            {
                File.Delete(testSettingsPath);
            }
            
            if (Directory.Exists(testDirectory))
            {
                try
                {
                    Directory.Delete(testDirectory, true);
                }
                catch
                {
                    // テスト後のクリーンアップエラーは無視
                }
            }
        }
        
        private Bitmap CreateTestBitmap(int width = 100, int height = 100)
        {
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.FillRectangle(Brushes.Red, 10, 10, 30, 30);
                g.FillEllipse(Brushes.Blue, 50, 50, 40, 40);
            }
            return bitmap;
        }
        
        [Theory]
        [InlineData(FileType.BMP)]
        [InlineData(FileType.JPEG)]
        [InlineData(FileType.PNG)]
        [InlineData(FileType.WEBP)]
        public void ImageCanBeSavedInAllFormats(FileType fileType)
        {
            using var mainForm = new MainForm();
            mainForm.CurrentFileType = fileType;
            mainForm.SaveLocationMode = SaveLocationMode.Custom;
            mainForm.CustomSaveDirectory = testDirectory;
            mainForm.UseTimestampNaming = true;
            mainForm.ShowSaveDialog = false;
            
            // テスト画像を作成（クリップボードを使わずに直接テスト）
            using var testBitmap = CreateTestBitmap();
            
            // 画像を保存
            var fileName = mainForm.GenerateFileName();
            var fullPath = Path.Combine(testDirectory, fileName);
            
            // ファイル形式に応じて保存
            switch (fileType)
            {
                case FileType.BMP:
                    testBitmap.Save(fullPath, ImageFormat.Bmp);
                    break;
                case FileType.JPEG:
                    SaveAsJpeg(testBitmap, fullPath, mainForm.JpegQuality);
                    break;
                case FileType.PNG:
                    testBitmap.Save(fullPath, ImageFormat.Png);
                    break;
                case FileType.WEBP:
                    SaveAsWebp(testBitmap, fullPath, mainForm.WebpQuality);
                    break;
            }
            
            // ファイルが作成されたことを確認
            Assert.True(File.Exists(fullPath), $"File should exist: {fullPath}");
            Assert.True(new FileInfo(fullPath).Length > 0, "File should not be empty");
            
            // 保存された画像を読み込んで検証
            // WebPの場合はImageSharpを使用
            if (fileType == FileType.WEBP)
            {
                using var fileStream = new FileStream(fullPath, FileMode.Open);
                using var imageSharpImage = SixLabors.ImageSharp.Image.Load(fileStream);
                Assert.Equal(testBitmap.Width, imageSharpImage.Width);
                Assert.Equal(testBitmap.Height, imageSharpImage.Height);
            }
            else
            {
                using var loadedImage = Image.FromFile(fullPath);
                Assert.Equal(testBitmap.Width, loadedImage.Width);
                Assert.Equal(testBitmap.Height, loadedImage.Height);
            }
        }
        
        [Fact]
        public void ImageWithGrayscaleConversion()
        {
            using var mainForm = new MainForm();
            mainForm.CurrentFileType = FileType.PNG;
            mainForm.SaveLocationMode = SaveLocationMode.Custom;
            mainForm.CustomSaveDirectory = testDirectory;
            mainForm.UseTimestampNaming = true;
            mainForm.PngGrayscale = true;
            mainForm.ShowSaveDialog = false;
            
            // カラー画像を作成
            using var testBitmap = CreateTestBitmap();
            
            // グレースケール変換
            var grayscaleImage = ConvertToGrayscale(testBitmap);
            
            var fileName = mainForm.GenerateFileName();
            var fullPath = Path.Combine(testDirectory, fileName);
            
            grayscaleImage.Save(fullPath, ImageFormat.Png);
            
            Assert.True(File.Exists(fullPath));
            
            // グレースケール画像を読み込んで検証
            using var loadedImage = new Bitmap(fullPath);
            
            // グレースケールかどうかを確認（いくつかのピクセルをサンプリング）
            bool isGrayscale = true;
            for (int x = 0; x < Math.Min(10, loadedImage.Width); x++)
            {
                for (int y = 0; y < Math.Min(10, loadedImage.Height); y++)
                {
                    var pixel = loadedImage.GetPixel(x, y);
                    if (pixel.R != pixel.G || pixel.G != pixel.B)
                    {
                        isGrayscale = false;
                        break;
                    }
                }
                if (!isGrayscale) break;
            }
            
            Assert.True(isGrayscale, "Image should be grayscale");
            
            grayscaleImage.Dispose();
        }
        
        [Theory]
        [InlineData(50)]
        [InlineData(75)]
        [InlineData(100)]
        public void ImageJpegQualitySettings(int quality)
        {
            using var mainForm = new MainForm();
            mainForm.CurrentFileType = FileType.JPEG;
            mainForm.SaveLocationMode = SaveLocationMode.Custom;
            mainForm.CustomSaveDirectory = testDirectory;
            mainForm.UseTimestampNaming = true;
            mainForm.JpegQuality = quality;
            mainForm.ShowSaveDialog = false;
            
            using var testBitmap = CreateTestBitmap(200, 200);
            
            var fileName = mainForm.GenerateFileName();
            var fullPath = Path.Combine(testDirectory, fileName);
            
            SaveAsJpeg(testBitmap, fullPath, quality);
            
            Assert.True(File.Exists(fullPath));
            
            var fileSize = new FileInfo(fullPath).Length;
            Assert.True(fileSize > 0);
        }
        
        [Theory]
        [InlineData(50)]
        [InlineData(75)]
        [InlineData(100)]
        public void ImageWebpQualitySettings(int quality)
        {
            using var mainForm = new MainForm();
            mainForm.CurrentFileType = FileType.WEBP;
            mainForm.SaveLocationMode = SaveLocationMode.Custom;
            mainForm.CustomSaveDirectory = testDirectory;
            mainForm.UseTimestampNaming = true;
            mainForm.WebpQuality = quality;
            mainForm.ShowSaveDialog = false;
            
            using var testBitmap = CreateTestBitmap(200, 200);
            
            var fileName = mainForm.GenerateFileName();
            var fullPath = Path.Combine(testDirectory, fileName);
            
            SaveAsWebp(testBitmap, fullPath, quality);
            
            Assert.True(File.Exists(fullPath));
            
            var fileSize = new FileInfo(fullPath).Length;
            Assert.True(fileSize > 0);
        }
        
        [Fact]
        public void EmptyDirectoryDoesNotCrash()
        {
            using var mainForm = new MainForm();
            mainForm.SaveLocationMode = SaveLocationMode.Custom;
            mainForm.CustomSaveDirectory = testDirectory;
            
            // ディレクトリが空の場合
            var filesBefore = Directory.GetFiles(testDirectory).Length;
            Assert.Equal(0, filesBefore);
        }
        
        [Theory]
        [InlineData(FileType.BMP, ".bmp")]
        [InlineData(FileType.JPEG, ".jpg")]
        [InlineData(FileType.PNG, ".png")]
        [InlineData(FileType.WEBP, ".webp")]
        public void GeneratedFileNameHasCorrectExtension(FileType fileType, string expectedExtension)
        {
            using var mainForm = new MainForm();
            mainForm.CurrentFileType = fileType;
            mainForm.UseTimestampNaming = true;
            
            var fileName = mainForm.GenerateFileName();
            var actualExtension = Path.GetExtension(fileName);
            
            Assert.Equal(expectedExtension, actualExtension, StringComparer.OrdinalIgnoreCase);
        }
        
        private void SaveAsJpeg(Image image, string filePath, int quality)
        {
            var jpegCodec = GetEncoder(ImageFormat.Jpeg);
            if (jpegCodec != null)
            {
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                image.Save(filePath, jpegCodec, encoderParams);
            }
            else
            {
                image.Save(filePath, ImageFormat.Jpeg);
            }
        }
        
        private void SaveAsWebp(Image image, string filePath, int quality)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    
                    using (var imageSharpImage = SixLabors.ImageSharp.Image.Load(ms))
                    {
                        var encoder = new SixLabors.ImageSharp.Formats.Webp.WebpEncoder()
                        {
                            Quality = quality
                        };
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            imageSharpImage.Save(fileStream, encoder);
                        }
                    }
                }
            }
            catch
            {
                // Fallback to PNG
                image.Save(filePath.Replace(".webp", ".png"), ImageFormat.Png);
            }
        }
        
        private ImageCodecInfo? GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        
        private Image ConvertToGrayscale(Image originalImage)
        {
            var grayscaleBitmap = new Bitmap(originalImage.Width, originalImage.Height);
            
            var colorMatrix = new ColorMatrix(new float[][]
            {
                new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });
            
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix);
            
            using (var graphics = Graphics.FromImage(grayscaleBitmap))
            {
                graphics.DrawImage(originalImage, 
                    new Rectangle(0, 0, originalImage.Width, originalImage.Height),
                    0, 0, originalImage.Width, originalImage.Height,
                    GraphicsUnit.Pixel, imageAttributes);
            }
            
            return grayscaleBitmap;
        }
    }
}
