using System;
using Newtonsoft.Json;
using Forkleans.Runtime;

namespace Forkleans.Streams
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    [GenerateSerializer]
    internal sealed class PubSubPublisherState : IEquatable<PubSubPublisherState>
    {
        // IMPORTANT!!!!!
        // These fields have to be public non-readonly for JsonSerialization to work!
        // Implement ISerializable if changing any of them to readonly
        [JsonProperty]
        [Id(0)]
        public QualifiedStreamId Stream;

        [JsonProperty]
        [Id(1)]
        public GrainId Producer; // the field needs to be of a public type, otherwise we will not generate an Orleans serializer for that class.

        // This constructor has to be public for JsonSerialization to work!
        // Implement ISerializable if changing it to non-public
        public PubSubPublisherState(QualifiedStreamId streamId, GrainId streamProducer)
        {
            Stream = streamId;
            Producer = streamProducer;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            // Note: Can't use the 'as' operator on PubSubPublisherState because it is a struct.
            return obj is PubSubPublisherState && Equals((PubSubPublisherState)obj);
        }
        public bool Equals(PubSubPublisherState other)
        {
            // Note: PubSubPublisherState is a struct, so 'other' can never be null.
            return Equals(other.Stream, other.Producer);
        }
        public bool Equals(QualifiedStreamId streamId, GrainId streamProducer)
        {
            if (Stream == default) return false;
            return Stream.Equals(streamId) && Producer.Equals(streamProducer);
        }

        public static bool operator ==(PubSubPublisherState left, PubSubPublisherState right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(PubSubPublisherState left, PubSubPublisherState right)
        {
            return !left.Equals(right);
        }
        public override int GetHashCode() => HashCode.Combine(Stream, Producer);

        public override string ToString()
        {
            return string.Format("PubSubPublisherState:StreamId={0},Producer={1}.", Stream, Producer);
        }
    }
}
