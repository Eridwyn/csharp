#nullable disable

public class CustomProgressBar : ProgressBar
{
    public CustomProgressBar()
    {
        this.SetStyle(ControlStyles.UserPaint, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Rectangle rect = this.ClientRectangle;
        if (ProgressBarRenderer.IsSupported)
            ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rect);
        rect.Inflate(-0, -0);
        if (this.Value > 0)
        {

            Rectangle clip = new Rectangle(rect.X, rect.Y, 
                (int)Math.Round(((float)this.Value / this.Maximum) * rect.Width), rect.Height);
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(13, 129, 48)), clip);
        }
    }
}

public partial class ProgressForm : Form
{
    private ProgressBar progressBar;
    private Label titleLabel;
    private Label progressLabel;

    public ProgressForm()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.FromArgb(31, 102, 52);
        this.TransparencyKey = Color.FromArgb(31, 102, 52);
        this.Size = new Size(200, 80);
        this.StartPosition = FormStartPosition.Manual;
        this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width - 10, Screen.PrimaryScreen.Bounds.Height - this.Height - 60);
        
        Panel panel = new Panel();
        panel.Dock = DockStyle.Fill;
        panel.Padding = new Padding(10);
        panel.BackColor = Color.FromArgb(12, 0, 17);
        this.Controls.Add(panel);
                
        progressLabel = new Label();
        progressLabel.Dock = DockStyle.Bottom;
        progressLabel.ForeColor = Color.White;
        progressLabel.TextAlign = ContentAlignment.MiddleCenter;
        panel.Controls.Add(progressLabel);

        titleLabel = new Label();
        titleLabel.Dock = DockStyle.Top;
        titleLabel.ForeColor = Color.White;
        titleLabel.Text = "Sauvegarde en cours";
        titleLabel.TextAlign = ContentAlignment.MiddleCenter;
        panel.Controls.Add(titleLabel);

        progressBar = new CustomProgressBar();
        progressBar.Dock = DockStyle.Fill;
        progressBar.Minimum = 0;
        progressBar.Maximum = 100;
        progressBar.Style = ProgressBarStyle.Continuous;
        progressBar.ForeColor = Color.FromArgb(31, 102, 52);
        progressBar.BackColor = Color.FromArgb(12, 0, 17);
        panel.Controls.Add(progressBar);

    }

    public void UpdateProgress(int percent, string text)
    {
        progressBar.Value = percent;
        progressLabel.Text = text;
    }
}
