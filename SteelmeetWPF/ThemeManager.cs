using System;
using System.Windows;

namespace SteelmeetWPF
{
    public static class ThemeManager
    {
        public static void SetTheme( string themeName )
        {
            var dict = new ResourceDictionary();
            switch( themeName )
            {
                case "Borlänge":
                    dict.Source = new Uri( "Themes/BorlangeTheme.xaml", UriKind.Relative );
                    break;
                case "Khaki":
                    dict.Source = new Uri( "Themes/KhakiTheme.xaml", UriKind.Relative );
                    break;
                case "OGBlue":
                    dict.Source = new Uri( "Themes/OGBlue.xaml", UriKind.Relative );
                    break;
                // Add cases for other themes
                default:
                    throw new ArgumentException( "Theme not found." );
            }

            // Clear existing merged dictionaries and add new one
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add( dict );
        }

        public static string[] GetAvailableThemes()
        {
            return new[] { "Borlänge", "Khaki", "OGBlue" }; // Add other theme names here
        }
    }

}