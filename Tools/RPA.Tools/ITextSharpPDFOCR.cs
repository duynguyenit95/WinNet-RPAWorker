using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Filter;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Utils;
using SharpCifs.Util.Sharpen;
using Path = System.IO.Path;

namespace RPA.Tools
{
    public class ITextSharpPDFOCR
    {
        public enum PageOrientation
        {
            Portrait,
            Landscape
        }

        public class PageDimension
        {
            public float Width { get; set; }
            public float Height { get; set; }
            public PageOrientation Orientation { get; set; }

            public PageDimension(float width, float height, PageOrientation orientation)
            {
                Width = width;
                Height = height;
                Orientation = orientation;
            }
        }

        public enum PageType
        {
            A3,
            A4,
            A5
        }
        public static PageDimension GetPageDimension(PageType pageType, PageOrientation orientation)
        {
            int width, height;

            switch (pageType)
            {
                case PageType.A3:
                    width = 842;
                    height = 1191;
                    break;
                case PageType.A4:
                    width = 595;
                    height = 842;
                    break;
                case PageType.A5:
                    width = 420;
                    height = 595;
                    break;
                default:
                    throw new ArgumentException("Invalid page type.");
            }

            // Đổi chiều nếu là Landscape
            if (orientation == PageOrientation.Landscape)
            {
                (width, height) = (height, width);
            }

            return new PageDimension(width, height, orientation);
        }


        public static List<string> SplitPage(string pdfPath)
        {
            var res = new List<string>();
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfPath)))
            {
                if(pdfDocument.GetNumberOfPages() > 1)
                {
                    var splitter = new CustomPdfSplitter(pdfDocument, pdfPath);
                    ;
                    foreach(var doc in splitter.SplitByPageCount(1))
                    {
                        doc.Close();
                    }
                    res.AddRange(splitter.Result);
                }
                else
                {
                    res.Add(pdfPath);
                }

            }
            return res;
        }

        public static string ReadFile(string pdfPath)
        {
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfPath)))
            {
                return GetText(pdfDocument);
            }
        }
        public static string ReadFile(Stream stream)
        {
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(stream)))
            {
                return GetText(pdfDocument);
            }
        }
        public static string GetText(PdfDocument pdfDocument)
        {
            var pageText = new StringBuilder();
            var pageNumbers = pdfDocument.GetNumberOfPages();
            for (int i = 1; i <= pageNumbers; i++)
            {
                LocationTextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
                parser.ProcessPageContent(pdfDocument.GetPage(i));
                pageText.Append(strategy.GetResultantText());
            }
            return pageText.ToString();
        }
        public static string ExtractHorizontalColumnText(string pdfPath, PageDimension pageDimension, string keyword, float top = 0, float bottom = 0)
        {
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfPath)))
            {
                //Lấy toạ độ của keyword
                var (keywordPosition, keywordPageNumber) = FindKeyWordPosition(pdfDocument, keyword);
                if (keywordPosition == null) return string.Empty;

                float X = keywordPosition.GetX();
                float Y = keywordPosition.GetY() - top;
                float width = pageDimension.Width;
                float height = keywordPosition.GetHeight() + top + bottom;

                Rectangle area = new Rectangle(X, Y, width, height);
                var content = ExtractText(pdfDocument.GetPage(keywordPageNumber), area);
                return content.ToString();
            }
        }

        public static string ExtractVerticalColumnText(string pdfPath, PageDimension pageDimension, string keyword, string endKeyword, float left = 0, float right = 0)
        {
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfPath)))
            {
                var columnText = new StringBuilder();
                int pageNumbers = pdfDocument.GetNumberOfPages();

                //Lấy toạ độ của keyword
                var (keywordPosition, keywordPageNumber) = FindKeyWordPosition(pdfDocument, keyword);
                if (keywordPosition == null) return string.Empty;

                //Lấy toạ độ của endKeyword
                var (endKeywordPosition, endKeywordPageNumber) = FindKeyWordPosition(pdfDocument, endKeyword);

                //Khoanh 1 khu vực với các toạ độ X,Y kèm theo chiều rộng (từ trái qua phải) và chiều cao (lấy từ dưới lên trên)
                //Hoành độ X lấy theo X của keyword, điều chỉnh thêm theo lề trái
                float X = keywordPosition.GetX() - left;
                //Chiều rộng khu vực lấy theo chiều rộng của keyword, điều chỉnh thêm theo 2 lề trái phải
                float width = keywordPosition.GetWidth() + left + right;

                for (int i = keywordPageNumber; i <= pageNumbers; i++)
                {
                    //Do thuật toán lấy khu vực theo trục tung Y là tăng dần từ điểm dưới lên điểm trên của văn bản
                    //Tung độ Y sẽ lấy theo vị trí Y của endKeyWord
                    float Y, height;
                    //Trường hợp 1: endKeyword == null thì lấy điểm dưới cùng văn bản
                    //=> Y = 0
                    //=> height = height của trang - height của lề trên
                    float topMarginHeight = pageDimension.Height - keywordPosition.GetY() - keywordPosition.GetHeight();
                    if (endKeywordPosition == null)
                    {
                        Y = 0;
                        height = pageDimension.Height - topMarginHeight;
                    }
                    //Trường hợp 2: endKeyword != null
                    else
                    {
                        //2.1. Vị trí endKeyword nằm ở SAU trang hiện tại
                        //=> Y = 0
                        //=> height = height của trang - height của lề trên
                        float bottomMarginHeight = endKeywordPosition.GetY() + endKeywordPosition.GetHeight();
                        if (endKeywordPageNumber > i)
                        {
                            Y = 0;
                            height = pageDimension.Height - topMarginHeight;
                        }
                        //2.2. Vị trí endKeyword nằm ở trang hiện tại
                        //=> Y = vị trí endKeyword (sát lề trên)
                        //=> height = height của trang - height của lề trên - height của lề dưới
                        else if (endKeywordPageNumber == i)
                        {
                            Y = endKeywordPosition.GetY() + endKeywordPosition.GetHeight();
                            height = pageDimension.Height - (i == keywordPageNumber ? topMarginHeight : 0) - bottomMarginHeight;
                        }
                        //2.3. Vị trí endKeyword nằm TRƯỚC trang hiện tại => không lấy dữ liệu ở trang hiện tại
                        else continue;
                    }
                    
                    var area = new Rectangle(X, Y, width, height);
                    var areaContent = ExtractText(pdfDocument.GetPage(i), area);
                    columnText.Append(areaContent);
                }
                return columnText.ToString();
            }
        }
        private static (Rectangle, int pagenumber) FindKeyWordPosition(PdfDocument pdfDocument, string keyword)
        {
            var strategy = new CustomLocationTextExtractionStrategy();
            PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
            int pageNumbers = pdfDocument.GetNumberOfPages();
            (Rectangle, int pagenumber) result = (null, 0);
            for (int i = 1; i <= pageNumbers; i++)
            {
                parser.ProcessPageContent(pdfDocument.GetPage(i));
                foreach (var (text, rect) in strategy.TextLocations)
                {
                    if (text.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        return (rect,i);
                    }
                }
            }
            return result;
        }
        private static string ExtractText(PdfPage page, Rectangle extractionRegion)
        {
            var columnText = new StringBuilder();

            //Initialize our custom text extraction strategy
            var customStrategy = new CustomLocationTextExtractionStrategy();

            // Use TextRegionEventFilter to focus extraction within the specified region
            var regionFilter = new TextRegionEventFilter(extractionRegion);
            var strategy = new FilteredTextEventListener(customStrategy, regionFilter);

            var parser = new PdfCanvasProcessor(strategy);
            parser.ProcessPageContent(page);

            // Finalize accumulated text after processing the page
            customStrategy.FinalizeAccumulatedText();

            // Collect the extracted text from the custom strategy's TextLocations
            foreach (var (text, rect) in customStrategy.TextLocations)
            {
                columnText.AppendLine(text);
            }

            return columnText.ToString();
        }
    }
    public class CustomPdfSplitter : PdfSplitter
    {
        int _partNumber = 1;
        string _output;
        string _inputFileName;
        string _processDir;
        public List<string> Result { get; set; } = new List<string>();
        public CustomPdfSplitter(PdfDocument pdfDocument, string filePath) : base(pdfDocument)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            _processDir = Path.GetDirectoryName(filePath);
            _inputFileName = fileName;
            _output = Path.Combine(_processDir,fileName);
            if (!Directory.Exists(_output))
            {
                Directory.CreateDirectory(_output);
            }
        }

        protected override PdfWriter GetNextPdfWriter(PageRange documentPageRange)
        {
            try
            {
                var expectedOutputFile = Path.Combine(_output, $"{_inputFileName}_{_partNumber++}.pdf");// $"{_output}/{_inputFileName}_{_partNumber++}.pdf";
                Result.Add(Path.Combine(_processDir, expectedOutputFile));
                return new PdfWriter(expectedOutputFile);
            }
            catch (FileNotFoundException e)
            {
                throw e;
            }
        }
    }

    public class CustomLocationTextExtractionStrategy : LocationTextExtractionStrategy
    {
        public List<(string Text, Rectangle Rect)> TextLocations { get; } = new List<(string Text, Rectangle Rect)>();
        public string accumulatedText = "";
        public Rectangle accumulatedRectangle = null;
        public float widthThresHold = 5.0f;
        public float heightThresHold = 0.5f;


        // Use the 'new' keyword to hide the inherited method.
        public override void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_TEXT)
            {
                var renderInfo = (TextRenderInfo)data;
                Console.WriteLine(renderInfo.GetText());
                var descentLine = renderInfo.GetDescentLine();
                var ascentLine = renderInfo.GetAscentLine();

                float Xas = ascentLine.GetStartPoint().Get(0);
                float Xds = descentLine.GetStartPoint().Get(0);
                float Xae = ascentLine.GetEndPoint().Get(0);
                float Xde = descentLine.GetEndPoint().Get(0);

                float Yas = ascentLine.GetStartPoint().Get(1);
                float Yds = descentLine.GetStartPoint().Get(1);
                float Yae = ascentLine.GetEndPoint().Get(1);
                float Yde = descentLine.GetEndPoint().Get(1);

                float[] X_Positions = new float[] { Xas, Xds, Xae, Xde };
                float[] Y_Positions = new float[] { Yas, Yds, Yae, Yde };

                float MinX = Enumerable.Min(X_Positions);
                float MinY = Enumerable.Min(Y_Positions);

                float MaxX = Enumerable.Max(X_Positions);
                float MaxY = Enumerable.Max(Y_Positions);              

                // Calculate the bounding rectangle for the current text segment (RenderInfo)
                var rectangle = new Rectangle(
                    MinX,
                    MinY,
                    Math.Abs(MaxX - MinX),     // Width
                    Math.Abs(MaxY - MinY)   // Height
                );

                if (accumulatedRectangle == null)
                {
                    accumulatedText = renderInfo.GetText();
                    accumulatedRectangle = rectangle;
                }
                else
                {
                    if (Math.Abs(rectangle.GetX() - accumulatedRectangle.GetX() - accumulatedRectangle.GetWidth()) < widthThresHold &&
                        Math.Abs(rectangle.GetY() - accumulatedRectangle.GetY()) <= heightThresHold)
                    {
                        accumulatedText += renderInfo.GetText();
                        // Update accumulatedRectangle to include the current segment
                        float newMinX = Math.Min(accumulatedRectangle.GetX(), rectangle.GetX());
                        float newMinY = Math.Min(accumulatedRectangle.GetY(), rectangle.GetY());
                        float newMaxX = Math.Max(accumulatedRectangle.GetX() + accumulatedRectangle.GetWidth(), rectangle.GetX() + rectangle.GetWidth());
                        float newMaxY = Math.Max(accumulatedRectangle.GetY() + accumulatedRectangle.GetHeight(), rectangle.GetY() + rectangle.GetHeight());

                        accumulatedRectangle = new Rectangle(newMinX, newMinY, newMaxX - newMinX, newMaxY - newMinY);
                    }
                    else
                    {
                        Console.WriteLine(accumulatedText, accumulatedRectangle);
                        // Add the accumulated text and rectangle to TextLocations
                        TextLocations.Add((accumulatedText, accumulatedRectangle));

                        // Reset accumulation with the current segment
                        accumulatedText = renderInfo.GetText();
                        accumulatedRectangle = rectangle;
                    }
                }
            }
        }
        // After processing the page, add any remaining accumulated text
        public void FinalizeAccumulatedText()
        {
            if (!string.IsNullOrEmpty(accumulatedText) && accumulatedRectangle != null)
            {
                TextLocations.Add((accumulatedText, accumulatedRectangle));
                accumulatedText = "";
                accumulatedRectangle = null;
            }
        }
        public override ICollection<EventType> GetSupportedEvents()
        {
            return null; // Return null to listen to all events
        }
    }

}
