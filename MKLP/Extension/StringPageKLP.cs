using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKLP.Functions
{
    internal class StringPageKLP
    {
        public int MaxLine;
        public string[] TextPage;
        /// <summary>
        /// <br>example: List of Items page ({0}/{1})
        /// </br>
        /// <br>{0} is page</br>
        /// <br>{1} is max page</br>
        /// </summary>
        public string DisplayText = "Items page ({0}/{1})";
        /// <summary>
        /// if the page/textpage contains nothing
        /// </summary>
        public string EmptyText = "Empty";
        /// <summary>
        /// <br>example: do /command {0} for more
        /// </br>
        /// <br>{0} is next page</br>
        /// </summary>
        public string SubText = "";
        public StringPageKLP(int maxline, string[] text)
        {
            MaxLine = maxline;
            TextPage = text;
        }

        public static int ParsePage(int index, List<string> parameters)
        {
            return ParsePage(index, parameters.ToArray());
        }
        public static int ParsePage(int index, string[] parameters)
        {
            int result = 1;
            if (parameters.Length <= index) return result;
            int.TryParse(parameters[index], out result);
            return result;
        }
        public static bool TryParsePage(int index, List<string> parameters, out int page)
        {
            return TryParsePage(index, parameters.ToArray(), out page);
        }
        public static bool TryParsePage(int index, string[] parameters, out int page)
        {
            page = 1;
            if (parameters.Length >= index)
            {
                return false;
            }
            return int.TryParse(parameters[index], out page);
        }

        public int GetMaxPage()
        {
            if (TextPage.Length <= 0)
            {
                return 0;
            }
            return (int)Math.Ceiling((decimal)((decimal)TextPage.Length / (decimal)MaxLine));
        }

        public bool IsMaxPage(int page)
        {
            if (TextPage.Length <= 0)
            {
                return true;
            }
            if (page < 1) page = 1;
            int getmaxpage = (int)Math.Ceiling((decimal)((decimal)TextPage.Length / (decimal)MaxLine));
            if (getmaxpage < page) page = getmaxpage;
            return page >= getmaxpage;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="EmptyIfUnnecessary"></param>
        /// <returns></returns>
        public string GetDisplayText(int page, bool EmptyIfUnnecessary = false)
        {
            if (TextPage.Length <= 0)
            {
                return EmptyIfUnnecessary ? "" : string.Format(DisplayText, 0, 0);
            }
            if (page < 1) page = 1;
            int getmaxpage = (int)Math.Ceiling((decimal)((decimal)TextPage.Length / (decimal)MaxLine));
            if (getmaxpage < page) page = getmaxpage;
            return string.Format(DisplayText, page, getmaxpage);
        }
        public string GetSubText(int page)
        {
            if (TextPage.Length <= 0)
            {
                return "";
            }
            if (page < 1) page = 1;
            int getmaxpage = (int)Math.Ceiling((decimal)((decimal)TextPage.Length / (decimal)MaxLine));
            if (getmaxpage < page) page = getmaxpage;
            return page < getmaxpage ? string.Format(SubText, page + 1) : "";
        }

        public string GetText(int page)
        {
            if (TextPage.Length <= 0)
            {
                return EmptyText;
            }
            if (page < 1) page = 1;
            int getmaxpage = (int)Math.Ceiling((decimal)((decimal)TextPage.Length / (decimal)MaxLine));
            if (getmaxpage < page) page = getmaxpage;
            page--;
            string spage = "";
            int e = 0;
            for (int i = 0; i < TextPage.Length; i++)
            {
                if (e >= MaxLine) break;
                if (((i + 1) / MaxLine) < page) continue;
                spage += TextPage[i];
                e++;
            }

            return spage;
        }
    }
}
