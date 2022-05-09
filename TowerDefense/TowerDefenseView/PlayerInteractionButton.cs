using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TowerDefenseView
{
    /// <summary>
    /// Játékos művelet kiválasztásra használatos gomb
    /// </summary>
    public class PlayerInteractionButton : Button
    {
        #region Fields
        private string toolTipText;

        private Image itemImage;

        private Thickness defaultThickness = new Thickness(1);
        private Thickness highLightedThickness = new Thickness(6);

        private DependencyPropertyDescriptor highLightDPD;
        private DependencyPropertyDescriptor itemImageSrcDPD;
        /// <summary>
        /// <see cref="DependencyProperty"/> a <see cref="HighLightNum"/> Property-hez
        /// </summary>
        public static readonly DependencyProperty HighLightProperty = DependencyProperty.Register(
          name: nameof(HighLightNum),
          propertyType: typeof(int),
          ownerType: typeof(PlayerInteractionButton),
          typeMetadata: new FrameworkPropertyMetadata()
          );
        /// <summary>
        /// <see cref="DependencyProperty"/> az <see cref="ItemImageSource"/> Property-hez
        /// </summary>
        public static readonly DependencyProperty ItemImageSourceProperty = DependencyProperty.Register(
          name: nameof(ItemImageSource),
          propertyType: typeof(string),
          ownerType: typeof(PlayerInteractionButton),
          typeMetadata: new FrameworkPropertyMetadata()
          );

        #endregion
        #region Properties
        /// <summary>
        /// Külső állapotot tárol, ha megegyezik <see cref="HighLightCondition"/>-nal, akkor a gomb megjelenése megváltozik
        /// </summary>
        public int HighLightNum
        {
            get => (int)GetValue(HighLightProperty);
            set => SetValue(HighLightProperty, value);
        }
        /// <summary>
        /// A gomb elsődleges képének forrása fájlnévként
        /// </summary>
        public string ItemImageSource
        {
            get => (string)GetValue(ItemImageSourceProperty);
            set => SetValue(ItemImageSourceProperty, value);
        }
        /// <summary>
        /// Megadja, hogy a <see cref="HighLightNum"/> melyik értéke esetén kerüljön kiemelt állapotba a gomb
        /// </summary>
        public int HighLightCondition { get; set; }
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
        #endregion
        #region Constructor
        /// <summary>
        /// Konstruktor
        /// </summary>
        public PlayerInteractionButton()
        {
            this.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            this.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            this.BorderThickness = defaultThickness;
            itemImage = new Image();
            this.Content = itemImage;

            highLightDPD = DependencyPropertyDescriptor.FromProperty(HighLightProperty, typeof(PlayerInteractionButton));
            highLightDPD.AddValueChanged(this, HighLightValue_Changed);

            itemImageSrcDPD = DependencyPropertyDescriptor.FromProperty(ItemImageSourceProperty, typeof(PlayerInteractionButton));
            itemImageSrcDPD.AddValueChanged(this, ItemImageSourceValue_Changed);

        }
        #endregion
        #region Private methods
        private void ItemImageSourceValue_Changed(object sender, EventArgs e)
        {
            itemImage.Source = new BitmapImage(new Uri(ItemImageSource, UriKind.Relative));

        }
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
        #endregion
    }
}
