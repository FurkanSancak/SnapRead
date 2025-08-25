using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace SnapRead
{
    public partial class NewText : ContentPage
    {
        public event EventHandler<Book>? MetinEklendi;

        public NewText()
        {
            InitializeComponent();
        }

        private async void OnAddPdfClicked(object sender, EventArgs e)
        {
            var fileResult = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Pdf // Yalnýzca PDF dosyalarýný seçmesine izin ver
            });

            if (fileResult != null)
            {
                // Metni Editor'a yazdýr
                String pdf = PdfReaderService.ExtractTextFromPdf(fileResult.FullPath);

                pdf = pdf.Replace("-\n", "");

                pdf = pdf.Replace("\n", " ");

                pdf = pdf.Replace("\t", " ");

                string name = Path.GetFileNameWithoutExtension(fileResult.FullPath);

                var yeniMetin = new Book(name, pdf);

                MetinEklendi?.Invoke(this, yeniMetin);

                string path = Path.Combine(FileSystem.AppDataDirectory, name + ".txt");
                File.WriteAllText(path, pdf);

                await Navigation.PopModalAsync();
            }
        }

        private async void OnAddTextClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(HeaderInput.Text) && !string.IsNullOrWhiteSpace(TextInput.Text))
            {
                var yeniMetin = new Book(HeaderInput.Text, TextInput.Text);
                MetinEklendi?.Invoke(this, yeniMetin);

                string path = Path.Combine(FileSystem.AppDataDirectory, HeaderInput.Text + ".txt");
                File.WriteAllText(path, TextInput.Text);

                await Navigation.PopModalAsync();
            }
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }

    /// <summary>
    /// Returns the text on the page excluding the title.
    /// </summary>
    public class TitlePruneStrategy : IEventListener
    {
        public StringBuilder page = new();
        float titleY;

        public TitlePruneStrategy(float _titleY)
        {
            //Subtracting 1 provides flexibility in case of titles are not perfectly aligned.
            titleY = _titleY - 1;
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_TEXT)
            {
                var textRenderInfo = (TextRenderInfo)data;
                string text = textRenderInfo.GetText();
                var rect = textRenderInfo.GetBaseline().GetBoundingRectangle();

                if (rect.GetY() < titleY)
                {
                    page.Append(text);
                }

            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the text on the page excluding the page number.
    /// </summary>
    public class PageNoPruneStrategy : IEventListener
    {
        public StringBuilder page = new();
        float pageNoY;

        public PageNoPruneStrategy(float _pageNoY)
        {
            //Adding 1 provides flexibility in case of numbers are not perfectly aligned.
            pageNoY = _pageNoY + 1;
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_TEXT)
            {
                var textRenderInfo = (TextRenderInfo)data;
                string text = textRenderInfo.GetText();
                var rect = textRenderInfo.GetBaseline().GetBoundingRectangle();

                if (rect.GetY() > pageNoY)
                {
                    page.Append(text);
                }

            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the text on the page excluding the title and page number..
    /// </summary>
    public class BothPruneStrategy : IEventListener
    {
        public StringBuilder page = new();
        float titleY, pageNoY;
        float y;

        public BothPruneStrategy(float _titleY, float _pageNoY)
        {
            titleY = _titleY - 1;   //-1 for flexibility
            pageNoY = _pageNoY + 1; //+1 for flexibility
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_TEXT)
            {
                var textRenderInfo = (TextRenderInfo)data;
                string text = textRenderInfo.GetText();
                var rect = textRenderInfo.GetBaseline().GetBoundingRectangle();

                y = rect.GetY();
                if (y < titleY && y > pageNoY)
                {
                    page.Append(text);
                }

            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return null; // null = all events
        }
    }

    /// <summary>
    /// Stores the first line text and position.
    /// Also if it is a number stores last line and position.
    /// </summary>
    public class PruningStrategy : IEventListener
    {
        public StringBuilder firstLine = new();
        public int pageNo = 0;
        public float? firstLineY = null;
        public float lastLineY = 9999;
        bool isPageNo = false;

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_TEXT)
            {
                //Get letter and location
                var textRenderInfo = (TextRenderInfo)data;
                string text = textRenderInfo.GetText();
                var rect = textRenderInfo.GetBaseline().GetBoundingRectangle();

                //Set first location as first line location
                if (firstLineY == null)
                    firstLineY = rect.GetY();

                //Get first line
                if (Math.Abs(rect.GetY() - firstLineY.Value) < 1)
                {
                    firstLine.Append(text);
                }

                //Check if it is next line
                if (lastLineY - rect.GetY() > 1)
                {
                    lastLineY = rect.GetY();
                    isPageNo = true;
                }

                //Check if current character is a number
                if (isPageNo)
                {
                    if (IsDigit(text))
                    {
                        pageNo = (pageNo * 10) + Convert.ToInt32(text);
                    }else if(text != " " && !text.Contains('\n') && !text.Contains('\r'))
                    {
                        isPageNo = false;
                        pageNo = 0;
                    } 
                }
            }
        }

        public void reset()
        {
            firstLine.Clear();
            pageNo = 0;
            firstLineY = null;
            lastLineY = 9999;
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return null;
        }

        public static bool IsDigit(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class PdfReaderService
    {
        /// <summary>
        /// Extracts text from PDF file and prunes and returns
        /// </summary>
        /// <param name="filePath">Full path to target file</param>
        /// <returns>Text</returns>
        public static string ExtractTextFromPdf(string filePath)
        {
            StringBuilder text = new();
            float titleY = -1;
            float pageNoY = -1;
            int firstPage = 1;

            // Open PDF
            using (PdfReader reader = new(filePath))
            {
                using (PdfDocument pdfDoc = new(reader))
                {
                    //If it is 5 pages or longer
                    if (pdfDoc.GetNumberOfPages() > 4)
                    {
                        /*
                         * Page titles can be either the title of the book or the title of the chapter.
                         * Sometimes the title of the book can be on one page and the title of the chapter on another page.
                         * Therefore, instead of checking the title of the book and consecutive pages,
                         * checking the similarity between two pages with 1 space between them works in all cases.
                        */
                        int page = pdfDoc.GetNumberOfPages() / 2;
                        var strategy = new PruningStrategy();
                        PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
                        parser.ProcessPageContent(pdfDoc.GetPage(page));

                        string line1 = strategy.firstLine.ToString();
                        int pageNo1 = strategy.pageNo;

                        page += 2;
                        strategy.reset();
                        parser.Reset();
                        parser.ProcessPageContent(pdfDoc.GetPage(page));

                        string line2 = strategy.firstLine.ToString();
                        int pageNo2 = strategy.pageNo;

                        if (line1 == line2)
                        {
                            titleY = strategy.firstLineY.Value;
                        }
                        else
                        {
                            /* Second checkpoint to detect title in case of no match due to chapter change or other reasons. */
                            page--;
                            strategy.reset();
                            parser.Reset();
                            parser.ProcessPageContent(pdfDoc.GetPage(page));

                            line1 = strategy.firstLine.ToString();

                            page += 2;
                            strategy.reset();
                            parser.Reset();
                            parser.ProcessPageContent(pdfDoc.GetPage(page));

                            line2 = strategy.firstLine.ToString();

                            if (line1 == line2)
                            {
                                titleY = strategy.firstLineY.Value - 1;
                            }
                        }

                        /*If there are numbers at the end of the page, it checks those numbers.
                         * If the difference between the numbers that are two pages apart is two, they are the page numbers.
                         * A page number is subtracted from the page index and
                         * the reading starts from the next page as much as the difference,
                         * because if the page number is smaller than the index,
                         * it shows that a few pages are separated as cover and contents.
                        */
                        if (pageNo2 - pageNo1 == 2)
                        {
                            firstPage = (pdfDoc.GetNumberOfPages() / 2) - pageNo1 + 1;
                            pageNoY = strategy.lastLineY;
                        }

                    }

                    if (titleY > -1)
                    {
                        if(pageNoY > -1)
                        {
                            for (int i = firstPage; i <= pdfDoc.GetNumberOfPages(); i++)
                            {
                                var strategy = new BothPruneStrategy(titleY, pageNoY);
                                PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
                                parser.ProcessPageContent(pdfDoc.GetPage(i));
                                text.Append(strategy.page);
                            }
                        }
                        else
                        {
                            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                            {
                                var strategy = new TitlePruneStrategy(titleY);
                                PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
                                parser.ProcessPageContent(pdfDoc.GetPage(i));
                                text.Append(strategy.page);
                            }
                        }
                    }
                    else if(pageNoY > -1)
                    {
                        for (int i = firstPage; i <= pdfDoc.GetNumberOfPages(); i++)
                        {
                            var strategy = new PageNoPruneStrategy(pageNoY);
                            PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
                            parser.ProcessPageContent(pdfDoc.GetPage(i));
                            text.Append(strategy.page);
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                        {
                            var strategy = new SimpleTextExtractionStrategy();
                            string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), strategy);
                            text.Append(pageText);
                        }
                    }
                }
            }

            return text.ToString();
        }
    }
}