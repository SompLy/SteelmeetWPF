﻿using System;
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
using System.Windows.Media;
using System.Collections.Specialized;

namespace SteelmeetWPF
{
    public class DataGirdCustom : DataGrid
    {
        private readonly HashSet<string> _hiddenColumns = new(); // Stores hidden columns dynamically

        public DataGirdCustom()
        {
            this.AutoGeneratingColumn += OnAutoGeneratingColumn;

            this.Loaded += ( s, e ) => AutoScaleDataGrid();
            this.SizeChanged += ( s, e ) => AutoScaleDataGrid();
        }

        private void OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (_hiddenColumns.Contains(e.PropertyName))
            {
                e.Cancel = true;
                return;
            }

            PropertyInfo? property = typeof(DgFormat).GetProperty(e.PropertyName); ;
            if (sender is DataGrid dataGrid)
            {            
                if( dataGrid.Name == "                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  " )
                {
                    property = typeof( DgFormat ).GetProperty( e.PropertyName );
                }
                else if( dataGrid.Name == "weightInDg")
                {
                    property = typeof( WeighInDgFormat ).GetProperty( e.PropertyName );
                }
                else if( dataGrid.Name == "spectatorDg")
                {
                    property = typeof( DgFormat ).GetProperty( e.PropertyName );
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

        public void AutoScaleDataGrid()
        {
            if (this.Columns == null || this.Columns.Count == 0)
                return;

            AutoSizeColumns();
            AutoSizeFont();

            this.Dispatcher.BeginInvoke // This is dumb... But this works
            ( System.Windows.Threading.DispatcherPriority.ContextIdle, new Action( () =>
            {
                AutoSizeColumns();
                AutoSizeFont();
                this.Dispatcher.BeginInvoke
                ( System.Windows.Threading.DispatcherPriority.ContextIdle, new Action( () =>
                {
                    AutoSizeColumns();
                    AutoSizeFont();
                    this.Dispatcher.BeginInvoke
                    ( System.Windows.Threading.DispatcherPriority.ContextIdle, new Action( () =>
                    {
                        AutoSizeColumns();
                        AutoSizeFont();
                    } ) );
                } ) );
            } ) );
        }


        private void AutoSizeFont()
        {
            double totalWidth = this.ActualWidth;
            
            if ( IsVerticalScrollBarVisible() )
            {
                totalWidth -= SystemParameters.VerticalScrollBarWidth;
            }

            double dataGridWidth = this.Columns.Sum(c => c.ActualWidth); //GetDataGridActualWidth();

            double multiplier = totalWidth / dataGridWidth;
            this.FontSize = Math.Clamp( this.FontSize * multiplier, 10, 35791 );
        }
        private bool IsVerticalScrollBarVisible()
        {
            if (VisualTreeHelper.GetChildrenCount( this ) == 0)
                return false;

            Decorator border = VisualTreeHelper.GetChild( this, 0) as Decorator;
            if (border is null || border.Child is not ScrollViewer scrollViewer)
                return false;

            return scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible;
        }

        public void AutoSizeColumns()
        {
            double minWidth = 100;

            foreach (var column in this.Columns)
            {
                column.Width = 10;
            }

            foreach (var column in this.Columns)
            {
                DataGridLength value = new DataGridLength( 1, DataGridLengthUnitType.Auto );
                column.Width = value;
            }

        }
    }
}
