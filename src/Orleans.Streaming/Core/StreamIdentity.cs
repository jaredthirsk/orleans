using System;
using System.Collections.Generic;
using Forkleans.Runtime;

namespace Forkleans.Streams
{
    /// <summary>
    /// Stream identity contains the public stream information use to uniquely identify a stream.
    /// Stream identities are only unique per stream provider.
    /// </summary>
    /// <remarks>
    /// Use <see cref="StreamId"/> where possible, instead.
    /// </remarks>
    [Serializable, GenerateSerializer, Immutable]
    public sealed class StreamIdentity : IStreamIdentity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamIdentity"/> class.
        /// </summary>
        /// <param name="streamGuid">The stream unique identifier.</param>
        /// <param name="streamNamespace">The stream namespace.</param>
        public StreamIdentity(Guid streamGuid, string streamNamespace)
        {
            Guid = streamGuid;
            Namespace = streamNamespace;
        }

        /// <summary>
        /// Gets the stream identifier.
        /// </summary>
        [Id(0)]
        public Guid Guid { get; }

        /// <summary>
        /// Gets the stream namespace.
        /// </summary>
        [Id(1)]
        public string Namespace { get; }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is StreamIdentity identity && this.Guid.Equals(identity.Guid) && this.Namespace == identity.Namespace;

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Guid, Namespace);
    }
}
