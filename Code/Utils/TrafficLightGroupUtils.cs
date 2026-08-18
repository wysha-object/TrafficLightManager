using TrafficLightManager.Code.Components;
using Unity.Collections;
using Unity.Entities;

namespace TrafficLightManager.Code.Utils;

public class TrafficLightGroupUtils
{
    public static bool ValidateTrafficLightGroup(
        in int unfilteredChunkIndex,
        in Entity entity,
        in EntityStorageInfoLookup entityStorageInfoLookup,
        in ComponentLookup<Game.Net.TrafficLights> trafficLightsLookup,
        in ComponentLookup<CustomTrafficLights> customTrafficLightsLookup,
        ref DynamicBuffer<TrafficLightsMemberRef> trafficLightsMemberRefBuffer,
        ref EntityCommandBuffer.ParallelWriter entityCommandBuffer,
        ref NativeList<Entity> memberEntities
    )
    {
        memberEntities.Clear();
        for (int j = trafficLightsMemberRefBuffer.Length - 1; j >= 0; j--)
        {
            if (
                !entityStorageInfoLookup.Exists(trafficLightsMemberRefBuffer[j].m_Entity)
                // 路口可能被其它方式删除
                || !trafficLightsLookup.HasComponent(trafficLightsMemberRefBuffer[j].m_Entity)
                || !customTrafficLightsLookup.HasComponent(trafficLightsMemberRefBuffer[j].m_Entity)
            )
            {
                if (!trafficLightsLookup.HasComponent(trafficLightsMemberRefBuffer[j].m_Entity) && customTrafficLightsLookup.HasComponent(trafficLightsMemberRefBuffer[j].m_Entity))
                {
                    entityCommandBuffer.RemoveComponent<CustomTrafficLights>(unfilteredChunkIndex, trafficLightsMemberRefBuffer[j].m_Entity);
                }
                trafficLightsMemberRefBuffer.RemoveAtSwapBack(j);
                continue;
            }

            memberEntities.Add(trafficLightsMemberRefBuffer[j].m_Entity);
        }
        if (trafficLightsMemberRefBuffer.Length == 0)
        {
            entityCommandBuffer.DestroyEntity(unfilteredChunkIndex, entity);
            return false;
        }
        memberEntities.Sort();
        return true;
    }
}
