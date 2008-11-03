// StandardActionKinds.cs
//

using System;

namespace System.Scripting.Actions {

    [Flags]
    public enum StandardActionKinds {

        GetMember = 0x01,

        SetMember = 0x02,

        DeleteMember = 0x04,

        Invoke = 0x08,

        Call = 0x10,

        Convert = 0x20,

        Create = 0x40,

        Operation = 0x80,
    }
}
