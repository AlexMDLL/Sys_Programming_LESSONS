namespace KeyboardHookLab;

// П1: ловим нажатие и отпускание, Esc чистит журнал.
public sealed class MainForm : Form
{
    private readonly TextBox _logTextBox;

    public MainForm()
    {
        Text = "Keyboard Events";
        Width = 700;
        Height = 450;
        StartPosition = FormStartPosition.CenterScreen;

        // Форма получает клавиши раньше дочерних контролов, иначе всё съест TextBox.
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
        KeyUp += OnKeyUp;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Esc не пишем, а чистим им журнал.
        if (e.KeyCode == Keys.Escape)
        {
            _logTextBox.Clear();
            AppendLog("System", "Журнал очищен (Esc)");

            // Чтоб Windows не пищала.
            e.SuppressKeyPress = true;
            e.Handled = true;
            return;
        }

        AppendLog("KeyDown", Describe(e));
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
            return;

        AppendLog("KeyUp", Describe(e));
    }

    // Модификаторы показываем отдельно: Alt-комбинации приходят с тем же KeyCode, но с флагом.
    private static string Describe(KeyEventArgs e) =>
        e.Modifiers == Keys.None
            ? e.KeyCode.ToString()
            : $"{e.KeyCode} ({e.Modifiers})";

    private void AppendLog(string source, string text)
    {
        _logTextBox.AppendText($"[{source}] {text}{Environment.NewLine}");
    }
}
