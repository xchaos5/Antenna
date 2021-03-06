﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AntennaLibrary
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const int MINIMUM_SPLASH = 1000;

        protected override async void OnStartup(StartupEventArgs e)
        {
            SplashScreen splash = new SplashScreen("Splash.png");
            splash.Show(false);

            base.OnStartup(e);
            MainWindow mainWindow = new MainWindow();
            var init = Task.Run(() => mainWindow.Initialize());

            await Task.Delay(MINIMUM_SPLASH);
            try
            {
                init.Wait();
            }
            catch (AggregateException ae)
            {
                string exception = "Initialization failed:" + Environment.NewLine;
                foreach (var innerException in ae.InnerExceptions)
                {
                    exception += innerException.Message + Environment.NewLine;
                }
                MessageBox.Show(exception);
                splash.Close(new TimeSpan(0));
                mainWindow.Close();
                return;
            }
            mainWindow.Show();
            splash.Close(new TimeSpan(0));
        }
    }
}
