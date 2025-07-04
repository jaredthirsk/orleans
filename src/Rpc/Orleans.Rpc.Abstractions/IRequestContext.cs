using System;

namespace Granville.Runtime
{
    /// <summary>
    /// Minimal request context interface for RPC.
    /// </summary>
    public interface IRequestContext
    {
        /// <summary>
        /// Gets the request ID.
        /// </summary>
        Guid RequestId { get; }
    }
}