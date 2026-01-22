using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace KoClipCS
{
    public partial class SettingsForm : Form
    {
        private MainForm mainForm;
        
        // Tab control and tab pages
        private TabControl tabControl = null!;
        private TabPage generalTab = null!;
        private TabPage fileTab = null!;
        private TabPage formatTab = null!;
        private TabPage soundTab = null!;
        
        // General tab controls
        private CheckBox autoStartCheckBox = null!;
        private CheckBox showSaveDialogCheckBox = null!;
        private RadioButton currentDirRadio = null!;
        private RadioButton myPicturesRadio = null!;
        private RadioButton customDirRadio = null!;
        private TextBox customDirTextBox = null!;
        private Button browseDirButton = null!;
        private Label intervalLabel = null!;
        private NumericUpDown intervalNumeric = null!;
        
        // File tab controls
        private RadioButton timestampRadio = null!;
        private RadioButton customNameRadio = null!;
        private Label prefixLabel = null!;
        private TextBox prefixTextBox = null!;
        private Label suffixLabel = null!;
        private TextBox suffixTextBox = null!;
        private Label sequenceLabel = null!;
        private NumericUpDown sequenceNumeric = null!;
        private Label digitsLabel = null!;
        private NumericUpDown digitsNumeric = null!;
        private Label previewLabel = null!;
        private TextBox previewValueLabel = null!;
        
        // Format tab controls
        private ComboBox formatComboBox = null!;
        private Label qualityLabel = null!;
        private NumericUpDown qualityNumeric = null!;
        private CheckBox grayscaleCheckBox = null!;
        
        // Sound tab controls
        private RadioButton beepRadio = null!;
        private RadioButton defaultWavRadio = null!;
        private RadioButton noSoundRadio = null!;
        
        // Buttons
        private Button okButton = null!;
        private Button cancelButton = null!;
        
        public SettingsForm(MainForm mainForm)
        {
            this.mainForm = mainForm;
            InitializeComponent();
            LoadSettings();
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form settings
            this.Text = "設定";
            this.Size = new Size(470, 450);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Tab control
            tabControl = new TabControl();
            tabControl.Location = new Point(12, 12);
            tabControl.Size = new Size(430, 340);
            
            // Create tabs
            CreateGeneralTab();
            CreateFileTab();
            CreateFormatTab();
            CreateSoundTab();
            
            // Add tabs to control
            tabControl.TabPages.Add(generalTab);
            tabControl.TabPages.Add(fileTab);
            tabControl.TabPages.Add(formatTab);
            tabControl.TabPages.Add(soundTab);
            
            // OK button
            okButton = new Button();
            okButton.Text = "OK";
            okButton.Size = new Size(75, 28);
            okButton.Location = new Point(287, 365);
            okButton.Click += OkButton_Click;
            
            // Cancel button
            cancelButton = new Button();
            cancelButton.Text = "キャンセル";
            cancelButton.Size = new Size(75, 28);
            cancelButton.Location = new Point(367, 365);
            cancelButton.Click += CancelButton_Click;
            
            // Add controls to form
            this.Controls.Add(tabControl);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);
            
            this.ResumeLayout(false);
        }
        
        private void CreateGeneralTab()
        {
            generalTab = new TabPage("全般");
            
            // Auto start checkbox
            autoStartCheckBox = new CheckBox();
            autoStartCheckBox.Text = "ソフト起動時にクリップボードの監視を開始";
            autoStartCheckBox.Location = new Point(20, 15);  // Y座標を20→15に調整
            autoStartCheckBox.Size = new Size(350, 20);
            
            // Show save dialog checkbox
            showSaveDialogCheckBox = new CheckBox();
            showSaveDialogCheckBox.Text = "保存時に確認ダイアログを表示";
            showSaveDialogCheckBox.Location = new Point(20, 40);  // Y座標を50→40に調整
            showSaveDialogCheckBox.Size = new Size(250, 20);
            
            // Save location group
            var saveLocationGroup = new GroupBox();
            saveLocationGroup.Text = "保存先";
            saveLocationGroup.Location = new Point(20, 70);  // Y座標を80→70に調整
            saveLocationGroup.Size = new Size(380, 110);  // 幅を360→380に拡大
            
            currentDirRadio = new RadioButton();
            currentDirRadio.Text = "カレントディレクトリ";
            currentDirRadio.Location = new Point(15, 20);  // Y座標を25→20に調整
            currentDirRadio.Size = new Size(200, 20);
            
            myPicturesRadio = new RadioButton();
            myPicturesRadio.Text = "マイピクチャ";
            myPicturesRadio.Location = new Point(15, 40);  // Y座標を50→40に調整
            myPicturesRadio.Size = new Size(200, 20);
            
            customDirRadio = new RadioButton();
            customDirRadio.Text = "ユーザー指定フォルダ";
            customDirRadio.Location = new Point(15, 60);  // Y座標を75→60に調整
            customDirRadio.Size = new Size(200, 20);
            customDirRadio.CheckedChanged += CustomDirRadio_CheckedChanged;
            
            customDirTextBox = new TextBox();
            customDirTextBox.Location = new Point(35, 80);  // Y座標を95→80に調整
            customDirTextBox.Size = new Size(260, 23);
            
            browseDirButton = new Button();
            browseDirButton.Text = "参照";
            browseDirButton.Location = new Point(300, 80);  // X座標を280→300に調整
            browseDirButton.Size = new Size(75, 28);
            browseDirButton.Click += BrowseDirButton_Click;
            
            saveLocationGroup.Controls.Add(currentDirRadio);
            saveLocationGroup.Controls.Add(myPicturesRadio);
            saveLocationGroup.Controls.Add(customDirRadio);
            saveLocationGroup.Controls.Add(customDirTextBox);
            saveLocationGroup.Controls.Add(browseDirButton);
            
            // Clipping interval
            intervalLabel = new Label();
            intervalLabel.Text = "クリッピング間隔 (ms):";
            intervalLabel.Location = new Point(20, 195);  // Y座標を220→195に調整
            intervalLabel.Size = new Size(150, 20);
            
            intervalNumeric = new NumericUpDown();
            intervalNumeric.Location = new Point(180, 193);  // Y座標を218→193に調整
            intervalNumeric.Size = new Size(80, 23);
            intervalNumeric.Minimum = 100;
            intervalNumeric.Maximum = 5000;
            intervalNumeric.Increment = 100;
            
            generalTab.Controls.Add(autoStartCheckBox);
            generalTab.Controls.Add(showSaveDialogCheckBox);
            generalTab.Controls.Add(saveLocationGroup);
            generalTab.Controls.Add(intervalLabel);
            generalTab.Controls.Add(intervalNumeric);
        }
        
        private void CreateFileTab()
        {
            fileTab = new TabPage("ファイル");
            
            // File naming group
            var namingGroup = new GroupBox();
            namingGroup.Text = "ファイル名生成方法";
            namingGroup.Location = new Point(20, 15);  // Y座標を20→15に調整
            namingGroup.Size = new Size(380, 70);  // 幅を360→380に拡大
            
            timestampRadio = new RadioButton();
            timestampRadio.Text = "現在時刻 (yyyyMMddHHmmss)";
            timestampRadio.Location = new Point(15, 20);  // Y座標を25→20に調整
            timestampRadio.Size = new Size(250, 20);
            timestampRadio.CheckedChanged += NamingRadio_CheckedChanged;
            
            customNameRadio = new RadioButton();
            customNameRadio.Text = "ユーザー指定";
            customNameRadio.Location = new Point(15, 40);  // Y座標を50→40に調整
            customNameRadio.Size = new Size(150, 20);
            customNameRadio.CheckedChanged += NamingRadio_CheckedChanged;
            
            namingGroup.Controls.Add(timestampRadio);
            namingGroup.Controls.Add(customNameRadio);
            
            // Custom naming settings (arranged vertically)
            var customGroup = new GroupBox();
            customGroup.Text = "カスタム設定";
            customGroup.Location = new Point(20, 95);  // Y座標を110→95に調整
            customGroup.Size = new Size(380, 130);  // 幅を360→380に拡大
            
            prefixLabel = new Label();
            prefixLabel.Text = "接頭語:";
            prefixLabel.Location = new Point(15, 20);  // Y座標を25→20に調整
            prefixLabel.Size = new Size(60, 20);
            
            prefixTextBox = new TextBox();
            prefixTextBox.Location = new Point(80, 18);  // Y座標を23→18に調整
            prefixTextBox.Size = new Size(120, 23);
            prefixTextBox.TextChanged += CustomNaming_Changed;
            
            sequenceLabel = new Label();
            sequenceLabel.Text = "連番:";
            sequenceLabel.Location = new Point(15, 45);  // Y座標を55→45に調整
            sequenceLabel.Size = new Size(60, 20);
            
            sequenceNumeric = new NumericUpDown();
            sequenceNumeric.Location = new Point(80, 43);  // Y座標を53→43に調整
            sequenceNumeric.Size = new Size(80, 23);
            sequenceNumeric.Minimum = 1;
            sequenceNumeric.Maximum = 99999;
            sequenceNumeric.ValueChanged += CustomNaming_Changed;
            
            suffixLabel = new Label();
            suffixLabel.Text = "接尾語:";
            suffixLabel.Location = new Point(15, 70);  // Y座標を85→70に調整
            suffixLabel.Size = new Size(60, 20);
            
            suffixTextBox = new TextBox();
            suffixTextBox.Location = new Point(80, 68);  // Y座標を83→68に調整
            suffixTextBox.Size = new Size(120, 23);
            suffixTextBox.TextChanged += CustomNaming_Changed;
            
            digitsLabel = new Label();
            digitsLabel.Text = "桁数:";
            digitsLabel.Location = new Point(15, 95);  // Y座標を115→95に調整
            digitsLabel.Size = new Size(60, 20);
            
            digitsNumeric = new NumericUpDown();
            digitsNumeric.Location = new Point(80, 93);  // Y座標を113→93に調整
            digitsNumeric.Size = new Size(60, 23);
            digitsNumeric.Minimum = 1;
            digitsNumeric.Maximum = 10;
            digitsNumeric.ValueChanged += CustomNaming_Changed;
            
            customGroup.Controls.Add(prefixLabel);
            customGroup.Controls.Add(prefixTextBox);
            customGroup.Controls.Add(sequenceLabel);
            customGroup.Controls.Add(sequenceNumeric);
            customGroup.Controls.Add(suffixLabel);
            customGroup.Controls.Add(suffixTextBox);
            customGroup.Controls.Add(digitsLabel);
            customGroup.Controls.Add(digitsNumeric);
            
            // Preview
            previewLabel = new Label();
            previewLabel.Text = "プレビュー:";
            previewLabel.Location = new Point(20, 235);
            previewLabel.Size = new Size(80, 20);
            
            previewValueLabel = new TextBox();
            previewValueLabel.Location = new Point(100, 233);
            previewValueLabel.Size = new Size(300, 23);
            previewValueLabel.ReadOnly = true;
            previewValueLabel.BackColor = SystemColors.Window;
            
            fileTab.Controls.Add(namingGroup);
            fileTab.Controls.Add(customGroup);
            fileTab.Controls.Add(previewLabel);
            fileTab.Controls.Add(previewValueLabel);
        }
        
        private void CreateFormatTab()
        {
            formatTab = new TabPage("形式");
            
            // File format
            var formatLabel = new Label();
            formatLabel.Text = "ファイル形式:";
            formatLabel.Location = new Point(20, 20);  // Y座標を30→20に調整
            formatLabel.Size = new Size(100, 20);
            
            formatComboBox = new ComboBox();
            formatComboBox.Location = new Point(130, 18);  // Y座標を28→18に調整
            formatComboBox.Size = new Size(120, 23);
            formatComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            formatComboBox.Items.AddRange(new string[] { "BMP", "JPEG", "PNG", "Webp" });
            formatComboBox.SelectedIndexChanged += FormatComboBox_SelectedIndexChanged;
            
            // Quality settings (positioned to show in same location)
            qualityLabel = new Label();
            qualityLabel.Text = "品質:";
            qualityLabel.Location = new Point(20, 60);  // Y座標を80→60に調整
            qualityLabel.Size = new Size(40, 20);
            
            qualityNumeric = new NumericUpDown();
            qualityNumeric.Location = new Point(70, 58);  // Y座標を78→58に調整
            qualityNumeric.Size = new Size(60, 23);
            qualityNumeric.Minimum = 1;
            qualityNumeric.Maximum = 100;
            qualityNumeric.Value = 100;
            
            // Grayscale checkbox
            grayscaleCheckBox = new CheckBox();
            grayscaleCheckBox.Text = "グレースケール";
            grayscaleCheckBox.Location = new Point(20, 90);  // Y座標を120→90に調整
            grayscaleCheckBox.Size = new Size(120, 20);
            
            formatTab.Controls.Add(formatLabel);
            formatTab.Controls.Add(formatComboBox);
            formatTab.Controls.Add(qualityLabel);
            formatTab.Controls.Add(qualityNumeric);
            formatTab.Controls.Add(grayscaleCheckBox);
        }
        
        private void CreateSoundTab()
        {
            soundTab = new TabPage("サウンド");
            
            var soundGroup = new GroupBox();
            soundGroup.Text = "保存時のサウンド";
            soundGroup.Location = new Point(20, 15);  // Y座標を20→15に調整
            soundGroup.Size = new Size(380, 100);  // 幅を360→380に拡大
            
            beepRadio = new RadioButton();
            beepRadio.Text = "ビープ音";
            beepRadio.Location = new Point(15, 20);  // Y座標を25→20に調整
            beepRadio.Size = new Size(150, 20);
            
            defaultWavRadio = new RadioButton();
            defaultWavRadio.Text = "デフォルトWAV";
            defaultWavRadio.Location = new Point(15, 40);  // Y座標を50→40に調整
            defaultWavRadio.Size = new Size(150, 20);
            
            noSoundRadio = new RadioButton();
            noSoundRadio.Text = "無音";
            noSoundRadio.Location = new Point(15, 60);  // Y座標を75→60に調整
            noSoundRadio.Size = new Size(150, 20);
            
            soundGroup.Controls.Add(beepRadio);
            soundGroup.Controls.Add(defaultWavRadio);
            soundGroup.Controls.Add(noSoundRadio);
            
            soundTab.Controls.Add(soundGroup);
        }
        
        private void CustomDirRadio_CheckedChanged(object? sender, EventArgs e)
        {
            customDirTextBox.Enabled = customDirRadio.Checked;
            browseDirButton.Enabled = customDirRadio.Checked;
        }
        
        private void BrowseDirButton_Click(object? sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "保存先フォルダを選択してください";
                dialog.SelectedPath = customDirTextBox.Text;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    customDirTextBox.Text = dialog.SelectedPath;
                    System.Diagnostics.Debug.WriteLine($"Selected custom directory: {dialog.SelectedPath}");
                }
            }
        }
        
        private void NamingRadio_CheckedChanged(object? sender, EventArgs e)
        {
            bool customEnabled = customNameRadio.Checked;
            
            prefixLabel.Enabled = customEnabled;
            prefixTextBox.Enabled = customEnabled;
            suffixLabel.Enabled = customEnabled;
            suffixTextBox.Enabled = customEnabled;
            sequenceLabel.Enabled = customEnabled;
            sequenceNumeric.Enabled = customEnabled;
            digitsLabel.Enabled = customEnabled;
            digitsNumeric.Enabled = customEnabled;
            
            UpdatePreview();
        }
        
        private void CustomNaming_Changed(object? sender, EventArgs e)
        {
            UpdatePreview();
        }
        
        private void FormatComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selectedFormat = (FileType)formatComboBox.SelectedIndex;
            
            // Show/hide quality and grayscale controls based on format
            bool showQuality = selectedFormat == FileType.JPEG || selectedFormat == FileType.WEBP;
            bool showGrayscale = selectedFormat == FileType.JPEG || selectedFormat == FileType.PNG || selectedFormat == FileType.WEBP;
            
            qualityLabel.Visible = showQuality;
            qualityNumeric.Visible = showQuality;
            grayscaleCheckBox.Visible = showGrayscale;
            
            // Update quality default values
            if (selectedFormat == FileType.JPEG)
            {
                qualityNumeric.Value = mainForm.JpegQuality;
                grayscaleCheckBox.Checked = mainForm.JpegGrayscale;
            }
            else if (selectedFormat == FileType.WEBP)
            {
                qualityNumeric.Value = mainForm.WebpQuality;
                grayscaleCheckBox.Checked = mainForm.WebpGrayscale;
            }
            else if (selectedFormat == FileType.PNG)
            {
                grayscaleCheckBox.Checked = mainForm.PngGrayscale;
            }
        }
        
        private void UpdatePreview()
        {
            if (timestampRadio.Checked)
            {
                var extension = GetCurrentExtension();
                previewValueLabel.Text = DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
            }
            else if (customNameRadio.Checked)
            {
                var extension = GetCurrentExtension();
                string sequenceStr = ((int)sequenceNumeric.Value).ToString().PadLeft((int)digitsNumeric.Value, '0');
                previewValueLabel.Text = prefixTextBox.Text + sequenceStr + suffixTextBox.Text + extension;
            }
        }
        
        private string GetCurrentExtension()
        {
            return formatComboBox.SelectedIndex switch
            {
                0 => ".bmp",   // BMP
                1 => ".jpg",   // JPEG
                2 => ".png",   // PNG
                3 => ".webp",  // WebP
                _ => ".bmp"
            };
        }
        
        private void LoadSettings()
        {
            // General tab
            autoStartCheckBox.Checked = mainForm.AutoStart;
            showSaveDialogCheckBox.Checked = mainForm.ShowSaveDialog;
            
            switch (mainForm.SaveLocationMode)
            {
                case SaveLocationMode.CurrentDirectory:
                    currentDirRadio.Checked = true;
                    break;
                case SaveLocationMode.MyPictures:
                    myPicturesRadio.Checked = true;
                    break;
                case SaveLocationMode.Custom:
                    customDirRadio.Checked = true;
                    break;
            }
            
            customDirTextBox.Text = mainForm.CustomSaveDirectory;
            intervalNumeric.Value = mainForm.ClippingWaitInterval;
            
            // File tab
            if (mainForm.UseTimestampNaming)
            {
                timestampRadio.Checked = true;
            }
            else
            {
                customNameRadio.Checked = true;
            }
            
            prefixTextBox.Text = mainForm.FilenamePrefix;
            suffixTextBox.Text = mainForm.FilenameSuffix;
            sequenceNumeric.Value = mainForm.FilenameSequenceNumber;
            digitsNumeric.Value = mainForm.FilenameDigits;
            
            // Format tab
            formatComboBox.SelectedIndex = (int)mainForm.CurrentFileType;
            
            // Sound tab
            switch (mainForm.SoundMode)
            {
                case SoundMode.Beep:
                    beepRadio.Checked = true;
                    break;
                case SoundMode.DefaultWav:
                    defaultWavRadio.Checked = true;
                    break;
                case SoundMode.NoSound:
                    noSoundRadio.Checked = true;
                    break;
            }
            
            // Update UI state
            CustomDirRadio_CheckedChanged(null, EventArgs.Empty);
            NamingRadio_CheckedChanged(null, EventArgs.Empty);
            FormatComboBox_SelectedIndexChanged(null, EventArgs.Empty);
            UpdatePreview();
        }
        
        private void SaveSettings()
        {
            System.Diagnostics.Debug.WriteLine("=== SaveSettings called ===");
            
            // General tab
            mainForm.AutoStart = autoStartCheckBox.Checked;
            mainForm.ShowSaveDialog = showSaveDialogCheckBox.Checked;
            
            if (currentDirRadio.Checked)
            {
                mainForm.SaveLocationMode = SaveLocationMode.CurrentDirectory;
                System.Diagnostics.Debug.WriteLine("Save location: CurrentDirectory");
            }
            else if (myPicturesRadio.Checked)
            {
                mainForm.SaveLocationMode = SaveLocationMode.MyPictures;
                System.Diagnostics.Debug.WriteLine("Save location: MyPictures");
            }
            else if (customDirRadio.Checked)
            {
                mainForm.SaveLocationMode = SaveLocationMode.Custom;
                mainForm.CustomSaveDirectory = customDirTextBox.Text;
                System.Diagnostics.Debug.WriteLine($"Save location: Custom - {customDirTextBox.Text}");
            }
            
            mainForm.ClippingWaitInterval = (int)intervalNumeric.Value;
            
            // File tab
            mainForm.UseTimestampNaming = timestampRadio.Checked;
            mainForm.FilenamePrefix = prefixTextBox.Text;
            mainForm.FilenameSuffix = suffixTextBox.Text;
            mainForm.FilenameSequenceNumber = (int)sequenceNumeric.Value;
            mainForm.FilenameDigits = (int)digitsNumeric.Value;
            
            System.Diagnostics.Debug.WriteLine($"Filename settings - UseTimestamp: {mainForm.UseTimestampNaming}, Prefix: '{mainForm.FilenamePrefix}', Suffix: '{mainForm.FilenameSuffix}', Sequence: {mainForm.FilenameSequenceNumber}, Digits: {mainForm.FilenameDigits}");
            
            // Format tab
            mainForm.CurrentFileType = (FileType)formatComboBox.SelectedIndex;
            
            if (mainForm.CurrentFileType == FileType.JPEG)
            {
                mainForm.JpegQuality = (int)qualityNumeric.Value;
                mainForm.JpegGrayscale = grayscaleCheckBox.Checked;
                System.Diagnostics.Debug.WriteLine($"JPEG settings - Quality: {mainForm.JpegQuality}, Grayscale: {mainForm.JpegGrayscale}");
            }
            else if (mainForm.CurrentFileType == FileType.WEBP)
            {
                mainForm.WebpQuality = (int)qualityNumeric.Value;
                mainForm.WebpGrayscale = grayscaleCheckBox.Checked;
                System.Diagnostics.Debug.WriteLine($"WebP settings - Quality: {mainForm.WebpQuality}, Grayscale: {mainForm.WebpGrayscale}");
            }
            else if (mainForm.CurrentFileType == FileType.PNG)
            {
                mainForm.PngGrayscale = grayscaleCheckBox.Checked;
                System.Diagnostics.Debug.WriteLine($"PNG settings - Grayscale: {mainForm.PngGrayscale}");
            }
            
            // Sound tab
            if (beepRadio.Checked)
            {
                mainForm.SoundMode = SoundMode.Beep;
            }
            else if (defaultWavRadio.Checked)
            {
                mainForm.SoundMode = SoundMode.DefaultWav;
            }
            else if (noSoundRadio.Checked)
            {
                mainForm.SoundMode = SoundMode.NoSound;
            }
            
            System.Diagnostics.Debug.WriteLine($"Sound mode: {mainForm.SoundMode}");
            
            // Save to file
            mainForm.SaveSettings();
            System.Diagnostics.Debug.WriteLine("=== SaveSettings completed ===");
        }
        
        private void OkButton_Click(object? sender, EventArgs e)
        {
            SaveSettings();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        
        private void CancelButton_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}