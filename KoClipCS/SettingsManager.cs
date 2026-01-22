using System;
using System.IO;
using System.Text.Json;

namespace KoClipCS
{
    public static class SettingsManager
    {
        private static string? _customSettingsPath;
        
        private static string SettingsFilePath => _customSettingsPath ?? Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "KoClip.json"
        );
        
        // テスト用にカスタムパスを設定するメソッド
        public static void SetCustomSettingsPath(string? path)
        {
            _customSettingsPath = path;
        }

        public static void SaveSettings(MainForm mainForm)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Saving settings to: {SettingsFilePath} ===");
                
                var settings = new SettingsData
                {
                    SaveLocationMode = mainForm.SaveLocationMode,
                    CustomSaveDirectory = mainForm.CustomSaveDirectory,
                    ClippingWaitInterval = mainForm.ClippingWaitInterval,
                    AutoStart = mainForm.AutoStart,
                    ShowSaveDialog = mainForm.ShowSaveDialog,
                    CurrentFileType = mainForm.CurrentFileType,
                    JpegQuality = mainForm.JpegQuality,
                    JpegGrayscale = mainForm.JpegGrayscale,
                    PngGrayscale = mainForm.PngGrayscale,
                    WebpGrayscale = mainForm.WebpGrayscale,
                    WebpQuality = mainForm.WebpQuality,
                    UseTimestampNaming = mainForm.UseTimestampNaming,
                    FilenamePrefix = mainForm.FilenamePrefix,
                    FilenameSuffix = mainForm.FilenameSuffix,
                    FilenameSequenceNumber = mainForm.FilenameSequenceNumber,
                    FilenameDigits = mainForm.FilenameDigits,
                    SoundMode = mainForm.SoundMode
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonString = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsFilePath, jsonString);
                
                System.Diagnostics.Debug.WriteLine($"Settings saved successfully:");
                System.Diagnostics.Debug.WriteLine($"  SaveLocationMode: {settings.SaveLocationMode}");
                System.Diagnostics.Debug.WriteLine($"  CustomSaveDirectory: {settings.CustomSaveDirectory}");
                System.Diagnostics.Debug.WriteLine($"  CurrentFileType: {settings.CurrentFileType}");
                System.Diagnostics.Debug.WriteLine($"  UseTimestampNaming: {settings.UseTimestampNaming}");
                System.Diagnostics.Debug.WriteLine($"  FilenamePrefix: '{settings.FilenamePrefix}'");
                System.Diagnostics.Debug.WriteLine($"  FilenameSuffix: '{settings.FilenameSuffix}'");
                System.Diagnostics.Debug.WriteLine($"  FilenameSequenceNumber: {settings.FilenameSequenceNumber}");
                System.Diagnostics.Debug.WriteLine($"  FilenameDigits: {settings.FilenameDigits}");
                System.Diagnostics.Debug.WriteLine($"  SoundMode: {settings.SoundMode}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public static void LoadSettings(MainForm mainForm)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Loading settings from: {SettingsFilePath} ===");
                
                if (!File.Exists(SettingsFilePath))
                {
                    System.Diagnostics.Debug.WriteLine("Settings file not found, using defaults");
                    return;
                }

                string jsonString = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<SettingsData>(jsonString);

                if (settings != null)
                {
                    mainForm.SaveLocationMode = settings.SaveLocationMode;
                    mainForm.CustomSaveDirectory = settings.CustomSaveDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    mainForm.ClippingWaitInterval = settings.ClippingWaitInterval;
                    mainForm.AutoStart = settings.AutoStart;
                    mainForm.ShowSaveDialog = settings.ShowSaveDialog;
                    mainForm.CurrentFileType = settings.CurrentFileType;
                    mainForm.JpegQuality = settings.JpegQuality;
                    mainForm.JpegGrayscale = settings.JpegGrayscale;
                    mainForm.PngGrayscale = settings.PngGrayscale;
                    mainForm.WebpGrayscale = settings.WebpGrayscale;
                    mainForm.WebpQuality = settings.WebpQuality;
                    mainForm.UseTimestampNaming = settings.UseTimestampNaming;
                    mainForm.FilenamePrefix = settings.FilenamePrefix ?? "";
                    mainForm.FilenameSuffix = settings.FilenameSuffix ?? "";
                    mainForm.FilenameSequenceNumber = settings.FilenameSequenceNumber;
                    mainForm.FilenameDigits = settings.FilenameDigits;
                    mainForm.SoundMode = settings.SoundMode;
                    
                    System.Diagnostics.Debug.WriteLine($"Settings loaded successfully:");
                    System.Diagnostics.Debug.WriteLine($"  SaveLocationMode: {settings.SaveLocationMode}");
                    System.Diagnostics.Debug.WriteLine($"  CustomSaveDirectory: {settings.CustomSaveDirectory}");
                    System.Diagnostics.Debug.WriteLine($"  CurrentFileType: {settings.CurrentFileType}");
                    System.Diagnostics.Debug.WriteLine($"  UseTimestampNaming: {settings.UseTimestampNaming}");
                    System.Diagnostics.Debug.WriteLine($"  FilenamePrefix: '{settings.FilenamePrefix}'");
                    System.Diagnostics.Debug.WriteLine($"  FilenameSuffix: '{settings.FilenameSuffix}'");
                    System.Diagnostics.Debug.WriteLine($"  FilenameSequenceNumber: {settings.FilenameSequenceNumber}");
                    System.Diagnostics.Debug.WriteLine($"  FilenameDigits: {settings.FilenameDigits}");
                    System.Diagnostics.Debug.WriteLine($"  SoundMode: {settings.SoundMode}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }
    }

    public class SettingsData
    {
        public SaveLocationMode SaveLocationMode { get; set; } = SaveLocationMode.MyPictures;
        public string? CustomSaveDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        public int ClippingWaitInterval { get; set; } = 1000;
        public bool AutoStart { get; set; } = false;
        public bool ShowSaveDialog { get; set; } = false;
        public FileType CurrentFileType { get; set; } = FileType.PNG;
        public int JpegQuality { get; set; } = 100;
        public bool JpegGrayscale { get; set; } = false;
        public bool PngGrayscale { get; set; } = false;
        public bool WebpGrayscale { get; set; } = false;
        public int WebpQuality { get; set; } = 100;
        public bool UseTimestampNaming { get; set; } = true;
        public string? FilenamePrefix { get; set; } = "";
        public string? FilenameSuffix { get; set; } = "";
        public int FilenameSequenceNumber { get; set; } = 1;
        public int FilenameDigits { get; set; } = 3;
        public SoundMode SoundMode { get; set; } = SoundMode.DefaultWav;
    }
}