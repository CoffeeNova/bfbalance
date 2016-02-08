using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using xNet.Net;
using xNet.Text;
using xNet.Collections;
using System.Collections.ObjectModel;
using NWTweak;
using NLog;
using System.Globalization;
using System.Security;
using CoffeeJelly.Byfly.BFlib;
using CoffeeJelly.Byfly.BFlib.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MahApps.Metro.Controls;

namespace CoffeeJelly.Byfly.ByflyView
{
    using Tools = CoffeeJelly.Byfly.ByflyView.ByflyTools;
    using Consts = CoffeeJelly.Byfly.ByflyView.ByFlyConstants;

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        public ObservableCollection<ByflyClient> ByFlyCollection { get; set; }


        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            string plaintext = "2547";
            string pass = Environment.MachineName + Environment.UserName + "lol";

            Console.WriteLine("Your encrypted string is:");
            string encryptedstring = StringCipher.Encrypt(plaintext, pass);
            Console.WriteLine(encryptedstring);
            Console.WriteLine("");

            Console.WriteLine("Your decrypted string is:");
            string decryptedstring = StringCipher.Decrypt(encryptedstring, pass);
            Console.WriteLine(decryptedstring);
            Console.WriteLine("");

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();


            // string log = ByflyTools.EncryptText("1652001316402", key, iv);
            // string pass = ByflyTools.EncryptText("2547", key, iv);

            InitializeComponent();
            //поправка инициализации
            //mainListBox.Height = window.Height;
            this.AllowsTransparency = true; // fixes MahApps.Metro window bug with no working transparency

            // int requestTime = Tools.GetRequestTime(UrlTextBox.Text, null, Timeout.Infinite);
            // Console.WriteLine(requestTime);
            ByFlyCollection = GetSavedCollection();
            if (ByFlyCollection.Count == 0)
                ByFlyCollection.Add(new ByflyClient("", ""));
            mainListBox.ItemsSource = ByFlyCollection;

            UpdateAllProfiles();
            //Thread.Sleep(5000);
            
        }

        //
        private ObservableCollection<ByflyClient> GetSavedCollection()
        {
            var collection = new ObservableCollection<ByflyClient>();
            var profilesList = GetSavedProfiles();

            foreach (string profile in profilesList)
            {
                string login = profile;
                string cryptedPassword = GetSavedPassword(Consts._SETTINGS_LOCATION + "\\" + Consts._PROFILES_LOCATION + "\\" + profile);
                string password = "";
                if (cryptedPassword != null)
                {
                    try
                    {
                        password = StringCipher.Decrypt(cryptedPassword, Environment.MachineName + Environment.UserName + "lol");
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Cannot decrypt saved password, wrong passPhrase or smthng: " + ex.Message);
                    }
                }

                collection.Add(new ByflyClient(login, password));
            }

            return collection;
        }


        private void ParserCallback(ByflyClient client)
        {
            client.GetAccountData();
        }
        private void parserCallback2()
        {
            try
            {
                //string login = Dispatcher.Invoke(new Func<string>(() => { return login2.Text; }));
                //string password = Dispatcher.Invoke(new Func<string>(() => { return password2.Text; }));
                //ByflyClient bfc = new ByflyClient("1652001316407", "2584");
                //bfc.GetAccountData();
            }
            catch { }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //_parserThread = new Thread(() => parserCallback());
            //_parserThread.Start();
            //_parserThread2 = new Thread(() => parserCallback2());
            //_parserThread2.Start();
        }

        private void SaveProfile()
        {

        }

        private List<string> GetSavedProfiles()
        {
            try
            {
                return RegistryWorker.GetKeyValue<string[]>(Microsoft.Win32.RegistryHive.LocalMachine, Consts._SETTINGS_LOCATION + "\\" + Consts._PROFILES_LOCATION, Consts._PROFILESLIST).ToList();
            }
            catch (System.IO.IOException ex)
            {
                _log.Error(ex.Message);
                if (CreateProfilesKey())
                    return GetSavedProfiles();
                else
                    return new List<string>();
            }
            catch (ArgumentNullException ex)
            {
                _log.Error(ex.Message);
                if (CreateProfilesListValue())
                    return GetSavedProfiles();
                else
                    return new List<string>();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return new List<string>();
            }
        }


        private bool CreateProfilesKey()
        {
            try
            {
              return RegistryWorker.CreateSubKey(Microsoft.Win32.RegistryHive.LocalMachine, Consts._SETTINGS_LOCATION + "\\" + Consts._PROFILES_LOCATION);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return false;
            }
        }

        private bool CreateProfilesListValue()
        {
            try
            {

               return RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, Consts._SETTINGS_LOCATION + "\\" + Consts._PROFILES_LOCATION, Microsoft.Win32.RegistryValueKind.MultiString, Consts._PROFILESLIST, new string[]{});
            }
            catch(Exception ex)
            {
                _log.Error(ex.Message);
                return false;
            }
        }

        private string GetSavedPassword(string location)
        {
            try
            {
                return RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, location, Consts._PASSWORD);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return "";
            }
        }

        private delegate void parserCallbackDelegate(ByflyClient bfc);

        private void UpdateAllProfiles()
        {
            parserCallbackDelegate ucd = ParserCallback;
            foreach (var item in ByFlyCollection)
            {
                //new Thread(() => parserCallback(item)).Start();
                ucd.BeginInvoke(item, null, null);
            }
        }

        private void loginTb_LostFocus(object sender, RoutedEventArgs e)
        {
            //var profile = (ByflyClient)mainListBox.SelectedItem;
            //string profileName = profile.Login;
            //if(!(sender as TextBox).Text.Equals(profileName))
            //    SaveProfile();

        }

        private void loginTb_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

    }


    [ValueConversion(typeof(String), typeof(String))]
    public class StringUpperConverter : IValueConverter
    {
        string text;

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            text = (string)value;
            return text.ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return text;
        }
    }

    [ValueConversion(typeof(String), typeof(String))]
    public class RequiredFieldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() + "*";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value.ToString();
            return str.Substring(0, str.Length - 2);
        }
    }
}
