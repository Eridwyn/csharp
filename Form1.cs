using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Diagnostics;

public partial class Form1 : Form
{
    private NotifyIcon notifyIcon;
    private ContextMenuStrip contextMenuStrip;
    private ToolStripMenuItem exitToolStripMenuItem;
    private ToolStripMenuItem settingsToolStripMenuItem;

    public Form1()
    {
        notifyIcon = new NotifyIcon();
        contextMenuStrip = new ContextMenuStrip();
        exitToolStripMenuItem = new ToolStripMenuItem();
        settingsToolStripMenuItem = new ToolStripMenuItem();

        exitToolStripMenuItem.Text = "Exit";
        exitToolStripMenuItem.Click += new EventHandler(ExitToolStripMenuItem_Click);

        settingsToolStripMenuItem.Text = "Settings";
        settingsToolStripMenuItem.Click += new EventHandler(SettingsToolStripMenuItem_Click);

        contextMenuStrip.Items.Add(settingsToolStripMenuItem);
        contextMenuStrip.Items.Add(exitToolStripMenuItem);
        
        notifyIcon.Icon = SystemIcons.Application;
        notifyIcon.ContextMenuStrip = contextMenuStrip;
        notifyIcon.Visible = true;

        this.WindowState = FormWindowState.Minimized;
        this.ShowInTaskbar = false;
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
}

public partial class SettingsForm : Form
{
    private TabControl tabControl;
    private TabPage sourceTabPage;
    private TabPage destinationTabPage;
    private ListBox sourceListBox;
    private Button addSourceButton;
    private Button removeSourceButton;
    private TextBox destinationTextBox;
    private Button selectDestinationButton;

    public SettingsForm()
    {
        this.Width = 600;
        this.Height = 400;
        this.FormClosing += SettingsForm_FormClosing;

        tabControl = new TabControl();
        tabControl.Dock = DockStyle.Fill;
        this.Controls.Add(tabControl);

        sourceTabPage = new TabPage("Source");
        destinationTabPage = new TabPage("Destination");
        tabControl.TabPages.Add(sourceTabPage);
        tabControl.TabPages.Add(destinationTabPage);

        sourceListBox = new ListBox();
        sourceListBox.Width = 500;
        sourceListBox.Height = 200;
        sourceListBox.Location = new System.Drawing.Point(20, 20);
        sourceTabPage.Controls.Add(sourceListBox);

        addSourceButton = new Button();
        addSourceButton.Text = "Add Source";
        addSourceButton.Location = new System.Drawing.Point(20, 240);
        addSourceButton.Click += new EventHandler(AddSourceButton_Click);
        sourceTabPage.Controls.Add(addSourceButton);

        removeSourceButton = new Button();
        removeSourceButton.Text = "Remove Source";
        removeSourceButton.Location = new System.Drawing.Point(120, 240);
        removeSourceButton.Click += new EventHandler(RemoveSourceButton_Click);
        sourceTabPage.Controls.Add(removeSourceButton);

        destinationTextBox = new TextBox();
        destinationTextBox.Width = 500;
        destinationTextBox.Location = new System.Drawing.Point(20, 20);
        destinationTabPage.Controls.Add(destinationTextBox);

        selectDestinationButton = new Button();
        selectDestinationButton.Text = "Select Destination";
        selectDestinationButton.Location = new System.Drawing.Point(20, 60);
        selectDestinationButton.Click += new EventHandler(SelectDestinationButton_Click);
        destinationTabPage.Controls.Add(selectDestinationButton);

        try
        {
            string sourcePathsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "sourcePaths.json");
            if (File.Exists(sourcePathsFilePath))
            {
                string sourcePathsJson = File.ReadAllText(sourcePathsFilePath);
                List<string> sourcePaths = JsonSerializer.Deserialize<List<string>>(sourcePathsJson);
                foreach (string sourcePath in sourcePaths)
                {
                    sourceListBox.Items.Add(sourcePath);
                }
                //MessageBox.Show($"Source paths loaded from {sourcePathsFilePath}");
            }

            string destinationPathFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "destinationPath.txt");
            if (File.Exists(destinationPathFilePath))
            {
                string destinationPath = File.ReadAllText(destinationPathFilePath);
                destinationTextBox.Text = destinationPath;
                //MessageBox.Show($"Destination path loaded from {destinationPathFilePath}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while loading paths: {ex}");
        }
    }

    private void AddSourceButton_Click(object? Sender, EventArgs e)
    {
        using (var fbd = new FolderBrowserDialog())
        {
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                sourceListBox.Items.Add(fbd.SelectedPath);
            }
        }
    }

    private void RemoveSourceButton_Click(object? Sender, EventArgs e)
    {
        if (sourceListBox.SelectedItem != null)
        {
            sourceListBox.Items.Remove(sourceListBox.SelectedItem);
        }
    }

    private void SelectDestinationButton_Click(object? Sender, EventArgs e)
    {
        using (var fbd = new FolderBrowserDialog())
        {
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                destinationTextBox.Text = fbd.SelectedPath;
            }
        }
    }

    private void SettingsForm_FormClosing(object Sender, FormClosingEventArgs e)
    {
        try
        {
            List<string> sourcePaths = new List<string>();
            foreach (var item in sourceListBox.Items)
            {
                sourcePaths.Add(item.ToString());
            }
            string sourcePathsJson = JsonSerializer.Serialize(sourcePaths);
            string sourcePathsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "sourcePaths.json");
            File.WriteAllText(sourcePathsFilePath, sourcePathsJson);
            //MessageBox.Show($"Source paths saved to {sourcePathsFilePath}");

            string destinationPath = destinationTextBox.Text;
            string destinationPathFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "destinationPath.json");
            File.WriteAllText(destinationPathFilePath, destinationPath);
            //MessageBox.Show($"Destination path saved to {destinationPathFilePath}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while saving paths: {ex}");
        }
    }


}
