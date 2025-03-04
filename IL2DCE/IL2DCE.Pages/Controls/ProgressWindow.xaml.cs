// IL2DCE: A dynamic campaign engine for IL-2 Sturmovik: Cliffs of Dover Blitz + Desert Wings
// Copyright (C) 2016 Stefan Rothdach & 2025 silkyskyj
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows;

namespace IL2DCE.Pages.Controls
{
    /// <summary>
    /// ProgressWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public delegate void BackgrowndWorkerEventHandler(object sender, BackgrowndWorkerEventArgs e);

        public event BackgrowndWorkerEventHandler BackgrowndWorkerEvent;

        private BackgroundWorker worker = new BackgroundWorker();

        public bool IsCanceled
        {
            get;
            private set;
        }

        public bool IsCompleted
        {
            get;
            private set;
        }

        public ProgressWindowModel Model
        {
            get;
            set;
        }

        public ProgressWindow(ProgressWindowModel context, BackgrowndWorkerEventHandler action)
        {
            InitializeComponent();

            Model = context;
            // Model.PropertyChanged += new PropertyChangedEventHandler(this.PropertyChanged);

            this.BackgrowndWorkerEvent += action;

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += DoWork;
            worker.ProgressChanged += ProgressChanged;
            worker.RunWorkerCompleted += RunWorkerCompleted;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            worker.RunWorkerAsync(Model);
        }

        public void PropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Title":
                    Title = Model.Title;
                    break;
                case "Status":
                    labelStatus.Content = Model.Status;
                    break;
                case "Minimum":
                    progressBar.Minimum = Model.Minimum;
                    break;
                case "Maximum":
                    progressBar.Maximum = Model.Maximum;
                    break;
                case "Percent":
                    textBlockPercent.Text = Model.Percent;
                    break;
            }
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            if (BackgrowndWorkerEvent != null)
            {
                BackgrowndWorkerEvent(sender, new BackgrowndWorkerEventArgs(e));
            }
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage >= 0)
            {
                progressBar.Value = e.ProgressPercentage;
                labelStatus.Content = e.UserState as string;
            }
            else
            {
                int minimum = 0;
                int maximum = 100;
                string[] nums = (e.UserState as string).Split('|');
                if (nums.Length >= 2)
                {
                    int.TryParse(nums[0], out minimum);
                    int.TryParse(nums[1], out maximum);
                    if (minimum > maximum || minimum < 0 || maximum < 0)
                    {
                        minimum = 0;
                        maximum = 100;
                    }
                }
                progressBar.Minimum = minimum;
                progressBar.Maximum = maximum;
                progressBar.Value = 0;
                labelStatus.Content = string.Empty;
            }
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsCompleted = true;
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!IsCompleted)
            {
                if (IsCanceled || CancelConfirmation())
                {
                    worker.CancelAsync();
                    IsCanceled = true;
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (CancelConfirmation())
            {
                worker.CancelAsync();
                IsCanceled = true;
            }
        }

        private bool CancelConfirmation()
        {
            return MessageBox.Show("Are you sure you want to cancel this process ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }

    public class BackgrowndWorkerEventArgs : EventArgs
    {
        public DoWorkEventArgs Args
        {
            get;
            set;
        }

        public BackgrowndWorkerEventArgs(DoWorkEventArgs args)
        {
            Args = args;
        }
    }

    public class ProgressWindowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Property

        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                NotifyPropertyChanged("Title");
            }
        }
        private string title = string.Empty;

        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                NotifyPropertyChanged("Status");
            }
        }
        private string status = string.Empty;

        public int Minimum
        {
            get
            {
                return minimum;
            }
            set
            {
                minimum = value;
                NotifyPropertyChanged("Minimum");
            }
        }
        private int minimum = 0;

        public int Maximum
        {
            get
            {
                return maximum;
            }
            set
            {
                maximum = value;
                NotifyPropertyChanged("Maximum");
            }
        }
        private int maximum = 100;

        public int Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                int range = (maximum - minimum) + 1;
                int percent = (int)(((double)value / range) * 100);
                this.percent = percent.ToString() + "%";
                NotifyPropertyChanged("Value");
            }
        }
        private int value = 0;

        public string Percent
        {
            get
            {
                return percent;
            }
            set
            {
                percent = value;
                NotifyPropertyChanged("Percent");
            }
        }
        private string percent = string.Empty;

        public object Context
        {
            get;
            set;
        }

        public object Result
        {
            get;
            set;
        }

        #endregion

        public ProgressWindowModel()
        {
        }

        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
