using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TowerDefenseView
{

    public partial class ExtraSettingsWindow : Window
    {
        #region Fields
        private int rows = 5;
        private int cols = 5;
        private int money = 100;
        private Brush blackBrush = new SolidColorBrush(Colors.Black);
        private Brush redBrush = new SolidColorBrush(Colors.Red);
        #endregion
        #region Properties
        /// <summary>
        /// A pálya sorainak száma
        /// </summary>
        public int Rows
        {
            get => rows; set
            {
                if (0 < value)
                    rows = value;
                RowsTextBox.Text = Rows.ToString();
                CheckValues();
            }
        }
        /// <summary>
        /// A pálya oszlopainak száma
        /// </summary>
        public int Cols
        {
            get => cols; set
            {
                if (0 < value)
                {
                    cols = value;
                    ColsTextBox.Text = Cols.ToString();
                    CheckValues();
                }
            }
        }
        /// <summary>
        /// A játékosok kezdőtőkéje
        /// </summary>
        public int Money
        {
            get => money; set
            {
                if (0 <= value)
                {
                    money = value;
                    MoneyTextBox.Text = Money.ToString();
                    CheckValues();
                }
            }
        }
        #endregion
        #region Constructor
        /// <summary>
        /// Konstruktor
        /// </summary>
        public ExtraSettingsWindow()
        {
            InitializeComponent();
            MoneyTextBox.KeyUp += MoneyTextBox_KeyUp;
            RowsTextBox.KeyUp += RowsTextBox_KeyUp;
            ColsTextBox.KeyUp += ColsTextBox_KeyUp;
            MoneyTextBox.FontSize = 20;
            RowsTextBox.FontSize = 20;
            ColsTextBox.FontSize = 20;

            MoneyPlus.Click += (sender, args) => { Money += 10; };
            MoneyMinus.Click += (sender, args) => { Money -= 10; };
            RowsPlus.Click += (sender, args) => { Rows++; };
            RowsMinus.Click += (sender, args) => { Rows--; };
            ColsPlus.Click += (sender, args) => { Cols++; };
            ColsMinus.Click += (sender, args) => { Cols--; };
        }
        #endregion
        #region Private methods
        private void CheckValues()
        {
            okButton.IsEnabled = true;
            if (4 <= Rows && Rows <= 10)
            {
                RowsTextBox.Foreground = blackBrush;
            }
            else
            {
                RowsTextBox.Foreground = redBrush;
                okButton.IsEnabled = false;
            }

            if (4 <= Cols && Cols <= 10)
            {
                ColsTextBox.Foreground = blackBrush;
            }
            else
            {
                ColsTextBox.Foreground = redBrush;
                okButton.IsEnabled = false;
            }

            if (100 <= Money && Money <= 2000)
            {
                MoneyTextBox.Foreground = blackBrush;
            }
            else
            {
                MoneyTextBox.Foreground = redBrush;
                okButton.IsEnabled = false;
            }
        }

        private void ColsTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            int cols_new;
            if (int.TryParse((sender as TextBox).Text, out cols_new))
            {
                cols = cols_new;
            }

            (sender as TextBox).Text = cols.ToString();
            CheckValues();
        }

        private void RowsTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            int rows_new;
            if (int.TryParse((sender as TextBox).Text, out rows_new))
            {
                rows = rows_new;
            }

            (sender as TextBox).Text = rows.ToString();
            CheckValues();
        }

        private void MoneyTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            int money_new;
            if (int.TryParse((sender as TextBox).Text, out money_new))
            {
                money = money_new;
            }

            (sender as TextBox).Text = money.ToString();
            CheckValues();
        }
        #endregion
    }
}
