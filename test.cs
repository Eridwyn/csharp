using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

public class ProgressForm : Form
{
    private ProgressBar progressBar;
    private Label messageLabel;

    public ProgressForm()
    {
        this.Text = "Sauvegarde en cours";
        this.Size = new Size(400, 200);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimizeBox = true;

        progressBar = new ProgressBar();
        progressBar.Location = new Point(20, 60);
        progressBar.Size = new Size(360, 20);
        progressBar.Style = ProgressBarStyle.Continuous;
        progressBar.Minimum = 0;
        progressBar.Maximum = 100;
        this.Controls.Add(progressBar);

        messageLabel = new Label();
        messageLabel.Text = "Sauvegarde en cours...";
        messageLabel.AutoSize = true;
        messageLabel.Location = new Point(20, 100);
        messageLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        this.Controls.Add(messageLabel);

        Thread workerThread = new Thread(DoWork);
        workerThread.Start();
    }

    private void DoWork()
    {
        for (int i = 0; i <= 100; i += 10)
        {
            UpdateProgress(i);
            Thread.Sleep(1000);
        }
    }

    private void UpdateProgress(int value)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action<int>(UpdateProgress), new object[] { value });
            return;
        }

        progressBar.Value = value;
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new ProgressForm());
    }
}
