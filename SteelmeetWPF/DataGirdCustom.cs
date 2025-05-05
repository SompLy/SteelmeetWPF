using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Globalization;

namespace SteelmeetWPF
{
    class DataGirdCustom : DataGrid
    {
        private readonly HashSet<string> _hiddenColumns = new(); // Stores hidden columns dynamically

        public DataGirdCustom()
        {
            this.AutoGeneratingColumn += OnAutoGeneratingColumn;

            this.Loaded += ( s, e ) => FontAutoScale();
            this.SizeChanged += ( s, e ) => FontAutoScale();

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
                else if( dataGrid.Name == "spectatorDg")
                {
                    property = typeof( SpectatorDgFormat ).GetProperty( e.PropertyName );
                }
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

        private void FontAutoScale()
        {
            if (this.Columns == null || this.Columns.Count == 0)
                return;

            AutoSizeColumns();
            this.Dispatcher.BeginInvoke
                ( System.Windows.Threading.DispatcherPriority.ContextIdle, new Action( () =>
                {
                    //FontSizeCalculation();             
                } ) );
        }
        private void FontSizeCalculation()
        {
            double totalWidth = this.ActualWidth;
            double dataGridWidth = GetDataGridActualWidth();

            double multiplier = totalWidth / dataGridWidth;
            this.FontSize = Math.Clamp( this.FontSize * multiplier, 0, 35791 );

            //this.FontSize *= double.Parse( multiplier.ToString().Replace( ',', '.' ) );
        }
        private double GetDataGridActualWidth()
        {
            double width = 0;
            for (int i = 0; i < this.Columns.Count; i++)
                width += this.Columns [i].ActualWidth;
            return width;
        }
      
        public void AutoSizeColumns()
        {
            double minWidth = 100;

            foreach (var column in this.Columns)
            {
                column.Width = 10;
            }

            this.Dispatcher.BeginInvoke
                ( System.Windows.Threading.DispatcherPriority.ContextIdle, new Action( () =>
                {
                    foreach (var column in this.Columns)
                    {
                        //   column.Width = new DataGridLength( 1, DataGridLengthUnitType.SizeToCells );
                        //   column.Width = new DataGridLength( 1, DataGridLengthUnitType.SizeToHeader );
                        column.Width = new DataGridLength( 1, DataGridLengthUnitType.Auto );
                    }
                } ) );
            //7if ( true )
            //7    return;


            this.Dispatcher.BeginInvoke
                ( System.Windows.Threading.DispatcherPriority.ContextIdle, new Action( () =>
            {
                Random kalle = new Random();
                    foreach (var column in this.Columns)
                    {
                        //column.Width = kalle.Next(20, 100);
                        //if (column.ActualWidth < minWidth )
                        //    column.Width = new DataGridLength( minWidth, DataGridLengthUnitType.Pixel );
                    }
            } ) );
        }
    }
}
