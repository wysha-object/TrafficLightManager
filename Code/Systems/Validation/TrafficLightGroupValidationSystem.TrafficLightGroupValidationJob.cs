using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Net;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Job;
using Unity.Entities;
using Unity.Jobs;

namespace TrafficLightManager.Code.Systems.Validation
{
    public partial class TrafficLightGroupValidationSystem : GameSystemBase
    {
        private JobHandle ScheduleTrafficLightGroupValidationJob(in JobHandle dependsOn, EntityCommandBuffer entityCommandBuffer)
        {
            JobHandle dependency = dependsOn;
            dependency = JobChunkExtensions.ScheduleParallel(
                new TrafficLightGroupValidationJob
                {
                    m_EntityStorageInfoLookup = SystemAPI.GetEntityStorageInfoLookup(),
                    m_EntityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_TrafficLightGroupType = SystemAPI.GetComponentTypeHandle<TrafficLightGroup>(isReadOnly: false),
                    m_TrafficLightsMemberRefType = SystemAPI.GetBufferTypeHandle<TrafficLightsMemberRef>(isReadOnly: false),
                    m_CustomPhaseDataBufferType = SystemAPI.GetBufferTypeHandle<CustomPhaseData>(isReadOnly: false),
                    m_TrafficLightsLookup = SystemAPI.GetComponentLookup<TrafficLights>(isReadOnly: true),
                    m_CustomTrafficLightsLookup = SystemAPI.GetComponentLookup<CustomTrafficLights>(isReadOnly: true),
                },
                m_TrafficLightGroupQuery,
                dependency
            );
            return dependency;
        }
    }
}
