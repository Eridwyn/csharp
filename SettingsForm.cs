#nullable enable
using System.Text.Json;
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
        backupSizeLimitNumericUpDown.Location = new System.Drawing.Point(20, 130);
        destinationTabPage.Controls.Add(backupSizeLimitNumericUpDown);

        string backupSizeLimitFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "backupSizeLimit.json");
        if (File.Exists(backupSizeLimitFilePath))
        {
            string backupSizeLimitJson = File.ReadAllText(backupSizeLimitFilePath);
            double backupSizeLimit = JsonSerializer.Deserialize<double>(backupSizeLimitJson);
            backupSizeLimitNumericUpDown.Value = (decimal)backupSizeLimit;
        }

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
            }

            string destinationPathFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "destinationPath.json");
            if (File.Exists(destinationPathFilePath))
            {
                string destinationPath = File.ReadAllText(destinationPathFilePath);
                destinationTextBox.Text = destinationPath;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des chemins: {ex}");
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
        Form1.maxBackupSizeInGB = (double)backupSizeLimitNumericUpDown.Value;
        string backupSizeLimitFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "backupSizeLimit.json");
        double backupSizeLimit = (double)backupSizeLimitNumericUpDown.Value;
        string backupSizeLimitJson = JsonSerializer.Serialize(backupSizeLimit);
        File.WriteAllText(backupSizeLimitFilePath, backupSizeLimitJson);

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

            string destinationPath = destinationTextBox.Text;
            string destinationPathFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "destinationPath.json");
            File.WriteAllText(destinationPathFilePath, destinationPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la sauvegarde des chemins: {ex}");
        }
    }
}
