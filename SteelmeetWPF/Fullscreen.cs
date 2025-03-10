﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SteelmeetWPF
{
    internal class Fullscreen
    {
        public void ToggleFullscreen( bool _isFullscreen, Window _window)
        {
            if ( _isFullscreen ) 
            {
                _window.ResizeMode = ResizeMode.CanResize;
                _window.WindowState = WindowState.Normal;
            }
            else 
            {
                _window.ResizeMode = ResizeMode.NoResize;
                _window.WindowState = WindowState.Maximized;
            }
        }
    }
}
