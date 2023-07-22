#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel; 

//TODO meilleurs gestions de fichiers de sauvegardes
public class Config
{
    public double BackupSizeLimit { get; set; }
    public double BackupInterval { get; set; }
    public List<string> SourcePaths { get; set; }
    public string DestinationPath { get; set; }
}

public partial class Form1 : Form
{
    private NotifyIcon notifyIcon;
    private ContextMenuStrip contextMenuStrip;
    private ToolStripMenuItem exitToolStripMenuItem;
    private ToolStripMenuItem settingsToolStripMenuItem;
    private ToolStripMenuItem backupToolStripMenuItem;
    private System.Timers.Timer backupTimer;
    public static double maxBackupSizeInGB = 20.0;  // Limite de la sauvegarde à 20 Go


    public Form1()
    {
        notifyIcon = new NotifyIcon();
        notifyIcon.Icon = new System.Drawing.Icon("maing.ico"); // Set the application icon
        notifyIcon.Text = "Logiciel de sauvegarde";
        contextMenuStrip = new ContextMenuStrip();
        exitToolStripMenuItem = new ToolStripMenuItem();
        settingsToolStripMenuItem = new ToolStripMenuItem();
        backupToolStripMenuItem = new ToolStripMenuItem();

        exitToolStripMenuItem.Text = "Quitter";
        exitToolStripMenuItem.Click += new EventHandler(ExitToolStripMenuItem_Click);

        settingsToolStripMenuItem.Text = "Paramètres";
        settingsToolStripMenuItem.Click += new EventHandler(SettingsToolStripMenuItem_Click);

        backupToolStripMenuItem.Text = "Sauvegarder maintenant";
        backupToolStripMenuItem.Click += new EventHandler(BackupToolStripMenuItem_Click);

        contextMenuStrip.Items.Add(backupToolStripMenuItem);
        contextMenuStrip.Items.Add(settingsToolStripMenuItem);
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
            backupTimer.Interval = config.BackupInterval * 3600000;  // Convert hours to milliseconds
            maxBackupSizeInGB = config.BackupSizeLimit;
        }

        backupTimer.Start();

        this.WindowState = FormWindowState.Minimized;
        this.ShowInTaskbar = false;
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
        notifyIcon.Visible = false;
        Application.Exit();
    }

    private void SettingsToolStripMenuItem_Click(object? Sender, EventArgs e)
    {
        SettingsForm settingsForm = new SettingsForm();
        settingsForm.Show();
    }

    private void BackupToolStripMenuItem_Click(object? Sender, EventArgs e)
    {
        ProgressForm progressForm = new ProgressForm();
        progressForm.Show();

        BackgroundWorker backupWorker = new BackgroundWorker();
        backupWorker.WorkerReportsProgress = true;

        backupWorker.DoWork += (s, args) =>
        {
            string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".savetool", "config.json");

            if (File.Exists(configFilePath))
            {
                string configJson = File.ReadAllText(configFilePath);
                var config = JsonSerializer.Deserialize<Config>(configJson);

                int totalFiles = config.SourcePaths.Sum(path => Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Count());

                int processedFiles = 0;
                foreach (string sourcePath in config.SourcePaths)
                {
                    PerformDifferentialBackup(sourcePath, config.DestinationPath, backupWorker, totalFiles, ref processedFiles);
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
        };

        backupWorker.RunWorkerAsync();
    }

    private void PerformDifferentialBackup(string sourceDirectory, string destinationDirectory, BackgroundWorker backupWorker, int totalFiles, ref int processedFiles)
    {
        DriveInfo drive = new DriveInfo(Path.GetPathRoot(destinationDirectory));
        
        double freeSpaceInGB = drive.AvailableFreeSpace / Math.Pow(1024, 3);

        // Get the total size of source directory
        double sourceSizeInGB = GetDirectorySizeInGB(sourceDirectory);

        if (freeSpaceInGB < sourceSizeInGB || sourceSizeInGB > maxBackupSizeInGB)
        {
            MessageBox.Show("Espace disque insuffisant pour la sauvegarde. Veuillez libérer de l'espace et réessayer.");
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

            if (!File.Exists(destinationFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));
                File.Copy(sourceFilePath, destinationFilePath);
            }
            else
            {
                FileInfo sourceFileInfo = new FileInfo(sourceFilePath);
                if (sourceFileInfo.LastWriteTime > lastFullBackupTime)
                {
                    File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
                }
            }

            processedFiles++;
            int progressPercentage = processedFiles * 100 / totalFiles;
            backupWorker.ReportProgress(progressPercentage);
        }

        File.WriteAllText(lastFullBackupFilePath, DateTime.Now.ToString());
    }

    private double GetDirectorySizeInGB(string directoryPath)
    {
        DirectoryInfo di = new DirectoryInfo(directoryPath);
        long sizeInBytes = di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        return sizeInBytes / Math.Pow(1024, 3);
    }
}