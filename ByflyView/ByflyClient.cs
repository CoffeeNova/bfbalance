using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xNet.Net;
using xNet.Text;
using xNet.Collections;
using System.Threading;
using NLog;
using CoffeeJelly.Byfly.BFlib.Text;
using CoffeeJelly.Byfly.BFlib.Controls;
using System.Security;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoffeeJelly.Byfly.ByflyView
{
    using Consts = ByFlyConstants;

    public class ByflyClient : IDisposable, INotifyPropertyChanged
    {
        #region properties (public)

        /// <summary>
        /// ФИО/название организации абонента
        /// </summary>
        public string Abonent
        {
            get { return _abonent; }
            private set
            {
                _abonent = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Номер абонентского счета
        /// </summary>
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Капча
        /// </summary>
        public string Captcha { get; set; }

        /// <summary>
        /// Статус блокировки
        /// </summary>
        public string Status
        {
            get { return _status; }
            private set
            {
                _status = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Тарифный план
        /// </summary>
        public string TariffPlan
        {
            get { return _tariffPlan; }
            private set
            {
                _tariffPlan = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Текущее состояние счета
        /// </summary>
        public string ActualBalance
        {
            get { return _actualBalance; }
            private set
            {
                _actualBalance = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Сообщение об ошибке, сброс сообщения производится вызовом метода <paramref name="ErrorReset()"/> 
        /// </summary>
        public string ErrorMessage
        {
            get { return _errorMessage; }
            private set
            {
                _errorMessage = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Время до конца блокировки
        /// </summary>
        public TimeSpan BlockTime
        {
            get { return TimeSpan.FromMilliseconds(AuthBlockTimer.TimeLeft); }
            private set
            {
                AuthBlockTimer.Interval = value.TotalMilliseconds;
                if (value != TimeSpan.Zero)
                {
                    AuthBlockTimer.Start();
                    IsBlocked = true;
                }
            }
        }

        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set { _isIndeterminate = value; NotifyPropertyChanged(); }
        }

        public object ErrorReset
        {
            set
            {
                if (value !=null && value.GetType() == typeof(bool) && (bool)value)
                    ResetError();
            }
        }
        /// <summary>
        /// Возвращает значение, указывающее является ли клиент в блокировке из-за частых обращений к серверу
        /// </summary>
        public bool IsBlocked { get; private set; }

        /// <summary>
        /// Возвращает значение, указывающее необходимость ввода капчи
        /// </summary>
        public bool IsCaptchaRequired { get; private set; }

        /// <summary>
        /// Куки 
        /// </summary>
        public CookieDictionary Cookies { get { return _cookies; } }


        #endregion

        #region properties (private)

        #endregion

        #region fields (public)
        #endregion

        #region fields (private)

        private bool _isIndeterminate = false;
        private string _abonent = "";
        private string _status = "";
        private string _tariffPlan = "";
        private string _actualBalance = "";
        private string _errorMessage = "";
        private string _login = "";
        private string _password = "";
        private CookieDictionary _cookies = new CookieDictionary{
                    {"PHPSESSID", ""},
                    {"__utma", "254710043.950298654.1443082145.1443083140.1447303912.3"},
                    {"__utmc", "254710043"},
                    {"__utmz", "254710043.1447303912.3.3"},
                    {"has_js" , "1"},
                    {"treeview", "111111111111"}
                };
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private TimerImproved AuthBlockTimer;

        #endregion

        #region static properties (public)

        public static bool HasActiveConnection { get { return _hasActiveConnection; } }

        #endregion

        #region static fields (private)

        private static bool _hasActiveConnection = false;


        #endregion
        public ByflyClient(string login, string password)
        {
            Login = login;
            Password = password;
            AuthBlockTimer = new TimerImproved();
            AuthBlockTimer.Elapsed += AuthBlockTimer_Elapsed;
            AuthBlockTimer.AutoReset = false;


            string phpsessid = GetSavedPHPSESSID(login);
            _cookies.Remove("PHPSESSID");
            if (phpsessid != "")
                _cookies.Add("PHPSESSID", phpsessid);

        }

        #region methods (public)
        /// <summary>
        /// Реализует подключение к серверу белтелеком, делает http запросы и получает информацию аб аккаунте
        /// </summary>
        /// <returns><see langword="true"/>, если данные получены. <see langword="false"/>, если данные не удалось получить</returns>
        public bool GetAccountData()
        {
            try
            {
                IsIndeterminate = true;
                Thread.Sleep(3000);
                return AccountData();
            }
            finally
            {
                IsIndeterminate = false;
            }
        }

        /// <summary>
        /// Сброс сообщения ошибки (очищает свойство <see cref="ErrorMEssage"/>)
        /// </summary>
        public void ResetError()
        {
            ErrorMessage = "";
        }

        /// <summary>
        /// Освобождает все ресурсы, используемые текущим экземпляром класса <see cref="ByflyClient"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
        #region methods (private)
        /// <summary>
        /// Отправляет Post запрос на сайт белтелекома с параметрами <paramref name="parametrs"/>/>
        /// </summary>
        /// <param name="parametrs">Параметры запроса</param>
        /// <returns>html страницу</returns>
        private string GetHtmlContent(RequestParams parametrs)
        {
            string htmlContent = "";
            using (var request = new HttpRequest())
            {
                request.UserAgent = HttpHelper.ChromeUserAgent();
                request.EnableAdditionalHeaders = true;
                request.EnableEncodingContent = true;

                try
                {
                    request.Cookies = _cookies;
                    htmlContent = request.Post(Consts._BELTELECOM_URL, parametrs).ToString();
                    _cookies = request.Cookies;
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                    ErrorMessage = ex.Message;
                }
            }
            return htmlContent;
        }

        /// <summary>
        /// Получает html контент после аутинтификации на сайт белтелекома
        /// </summary>
        /// <param name="login">Номер аккаунта (Лицевой номер)</param>
        /// <param name="password">Пароль от аккаунта</param>
        /// <returns></returns>
        private string Authentication(string login, string password)
        {
            RequestParams rqp = new RequestParams();
            rqp["redirect"] = @"/main.html";
            rqp["oper_user"] = login;
            rqp["passwd"] = password;

            return GetHtmlContent(rqp);
        }

        /// <summary>
        /// Считывает с реестра PHPSESSID последего входа для данного аккаунта
        /// </summary>
        /// <param name="keyName">Имя ключа реестра (аккаунт)</param>
        /// <returns>PHPSESSID последего входа, если такого нет, возвращает пустую строку </returns>
        private string GetSavedPHPSESSID(string keyName)
        {
            string value = "";
            try
            {
                ByflyTools.CheckRegistrySettings(ref value, "PHPSESSID", Consts._SETTINGS_LOCATION + "\\" + Consts._PROFILES_LOCATION + "\\" + keyName, "");
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                ErrorMessage = ex.Message;
            }
            return value;
        }

        /// <summary>
        /// Сохраняет в реестр PHPSESSID новой сессии
        /// </summary>
        /// <param name="key">Имя ключа реестра (аккаунт)</param>
        /// <param name="value">Сохраняемое значегие PHPSESSID</param>
        private void SavePHPSESSID(string key, string value)
        {
            try
            {
                ByflyTools.SaveRegistrySettings("PHPSESSID", Consts._SETTINGS_LOCATION + "\\" + Consts._PROFILES_LOCATION + "\\" + key, value);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                ErrorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Определяет является ли исходная страница страницей выбора логина для управления (в случае нескольких интернет аккаунтов для одного клиента)
        /// </summary>
        /// <param name="htmlContent">html страница</param>
        /// <returns></returns>
        private bool IsChoicePage(string htmlContent)
        {
            //Console.WriteLine("<a href=\"//choice.html\" ");
            string choiceString = htmlContent.Substrings("<a href=\"/choice.html\" ", ">").First();
            return choiceString == "class=\"current\"" ? true : false;
        }

        /// <summary>
        /// Парсит параметр live, уникальный для данного логина 
        /// </summary>
        /// <param name="htmlContent">Исходный html контент после ввода пароля на сайт issa.beltelecom.by</param>
        /// <param name="pril_sel">Логин от аккаунта byfly (без @beltel.by)(</param>
        /// <returns>какой-то номер, я хз, он уникален</returns>
        private string ParseLiveLogin(string htmlContent, string pril_sel)
        {
            return htmlContent.Substrings(String.Format(@"pril_sel={0}&live=", pril_sel), @"&chpril=").First();
        }

        private string ParseChpril(string htmlContent, string pril_sel, string liveNumeric)
        {
            Console.WriteLine(String.Format(@"pril_sel={0}&live={1}&chpril=", pril_sel, liveNumeric), "\')\"");
            return htmlContent.Substrings(String.Format(@"pril_sel={0}&live={1}&chpril=", pril_sel, liveNumeric), "\')\"").First();
        }

        private string GetHtmlFromAccountPage(string htmlContent, string login)
        {
            string live = ParseLiveLogin(htmlContent, login);
            //string liveNumeric = live.Split('_')[1];
            string chpril = ParseChpril(htmlContent, login, live);

            RequestParams rqp = new RequestParams();
            rqp["pril_sel"] = login;
            rqp["live"] = live;
            rqp["chpril"] = chpril;

            return GetHtmlContent(rqp);
        }

        /// <summary>
        /// Проверяет успешность авторизации
        /// </summary>
        /// <param name="htmlContent">html контент</param>
        /// <returns><paramref name="AuthFault"/> в зависимости от типа ошибки</returns>
        private AuthFault CheckAuthSuccess(string htmlContent)
        {
            AuthFault code = AuthFault.UnknownError;
            if (htmlContent.Contains("Вы совершаете слишком частые попытки авторизации"))
                code = AuthFault.Block;
            else if (htmlContent.Contains("Введен неверный пароль или абонент не существует"))
                code = AuthFault.WrongLogin;
            else if (htmlContent.Contains("<script>$.jGrowl(\'<hr>Ошибка символов\'"))
                code = AuthFault.WrongSymbols;
            else if (htmlContent.Contains(""))
                code = AuthFault.Success;
            return code;
        }

        /// <summary>
        /// Парсит время бана в минутах
        /// </summary>
        /// <param name="htmlContent">html мусор</param>
        /// <returns>Время бана в минутах. 0 - если что-то пошло не так</returns>
        private int ParseBlockTime(string htmlContent)
        {
            try
            {
                string minuts = htmlContent.Substrings("К сожалению, мы вынуждены вас заблокировать на ", " минут").First();
                return Int32.Parse(minuts);
            }
            catch { return 0; }
        }


        private void AuthBlockTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            IsBlocked = false;
            ResetError();
        }

        private void ParseAccountData(string htmlContent)
        {
            try
            {
                //try to parse our table, so we have 2 tables in a page and our are first one
                string tableHtml = htmlContent.Substrings("<table", "</table>").First();
                string[] tableLines = tableHtml.Substrings("<tr>", "</tr>"); //схороним все строки нашей таблицы

                var accountDataDict = new Dictionary<string, string>();
                foreach (string line in tableLines)
                {
                    string[] pair = line.Substrings(">", "</td>");
                    //accountDataList.Add(new KeyValuePair<string, string>(pair[0], pair[1]));
                    accountDataDict.Add(pair[0], pair[1]);
                }
                RemoveHtml(ref accountDataDict);
                Abonent = accountDataDict.Single(i => i.Key == "Абонент").Value;
                Status = accountDataDict.Single(i => i.Key == "Статус блокировки").Value;
                Status = Status.RemoveHistoryBlock();
                TariffPlan = accountDataDict.Single(i => i.Key == "Тарифный план на услуги").Value;
                ActualBalance = htmlContent.Substrings("Актуальный баланс: <b>", ", руб.</b>").First();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                ErrorMessage = "Невозможно получить сведения аккаунта";
            };

        }

        /// <summary>
        /// Выполняем перед отправлением запросов на сервер
        /// </summary>
        /// <returns></returns>
        private bool ReadyToGetAccountData()
        {
            if (IsBlocked)
                return false;
            if (!LoginVerification(_login))
            {
                ErrorMessage = "Некорректный логин. Должна быть комбинация цифр";
                _log.Warn("Некорректный логин");
                return false;
            }
            if (!PasswordVerification(_password))
            {
                ErrorMessage = "Заполните форму ввода пароля без пустных знаков";
                _log.Warn("Пустой пароль");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Проверка свойства Login, должен быть не пустым и содеражть только цифры
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        private bool LoginVerification(string login)
        {
            if (String.IsNullOrWhiteSpace(login))
                return false;
            long t;
            if (!Int64.TryParse(login, out t))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Проверка свойства Password, должен быть не пустым.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool PasswordVerification(string password)
        {
            //string password = new System.Net.NetworkCredential(string.Empty, password).Password;
            Console.WriteLine(password);
            if (String.IsNullOrWhiteSpace(password))
                return false;
            else
                return true;
        }

        ///Удаляет хтмл тэги из списка
        private void RemoveHtml(ref Dictionary<string, string> dict)
        {
            dict = dict.Select(((i) => { return new KeyValuePair<string, string>(i.Key.RemoveHtmlTags(), i.Value.RemoveHtmlTags()); })).ToDictionary(x => x.Key, x => x.Value);
        }
        #region Auth Error handlers
        /// <summary>
        /// Обработчик ошибки авторизации (неизвестная ошибка)
        /// </summary>
        /// <returns>Всегда false</returns>
        private bool UnknownErrorHandler()
        {
            _log.Warn(String.Format("Неизвестная ошибка при попытке авторизации аккаунтa {0}, PHPSESSID = {1}", _login, _cookies.Single((p) => p.Key == "PHPSESSID").Value));
            ErrorMessage = "Неизвестная ошибка при попытке авторизации";
            return false;
        }

        /// <summary>
        /// Обработчик ошибки авторизации (частая попытка авторизации и блокировка)
        /// </summary>
        /// <param name="htmlContent"></param>
        /// <returns>Всегда <see langword="false"/></returns>
        private bool BlockErrorHandler(string htmlContent)
        {
            //узнаем на сколько минут нас заблокировали
            int blockTime = ParseBlockTime(htmlContent);
            BlockTime = TimeSpan.FromMinutes(blockTime);
            string message = String.Format("Слишком частые попытки авторизации. Блокировка на {0} минут.", blockTime);
            _log.Warn(message);
            ErrorMessage = message;
            return false;
        }

        /// <summary>
        /// Обработчик ошибки авторизации (неверный логин или пароль)
        /// </summary>
        /// <param name="htmlContent"></param>
        /// <returns>Всегда <see langword="false"/></returns>
        private bool WrongLoginErrorHandler()
        {
            string message = "Введен неверный пароль или абонент не существует";
            _log.Warn(message);
            ErrorMessage = message;
            return false;
        }

        private bool WrongSymbolsErrorHandler()
        {
            //string message = "Неправильно введена капча";
            //_log.Warn(message);
            //_errorMessage = message;
            _cookies.Remove("PHPSESSID"); //удалим куки и попробуем еще раз
            GetAccountData();
            return false;
        }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool AccountData()
        {
            if (!ReadyToGetAccountData())
                return false;
            ResetError();

            //проверим доступность сайта белтелеком, проверим необходимость ввода капчи
            var rqp = new RequestParams();
            rqp["redirect"] = @"/main.html";
            string byflyPage = GetHtmlContent(rqp);
            if (String.IsNullOrEmpty(byflyPage))
                return false;
            //string CAPTCHA (оставим этоп обрабтки капчи на потом)
            //if(IsCaptchaRequired)

            //авторизуемся
            byflyPage = Authentication(_login, _password);
            Captcha = String.Empty;
            if (String.IsNullOrEmpty(byflyPage))
                return false;

            var phpsessidCookie = _cookies.Single((p) => p.Key == "PHPSESSID");
            SavePHPSESSID(_login, phpsessidCookie.Value); //сохраним сессию

            if (!AuthorizationHandler(byflyPage)) //Проверим успешность авторизации, при ошибках проведем соответствующие действия
                return false;
            //если нас перебросило на страницу выбора логина для управления (в случае нескольких интернет аккаунтов для одного клиента) сделаем еще один запрос для нужного нам логина
            if (IsChoicePage(byflyPage))
                byflyPage = GetHtmlFromAccountPage(byflyPage, _login);

            ParseAccountData(byflyPage);
            //Console.WriteLine(_abonent);
            //Console.WriteLine(Login);
            //Console.WriteLine(Password);
            //Console.WriteLine(_tariffPlan);
            //Console.WriteLine(_actualBalance);
            return true;
        }

        private bool AuthorizationHandler(string htmlContent)
        {
            AuthFault authSuccess = CheckAuthSuccess(htmlContent);
            if (authSuccess == AuthFault.UnknownError)
                return UnknownErrorHandler();
            if (authSuccess == AuthFault.Block)
                return BlockErrorHandler(htmlContent);
            if (authSuccess == AuthFault.WrongLogin)
                return WrongLoginErrorHandler();
            if (authSuccess == AuthFault.WrongSymbols)
                return WrongSymbolsErrorHandler();
            if (authSuccess != AuthFault.Success)
                return false;
            return true;
        }

        #endregion

        #endregion

        #region protected methods

        /// Освобождает неуправляемые (а при необходимости и управляемые) ресурсы, используемые объектом <see cref="ByflyClient"/>.
        /// </summary>
        /// <param name="disposing">Значение <see langword="true"/> позволяет освободить управляемые и неуправляемые ресурсы; значение <see langword="false"/> позволяет освободить только неуправляемые ресурсы.</param>
        protected virtual void Dispose(bool disposing)
        {
            AuthBlockTimer.Dispose();
        }

        #endregion

        #region public events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        /// <summary>
        /// Ошибки аутентификации
        /// </summary>
        private enum AuthFault
        {
            Success = 1, //все халасё
            Block = 2, // нас заблочили на Х минут
            WrongLogin = 3, //неправильный логин или пароль
            WrongSymbols = 4, //ошибка символов (капча)
            UnknownError = 0 //неизвестная ошибка
        }
    }


}
