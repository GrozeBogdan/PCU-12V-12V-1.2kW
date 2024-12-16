using System.Windows.Media;

    /// <summary>
    /// Asserts the different type of backgrounds and foregrounds used in a UC.
    /// </summary>
    public class Theme
    {
        public Brush PrimaryBackground { get; set; }
        public Brush SecondaryBackground { get; set; }
        public Brush PrimaryForeground { get; set; }
        public Brush SecondaryForeground { get; set; }

        public Brush WindowBackground { get; set; }
        public Brush ButtonAndHighlightBackground { get; set; } 
        public Brush PanelBackground { get; set; }
        public Brush TextForeground { get; set; }
        public Brush ButtonContrast { get; set; }
        public Brush SVGBackground { get; set; }
    }


/*  If you want to add a preset just follow this example:
    Make sure to change this example to use acordingly to your needs (ex: If you use RGB or HEX values; not predefined).

    public static Theme YourThemeName = new Theme
    {
        PrimaryBackground = new SolidColorBrush(YourColor),
        SecondaryBackground = new SolidColorBrush(YourColor),
        PrimaryForeground = new SolidColorBrush(YourColor),
        SecondaryForeground = new SolidColorBrush(YourColor)
    };
*/

/// <summary>
/// Contains all the available theme presets.
/// </summary>
/// 
    public static class ThemePresets
    {
        public static Theme LightTheme = new Theme
        {
            PrimaryBackground = new SolidColorBrush(Colors.White),
            SecondaryBackground = new SolidColorBrush(Colors.LightGray),
            PrimaryForeground = new SolidColorBrush(Colors.Black),
            SecondaryForeground = new SolidColorBrush(Colors.DarkGray)
        };

        public static Theme DarkTheme = new Theme
        {
            PrimaryBackground = new SolidColorBrush(Colors.Black),
            SecondaryBackground = new SolidColorBrush(Colors.Gray),
            PrimaryForeground = new SolidColorBrush(Colors.White),
            SecondaryForeground = new SolidColorBrush(Colors.LightGray)
        };

        public static Theme OlaMamiCheSamset = new Theme
        { 
            WindowBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#451952")),
            ButtonAndHighlightBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5D3370")),
            PanelBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF310F38")),
            TextForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE8BC89")),
            ButtonContrast = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF442452")),
            SVGBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffe8bcb9"))
        };

        public static Theme PinkGuy = new Theme
        {
            WindowBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8546F0")),
            ButtonAndHighlightBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5E4DFF")),
            PanelBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEC7D7")),
            TextForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0E172C")),
            ButtonContrast = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF442452")),
            SVGBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0E172C"))
        };
}