using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows.Forms;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SystemDrawingImage = System.Drawing.Image;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace KoClipCS
{
    public partial class MainForm : Form
    {
        // Converted from Delphi TForm1 components
        public System.Windows.Forms.Timer clipboardTimer = null!;
        private System.Windows.Forms.Timer iconAnimationTimer = null!;
        private NotifyIcon notifyIcon = null!;
        private ContextMenuStrip trayContextMenu = null!;
        
        // Converted from Delphi global variables
        public SaveLocationMode SaveLocationMode = SaveLocationMode.MyPictures;
        public string CustomSaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        public int ClippingWaitInterval = 1000;
        public bool AutoStart = false;
        public bool ShowSaveDialog = false;
        public FileType CurrentFileType = FileType.PNG;
        public int JpegQuality = 100;
        public bool JpegGrayscale = false;
        public bool PngGrayscale = false;
        public bool WebpGrayscale = false;
        public int WebpQuality = 100;
        public bool UseTimestampNaming = true;
        private bool isMonitoring = false;
        
        // Custom filename settings
        public string FilenamePrefix = "";
        public string FilenameSuffix = "";
        public int FilenameSequenceNumber = 1;
        public int FilenameDigits = 3;
        
        // Sound settings
        public SoundMode SoundMode = SoundMode.DefaultWav;  // デフォルトWAVを初期値に
        
        // Track clipboard state to detect changes
        private SystemDrawingImage? lastClipboardImage = null;
        private bool isShowingDialog = false; // ダイアログ表示中フラグを追加
        
        // Icon animation
        private int iconAnimationState = 1;
        private Icon[]? animationIcons;
        
        public MainForm()
        {
            InitializeComponent();
            InitializeKoClipComponents();
        }
        
        protected override void SetVisibleCore(bool value)
        {
            // Prevent the form from becoming visible
            base.SetVisibleCore(false);
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Hide the form instead of closing it
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                base.OnFormClosing(e);
            }
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Set application icon
            try
            {
                using var iconStream = System.Reflection.Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("KoClipCS.Resources.app-icon.ico");
                if (iconStream != null)
                {
                    this.Icon = new Icon(iconStream);
                }
            }
            catch
            {
                // Ignore icon loading errors
            }
            
            // MainForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1, 1); // Minimal size instead of 0,0
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.Name = "MainForm";
            this.Text = "KoClip";
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
            this.Load += MainForm_Load;
            
            this.ResumeLayout(false);
        }
        
        private void InitializeKoClipComponents()
        {
            // Initialize clipboard monitoring timer
            InitializeClipboardTimer();
            
            // Initialize system tray
            InitializeSystemTray();
            
            // Initialize icon animation
            InitializeIconAnimation();
            
            // Load settings
            LoadSettings();
            
            // Auto-start if configured
            if (AutoStart)
            {
                StartClipboardMonitoring();
            }
        }
        
        private void MainForm_Load(object? sender, EventArgs e)
        {
            // Hide the form immediately when it loads
            this.Hide();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }
        
        private void InitializeClipboardTimer()
        {
            clipboardTimer = new System.Windows.Forms.Timer();
            clipboardTimer.Interval = ClippingWaitInterval;
            clipboardTimer.Tick += ClipboardTimer_Tick;
            clipboardTimer.Enabled = false;
        }
        
        private void ClipboardTimer_Tick(object? sender, EventArgs e)
        {
            MonitorClipboard();
        }
        
        private void MonitorClipboard()
        {
            // ダイアログ表示中は処理をスキップ
            if (isShowingDialog)
            {
                return;
            }
            
            try
            {
                if (Clipboard.ContainsImage())
                {
                    var image = Clipboard.GetImage();
                    if (image != null)
                    {
                        // Check if this is a new image (different from the last one we processed)
                        bool isNew = IsNewClipboardImage(image);
                        System.Diagnostics.Debug.WriteLine($"Clipboard contains image. Size: {image.Width}x{image.Height}, IsNew: {isNew}");
                        
                        if (isNew)
                        {
                            SaveClipboardImage(image);
                            // Update the last processed image
                            lastClipboardImage?.Dispose();
                            lastClipboardImage = new Bitmap(image);
                        }
                    }
                }
                else
                {
                    // Clear the last image if clipboard no longer contains an image
                    if (lastClipboardImage != null)
                    {
                        lastClipboardImage.Dispose();
                        lastClipboardImage = null;
                        System.Diagnostics.Debug.WriteLine("Clipboard no longer contains image, cleared last image");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle clipboard access errors silently
                System.Diagnostics.Debug.WriteLine($"Clipboard access error: {ex.Message}");
            }
        }
        
        private bool IsNewClipboardImage(SystemDrawingImage currentImage)
        {
            if (lastClipboardImage == null)
                return true;
            
            // Simple comparison - check if dimensions are different
            if (currentImage.Width != lastClipboardImage.Width || 
                currentImage.Height != lastClipboardImage.Height)
                return true;
            
            // For images with same dimensions, we need to check if it's actually a different image
            // We'll use a simple hash comparison of the image data
            try
            {
                // Convert both images to byte arrays and compare
                var currentHash = GetImageHash(currentImage);
                var lastHash = GetImageHash(lastClipboardImage);
                
                return !currentHash.Equals(lastHash);
            }
            catch
            {
                // If comparison fails, assume it's a new image to be safe
                return true;
            }
        }
        
        private string GetImageHash(SystemDrawingImage image)
        {
            using (var ms = new MemoryStream())
            {
                // Save as PNG to get consistent byte representation
                image.Save(ms, ImageFormat.Png);
                var bytes = ms.ToArray();
                
                // Simple hash - just use the first few bytes and length
                // This is not cryptographically secure but sufficient for our purpose
                if (bytes.Length > 16)
                {
                    return $"{bytes.Length}_{bytes[0]}_{bytes[1]}_{bytes[8]}_{bytes[15]}";
                }
                return bytes.Length.ToString();
            }
        }
        
        private void SaveClipboardImage(SystemDrawingImage image)
        {
            try
            {
                // 保存確認ダイアログが有効な場合は確認する
                if (ShowSaveDialog)
                {
                    // ダイアログ表示中フラグを設定
                    isShowingDialog = true;
                    
                    try
                    {
                        var result = MessageBox.Show(
                            "クリップボードの画像を保存しますか？",
                            "KoClip - 保存確認",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1
                        );
                        
                        if (result != DialogResult.Yes)
                        {
                            System.Diagnostics.Debug.WriteLine("User cancelled image save");
                            return; // ユーザーがキャンセルした場合は保存しない
                        }
                    }
                    finally
                    {
                        // ダイアログ表示中フラグをクリア
                        isShowingDialog = false;
                    }
                }
                
                string fileName = GenerateFileName();
                string fullPath = Path.Combine(GetSaveDirectory(), fileName);
                
                // Ensure directory exists
                var directoryPath = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                
                // Debug output
                System.Diagnostics.Debug.WriteLine($"Saving image to: {fullPath}");
                System.Diagnostics.Debug.WriteLine($"File type: {CurrentFileType}");
                
                // Convert to grayscale if needed
                SystemDrawingImage imageToSave = image;
                bool shouldConvertToGrayscale = (CurrentFileType == FileType.JPEG && JpegGrayscale) || 
                                               (CurrentFileType == FileType.PNG && PngGrayscale) ||
                                               (CurrentFileType == FileType.WEBP && WebpGrayscale);
                
                if (shouldConvertToGrayscale)
                {
                    imageToSave = ConvertToGrayscale(image);
                    System.Diagnostics.Debug.WriteLine("Converted image to grayscale");
                }
                
                // Save based on file type
                switch (CurrentFileType)
                {
                    case FileType.BMP:
                        imageToSave.Save(fullPath, ImageFormat.Bmp);
                        System.Diagnostics.Debug.WriteLine("Saved as BMP");
                        break;
                    case FileType.JPEG:
                        SaveAsJpeg(imageToSave, fullPath);
                        System.Diagnostics.Debug.WriteLine("Saved as JPEG");
                        break;
                    case FileType.PNG:
                        imageToSave.Save(fullPath, ImageFormat.Png);
                        System.Diagnostics.Debug.WriteLine("Saved as PNG");
                        break;
                    case FileType.WEBP:
                        SaveAsWebp(imageToSave, fullPath);
                        System.Diagnostics.Debug.WriteLine("Saved as WebP");
                        break;
                }
                
                // Dispose the grayscale image if we created one
                if (shouldConvertToGrayscale && imageToSave != image)
                {
                    imageToSave.Dispose();
                }
                
                // Debug output
                System.Diagnostics.Debug.WriteLine($"Image saved successfully: {fileName}");
                
                // Play notification sound
                PlayNotificationSound();
                
                // Show balloon tip
                notifyIcon.ShowBalloonTip(2000, "KoClip", $"Image saved: {fileName}", ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                // ダイアログ表示中フラグをクリア（例外が発生した場合も）
                isShowingDialog = false;
                
                System.Diagnostics.Debug.WriteLine($"Save error: {ex.Message}");
                notifyIcon.ShowBalloonTip(3000, "KoClip Error", $"Failed to save image: {ex.Message}", ToolTipIcon.Error);
            }
        }
        
        private void SaveAsJpeg(SystemDrawingImage image, string filePath)
        {
            var jpegCodec = GetEncoder(ImageFormat.Jpeg);
            if (jpegCodec != null)
            {
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, JpegQuality);
                
                image.Save(filePath, jpegCodec, encoderParams);
            }
            else
            {
                // Fallback to default JPEG save if codec not found
                image.Save(filePath, ImageFormat.Jpeg);
            }
        }
        
        private void SaveAsWebp(SystemDrawingImage image, string filePath)
        {
            try
            {
                // Convert System.Drawing.Image to ImageSharp Image
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Png); // Use PNG as intermediate format to preserve quality
                    ms.Position = 0;
                    
                    using (var imageSharpImage = ImageSharpImage.Load(ms))
                    {
                        var encoder = new WebpEncoder()
                        {
                            Quality = WebpQuality
                        };
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            imageSharpImage.Save(fileStream, encoder);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebP save error: {ex.Message}");
                // Fallback to PNG if WebP save fails
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
        
        private SystemDrawingImage ConvertToGrayscale(SystemDrawingImage originalImage)
        {
            // Create a new bitmap with the same dimensions
            var grayscaleBitmap = new Bitmap(originalImage.Width, originalImage.Height);
            
            // Create a ColorMatrix for grayscale conversion
            var colorMatrix = new ColorMatrix(new float[][]
            {
                new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });
            
            // Create ImageAttributes and set the color matrix
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix);
            
            // Draw the original image onto the new bitmap using the color matrix
            using (var graphics = Graphics.FromImage(grayscaleBitmap))
            {
                graphics.DrawImage(originalImage, 
                    new Rectangle(0, 0, originalImage.Width, originalImage.Height),
                    0, 0, originalImage.Width, originalImage.Height,
                    GraphicsUnit.Pixel, imageAttributes);
            }
            
            return grayscaleBitmap;
        }
        
        public string GenerateFileName()
        {
            string extension = CurrentFileType switch
            {
                FileType.BMP => ".bmp",
                FileType.JPEG => ".jpg",
                FileType.PNG => ".png",
                FileType.WEBP => ".webp",
                _ => ".bmp"
            };
            
            if (UseTimestampNaming)
            {
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
                System.Diagnostics.Debug.WriteLine($"Generated timestamp filename: {fileName}");
                return fileName;
            }
            else
            {
                // Use custom filename format
                string sequenceStr = FilenameSequenceNumber.ToString().PadLeft(FilenameDigits, '0');
                string fileName = FilenamePrefix + sequenceStr + FilenameSuffix + extension;
                System.Diagnostics.Debug.WriteLine($"Generated custom filename: {fileName} (Prefix: '{FilenamePrefix}', Sequence: {FilenameSequenceNumber}, Suffix: '{FilenameSuffix}', Digits: {FilenameDigits})");
                FilenameSequenceNumber++; // Increment for next file
                return fileName;
            }
        }
        
        private string GetSaveDirectory()
        {
            string dir;
            
            switch (SaveLocationMode)
            {
                case SaveLocationMode.CurrentDirectory:
                    dir = Directory.GetCurrentDirectory();
                    break;
                case SaveLocationMode.MyPictures:
                    dir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    break;
                case SaveLocationMode.Custom:
                    dir = CustomSaveDirectory;
                    // カスタムディレクトリが空または無効な場合はマイピクチャにフォールバック
                    if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(Path.GetDirectoryName(dir) ?? ""))
                    {
                        System.Diagnostics.Debug.WriteLine($"Custom directory '{dir}' is invalid, falling back to My Pictures");
                        dir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    }
                    break;
                default:
                    dir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    break;
            }
            
            System.Diagnostics.Debug.WriteLine($"Save directory mode: {SaveLocationMode}");
            System.Diagnostics.Debug.WriteLine($"Custom directory setting: '{CustomSaveDirectory}'");
            System.Diagnostics.Debug.WriteLine($"Final save directory: '{dir}'");
            return dir;
        }
        
        private void InitializeSystemTray()
        {
            try
            {
                notifyIcon = new NotifyIcon();
                
                // Load scissors icon from embedded resources
                try
                {
                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    using (var stream = assembly.GetManifestResourceStream("KoClipCS.Resources.HASAMI1.ICO"))
                    {
                        if (stream != null)
                        {
                            notifyIcon.Icon = new Icon(stream);
                        }
                        else
                        {
                            notifyIcon.Icon = SystemIcons.Application;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load scissors icon: {ex.Message}");
                    notifyIcon.Icon = SystemIcons.Application;
                }
                
                notifyIcon.Text = "KoClip - Clipboard Image Auto Save";
                notifyIcon.Visible = true;
                
                InitializeTrayContextMenu();
                notifyIcon.ContextMenuStrip = trayContextMenu;
                notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
                
                // Show a test balloon to confirm the icon is working
                notifyIcon.ShowBalloonTip(2000, "KoClip", "KoClip started successfully", ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize system tray: {ex.Message}", "KoClip Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            ToggleMonitoring();
        }
        
        private void InitializeTrayContextMenu()
        {
            trayContextMenu = new ContextMenuStrip();
            
            // 開始/停止メニュー
            var startStopMenuItem = new ToolStripMenuItem(isMonitoring ? "停止(&S)" : "開始(&S)");
            startStopMenuItem.Click += (s, e) => ToggleMonitoring();
            trayContextMenu.Items.Add(startStopMenuItem);
            
            // 設定メニュー
            var settingsMenuItem = new ToolStripMenuItem("設定(&C)");
            settingsMenuItem.Click += (s, e) => ShowSettings();
            trayContextMenu.Items.Add(settingsMenuItem);
            
            // 保存フォルダを開くメニュー
            var openFolderMenuItem = new ToolStripMenuItem("保存フォルダを開く(&O)");
            openFolderMenuItem.Click += (s, e) => OpenSaveFolder();
            trayContextMenu.Items.Add(openFolderMenuItem);
            
            // セパレータ
            trayContextMenu.Items.Add(new ToolStripSeparator());
            
            // ヘルプとバージョン情報メニュー（統合）
            var helpMenuItem = new ToolStripMenuItem("ヘルプとバージョン情報(&H)");
            helpMenuItem.Click += (s, e) => ShowHelpAndAbout();
            trayContextMenu.Items.Add(helpMenuItem);
            
            // セパレータ
            trayContextMenu.Items.Add(new ToolStripSeparator());
            
            // 終了メニュー
            var exitMenuItem = new ToolStripMenuItem("終了(&X)");
            exitMenuItem.Click += (s, e) => Application.Exit();
            trayContextMenu.Items.Add(exitMenuItem);
        }
        
        private void ToggleMonitoring()
        {
            if (isMonitoring)
            {
                StopClipboardMonitoring();
            }
            else
            {
                StartClipboardMonitoring();
            }
        }
        
        private void StartClipboardMonitoring()
        {
            isMonitoring = true;
            clipboardTimer.Enabled = true;
            iconAnimationTimer.Enabled = true;
            notifyIcon.Text = "KoClip - Monitoring (Active)";
            
            // Update context menu
            if (trayContextMenu.Items.Count > 0)
            {
                ((ToolStripMenuItem)trayContextMenu.Items[0]).Text = "停止(&S)";
            }
        }
        
        private void StopClipboardMonitoring()
        {
            isMonitoring = false;
            clipboardTimer.Enabled = false;
            iconAnimationTimer.Enabled = false;
            notifyIcon.Text = "KoClip - Monitoring (Stopped)";
            
            // ダイアログ表示中フラグもリセット
            isShowingDialog = false;
            
            // Clear the last clipboard image when stopping
            if (lastClipboardImage != null)
            {
                lastClipboardImage.Dispose();
                lastClipboardImage = null;
            }
            
            // Reset to first scissors icon when stopped
            if (animationIcons != null && animationIcons.Length > 0)
            {
                notifyIcon.Icon = animationIcons[0];
            }
            
            // Update context menu
            if (trayContextMenu.Items.Count > 0)
            {
                ((ToolStripMenuItem)trayContextMenu.Items[0]).Text = "開始(&S)";
            }
        }
        
        private void ShowSettings()
        {
            var settingsForm = new SettingsForm(this);
            settingsForm.ShowDialog();
        }
        
        private void ShowHelpAndAbout()
        {
            var helpForm = new Form();
            helpForm.Text = "ヘルプとバージョン情報";
            helpForm.Size = new Size(500, 400);
            helpForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            helpForm.MaximizeBox = false;
            helpForm.MinimizeBox = false;
            helpForm.StartPosition = FormStartPosition.CenterScreen;
            
            var tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            
            // ヘルプタブ
            var helpTab = new TabPage("ヘルプ");
            var helpTextBox = new TextBox();
            helpTextBox.Multiline = true;
            helpTextBox.ReadOnly = true;
            helpTextBox.ScrollBars = ScrollBars.Vertical;
            helpTextBox.Dock = DockStyle.Fill;
            helpTextBox.Font = new Font("MS Gothic", 10);
            helpTextBox.Text = "KoClip - クリップボード画像自動保存ツール\r\n\r\n" +
                              "【使用方法】\r\n" +
                              "1. 「開始」をクリックしてクリップボード監視を開始\r\n" +
                              "2. 画像をクリップボードにコピー\r\n" +
                              "3. 自動的に指定フォルダに保存されます\r\n\r\n" +
                              "【機能】\r\n" +
                              "・複数の画像形式に対応 (BMP, JPEG, PNG, WebP)\r\n" +
                              "・保存先フォルダの指定\r\n" +
                              "・ファイル名のカスタマイズ\r\n" +
                              "・JPEG/WebPの品質設定\r\n" +
                              "・グレースケール変換\r\n" +
                              "・保存時の通知音設定\r\n\r\n" +
                              "設定画面で保存形式や保存先を変更できます。";
            helpTab.Controls.Add(helpTextBox);
            
            // バージョン情報タブ
            var aboutTab = new TabPage("バージョン情報");
            var aboutTextBox = new TextBox();
            aboutTextBox.Multiline = true;
            aboutTextBox.ReadOnly = true;
            aboutTextBox.ScrollBars = ScrollBars.Vertical;
            aboutTextBox.Dock = DockStyle.Fill;
            aboutTextBox.Font = new Font("MS Gothic", 10);
            
            // アセンブリからバージョン情報を取得
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var versionString = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "2.0.1";
            
            aboutTextBox.Text = $"KoClip for .NET\r\n" +
                              $"Version {versionString}\r\n\r\n" +
                              "Original Delphi version by tikomo software\r\n" +
                              "C# conversion developed with Kiro AI\r\n\r\n" +
                              "© 2026 tikomo software\r\n\r\n" +
                              "クリップボード画像自動保存ツール\r\n\r\n" +
                              "【開発環境】\r\n" +
                              ".NET 9.0\r\n" +
                              "Windows Forms\r\n" +
                              "SixLabors.ImageSharp (WebP対応)\r\n\r\n" +
                              "【ライセンス】\r\n" +
                              "このソフトウェアはフリーウェアです。";
            aboutTab.Controls.Add(aboutTextBox);
            
            tabControl.TabPages.Add(helpTab);
            tabControl.TabPages.Add(aboutTab);
            
            var okButton = new Button();
            okButton.Text = "OK";
            okButton.Size = new Size(75, 28);
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            okButton.Location = new Point(helpForm.ClientSize.Width - 90, helpForm.ClientSize.Height - 40);
            okButton.Click += (s, e) => helpForm.Close();
            
            helpForm.Controls.Add(tabControl);
            helpForm.Controls.Add(okButton);
            
            helpForm.ShowDialog();
        }
        
        private void OpenSaveFolder()
        {
            try
            {
                string saveDir = GetSaveDirectory();
                
                // Ensure directory exists
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                
                // Open folder in Windows Explorer
                System.Diagnostics.Process.Start("explorer.exe", saveDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存フォルダを開けませんでした: {ex.Message}", "エラー", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void InitializeIconAnimation()
        {
            // Load scissors icons from embedded resources
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                
                // Load HASAMI1.ICO
                using (var stream1 = assembly.GetManifestResourceStream("KoClipCS.Resources.HASAMI1.ICO"))
                {
                    if (stream1 != null)
                    {
                        var icon1 = new Icon(stream1);
                        
                        // Load HASAMI2.ICO
                        using (var stream2 = assembly.GetManifestResourceStream("KoClipCS.Resources.HASAMI2.ICO"))
                        {
                            if (stream2 != null)
                            {
                                var icon2 = new Icon(stream2);
                                animationIcons = new Icon[] { icon1, icon2 };
                            }
                            else
                            {
                                // Fallback to single icon if second icon fails to load
                                animationIcons = new Icon[] { icon1, icon1 };
                            }
                        }
                    }
                    else
                    {
                        // Fallback to system icons if resource loading fails
                        animationIcons = new Icon[] { SystemIcons.Application, SystemIcons.Information };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load scissors icons: {ex.Message}");
                // Fallback to system icons
                animationIcons = new Icon[] { SystemIcons.Application, SystemIcons.Information };
            }
            
            iconAnimationTimer = new System.Windows.Forms.Timer();
            iconAnimationTimer.Interval = 500;
            iconAnimationTimer.Tick += IconAnimationTimer_Tick;
        }
        
        private void IconAnimationTimer_Tick(object? sender, EventArgs e)
        {
            // Simple icon animation when monitoring
            if (isMonitoring)
            {
                iconAnimationState = iconAnimationState == 1 ? 2 : 1;
                if (notifyIcon != null && animationIcons != null)
                {
                    notifyIcon.Icon = animationIcons[iconAnimationState - 1];
                }
            }
        }
        
        private void PlayNotificationSound()
        {
            try
            {
                switch (SoundMode)
                {
                    case SoundMode.Beep:
                        SystemSounds.Beep.Play();
                        break;
                        
                    case SoundMode.DefaultWav:
                        // Play embedded WAV file from resources
                        PlayEmbeddedWav();
                        break;
                        
                    case SoundMode.NoSound:
                        // No sound
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Audio playback error: {ex.Message}");
                // Fallback to system beep
                SystemSounds.Beep.Play();
            }
        }
        
        private void PlayEmbeddedWav()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("KoClipCS.Resources.A5_08261.WAV"))
                {
                    if (stream != null)
                    {
                        var player = new SoundPlayer(stream);
                        player.Play();
                    }
                    else
                    {
                        // Fallback to beep if resource not found
                        SystemSounds.Beep.Play();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Embedded WAV playback error: {ex.Message}");
                // Fallback to beep
                SystemSounds.Beep.Play();
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 設定を保存
                SaveSettings();
                
                clipboardTimer?.Dispose();
                iconAnimationTimer?.Dispose();
                notifyIcon?.Dispose();
                trayContextMenu?.Dispose();
                lastClipboardImage?.Dispose();
                
                if (animationIcons != null)
                {
                    foreach (var icon in animationIcons)
                    {
                        icon?.Dispose();
                    }
                }
            }
            base.Dispose(disposing);
        }
        
        private void LoadSettings()
        {
            // Load settings from INI file
            SettingsManager.LoadSettings(this);
        }
        
        public void SaveSettings()
        {
            // Save settings to INI file
            SettingsManager.SaveSettings(this);
        }
    }
    
    public enum FileType
    {
        BMP = 0,
        JPEG = 1,
        PNG = 2,
        WEBP = 3
    }
    
    public enum SaveLocationMode
    {
        CurrentDirectory = 0,
        MyPictures = 1,
        Custom = 2
    }
    
    public enum SoundMode
    {
        Beep = 0,
        DefaultWav = 1,
        NoSound = 2
    }
}