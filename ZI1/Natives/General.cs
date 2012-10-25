using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ZI1.Natives
{
    class General
    {
        static public void CheckError()
        {
            int error = Marshal.GetLastWin32Error();
            if (error != 0)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }
        }

    }
}
