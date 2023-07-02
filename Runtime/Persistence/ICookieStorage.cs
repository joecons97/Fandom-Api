using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace FandomApi.Persistence
{
    public interface ICookieStorage
    {
        string GetCookiesHeader();
        void SaveCookiesHeader(string cookies);

        void ClearCookie();
    }
}
