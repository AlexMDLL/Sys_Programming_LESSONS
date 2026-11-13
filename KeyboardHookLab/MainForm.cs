namespace KeyboardHookLab;

// Скелет пары: окно с журналом клавиш. Пока только обычное событие KeyDown, без hook.
public sealed class MainForm : Form
{
    private readonly TextBox _logTextBox;

    public MainForm()
    {
        Text = "Keyboard Events";
        Width = 700;
        Height = 450;
        StartPosition = FormStartPosition.CenterScreen;

        // Форма получает клавиши раньше дочерних контролов.
        KeyPreview = true;

        _logTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10f)
        };

        Controls.Add(_logTextBox);

        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        _logTextBox.AppendText($"KeyDown: {e.KeyCode}{Environment.NewLine}");
    }
}
