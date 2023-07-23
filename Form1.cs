#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel; 

public class Config
{
    public double BackupSizeLimit { get; set; }
    public double BackupInterval { get; set; }
    public required List<string> SourcePaths { get; set; }
    public required string DestinationPath { get; set; }
}

public class CustomToolStripRenderer : ToolStripProfessionalRenderer
{
    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (e.Item.Selected)
        {
            Rectangle rc = new Rectangle(Point.Empty, e.Item.Size);
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(31, 102, 52)), rc);
            e.Graphics.DrawRectangle(new Pen(Color.FromArgb(12, 0, 17)), 1, 0, rc.Width - 2, rc.Height - 1);
        }
        else
        {
            base.OnRenderMenuItemBackground(e);
        }
    }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        base.OnRenderToolStripBackground(e);
        e.Graphics.Clear(Color.FromArgb(12, 0, 17));
    }
}

public partial class Form1 : Form
{
    private NotifyIcon notifyIcon;
    private ContextMenuStrip contextMenuStrip;
    private ToolStripMenuItem exitToolStripMenuItem;
    private ToolStripMenuItem settingsToolStripMenuItem;
    private ToolStripMenuItem backupToolStripMenuItem;
    private System.Timers.Timer backupTimer;
    private ToolStripMenuItem ToolStripMenuLogItem;
    public static double maxBackupSizeInGB = 20.0;  // Limite de la sauvegarde à 20 Go
    private BackgroundWorker backupWorker = new BackgroundWorker();


    public Form1()
    {
        CreateDefaultConfigFileIfNotExists();
        
        notifyIcon = new NotifyIcon();
        notifyIcon.Icon = new System.Drawing.Icon("maing.ico"); // Set the application icon
        notifyIcon.Text = "Logiciel de sauvegarde";
        contextMenuStrip = new ContextMenuStrip();
        exitToolStripMenuItem = new ToolStripMenuItem();
        settingsToolStripMenuItem = new ToolStripMenuItem();
        backupToolStripMenuItem = new ToolStripMenuItem();
        ToolStripMenuLogItem = new ToolStripMenuItem();
        contextMenuStrip.Renderer = new CustomToolStripRenderer();
        contextMenuStrip.ShowImageMargin = false;

        ToolStripMenuLogItem.Text = "Afficher les logs";
        ToolStripMenuLogItem.Click += new EventHandler(ShowLogButton_Click);
        ToolStripMenuLogItem.ForeColor = Color.White;

        exitToolStripMenuItem.Text = "Quitter";
        exitToolStripMenuItem.Click += new EventHandler(ExitToolStripMenuItem_Click);
        exitToolStripMenuItem.ForeColor = Color.White;

        settingsToolStripMenuItem.Text = "Paramètres";
        settingsToolStripMenuItem.Click += new EventHandler(SettingsToolStripMenuItem_Click);
        settingsToolStripMenuItem.ForeColor = Color.White;

        backupToolStripMenuItem.Text = "Sauvegarder maintenant";
        backupToolStripMenuItem.Click += new EventHandler(BackupToolStripMenuItem_Click);
        backupToolStripMenuItem.ForeColor = Color.White;

        contextMenuStrip.Items.Add(backupToolStripMenuItem);
        contextMenuStrip.Items.Add(settingsToolStripMenuItem);
        contextMenuStrip.Items.Add(ToolStripMenuLogItem);
        contextMenuStrip.Items.Add(exitToolStripMenuItem);
        

        notifyIcon.ContextMenuStrip = contextMenuStrip;
        notifyIcon.Visible = true;

        backupTimer = new System.Timers.Timer();
        backupTimer.Elapsed += OnBackupTimerElapsed;

        string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".savetool", "config.json");
        if (File.Exists(configFilePath))
        {
            string configJson = File.ReadAllText(configFilePath);
            var config = JsonSerializer.Deserialize<Config>(configJson);
            if (config != null)
            {
                backupTimer.Interval = config.BackupInterval * 3600000;  // Convert hours to milliseconds
                maxBackupSizeInGB = config.BackupSizeLimit;
            }
            else
            {
                Trace.WriteLine($"[{DateTime.Now}]: Problemes de configuration null");

            }
        }
        backupTimer.Start();

        this.WindowState = FormWindowState.Minimized;
        this.ShowInTaskbar = false;
    }
    private void ShowLogButton_Click(object? sender, EventArgs e)
    {
        try
        {
            string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".savetool", "logfile.txt");

            if (File.Exists(logFilePath))
            {
                string logContent;

                try
                {
                    using (var stream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(stream))
                    {
                    logContent = reader.ReadToEnd();
                    }
                }
                catch (IOException ex)
                {
                    logContent = "Erreur lors de la lecture du fichier de log: " + ex.Message;
                }

                Form logForm = new Form();
                logForm.Icon = new Icon("maing.ico");
                logForm.Text = "Logs";
                logForm.FormBorderStyle = FormBorderStyle.Sizable;

                TextBox logTextBox = new TextBox();
                logTextBox.Multiline = true;
                logTextBox.Dock = DockStyle.Fill;
                logTextBox.ScrollBars = ScrollBars.Vertical;
                logTextBox.Width = 600;
                logTextBox.Height = 400;
                logTextBox.Text = logContent;
                logForm.Controls.Add(logTextBox);

                Panel borderPanel = new Panel();
                borderPanel.Dock = DockStyle.Fill;
                borderPanel.Padding = new Padding(1);
                borderPanel.Controls.Add(logTextBox);
                logForm.Controls.Add(borderPanel);

                logForm.Show();

                

            }
            else
            {
                MessageBox.Show("Fichier de log non trouvé.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[{DateTime.Now}]: Exception caught: " + ex.Message);
        }
    }

    private void OnBackupTimerElapsed(object? Sender, System.Timers.ElapsedEventArgs e)
    {
        this.Invoke((MethodInvoker)delegate
        {
            BackupToolStripMenuItem_Click(null, null);
        });
    }
    private void ExitToolStripMenuItem_Click(object? Sender, EventArgs e)
    {
        Trace.WriteLine($"[{DateTime.Now}]: Application fermée.");
        notifyIcon.Visible = false;
        Application.Exit();
    }

    private void SettingsToolStripMenuItem_Click(object? Sender, EventArgs e)
    {
        SettingsForm settingsForm = new SettingsForm();
        settingsForm.Show();
    }

    private void BackupToolStripMenuItem_Click(object? Sender, EventArgs? e)
    {
        if (backupWorker.IsBusy)
        {
            // Une sauvegarde est déjà en cours, ne rien faire.
            return;
        }
        Trace.WriteLine($"[{DateTime.Now}]: Initiallisation de la Sauvegarde.");
        ProgressForm progressForm = new ProgressForm();
        progressForm.Show();

        backupWorker = new BackgroundWorker();
        backupWorker.WorkerReportsProgress = true;

        backupWorker.DoWork += (s, args) =>
        {
            string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".savetool", "config.json");

            if (File.Exists(configFilePath))
            {
                string configJson = File.ReadAllText(configFilePath);
                var config = JsonSerializer.Deserialize<Config>(configJson);

                if (config != null)
                {
                    int totalFiles = config.SourcePaths.Sum(path => Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Count());

                    int processedFiles = 0;
                    foreach (string sourcePath in config.SourcePaths)
                    {
                        PerformDifferentialBackup(sourcePath, config.DestinationPath, backupWorker, totalFiles, ref processedFiles);
                    }
                }
                else
                {
                    Trace.WriteLine($"[{DateTime.Now}]: Impossible de verifier les fichiers");
                }
            }
        };

        backupWorker.ProgressChanged += (s, args) =>
        {
            progressForm.UpdateProgress(args.ProgressPercentage, $"Sauvegarde en cours : {args.ProgressPercentage}% terminé");
        };

        backupWorker.RunWorkerCompleted += (s, args) =>
        {
            progressForm.Close();
            notifyIcon.ShowBalloonTip(3000, "Sauvegarde terminée", "Sauvegarde effectuée avec succès.", ToolTipIcon.Info);
            Trace.WriteLine($"[{DateTime.Now}]: Sauvegarde effectuée avec succès.");

            // Réactiver le timer pour la sauvegarde automatique
            backupTimer.Start();
        };

        backupWorker.RunWorkerAsync();

        // Arrêter le timer pour la sauvegarde automatique pendant la sauvegarde manuelle
        backupTimer.Stop();
    }

    private void PerformDifferentialBackup(string sourceDirectory, string destinationDirectory, BackgroundWorker backupWorker, int totalFiles, ref int processedFiles)
    {
        try
        {

            string? driveName = Path.GetPathRoot(destinationDirectory);
    
            if (string.IsNullOrEmpty(driveName))
            {
                // Handle the case where driveName is null or empty
                Trace.WriteLine($"[{DateTime.Now}]: Destination est null ou vide.");
                return;
            }
            DriveInfo drive = new DriveInfo(driveName!);
            
            double freeSpaceInGB = drive.AvailableFreeSpace / Math.Pow(1024, 3);

            // Get the total size of source directory
            double sourceSizeInGB = GetDirectorySizeInGB(sourceDirectory);

            if (freeSpaceInGB < sourceSizeInGB || sourceSizeInGB > maxBackupSizeInGB)
            {
                MessageBox.Show("Espace disque insuffisant pour la sauvegarde. Veuillez libérer de l'espace et réessayer.");
                Trace.WriteLine($"[{DateTime.Now}]: Espace disque insuffisant pour la sauvegarde. Veuillez libérer de l'espace et réessayer.");
                return;
            }

            string lastFullBackupFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "lastFullBackup.txt");
            DateTime lastFullBackupTime;

            if (File.Exists(lastFullBackupFilePath))
            {
                string lastFullBackupTimeString = File.ReadAllText(lastFullBackupFilePath);
                lastFullBackupTime = DateTime.Parse(lastFullBackupTimeString);
            }
            else
            {
                lastFullBackupTime = DateTime.MinValue;
            }

            string userName = Environment.UserName;
            destinationDirectory = Path.Combine(destinationDirectory, userName, new DirectoryInfo(sourceDirectory).Name);

             foreach (var sourceFilePath in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string relativePath = sourceFilePath.Substring(sourceDirectory.Length + 1);
                string destinationFilePath = Path.Combine(destinationDirectory, relativePath);
                FileInfo sourceFileInfo = new FileInfo(sourceFilePath);
                FileInfo destinationFileInfo = new FileInfo(destinationFilePath);

                if (!destinationFileInfo.Exists || sourceFileInfo.LastWriteTime > destinationFileInfo.LastWriteTime)
                {
                    string? destinationDirectoryPath = Path.GetDirectoryName(destinationFilePath);
                    if (!string.IsNullOrEmpty(destinationDirectoryPath))
                    {
                        Directory.CreateDirectory(destinationDirectoryPath);
                        File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
                    }
                    else
                    {
                        Trace.WriteLine($"[{DateTime.Now}]: Invalid directory path for destination file: {destinationFilePath}");
                    }
                }
                processedFiles++;
                int progressPercentage = processedFiles * 100 / totalFiles;
                backupWorker.ReportProgress(progressPercentage);
            }

            File.WriteAllText(lastFullBackupFilePath, DateTime.Now.ToString());
        }
        catch (IOException ex)
        {
            Trace.WriteLine($"[{DateTime.Now}]: I/O Error during backup: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            // Handle permission errors
            Trace.WriteLine($"[{DateTime.Now}]: Permission Error during backup: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Handle any other errors
           Trace.WriteLine($"[{DateTime.Now}]: Unexpected error during backup: {ex.Message}");

        }
    }

    private double GetDirectorySizeInGB(string directoryPath)
    {
        DirectoryInfo di = new DirectoryInfo(directoryPath);
        long sizeInBytes = di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        return sizeInBytes / Math.Pow(1024, 3);
    }
    private void CreateDefaultConfigFileIfNotExists()
{
    string configDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".savetool");
    if (!Directory.Exists(configDirectoryPath))
    {
        Directory.CreateDirectory(configDirectoryPath);
    }

    string configFilePath = Path.Combine(configDirectoryPath, "config.json");
    if (!File.Exists(configFilePath))
    {
        // Le fichier de configuration n'existe pas, créons-en un avec les valeurs par défaut
        var defaultConfig = new Config
        {
            BackupSizeLimit = 20.0, // Default backup size limit of 20 GB
            BackupInterval = 1.0,   // Default backup interval of 1 hour
            SourcePaths = new List<string>(),
            DestinationPath = ""    // Default destination path is empty
        };

        string configJson = JsonSerializer.Serialize(defaultConfig);
        File.WriteAllText(configFilePath, configJson);
    }
}

}