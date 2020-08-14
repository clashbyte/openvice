using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace OpenVice.Data
{
    /// <summary>
    /// Enum of opcode arg types.
    /// </summary>
    public enum SCMType
    {
        EndOfArgList = 0x00,
        TInt32 = 0x01,
        TGlobal = 0x02,
        TLocal = 0x03,
        TInt8 = 0x04,
        TInt16 = 0x05,
        TFloat16 = 0x06,
        TString = 0x09,
    };

    struct SCMTypeInfo
    {
        byte size;
    };
}
