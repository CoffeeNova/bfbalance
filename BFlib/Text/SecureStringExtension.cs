using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;

namespace CoffeeJelly.Byfly.BFlib.Text
{
    /// <summary>
    /// Представляет статический класс, для расширеной работы с защищенными строками
    /// </summary>
    public static class SecureStringExtension
    {
        #region public methods
        public static string ToNormalString(this SecureString sStr)
        {
            return new System.Net.NetworkCredential(string.Empty, sStr).Password;
        }
        #endregion
    }
}
