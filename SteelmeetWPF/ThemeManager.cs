using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SteelmeetWPF
{
    public static class ThemeManager
    {

        private static readonly Dictionary<string, Dictionary<string, SolidColorBrush>> Themes = new();
        private static string currentTheme = "Borlänge";

        static ThemeManager()
        {
            Themes[ "Borlänge" ] = new Dictionary<string, SolidColorBrush>
            {
                { "background",     new SolidColorBrush( Color.FromRgb( 52, 52, 52 ) )    },
                { "background2",    new SolidColorBrush( Colors.DarkGray )                },
                { "middleground",   new SolidColorBrush( Colors.GhostWhite )              },
                { "accent",         new SolidColorBrush( Color.FromRgb( 15, 76, 117 ) )   },
                { "fontColorLight", new SolidColorBrush( Colors.White )                   },
                { "fontColorDark",  new SolidColorBrush( Colors.Black )                   }
            };
            Themes[ "Khaki" ] = new Dictionary<string, SolidColorBrush>
            {
                { "background",     new SolidColorBrush( Color.FromRgb( 52, 52, 52 ) )    }, // Black
                { "background2",    new SolidColorBrush( Color.FromRgb( 142, 139, 130 ) ) }, // Grey
                { "middleground",   new SolidColorBrush( Color.FromRgb( 233, 220, 190 ) ) }, // Khaki
                { "accent",         new SolidColorBrush( Color.FromRgb( 142, 139, 130 ) ) }, // Grey ( Maybe should be a darker brown? )
                { "fontColorLight", new SolidColorBrush( Colors.White )                   },
                { "fontColorDark",  new SolidColorBrush( Colors.Black )                   }
            };
            Themes[ "OGBlue" ] = new Dictionary<string, SolidColorBrush>
            {
                { "background",     new SolidColorBrush(Color.FromRgb( 27, 38, 44 ) )     },
                { "background2",    new SolidColorBrush(Color.FromRgb( 15, 76, 117 ) )    },
                { "middleground",   new SolidColorBrush(Color.FromRgb( 15, 76, 117 ) )    },
                { "accent",         new SolidColorBrush(Color.FromRgb( 187, 225, 250 ) )  },
                { "fontColorLight", new SolidColorBrush(Colors.White )                    },
                { "fontColorDark",  new SolidColorBrush(Colors.Black )                    }
            };
        }

        public static SolidColorBrush background     => Themes[currentTheme][ "background"     ];
        public static SolidColorBrush background2    => Themes[currentTheme][ "background2"    ];
        public static SolidColorBrush middleground   => Themes[currentTheme][ "middleground"   ];
        public static SolidColorBrush accent         => Themes[currentTheme][ "accent"         ];
        public static SolidColorBrush fontColorLight => Themes[currentTheme][ "fontColorLight" ];
        public static SolidColorBrush fontColorDark  => Themes[currentTheme][ "fontColorDark"  ];

        public static void SetTheme( string themeName ) 
        { 
            if( Themes.ContainsKey(themeName) )
                currentTheme = themeName;
            else
                throw new KeyNotFoundException( $"Theme '{themeName}' does not exist." );
        }

        public static string[] GetAvailableThemes() // For my epic UI to get a list of themes so it's more automagic
        {
            return Themes.Keys.ToArray();
        }
    }
    public class ThemeManagerWrapper : DependencyObject
    {
        public SolidColorBrush background => ThemeManager.background;
        public SolidColorBrush background2 => ThemeManager.background2;
        public SolidColorBrush middleground => ThemeManager.middleground;
        public SolidColorBrush accent => ThemeManager.accent;
        public SolidColorBrush fontColorLight => ThemeManager.fontColorLight;
        public SolidColorBrush fontColorDark => ThemeManager.fontColorDark;
    }
}