#nullable enable
using System.Text.Json;
using System.Diagnostics;
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
    private NumericUpDown backupSizeLimitNumericUpDown;
    private Label backupSizeLimitLabel;
    private NumericUpDown backupIntervalNumericUpDown;
    public SettingsForm()
    {
        this.Width = 600;
        this.Height = 400;
        this.FormClosing += SettingsForm_FormClosing;
        this.Icon = new Icon("maing.ico");
        this.Text = "Paramètres de sauvegarde";

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
        addSourceButton.Text = "Ajouter une source";
        addSourceButton.Location = new System.Drawing.Point(20, 240);
        addSourceButton.Click += new EventHandler(AddSourceButton_Click);
        sourceTabPage.Controls.Add(addSourceButton);

        removeSourceButton = new Button();
        removeSourceButton.Text = "Supprimer une source";
        removeSourceButton.Location = new System.Drawing.Point(120, 240);
        removeSourceButton.Click += new EventHandler(RemoveSourceButton_Click);
        sourceTabPage.Controls.Add(removeSourceButton);

        destinationTextBox = new TextBox();
        destinationTextBox.Width = 500;
        destinationTextBox.Location = new System.Drawing.Point(20, 20);
        destinationTabPage.Controls.Add(destinationTextBox);

        selectDestinationButton = new Button();
        selectDestinationButton.Text = "Sélectionner une destination";
        selectDestinationButton.Location = new System.Drawing.Point(20, 60);
        selectDestinationButton.Click += new EventHandler(SelectDestinationButton_Click);
        destinationTabPage.Controls.Add(selectDestinationButton);

        backupSizeLimitLabel = new Label();
        backupSizeLimitLabel.Text = "Taille max (Go):";
        backupSizeLimitLabel.Location = new System.Drawing.Point(20, 100);
        destinationTabPage.Controls.Add(backupSizeLimitLabel);

        backupSizeLimitNumericUpDown = new NumericUpDown();
        backupSizeLimitNumericUpDown.Minimum = 1;
        backupSizeLimitNumericUpDown.Maximum = 10000;  // Limite maximale de 10000 Go
        backupSizeLimitNumericUpDown.Value = (decimal)Form1.maxBackupSizeInGB;
        backupSizeLimitNumericUpDown.Location = new System.Drawing.Point(20, 120);
        destinationTabPage.Controls.Add(backupSizeLimitNumericUpDown);

        Label backupIntervalLabel = new Label();
        backupIntervalLabel.Text = "Interval (en heures):";
        backupIntervalLabel.AutoSize = true;
        backupIntervalLabel.Location = new System.Drawing.Point(20, 150); // Positionnez le label au-dessus du NumericUpDown
        destinationTabPage.Controls.Add(backupIntervalLabel);

        backupIntervalNumericUpDown = new NumericUpDown();
        backupIntervalNumericUpDown.Minimum = 1;
        backupIntervalNumericUpDown.Maximum = 24;  // L'utilisateur peut choisir jusqu'à 24 heures
        backupIntervalNumericUpDown.Value = 1;     // Par défaut, la sauvegarde est effectuée toutes les heures
        backupIntervalNumericUpDown.Location = new System.Drawing.Point(20, 170); // Changez ces valeurs pour positionner le NumericUpDown à l'endroit souhaité
        destinationTabPage.Controls.Add(backupIntervalNumericUpDown);

        string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".savetool", "config.json");
        if (File.Exists(configFilePath))
        {
            string configJson = File.ReadAllText(configFilePath);
            var config = JsonSerializer.Deserialize<Config>(configJson);
            
            if (config != null)
            {
                backupIntervalNumericUpDown.Value = (decimal)config.BackupInterval;
                backupSizeLimitNumericUpDown.Value = (decimal)config.BackupSizeLimit;
            
                foreach (string sourcePath in config.SourcePaths)
                {
                    sourceListBox.Items.Add(sourcePath);
                }
                destinationTextBox.Text = config.DestinationPath;
            }
            else
            {
                // Handle the case where config is null
                Trace.WriteLine($"[{DateTime.Now}]: Configuration file could not be parsed.");
                Trace.Flush();
                backupIntervalNumericUpDown.Value = 1; // Default interval of 1 hour
                backupSizeLimitNumericUpDown.Value = 20; 
            }
        }
        else
        {
            // Set default values for the controls
            backupIntervalNumericUpDown.Value = 1; // Default interval of 1 hour
            backupSizeLimitNumericUpDown.Value = 20; // Default backup size limit of 20 GB
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

    private void SettingsForm_FormClosing(object? Sender, FormClosingEventArgs e)
    {
        string configDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".savetool");
        if (!Directory.Exists(configDirectoryPath))
        {
            Directory.CreateDirectory(configDirectoryPath);
        }

        string configFilePath = Path.Combine(configDirectoryPath, "config.json");
        var config = new Config
        {
            BackupSizeLimit = (double)backupSizeLimitNumericUpDown.Value,
            BackupInterval = (double)backupIntervalNumericUpDown.Value,
            SourcePaths = new List<string>(),
            DestinationPath = destinationTextBox.Text
        };
        foreach (var item in sourceListBox.Items)
        {
            if (item != null)
            {
                config.SourcePaths.Add(item.ToString() ?? "");
            }
            else
            {
                Trace.WriteLine($"[{DateTime.Now}]: Probleme impossible a realiser");
            }
        }

        string configJson = JsonSerializer.Serialize(config);
        File.WriteAllText(configFilePath, configJson);
    }

}
