using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security;

namespace CoffeeJelly.Byfly.BFlib.Text
{
    /// <summary>
    /// Представляет статический класс, для расширеной работы со строками
    /// </summary>
    public static class StringExtension
    {

#region public methods
        /// <summary>
        /// Удаляет html тэги из строки
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveHtmlTags(this string str)
        {
            return Regex.Replace(str, "<.*?>", string.Empty);
        }

        /// <summary>
        /// Возвращает SecureString
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static SecureString ToSecureString(this string str)
        {
            var sec = new SecureString();
            str.ToCharArray().ToList().ForEach(c => sec.AppendChar(c));
            return sec;
        }

        public static string RemoveHistoryBlock(this string str)
        {
            string delStr = " История блокировок";
            return str.Replace(delStr, "");
        }
#endregion

#region private methods

#endregion

#region private enums


#endregion
    }

}
