using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace PCU_GUI_Idea
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    DataTemplate headerTemplate = new DataTemplate();
        //    FrameworkElementFactory textBlock = new FrameworkElementFactory(typeof(TextBlock));

        //    textBlock.SetValue(TextBlock.TextProperty, "PCU GUI");
        //    textBlock.SetValue(TextBlock.ForegroundProperty, (Brush)new BrushConverter().ConvertFromString("#E8BC89")); // Custom Foreground color
        //    textBlock.SetValue(TextBlock.FontSizeProperty, 24.0); // Custom FontSize
        //    textBlock.SetValue(TextBlock.MarginProperty, new Thickness(10)); // Custom Margin
        //    textBlock.SetValue(TextBlock.WidthProperty, 200.0); // Custom Margin
        //    textBlock.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left);

        //    headerTemplate.VisualTree = textBlock;

        //    RadWindow radWindow = new RadWindow
        //    {
        //        Width = 1366,
        //        Height = 768,
        //        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen, // Center on screen
        //        Background = (Brush)new BrushConverter().ConvertFromString("#451952"),
        //        Foreground = (Brush)new BrushConverter().ConvertFromString("#E8BC89"),
        //        FontSize = 24,
        //        FontFamily = new FontFamily("Marina Budarina"),
        //        FontWeight = FontWeights.Medium,
        //        CornerRadius = new CornerRadius(24),
        //        HeaderTemplate = headerTemplate,
        //    };

        //    StyleManager.SetTheme(radWindow, new Windows11Theme());

        //    Windows11ThemeSizeHelper.SetEnableDynamicAnimation(radWindow, true);

        //    radWindow.Owner = null;  // or Application.Current.MainWindow
        //    radWindow.Show();
        //}

    }
}
