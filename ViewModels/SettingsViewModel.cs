﻿using Frogy.Methods;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Frogy.Resources.Language;
using System.Windows.Controls;

namespace Frogy.ViewModels
{
    class SettingsViewModel : INotifyPropertyChanged
    {
        public SettingsViewModel()
        {
            DataPath = ((App)Application.Current).AppData.StoragePath;
        }

        private Visibility infoVisibility = Visibility.Collapsed;
        public Visibility InfoVisibility
        {
            get { return infoVisibility; }
            set 
            { 
                infoVisibility = value;
                OnPropertyChanged();
            }
        }

        #region Saved data path setting
        private string dataPath = "";
        public string DataPath
        {
            get
            {
                return dataPath;
            }
            set
            {
                dataPath = value;
                OnPropertyChanged();
            }
        }

        private ICommand changeDataPathButton;
        public ICommand ChangeDataPathButton
        {
            get
            {
                if (changeDataPathButton == null)
                {
                    changeDataPathButton = new RelayCommand(
                        param => this.ChangeDataPathButton_Click(),
                        param => true
                    );
                }
                return changeDataPathButton;
            }
        }
        private void ChangeDataPathButton_Click()
        {
            try
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    ((App)Application.Current).AppData.StoragePath = dialog.FileName;
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            DataPath = ((App)Application.Current).AppData.StoragePath;
        }
        #endregion

        #region Language setting
        private List<string> languageList = new List<string>();
        public List<string> LanguageList
        {
            get 
            {
                languageList.Clear();
                languageList.Add(LanguageHelper.SupportedLanguage[((App)Application.Current).AppData.LanguageSetting]);

                foreach (KeyValuePair<string, string> pair in LanguageHelper.SupportedLanguage)
                    if (pair.Key != ((App)Application.Current).AppData.LanguageSetting)
                        languageList.Add(pair.Value);

                return languageList;
            }
        }

        private int languageListSelectedIndex = 0;
        public int LanguageListSelectedIndex
        {
            get { return languageListSelectedIndex; }
            set
            {
                InfoVisibility = Visibility.Visible;
                ((App)Application.Current).AppData.LanguageSetting = LanguageHelper.InquireCodeName(languageList[value]);

                languageListSelectedIndex = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Theme setting
        private int themeListSelectedIndex = ((App)Application.Current).AppData.ThemeSetting >= 0 && ((App)Application.Current).AppData.ThemeSetting <= 1 ?
            ((App)Application.Current).AppData.ThemeSetting : 0;
        public int ThemeListSelectedIndex
        {
            get { return themeListSelectedIndex; }
            set
            {
                InfoVisibility = Visibility.Visible;
                if (value >= 0 && value <= 1)
                    ((App)Application.Current).AppData.ThemeSetting = value;

                themeListSelectedIndex = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region
        private bool startupStatus = MyDeviceHelper.GetStartupStatus();
        public bool StartupStatus
        {
            get { return startupStatus; }
            set
            {
                if (value)
                    MyDeviceHelper.RegisterStartup();
                else
                    MyDeviceHelper.DeregisterStartup();

                startupStatus = value;
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
