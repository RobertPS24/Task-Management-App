﻿using System.Windows;

namespace TaskManagementApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            var login = new LoginWindow();

        }
    }
}
