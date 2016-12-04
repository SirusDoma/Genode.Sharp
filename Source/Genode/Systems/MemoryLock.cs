using System;
using System.Runtime.InteropServices;

namespace Genode
{
    /// <summary>
    /// Provides functionality to pin an object.
    /// This prevents Garbage Collector from reallocating / moving the object.
    /// </summary>
    public sealed class MemoryLock : IDisposable
    {
        private readonly GCHandle _handle;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryLock"/> class
        /// </summary>
        /// <param name="obj">The object to pin.</param>
        public MemoryLock(object obj)
        {
            _handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
        }

        /// <summary>
        /// Gets the address of the pinned memory.
        /// </summary>
        public IntPtr Address
        {
            get
            {
                return (_handle.AddrOfPinnedObject());
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="MemoryLock"/> .
        /// </summary>
        public void Dispose()
        {
            if (_handle.IsAllocated)
                _handle.Free();
        }
    }
}
