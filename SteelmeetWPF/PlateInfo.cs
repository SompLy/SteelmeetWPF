using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SteelmeetWPF
{
    public class PlateInfo
    {
        public PlateInfo( int plate50, int plate25, int plate20, int plate15, int plate10, int plate5, int plate25small, int plate05, int plate125, int plate025
        , Color col_plate50, Color col_plate25, Color col_plate20, Color col_plate15, Color col_plate10, Color col_plate5, Color col_plate25small, Color col_plate05, Color col_plate125, Color col_plate025 )
        {
            this.plate50 = plate50 / 2;
            this.plate25 = plate25 / 2;
            this.plate20 = plate20 / 2;
            this.plate15 = plate15 / 2;
            this.plate10 = plate10 / 2;
            this.plate5 = plate5 / 2;
            this.plate25small = plate25small / 2;
            this.plate125 = plate125 / 2;
            this.plate05 = plate05 / 2;
            this.plate025 = plate025 / 2;

            this.col_plate50 = col_plate50;
            this.col_plate25 = col_plate25;
            this.col_plate20 = col_plate20;
            this.col_plate15 = col_plate15;
            this.col_plate10 = col_plate10;
            this.col_plate5 = col_plate5;
            this.col_plate25small = col_plate25small;
            this.col_plate05 = col_plate05;
            this.col_plate125 = col_plate125;
            this.col_plate025 = col_plate025;
        }
        public int plate50 { get; set; }
        public int plate25 { get; set; }
        public int plate20 { get; set; }
        public int plate15 { get; set; }
        public int plate10 { get; set; }
        public int plate5 { get; set; }
        public int plate25small { get; set; }
        public int plate05 { get; set; }
        public int plate125 { get; set; }
        public int plate025 { get; set; }
        //Colors
        public Color col_plate50 { get; set; }
        public Color col_plate25 { get; set; }
        public Color col_plate20 { get; set; }
        public Color col_plate15 { get; set; }
        public Color col_plate10 { get; set; }
        public Color col_plate5 { get; set; }
        public Color col_plate25small { get; set; }
        public Color col_plate05 { get; set; }
        public Color col_plate125 { get; set; }
        public Color col_plate025 { get; set; }

    }
}
