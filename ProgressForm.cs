#nullable disable
public partial class ProgressForm : Form
{
    private ProgressBar progressBar;
    private Label titleLabel;
    private Label progressLabel;

    public ProgressForm()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.LimeGreen;
        this.TransparencyKey = Color.LimeGreen;
        this.Size = new Size(200, 80);
        this.StartPosition = FormStartPosition.Manual;
        this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width - 10, Screen.PrimaryScreen.Bounds.Height - this.Height - 60);
        
        Panel panel = new Panel();
        panel.Dock = DockStyle.Fill;
        panel.Padding = new Padding(10);
        panel.BackColor = Color.Black;
        this.Controls.Add(panel);

        titleLabel = new Label();
        titleLabel.Dock = DockStyle.Top;
        titleLabel.ForeColor = Color.White;
        titleLabel.Text = "Sauvegarde en cours";
        titleLabel.TextAlign = ContentAlignment.MiddleCenter;
        panel.Controls.Add(titleLabel);

        progressBar = new ProgressBar();
        progressBar.Dock = DockStyle.Fill;
        progressBar.Minimum = 0;
        progressBar.Maximum = 100;
        progressBar.Style = ProgressBarStyle.Continuous;
        progressBar.ForeColor = Color.LimeGreen;
        progressBar.BackColor = Color.DarkGray;
        panel.Controls.Add(progressBar);

        progressLabel = new Label();
        progressLabel.Dock = DockStyle.Bottom;
        progressLabel.ForeColor = Color.White;
        progressLabel.TextAlign = ContentAlignment.MiddleCenter;
        panel.Controls.Add(progressLabel);
    }

    public void UpdateProgress(int percent, string text)
    {
        progressBar.Value = percent;
        progressLabel.Text = text;
    }
}
