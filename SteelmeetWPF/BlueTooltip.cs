//using DocumentFormat.OpenXml.InkML;
//using System;
//using System.Drawing;
//using System.Windows.Forms;

//namespace SteelmeetWPF
//{
//    public class BlueToolTip : ToolTip
//    {
//        public BlueToolTip(  )
//        {
//            this.AutoPopDelay = 10000;
//            this.InitialDelay = 100;
//            this.OwnerDraw = true;

//            this.Draw += tip_Blue_Draw;
//            this.Popup += tip_Blue_Popup;
//        }
//        private void tip_Blue_Popup( object sender, PopupEventArgs e ) 
//        {
//            Font font = new Font( "Segoe UI", 10, FontStyle.Bold );
//            var temp = TextRenderer.MeasureText( this.GetToolTip( e.AssociatedControl ), font ).Height;
//            e.ToolTipSize = TextRenderer.MeasureText( this.GetToolTip( e.AssociatedControl ), font );
//        }
        
//        private void tip_Blue_Draw( object sender, DrawToolTipEventArgs e )
//        {
//            Font font = new Font( "Segoe UI", 10, FontStyle.Bold );
//            SolidBrush textBrush = new SolidBrush( Color.FromArgb( 187, 225, 250 ) );
//            SolidBrush rectBrush = new SolidBrush( Color.FromArgb( 27, 38, 44 ) );

//            StringFormat sf = new StringFormat();
//            sf.Alignment = StringAlignment.Center;
//            sf.LineAlignment = StringAlignment.Center;
//            sf.FormatFlags = StringFormatFlags.NoWrap;

//            // Draw Background
//            e.Graphics.FillRectangle( rectBrush, new Rectangle( e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height ) );

//            // Draw Boarder based on Bounds
//            ControlPaint.DrawBorder( e.Graphics, e.Bounds, SystemColors.WindowFrame, ButtonBorderStyle.Solid );

//            // Draw Text
//            e.Graphics.DrawString( e.ToolTipText, font, textBrush, e.Bounds.X  , e.Bounds.Y );
//        }

//        public void SetAllToolTips( Button _import, Button _export, Button _refresh, Button _comp )
//        {
//            SetToolTip( _import,  "Importera .xlsx fil med STEELMEET format" );
//            SetToolTip( _refresh, "Laddar om filen som har importerats" );
//            SetToolTip( _export,  "Exporterar i .xlsx Steelmeet format" );
//            SetToolTip( _comp,    "Skickar listan med lyftare till tävlingsföntret, samt återställer tävlingsfönstret." );
//        }
//    } 
//}
