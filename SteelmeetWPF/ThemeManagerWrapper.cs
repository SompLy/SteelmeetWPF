using System;
using System.Windows;

namespace SteelmeetWPF
{
    public class ThemeManagerWrapper
    {
        private ControlWindow controlWindow = null;
        public ThemeManagerWrapper( ControlWindow _controlWindow )
        {
            controlWindow = _controlWindow;
        }

        public void SetTheme( string themeName )
        {
            ThemeManager.SetTheme( themeName );

            // Re-Merge all Resource Dictionaries
            // Tabcontrol
            Application.Current.Resources.MergedDictionaries.Add( new ResourceDictionary
            {
                Source = new Uri( "TabControlStyleCustom.xaml", UriKind.Relative )
            } );

            // TabItem
            Application.Current.Resources.MergedDictionaries.Add( new ResourceDictionary
            {
                Source = new Uri( "TabItemStyleCustom.xaml", UriKind.Relative )
            } );

            // Button
            Application.Current.Resources.MergedDictionaries.Add( new ResourceDictionary
            {
                Source = new Uri( "ButtonStandardCustom.xaml", UriKind.Relative )
            } );

            // WeighInDataGridColumnHeader
            Application.Current.Resources.MergedDictionaries.Add( new ResourceDictionary
            {
                Source = new Uri( "WeighInDataGridColumnHeaderStyle.xaml", UriKind.Relative )
            } );
            // CheckBox
            Application.Current.Resources.MergedDictionaries.Add( new ResourceDictionary
            {
                Source = new Uri( "CheckBoxCustomStyle.xaml", UriKind.Relative )
            } );
            // DataGrid
            Application.Current.Resources.MergedDictionaries.Add( new ResourceDictionary
            {
                Source = new Uri( "DataGridCustom.xaml", UriKind.Relative )
            } );
            // ComboBox
            Application.Current.Resources.MergedDictionaries.Add( new ResourceDictionary
            {
                Source = new Uri( "ComboBoxCustomStyle.xaml", UriKind.Relative )
            } );

            //// 
            //Application.Current.Resources.MergedDictionaries.Add( new ResourceDictionary
            //{
            //    Source = new Uri( ".xaml", UriKind.Relative )
            //} );
        }
    }
}
