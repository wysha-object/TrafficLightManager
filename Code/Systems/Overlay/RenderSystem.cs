// Colossal.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Colossal.GizmosSystem
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464

using System.Collections.Generic;
using System.IO;
using Game;
using Game.Prefabs;
using Game.Rendering;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace TrafficLightManager.Code.Systems.Overlay
{
    public partial class RenderSystem : GameSystemBase
    {
        public const int kIconWidth = 256;

        public const int kIconHeight = 256;

        public enum Icon : uint
        {
            TrafficLightWrench = 0,

            TrafficLightLink = 1,

            TrafficLight = 2,
        }

        private PrefabSystem m_PrefabSystem;

        private Mesh m_IconMesh;

        private Material m_IconMaterial;

        private uint[] m_IconArgsArray;

        private ComputeBuffer m_IconArgsBuffer;

        private ComputeBuffer m_IconComputeBuffer;

        private int m_IconComputeBufferID;

        private Texture2DArray m_IconTextureArray;

        private Bounds m_IconBounds;

        private List<NotificationIconBufferSystem.InstanceData> m_IconInstanceData;

        private Vector3 m_CameraPosition;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_PrefabSystem = base.World.GetOrCreateSystemManaged<PrefabSystem>();

            IconSetup();

            RenderPipelineManager.beginContextRendering += Render;
        }

        protected unsafe void IconSetup()
        {
            m_IconInstanceData = [];
            m_IconArgsArray = new uint[5];
            m_IconArgsBuffer = new ComputeBuffer(1, m_IconArgsArray.Length * 4, ComputeBufferType.IndirectArguments);

            var iconValues = System.Enum.GetValues(typeof(Icon));
            m_IconTextureArray = new Texture2DArray(kIconWidth, kIconHeight, iconValues.Length, TextureFormat.ARGB32, mipChain: true);
            foreach (Icon icon in iconValues)
            {
                var iconTexture = new Texture2D(kIconWidth, kIconHeight);
                string imageResourceName = $"TrafficLightManager.Code.Resources.Textures.Icons.{System.Enum.GetName(typeof(Icon), icon)}.png";
                using Stream imageStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(imageResourceName);
                if (imageStream != null)
                {
                    byte[] image = new byte[imageStream.Length];
                    imageStream.Read(image, 0, image.Length);
                    ImageConversion.LoadImage(iconTexture, image);
                }
                else
                {
                    Mod.m_Log.Error($"{imageResourceName} does not exist.");
                }
                Graphics.CopyTexture(iconTexture, 0, m_IconTextureArray, (int)icon);
                Object.Destroy(iconTexture);
            }

            var configurationQuery = GetEntityQuery(ComponentType.ReadOnly<IconConfigurationData>());
            var singletonEntity = configurationQuery.GetSingletonEntity();
            var prefab = m_PrefabSystem.GetPrefab<IconConfigurationPrefab>(singletonEntity);
            m_IconMaterial = new Material(prefab.m_Material);
            m_IconMaterial.mainTexture = m_IconTextureArray;

            m_IconMesh = new Mesh();
            m_IconMesh.vertices = new Vector3[4] { new Vector3(-1f, -1f, 0f), new Vector3(-1f, 1f, 0f), new Vector3(1f, 1f, 0f), new Vector3(1f, -1f, 0f) };
            m_IconMesh.uv = new Vector2[4] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) };
            m_IconMesh.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };

            m_IconArgsArray[0] = m_IconMesh.GetIndexCount(0);
            m_IconArgsArray[1] = (uint)m_IconInstanceData.Count;
            m_IconArgsArray[2] = m_IconMesh.GetIndexStart(0);
            m_IconArgsArray[3] = m_IconMesh.GetBaseVertex(0);
            m_IconArgsArray[4] = 0u;

            m_IconComputeBufferID = Shader.PropertyToID("instanceBuffer");
            m_IconComputeBuffer = new ComputeBuffer(64, sizeof(NotificationIconBufferSystem.InstanceData), ComputeBufferType.Default, ComputeBufferMode.Dynamic);

            ClearIconList(); // Reset m_IconBounds
        }

        protected override void OnDestroy()
        {
            RenderPipelineManager.beginContextRendering -= Render;

            Object.Destroy(m_IconMesh);
            Object.Destroy(m_IconMaterial);
            Object.Destroy(m_IconTextureArray);
            m_IconArgsBuffer.Release();
            m_IconComputeBuffer.Release();
        }

        protected override void OnUpdate() { }

        private void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            foreach (Camera camera in cameras)
            {
                if (camera.cameraType == CameraType.Game)
                {
                    if (m_IconInstanceData.Count > 0)
                    {
                        var cameraPosition = camera.transform.position;
                        if (!m_CameraPosition.Equals(cameraPosition))
                        {
                            m_CameraPosition = cameraPosition;
                            float distance = math.distance(m_IconBounds.center, m_CameraPosition);
                            for (int i = 0; i < m_IconInstanceData.Count; i++)
                            {
                                var icon = m_IconInstanceData[i];
                                icon.m_Distance = distance;
                                m_IconInstanceData[i] = icon;
                            }
                            m_IconArgsArray[1] = (uint)m_IconInstanceData.Count;
                            m_IconArgsBuffer.SetData(m_IconArgsArray);
                            m_IconComputeBuffer.SetData(m_IconInstanceData, 0, 0, m_IconInstanceData.Count);
                            m_IconMaterial.SetBuffer(m_IconComputeBufferID, m_IconComputeBuffer);
                        }
                        Graphics.DrawMeshInstancedIndirect(
                            m_IconMesh,
                            0,
                            m_IconMaterial,
                            m_IconBounds,
                            m_IconArgsBuffer,
                            0,
                            null,
                            ShadowCastingMode.Off,
                            receiveShadows: false,
                            0,
                            camera
                        );
                    }
                }
            }
        }

        public unsafe void AddIcon(Vector3 position, Icon type)
        {
            var icon = new NotificationIconBufferSystem.InstanceData
            {
                m_Position = position,
                m_Icon = (float)type,
                m_Params = new float4(2f, 0f, 1f, 1f),
                m_Distance = 10000,
            };
            m_IconInstanceData.Add(icon);
            m_IconBounds.Encapsulate(position);
            if (m_IconInstanceData.Count >= m_IconComputeBuffer.count)
            {
                m_IconComputeBuffer.Release();
                m_IconComputeBuffer = new ComputeBuffer(
                    m_IconInstanceData.Count * 2,
                    sizeof(NotificationIconBufferSystem.InstanceData),
                    ComputeBufferType.Default,
                    ComputeBufferMode.Dynamic
                );
            }
            InvalidateBuffer();
        }

        public void ClearIconList()
        {
            m_IconInstanceData.Clear();
            m_IconBounds.SetMinMax(Vector3.positiveInfinity, Vector3.negativeInfinity);
        }

        public void InvalidateBuffer()
        {
            m_CameraPosition = Vector3.positiveInfinity;
        }
    }
}
