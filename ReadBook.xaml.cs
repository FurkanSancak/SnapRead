using System.Text.RegularExpressions;
namespace SnapRead;

public partial class ReadBook : ContentPage
{
    #region Variables
    private int _cursor, _nc;
    private int _charCount;
    private int wordDisplayTime;
    private int fontSize, currentCharCount;
    private bool isReading = false;
    string[] words;
    string path, currentText;
    #endregion

    public ReadBook(Book book)
    {
        InitializeComponent();

        BaslikLabel.Text = book.Header;

        int cursor = 0;
        path = Path.Combine(FileSystem.AppDataDirectory, book.Header + ".cursor");

        if (File.Exists(path))
        {
            cursor = Convert.ToInt32(File.ReadAllText(path));
        }
        _cursor = cursor;

        #region Load Settings

        string settingsPath = Path.Combine(FileSystem.AppDataDirectory, "FastReadSettings.bin");

        if (File.Exists(settingsPath))
        {
            var settings = File.ReadAllLines(settingsPath);
            if (settings.Length == 3)
            {
                _charCount = int.Parse(settings[0]);
                wordDisplayTime = int.Parse(settings[1]);
                fontSize = int.Parse(settings[2]);
            }
        }
        else
        {
            _charCount = 4;
            wordDisplayTime = 350;
            fontSize = 25;
        }

        #endregion

        MetinLabel.FontSize = fontSize;

        words = book.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        /*string.Concat(
            _book.Text.Split('\n').Select(s => s.TrimEnd()).Aggregate("", (prev, curr) =>
            prev.EndsWith("-") ? prev.TrimEnd('-') + curr : prev + " " + curr).Trim())
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);*/

        #region First Text

        _nc = _cursor;

        do
        {
            currentText = string.Concat(currentText, " ", words[_nc]);
            currentCharCount += words[_nc].Length;
            _nc++;
            if (_nc >= words.Length)
            {
                _nc = 0;
                File.WriteAllText(path, "0");
                isReading = false;
                BaslikLabel.IsVisible = true;
                Buttons.IsVisible = true;
                break;
            }
        }
        while (currentCharCount < _charCount);

        MetinLabel.Text = currentText;

        CursorLabel.Text = words.Length.ToString();
        CursorEntry.Text = _cursor.ToString();

        #endregion
    }

    private async void Read()
    {
        /*int tempSpeed = wordDisplayTime * 3;

        while (tempSpeed > wordDisplayTime && isReading)
        {
            currentText = string.Empty;
            currentCharCount = 0;

            while (currentCharCount < _charCount)
            {
                currentText = string.Concat(currentText, " ", words[_cursor]);
                currentCharCount += words[_cursor].Length;
                _cursor++;
                if (_cursor >= words.Length)
                {
                    _cursor = 0;
                    File.WriteAllText(path, "0");
                    isReading = false;
                    BaslikLabel.IsVisible = true;
                    Buttons.IsVisible = true;
                    break;
                }
                File.WriteAllText(path, _cursor.ToString());
            }

            MetinLabel.Text = currentText;

            // Metni g sterme s resi
            await Task.Delay(tempSpeed);
            tempSpeed -= 50;
        }*/

        // Loop to read the book until it's finished
        while (isReading)
        {
            currentText = string.Empty;
            currentCharCount = 0;
            _cursor = _nc;
            File.WriteAllText(path, _cursor.ToString());

            do
            {
                currentText = string.Concat(currentText, " ", words[_nc]);
                currentCharCount += words[_nc].Length;
                _nc++;
                if (_nc >= words.Length)
                {
                    _nc = 0;
                    File.WriteAllText(path, "0");
                    isReading = false;
                    BaslikLabel.IsVisible = true;
                    Buttons.IsVisible = true;
                    break;
                }
            }
            while (currentCharCount < _charCount);

            MetinLabel.Text = currentText;

            await Task.Delay(wordDisplayTime);
        }
    }

    private void OnCursorEntry(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(e.NewTextValue, out int newCursor) && newCursor >= 0 && newCursor < words.Length)
        {
            _nc = newCursor;
            File.WriteAllText(path, _nc.ToString());
            currentText = string.Empty;

            do
            {
                currentText = string.Concat(currentText, " ", words[_nc]);
                currentCharCount += words[_nc].Length;
                _nc++;
                if (_nc >= words.Length)
                {
                    _nc = 0;
                    File.WriteAllText(path, "0");
                    isReading = false;
                    BaslikLabel.IsVisible = true;
                    Buttons.IsVisible = true;
                    break;
                }
            }
            while (currentCharCount < _charCount);

            MetinLabel.Text = currentText;
        }
    }

    private void OnTapped(object sender, TappedEventArgs e)
    {
        isReading = !isReading;

        if (isReading)
        {
            BaslikLabel.IsVisible = false;
            Buttons.IsVisible = false;

            Read();

            SettingsPanel.IsVisible = false;
        }
        else
        {
            CursorEntry.Text = _cursor.ToString();

            BaslikLabel.IsVisible = true;
            Buttons.IsVisible = true;
        }

    }

    #region Buttons

    private void Backward(object sender, EventArgs e)
    {
        currentText = string.Empty;
        currentCharCount = 0;
        _nc = _cursor;

        do
        {
            _cursor--;

            if (_cursor < 0)
            {
                _cursor = 0;
                File.WriteAllText(path, "0");
                isReading = false;
                BaslikLabel.IsVisible = true;
                Buttons.IsVisible = true;
                break;
            }

            currentText = string.Concat(words[_cursor], " ", currentText);
            currentCharCount += words[_cursor].Length;
        }
        while (currentCharCount < _charCount);

        File.WriteAllText(path, _cursor.ToString());

        MetinLabel.Text = currentText;
        CursorEntry.Text = _cursor.ToString();
    }

    private void Forward(object sender, EventArgs e)
    {
        currentText = string.Empty;
        currentCharCount = 0;
        _cursor = _nc;
        File.WriteAllText(path, _cursor.ToString());

        while (currentCharCount < _charCount)
        {
            currentText = string.Concat(currentText, " ", words[_nc]);
            currentCharCount += words[_nc].Length;
            _nc++;
            if (_nc >= words.Length)
            {
                _nc = 0;
                File.WriteAllText(path, "0");
                isReading = false;
                BaslikLabel.IsVisible = true;
                Buttons.IsVisible = true;
                break;
            }
        }

        MetinLabel.Text = currentText;
        CursorEntry.Text = _cursor.ToString();
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private void OpenSettingsPanel(object sender, EventArgs e)
    {
        CharCountEntry.Text = _charCount.ToString();
        WordDisplayTimeEntry.Text = wordDisplayTime.ToString();
        FontSizeEntry.Text = fontSize.ToString();

        SettingsPanel.IsVisible = !SettingsPanel.IsVisible;
        BaslikLabel.IsVisible = !SettingsPanel.IsVisible;
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        string _test = CharCountEntry.Text;
        if (!String.IsNullOrEmpty(_test) && Regex.Match(_test, @"^[0-9]+$").Success)
        {
            _charCount = int.Parse(_test);
        }

        _test = WordDisplayTimeEntry.Text;
        if (!String.IsNullOrEmpty(_test) && Regex.Match(_test, @"^[0-9]+$").Success)
        {
            wordDisplayTime = int.Parse(_test);
        }

        _test = FontSizeEntry.Text;
        if (!String.IsNullOrEmpty(_test) && Regex.Match(_test, @"^[0-9]+$").Success)
        {
            fontSize = int.Parse(_test);
            MetinLabel.FontSize = fontSize;
        }

        //Save Settings
        string settingsPath = Path.Combine(FileSystem.AppDataDirectory, "FastReadSettings.bin");
        string[] settings = new string[]
        {
            _charCount.ToString(),
            wordDisplayTime.ToString(),
            fontSize.ToString()
        };

        File.WriteAllLines(settingsPath, settings);
    }

    #endregion
}