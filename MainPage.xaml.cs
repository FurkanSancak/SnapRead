using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace SnapRead
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<string> Files { get; set; }
        public ICommand OpenBookCommand { get; }
        public ICommand DeleteCommand { get; }

        public MainPage()
        {
            InitializeComponent();

            if (Preferences.Get("DarkMode", false))
            {
                ModeButton.Text = "🌙";
                ModeButton.BackgroundColor = Colors.Navy;

            }
            else
            {
                ModeButton.Text = "☀️";
                ModeButton.BackgroundColor = Colors.Cyan;
            }


            Files = new ObservableCollection<string>();

            // CollectionView'e bağlama
            BindingContext = this;

            OpenBookCommand = new Command<string>(async (fileName) => await OpenBook(fileName));
            DeleteCommand = new Command<string>(async (fileName) => await Delete(fileName));

            FileList.ItemsSource = Files;

            #region Dosyaları Listele

            string path = FileSystem.AppDataDirectory; // Cihazdaki veri klasörü

            // Eğer dizin yoksa oluştur
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // Tüm .txt dosyalarını al
            foreach (var file in Directory.GetFiles(path, "*.txt"))
            {
                Files.Add(Path.GetFileNameWithoutExtension(file));
            }

            #endregion
        }

        #region Buttons

        private async Task OpenBook(string fileName)
        {
            Book? book = Book.Find(fileName);

            if (book == null)
            {
                string path = Path.Combine(FileSystem.AppDataDirectory, fileName + ".txt");

                if (File.Exists(path))
                {
                    string text = File.ReadAllText(path);
                    book = new Book(fileName, text);
                    Book.Books.Add(book);
                }
                else
                {
                    return;
                }
            }

            await Navigation.PushModalAsync(new ReadBook(book));
        }

        private async void OnAddNewTextClicked(object sender, EventArgs e)
        {
            NewText yeniMetinSayfasi = new();
            yeniMetinSayfasi.MetinEklendi += (s, metin) =>
            {
                Book.Books.Add(metin);
                Files.Add(metin.Header);
            };

            await Navigation.PushModalAsync(yeniMetinSayfasi);
        }

        private async Task Delete(string file)
        {
            string path = Path.Combine(FileSystem.AppDataDirectory, file + ".txt");
            if (File.Exists(path))
                File.Delete(path);

            path = Path.Combine(FileSystem.AppDataDirectory, file + ".cursor");
            if (File.Exists(path))
                File.Delete(path);

            Files.Remove(file);
            await Task.CompletedTask;
        }

        private async void OnLanguageClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new SnapRead.Language());
        }

        private void DarkMode(object sender, EventArgs e)
        {            if (Preferences.Get("DarkMode", false))
            {
                Preferences.Set("DarkMode", false);
                ModeButton.Text = "☀️";
                ModeButton.BackgroundColor = Colors.Cyan;
            }
            else
            {
                Preferences.Set("DarkMode", true);
                ModeButton.Text = "🌙";
                ModeButton.BackgroundColor = Colors.Navy;
            }

            var app = (App)Application.Current;
            app.SetTheme();

        }

        #endregion
    }

    public class Book
    {
        public string Header;
        public string Text;

        public static List<Book> Books = new List<Book>();

        public Book(string header, string text)
        {
            Header = header;
            Text = text;
        }

        public static Book? Find(string _header)
        {
            foreach (Book book in Books)
            {
                if (_header == book.Header)
                {
                    return book;
                }
            }

            return null;
        }
    }
}