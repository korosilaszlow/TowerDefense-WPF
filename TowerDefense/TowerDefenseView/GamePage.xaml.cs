using System;
using System.Windows;
using System.Windows.Controls;
using TowerDefenseViewModel;

namespace TowerDefenseView
{
    /// <summary>
    /// Az alapjátékot megjelenítő lap
    /// </summary>
    public partial class GamePage : Page
    {
        private MainWindow mainWindow;

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="parentWindow">Ezen <see cref="Page"/>-et tartalmazó <see cref="MainWindow"/></param>
        public GamePage(MainWindow parentWindow)
        {
            InitializeComponent();
            this.mainWindow = parentWindow;

            mainWindow.SizeChanged += MainWindowSizeChanged;
            mainWindow.ContentRendered += MainWindowSizeChanged;
            mainWindow.ContentFrame.ContentRendered += MainWindowSizeChanged;
        }
        /// <summary>
        /// Az ablak megjelenésének frissítése
        /// </summary>
        public void RefreshSizes()
        {
            if (DataContext is not GameViewModel)
            {
                return;
            }

            if (!mainWindow.IsActive || !(DataContext as GameViewModel).Active)
            {
                return;
            }
            int rows = (DataContext as GameViewModel).GridRows;
            int cols = (DataContext as GameViewModel).GridCols;
            double allowedHeight = Math.Max(gridRowDef.ActualHeight, mainWindow.MinHeight / 2);
            itemsControl.Height = allowedHeight - 10;
            double newWidth = itemsControl.Height * cols / rows;

            gridWidthCol.Width = new GridLength(newWidth);
            itemsControl.Width = newWidth;
            mainWindow.MinWidth = newWidth + 50;

        }
        private void MainWindowSizeChanged(object sender, EventArgs e)
        {
            RefreshSizes();
        }
    }
}
