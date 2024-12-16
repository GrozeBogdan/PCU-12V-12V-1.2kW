using System.Windows;

public static class ThemeManager
{
    public static Theme CurrentTheme { get; private set; }

    public static void ApplyTheme(Theme theme)
    {
        CurrentTheme = theme;

        // Update application-wide resources
        Application.Current.Resources["PrimaryBackground"] = theme.PrimaryBackground;
        Application.Current.Resources["SecondaryBackground"] = theme.SecondaryBackground;
        Application.Current.Resources["PrimaryForeground"] = theme.PrimaryForeground;
        Application.Current.Resources["SecondaryForeground"] = theme.SecondaryForeground;

        Application.Current.Resources["WindowBackground"] = theme.WindowBackground;
        Application.Current.Resources["ButtonAndHighlightBackground"] = theme.ButtonAndHighlightBackground;
        Application.Current.Resources["PanelBackground"] = theme.PanelBackground;
        Application.Current.Resources["TextForeground"] = theme.TextForeground;
        Application.Current.Resources["ButtonContrast"] = theme.ButtonContrast;
        Application.Current.Resources["SVGBackground"] = theme.SVGBackground;
    }
}
