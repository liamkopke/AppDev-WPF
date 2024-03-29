﻿using System;
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


namespace EnterpriseBudget.DeptBudgets.HomeBudget
{
    /// <summary>
    /// Interaction logic for PieChart.xaml
    /// </summary>
    public partial class UserControlWindow : Window
    {
        Presenter presenter;
        /// <summary>
        /// Calls the appropriate methods in the chart user control
        /// </summary>
        /// <param name="presenter">The current presenter being user, is sent to user control</param>
        public UserControlWindow(Presenter presenter)
        {
            InitializeComponent();
            
            this.presenter = presenter;
            //theDataGridView.presenter = presenter;
            //theDataGridView.DataSource = theDataGridView.presenter.GetDataSource();
            //theDataGridView.FillComboBox();
            //theDataGridView.cbMonths.SelectedIndex = theDataGridView.cbMonths.Items.Count - 1;
        }
        public int GetCountItems()
        {
            //return theDataGridView.cbMonths.Items.Count - 1;    // take into account TOTALS, so - 1.
            return 0;
        }
        public void ShowError()
        {
            MessageBox.Show("There is no data to create a chart.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        /// <summary>
        /// Displays the settings window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow();
            sw.Show();
        }
        /// <summary>
        /// Closes all windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCloseAllWindows_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you really want to force-close the app? Changes are automatically saved.",
                    "Close App",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}