using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using KeyboardHookLab.Interop;

namespace KeyboardHookLab;

// П2: временной журнал, кнопка cleara и low-level hook на время активности.
public sealed class MainForm : Form
{
    private readonly TextBox _logTextBox;
    private readonly CheckBox _hookCheckBox;
    private readonly Button _clearButton;
    private readonly Label _statusLabel;
    private readonly Label _counterLabel;
    private readonly KeyboardHook _keyboardHook;

    private int _eventCount;

    public MainForm()
    {
        Text = "Keyboard Hook Laboratory";
        Width = 850;
        Height = 520;
        StartPosition = FormStartPosition.CenterScreen;

        // сначала форма, потом TextBox, чтобы невтыкал маленький
        KeyPreview = true;

        _keyboardHook = new KeyboardHook();
        _keyboardHook.KeyDown += OnHookKeyDown;

        _logTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10f)
        };

        _hookCheckBox = new CheckBox
        {
            Text = "Low-level hook",
            AutoSize = true,
            Margin = new Padding(8, 10, 12, 0)
        };

        _clearButton = new Button
        {
            Text = "Очистить журнал",
            AutoSize = true,
            Margin = new Padding(0, 4, 12, 0)
        };

        _statusLabel = new Label
        {
            Text = "Hook не установлен",
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 0)
        };

        _counterLabel = new Label
        {
            Text = "Событий: 0",
            AutoSize = true,
            Margin = new Padding(8, 4, 0, 0)
        };

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false,
            Padding = new Padding(4)
        };
        toolbar.Controls.Add(_hookCheckBox);
        toolbar.Controls.Add(_clearButton);
        toolbar.Controls.Add(_statusLabel);

        var bottomBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false,
            Padding = new Padding(4)
        };
        bottomBar.Controls.Add(_counterLabel);
        // Фиксируем сетку: тулбарчик, журнальчик, счетчик с докером чтобы мозги не еб**(пудрил)
        // Фиксированная сетка: тулбар, журнал, счётчик — без сюрпризов с порядком Dock.
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(_logTextBox, 0, 1);
        layout.Controls.Add(bottomBar, 0, 2);

        Controls.Add(layout);

        KeyDown += OnLocalKeyDown;
        KeyUp += OnLocalKeyUp;

        _clearButton.Click += (_, _) => ClearLog();
        _hookCheckBox.CheckedChanged += OnHookCheckedChanged;

        Activated += OnFormActivated;
        Deactivate += OnFormDeactivated;
        FormClosing += OnFormClosing;
    }

    private void OnLocalKeyDown(object? sender, KeyEventArgs e)
    {
        // Esc по прежнему чистит журнал
        if (e.KeyCode == Keys.Escape)
        {
            ClearLog();

            // Чтоб Windows не пищала
            e.SuppressKeyPress = true;
            e.Handled = true;
            return;
        }

        AppendKeyEvent("WinForms", "↓", Describe(e));
    }

    private void OnLocalKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
            return;

        AppendKeyEvent("WinForms", "↑", Describe(e));
    }

    private void OnHookKeyDown(HookKeyEvent e)
    {
        // Callback идёт из системного хука, UI трогаем только через очередь формы.
        if (IsDisposed || !IsHandleCreated)
            return;

        try
        {
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed)
                    return;

                string key = e.Key.ToString();

                // WM_SYSKEYDOWN приходит на Alt-комбинации.
                if (e.IsSystemKey)
                    key += " (SYS)";

                AppendKeyEvent("Hook", "↓", key);
            }));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
        {
            // Окно закрывается — писать некуда, это норма 
            Debug.WriteLine($"Не удалось обновить журнал: {ex}");
        }
    }

    private void OnHookCheckedChanged(object? sender, EventArgs e)
    {
        if (_hookCheckBox.Checked)
        {
            // Ставим, только если окно активно: иначе ловили бы чужие нажатия
            if (Form.ActiveForm == this)
                StartHook();
            else
                _hookCheckBox.Checked = false;
        }
        else
        {
            StopHook();
        }
    }

    private void OnFormActivated(object? sender, EventArgs e)
    {
        if (_hookCheckBox.Checked)
            StartHook();
    }

    private void OnFormDeactivated(object? sender, EventArgs e) { StopHook(); }

    private void StartHook()
    {
        if (_keyboardHook.IsInstalled)
            return;

        try
        {
            _keyboardHook.Start();

            _statusLabel.Text = "Hook установлен";
            AppendSystem("Hook установлен");
        }
        catch (Win32Exception ex)
        {
            _statusLabel.Text = "Ошибка установки hook";
            AppendSystem($"Не удалось установить hook: {ex.Message}");

            // Переключатель возвращаем в реальное состояние иначе хана
            _hookCheckBox.Checked = false;
        }
    }

    private void StopHook()
    {
        if (!_keyboardHook.IsInstalled)
            return;

        try
        {
            _keyboardHook.Stop();

            _statusLabel.Text = "Hook не установлен";
            AppendSystem("Hook снят");
        }
        catch (Win32Exception ex)
        {
            _statusLabel.Text = "Ошибка снятия hook";
            AppendSystem($"Не удалось снять hook: {ex.Message}");
        }
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        // Пока закрываемся, переключатель не трогаем иначе винду снесу нафиг
        // Directory.Delete("C:\\Windows\\System32", true) я знаю что это не ворк но я сделаю это ворк
        _hookCheckBox.Enabled = false;

        StopHook();
        _keyboardHook.Dispose();
    }

    private void ClearLog()
    {
        _logTextBox.Clear();
        _eventCount = 0;
        _counterLabel.Text = "Событий: 0";

        AppendSystem("Журнал очищен");
    }

    // Модификаторы показываем отдельно: Altы приходят с тем же KeyCodeом и с флагом.
    private static string Describe(KeyEventArgs e) =>
        e.Modifiers == Keys.None
            ? e.KeyCode.ToString()
            : $"{e.KeyCode} ({e.Modifiers})";

    private void AppendKeyEvent(string source, string arrow, string key)
    {
        AppendLine($"[{source}] {arrow} {key}");

        _eventCount++;
        _counterLabel.Text = $"Событий: {_eventCount}";
    }

    private void AppendSystem(string text) => AppendLine($"[System] {text}");

    private void AppendLine(string text)
    {
        string time = DateTime.Now.ToString("HH:mm:ss.fff");

        _logTextBox.AppendText($"[{time}] {text}{Environment.NewLine}");
    }
}
