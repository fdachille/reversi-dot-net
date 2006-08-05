using System;

namespace Reversi
{
	/// <summary>
	/// Summary description for FrankMath.
	/// </summary>
	public class FrankMath
	{
    private static Random m_random = new Random();

    public static int GetRandomInteger(int iMin, int iMax)
    {
      int selectedValue = m_random.Next( iMin, iMax + 1 );
      if ( selectedValue > iMax )
        selectedValue = iMax;
      return selectedValue;
    }
	}
}
