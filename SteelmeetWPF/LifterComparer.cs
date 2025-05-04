using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteelmeetWPF
{
    public class LifterComparer: IComparer<Lifter>
    {
        public int Compare( Lifter x, Lifter y )
        {
            if( x.isRetrying && !y.isRetrying )
            {
                return 1; // x should come after y
            }
            else if( !x.isRetrying && y.isRetrying )
            {
                return -1; // x should come before y
            }

            int indexX = (int)x.currentLiftType;
            int indexY = (int)y.currentLiftType;

            if( indexX >= 0 && indexX < x.sbdWeightsList.Count && indexY >= 0 && indexY < y.sbdWeightsList.Count )
            {
                float weightX = x.sbdWeightsList[indexX];
                float weightY = y.sbdWeightsList[indexY];

                int weightComparison = weightX.CompareTo(weightY);

                if( weightComparison != 0 )
                {
                    return weightComparison;
                }

                return x.lotNumber.CompareTo( y.lotNumber );
            }

            return 0;
        }
    }
}
