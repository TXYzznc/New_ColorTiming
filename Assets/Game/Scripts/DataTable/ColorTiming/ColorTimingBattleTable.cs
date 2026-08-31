//------------------------------------------------------------
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：__DATA_TABLE_CREATE_TIME__
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;
#if ENABLE_OBFUZ
[Obfuz.ObfuzIgnore(Obfuz.ObfuzScope.TypeName | Obfuz.ObfuzScope.MethodName)]
#endif
/// <summary>
/// ColorTiming battle and scene flow table
/// </summary>
public class ColorTimingBattleTable : DataRowBase
{
	private int m_Id = 0;
	/// <summary>
    ///
    /// </summary>
    public override int Id
    {
        get { return m_Id; }
    }

        /// <summary>
        /// ColorTimingSceneId
        /// </summary>
        public int SceneId
        {
            get;
            private set;
        }

        /// <summary>
        /// GF scene resource name
        /// </summary>
        public string SceneAsset
        {
            get;
            private set;
        }

        /// <summary>
        /// BattleKind
        /// </summary>
        public int BattleKind
        {
            get;
            private set;
        }

        /// <summary>
        /// Player config id
        /// </summary>
        public int PlayerId
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss config id
        /// </summary>
        public int BossId
        {
            get;
            private set;
        }

        /// <summary>
        /// Weapon spawn rule id
        /// </summary>
        public int WeaponSpawnRuleId
        {
            get;
            private set;
        }

        /// <summary>
        /// Next scene after victory
        /// </summary>
        public int NextSceneId
        {
            get;
            private set;
        }

        /// <summary>
        /// Retry scene
        /// </summary>
        public int RetrySceneId
        {
            get;
            private set;
        }

        /// <summary>
        /// Victory transition delay seconds
        /// </summary>
        public float VictoryDelay
        {
            get;
            private set;
        }

        /// <summary>
        /// Tutorial theme id
        /// </summary>
        public int TutorialId
        {
            get;
            private set;
        }

        /// <summary>
        /// Semantic BGM cue id
        /// </summary>
        public string BgmCueId
        {
            get;
            private set;
        }

        /// <summary>
        /// Minimum orthographic size
        /// </summary>
        public float CameraMinSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Maximum orthographic size
        /// </summary>
        public float CameraMaxSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Distance mapped to zoom
        /// </summary>
        public float CameraDistanceRange
        {
            get;
            private set;
        }

        /// <summary>
        /// Distance before zoom starts
        /// </summary>
        public float CameraStartDistance
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            index++;
            SceneId = int.Parse(columnStrings[index++]);
            SceneAsset = columnStrings[index++];
            BattleKind = int.Parse(columnStrings[index++]);
            PlayerId = int.Parse(columnStrings[index++]);
            BossId = int.Parse(columnStrings[index++]);
            WeaponSpawnRuleId = int.Parse(columnStrings[index++]);
            NextSceneId = int.Parse(columnStrings[index++]);
            RetrySceneId = int.Parse(columnStrings[index++]);
            VictoryDelay = float.Parse(columnStrings[index++]);
            TutorialId = int.Parse(columnStrings[index++]);
            BgmCueId = columnStrings[index++];
            CameraMinSize = float.Parse(columnStrings[index++]);
            CameraMaxSize = float.Parse(columnStrings[index++]);
            CameraDistanceRange = float.Parse(columnStrings[index++]);
            CameraStartDistance = float.Parse(columnStrings[index++]);

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    SceneId = binaryReader.Read7BitEncodedInt32();
                    SceneAsset = binaryReader.ReadString();
                    BattleKind = binaryReader.Read7BitEncodedInt32();
                    PlayerId = binaryReader.Read7BitEncodedInt32();
                    BossId = binaryReader.Read7BitEncodedInt32();
                    WeaponSpawnRuleId = binaryReader.Read7BitEncodedInt32();
                    NextSceneId = binaryReader.Read7BitEncodedInt32();
                    RetrySceneId = binaryReader.Read7BitEncodedInt32();
                    VictoryDelay = binaryReader.ReadSingle();
                    TutorialId = binaryReader.Read7BitEncodedInt32();
                    BgmCueId = binaryReader.ReadString();
                    CameraMinSize = binaryReader.ReadSingle();
                    CameraMaxSize = binaryReader.ReadSingle();
                    CameraDistanceRange = binaryReader.ReadSingle();
                    CameraStartDistance = binaryReader.ReadSingle();
                }
            }

            return true;
        }
}
