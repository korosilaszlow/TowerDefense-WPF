using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TowerDefenseView
{
    /// <summary>
    /// Vásárlásra használatos gombot megvalósító osztály
    /// </summary>
    public class BuyButton : Button
    {
        #region Fields
        private string itemImageSource;
        private string moneyImageSource;
        private string toolTipText;

        private Grid grid;
        private Grid subGrid;

        private Image itemImage;
        private Image moneyImage;

        private Label moneyLabel;

        private Thickness defaultThickness = new Thickness(1);
        private Thickness highLightedThickness = new Thickness(6);

        private DependencyPropertyDescriptor moneyDPD;
        private DependencyPropertyDescriptor highLightDPD;
        /// <summary>
        /// <see cref="DependencyProperty"/> a <see cref="Content"/> Property-hez
        /// </summary>
        public static new readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(object), typeof(BuyButton),
                                        new FrameworkPropertyMetadata(null,
                                        FrameworkPropertyMetadataOptions.AffectsRender |
                                        FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        /// <summary>
        /// <see cref="DependencyProperty"/> a <see cref="MoneyCost"/> Property-hez
        /// </summary>
        public static readonly DependencyProperty MoneyCostProperty =
            DependencyProperty.Register(name: nameof(MoneyCost),
                                        propertyType: typeof(int),
                                        ownerType: typeof(BuyButton),
                                        typeMetadata: new FrameworkPropertyMetadata()
                                        );
        /// <summary>
        /// <see cref="DependencyProperty"/> a <see cref="HighLightNum"/> Property-hez
        /// </summary>
        public static readonly DependencyProperty HighLightProperty =
            DependencyProperty.Register(name: nameof(HighLightNum),
                                        propertyType: typeof(int),
                                        ownerType: typeof(BuyButton),
                                        typeMetadata: new FrameworkPropertyMetadata()
            );


        #endregion
        #region Properties
        /// <summary>
        /// Megadja, hogy a <see cref="HighLightNum"/> melyik értéke esetén kerüljön kiemelt állapotba a gomb
        /// </summary>
        public int HighLightCondition { get; set; }
        /// <summary>
        /// A gomb elsődleges képének forrása fájlnévként
        /// </summary>
        public string ItemImageSource
        {
            get => itemImageSource;
            set
            {
                itemImageSource = value;
                itemImage.Source = new BitmapImage(new Uri(value, UriKind.Relative));
            }
        }
        /// <summary>
        /// A gomb pénzikon képének forrása fájlnévként
        /// </summary>
        public string MoneyImageSource
        {
            get => moneyImageSource;
            set
            {
                moneyImageSource = value;
                moneyImage.Source = new BitmapImage(new Uri(value, UriKind.Relative));
            }
        }
        /// <summary>
        /// A tooltip szövege
        /// </summary>
        public string ToolTipText
        {
            get => toolTipText;
            set
            {
                toolTipText = value;
                this.ToolTip = value;
            }
        }
        /// <summary>
        /// A megjelenített ár
        /// </summary>
        public int MoneyCost
        {
            get => (int)GetValue(MoneyCostProperty);
            set => SetValue(MoneyCostProperty, value);
        }
        /// <summary>
        /// Külső állapotot tárol, ha megegyezik <see cref="HighLightCondition"/>-nal, akkor a gomb megjelenése megváltozik
        /// </summary>
        public int HighLightNum
        {
            get => (int)GetValue(HighLightProperty);
            set => SetValue(HighLightProperty, value);
        }

        #endregion       
        #region Constructor
        /// <summary>
        /// Konstruktor
        /// </summary>
        public BuyButton()
        {
            this.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            this.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            this.BorderThickness = defaultThickness;

            grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            subGrid = new Grid();
            subGrid.ColumnDefinitions.Add(new ColumnDefinition());
            subGrid.ColumnDefinitions.Add(new ColumnDefinition());

            itemImage = new Image();
            moneyImage = new Image();

            moneyLabel = new Label();

            Grid.SetRow(itemImage, 0);
            grid.Children.Add(itemImage);

            Grid.SetRow(subGrid, 1);
            grid.Children.Add(subGrid);

            Grid.SetColumn(moneyLabel, 0);
            subGrid.Children.Add(moneyLabel);

            Grid.SetColumn(moneyImage, 1);
            subGrid.Children.Add(moneyImage);

            moneyLabel.FontSize = 20;
            moneyLabel.DataContext = MoneyCostProperty;
            moneyDPD = DependencyPropertyDescriptor.FromProperty(MoneyCostProperty, typeof(BuyButton));
            moneyDPD.AddValueChanged(this, MoneyCost_Changed);

            highLightDPD = DependencyPropertyDescriptor.FromProperty(HighLightProperty, typeof(BuyButton));
            highLightDPD.AddValueChanged(this, HighLightValue_Changed);

            this.ToolTip = "";

            this.Content = grid;
        }
        #endregion
        #region Private methods
        private void HighLightValue_Changed(object sender, EventArgs e)
        {
            if (HighLightNum == HighLightCondition)
            {
                this.BorderThickness = highLightedThickness;

            }
            else
            {
                this.BorderThickness = defaultThickness;
            }
        }

        private void MoneyCost_Changed(object sender, EventArgs e)
        {
            moneyLabel.Content = MoneyCost;
        }
        #endregion
    }
}
