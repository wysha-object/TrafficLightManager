using Unity.Collections;

namespace TrafficLightManager.Code.Utils
{
    public struct CustomPhaseTemplate
    {
        public struct Values
        {
            public FixedString64Bytes m_Name;

            // Options
            public bool m_IsPrioritiseTrack;
            public bool m_IsPrioritisePublicCar;
            public bool m_IsPrioritisePedestrian;

            public ushort m_MinimumDuration;

            public ushort m_MaximumDuration;

            public float m_TargetDurationMultiplier;

            public float m_LaneOccupiedMultiplier;

            public float m_IntervalExponent;

            public Values(CustomPhaseTemplate template)
            {
                m_Name = template.m_Name;
                m_IsPrioritiseTrack = template.m_IsPrioritiseTrack;
                m_IsPrioritisePublicCar = template.m_IsPrioritisePublicCar;
                m_IsPrioritisePedestrian = template.m_IsPrioritisePedestrian;
                m_MinimumDuration = template.m_MinimumDuration;
                m_MaximumDuration = template.m_MaximumDuration;
                m_TargetDurationMultiplier = template.m_TargetDurationMultiplier;
                m_LaneOccupiedMultiplier = template.m_LaneOccupiedMultiplier;
                m_IntervalExponent = template.m_IntervalExponent;
            }
        }

        public string m_Name;

        // Options
        public bool m_IsPrioritiseTrack;
        public bool m_IsPrioritisePublicCar;
        public bool m_IsPrioritisePedestrian;

        public ushort m_MinimumDuration;

        public ushort m_MaximumDuration;

        public float m_TargetDurationMultiplier;

        public float m_LaneOccupiedMultiplier;

        public float m_IntervalExponent;

        public static CustomPhaseTemplate Default = new CustomPhaseTemplate()
        {
            m_Name = "Default",
            m_IsPrioritiseTrack = true,
            m_IsPrioritisePublicCar = false,
            m_IsPrioritisePedestrian = false,
            m_MinimumDuration = 2,
            m_MaximumDuration = 300,
            m_TargetDurationMultiplier = 1f,
            m_LaneOccupiedMultiplier = 1f,
            m_IntervalExponent = 2f,
        };
    }
}
