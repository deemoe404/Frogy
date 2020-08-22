using Frogy.Methods;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Frogy.ViewModels
{
    class OptionViewModel : INotifyPropertyChanged
    {
        public OptionViewModel()
        {
            DataPath = ((App)Application.Current).appData.StoragePath;
        }

        /// <summary>
        /// 数据保存路径
        /// </summary>
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
        #region 更改数据路径按钮
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
                    ((App)Application.Current).appData.StoragePath = dialog.FileName;
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            DataPath = ((App)Application.Current).appData.StoragePath;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
