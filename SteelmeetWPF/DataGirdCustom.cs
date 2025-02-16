using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace SteelmeetWPF
{
    class DataGirdCustom : DataGrid
    {
        private readonly HashSet<string> _hiddenColumns = new(); // Stores hidden columns dynamically

        public DataGirdCustom()
        {
            this.AutoGeneratingColumn += OnAutoGeneratingColumn;
        }

        private void OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (_hiddenColumns.Contains(e.PropertyName))
            {
                e.Cancel = true;
                return;
            }

            PropertyInfo? property = typeof(ControlDgFormat).GetProperty(e.PropertyName); ;
            if (sender is DataGrid dataGrid)
            {            
                if( dataGrid.Name == "controlDg" )
                {
                    property = typeof( ControlDgFormat ).GetProperty( e.PropertyName );
                }
                else if( dataGrid.Name == "weightInDg")
                {
                    property = typeof( WeighInDgFormat ).GetProperty( e.PropertyName );
                }
                //else if( sender == spectatorDg )
                //{
                //    property = typeof( SpectatorDgFormat ).GetProperty( e.PropertyName );
                //}
            }
            
            var displayAttr = property?.GetCustomAttribute<DisplayAttribute>();
            if (displayAttr != null)
                e.Column.Header = displayAttr.Name;
        }
        public void ToggleColumnVisibility(string columnName, bool isVisible)
        {
            var column = this.Columns.FirstOrDefault(c => c.Header?.ToString() == columnName);
            if (column != null)
                column.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            else if (!isVisible)
                _hiddenColumns.Add(columnName);
        }
    }
}
