using System;
using System.Collections.Generic;
using System.Linq;
using Forkleans.Runtime;
using Forkleans.Transactions.Abstractions;

namespace Forkleans.Transactions
{
    [Serializable, GenerateSerializer, Immutable]
    public readonly struct ParticipantId
    {
        public static readonly IEqualityComparer<ParticipantId> Comparer = new IdComparer();

        [GenerateSerializer]
        [Flags]
        public enum Role
        {
            Resource = 1 << 0,
            Manager = 1 << 1,
            PriorityManager = 1 << 2
        }

        [Id(0)]
        public string Name { get; }

        [Id(1)]
        public GrainReference Reference { get; }

        [Id(2)]
        public Role SupportedRoles { get; }

        public ParticipantId(string name, GrainReference reference, Role supportedRoles)
        {
            this.Name = name;
            this.Reference = reference;
            this.SupportedRoles = supportedRoles;
        }

        public override string ToString()
        {
            return $"ParticipantId.{Name}.{Reference}";
        }

        [GenerateSerializer, Immutable]
        public sealed class IdComparer : IEqualityComparer<ParticipantId>
        {
            public bool Equals(ParticipantId x, ParticipantId y)
            {
                return string.CompareOrdinal(x.Name, y.Name) == 0 && Equals(x.Reference, y.Reference);
            }

            public int GetHashCode(ParticipantId obj) => HashCode.Combine(obj.Name, obj.Reference);
        }
    }

    public static class ParticipantRoleExtensions
    {
        public static bool SupportsRoles(this ParticipantId participant, ParticipantId.Role role)
        {
            return (participant.SupportedRoles & role) != 0;
        }

        public static bool IsResource(this ParticipantId participant)
        {
            return participant.SupportsRoles(ParticipantId.Role.Resource);
        }

        public static bool IsManager(this ParticipantId participant)
        {
            return participant.SupportsRoles(ParticipantId.Role.Manager);
        }

        public static bool IsPriorityManager(this ParticipantId participant)
        {
            return participant.SupportsRoles(ParticipantId.Role.PriorityManager);
        }

        public static IEnumerable<KeyValuePair<ParticipantId,AccessCounter>> SelectResources(this IEnumerable<KeyValuePair<ParticipantId, AccessCounter>> participants)
        {
            return participants.Where(p => p.Key.IsResource());
        }

        public static IEnumerable<KeyValuePair<ParticipantId, AccessCounter>> SelectManagers(this IEnumerable<KeyValuePair<ParticipantId, AccessCounter>> participants)
        {
            return participants.Where(p => p.Key.IsManager());
        }

        public static IEnumerable<ParticipantId> SelectPriorityManagers(this IEnumerable<ParticipantId> participants)
        {
            return participants.Where(p => p.IsPriorityManager());
        }
    }
}
