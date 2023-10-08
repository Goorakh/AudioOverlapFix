using System.Collections.Generic;
using UnityEngine;

namespace AudioOverlapFix
{
    public readonly struct TimeStampedSoundEvent(uint EventID, float TimeStamp)
    {
        public readonly uint EventID = EventID;
        public readonly float TimeStamp = TimeStamp;

        public readonly float TimeSince => Time.unscaledTime - TimeStamp;

        public static readonly IEqualityComparer<TimeStampedSoundEvent> EventIDComparer = new _EventIDComparer();
        sealed class _EventIDComparer : IEqualityComparer<TimeStampedSoundEvent>
        {
            public bool Equals(TimeStampedSoundEvent x, TimeStampedSoundEvent y)
            {
                return x.EventID == y.EventID;
            }

            public int GetHashCode(TimeStampedSoundEvent obj)
            {
                return obj.EventID.GetHashCode();
            }
        }
    }
}
