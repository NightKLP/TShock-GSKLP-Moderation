using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKLP.Functions
{
    public class StringPageKLP
    {
        public int MaxLine;
        public string[] TextPage;

        /// <summary>
        /// example: List of Items page ({0}/{1})
        /// {0} = current page
        /// {1} = max page
        /// </summary>
        public string DisplayText = "Items page ({0}/{1})";

        /// <summary>
        /// Displayed when there is no content.
        /// </summary>
        public string EmptyText = "Empty";

        /// <summary>
        /// example: do /command {0} for more
        /// {0} = next page
        /// </summary>
        public string SubText = "";

        public StringPageKLP(int maxline, string[] text)
        {
            if (maxline <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxline), "MaxLine must be greater than 0.");

            MaxLine = maxline;
            TextPage = text ?? Array.Empty<string>();
        }

        public static int ParsePage(int index, List<string> parameters)
        {
            return ParsePage(index, parameters.ToArray());
        }

        public static int ParsePage(int index, string[] parameters)
        {
            if (parameters == null || index < 0 || index >= parameters.Length)
                return 1;

            return int.TryParse(parameters[index], out int result) ? result : 1;
        }

        public static bool TryParsePage(int index, List<string> parameters, out int page)
        {
            return TryParsePage(index, parameters.ToArray(), out page);
        }

        public static bool TryParsePage(int index, string[] parameters, out int page)
        {
            page = 1;

            if (parameters == null || index < 0 || index >= parameters.Length) { return false; }

            return int.TryParse(parameters[index], out page);
        }

        public int GetMaxPage()
        {
            if (TextPage.Length == 0) { return 0; }

            return (TextPage.Length + MaxLine - 1) / MaxLine;
        }

        public bool IsMaxPage(int page)
        {
            int maxPage = GetMaxPage();

            if (maxPage == 0) { return true; }

            if (page < 1) { page = 1; }

            return page >= maxPage;
        }

        public string GetDisplayText(int page, bool EmptyIfUnnecessary = false)
        {
            int maxPage = GetMaxPage();

            if (maxPage == 0)
            {
                return EmptyIfUnnecessary ? "" : string.Format(DisplayText, 0, 0);
            }

            if (page < 1) { page = 1; }

            if (page > maxPage) { page = maxPage; }

            return string.Format(DisplayText, page, maxPage);
        }

        public string GetSubText(int page)
        {
            int maxPage = GetMaxPage();

            if (maxPage == 0) { return ""; }

            if (page < 1) { page = 1; }

            if (page > maxPage) { page = maxPage; }

            return page < maxPage ? string.Format(SubText, page + 1) : "";
        }

        public string GetText(int page)
        {
            if (TextPage.Length == 0) { return EmptyText; }

            int maxPage = GetMaxPage();

            if (page < 1) { page = 1; }

            if (page > maxPage) { page = maxPage; }

            int startIndex = (page - 1) * MaxLine;
            int endIndex = Math.Min(startIndex + MaxLine, TextPage.Length);

            StringBuilder sb = new StringBuilder();

            for (int i = startIndex; i < endIndex; i++)
            {
                sb.Append(TextPage[i]);
            }

            return sb.ToString();
        }
    }
}
