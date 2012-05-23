using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaLibrary.Shared
{
    /// <summary>
    /// Describes the reason for an inventory change.
    /// </summary>
    public enum InventoryChangeType : byte
    {
        Pickup = 0,
        Use,
        Drop,
        Sync = 255,
    }
}
