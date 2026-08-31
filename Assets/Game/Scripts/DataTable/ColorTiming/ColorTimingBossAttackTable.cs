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
/// ColorTiming weighted boss attack table
/// </summary>
public class ColorTimingBossAttackTable : DataRowBase
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
        /// Boss config id
        /// </summary>
        public int BossId
        {
            get;
            private set;
        }

        /// <summary>
        /// Body Head or Tail
        /// </summary>
        public string ActorPart
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss1DistanceZone
        /// </summary>
        public int DistanceZone
        {
            get;
            private set;
        }

        /// <summary>
        /// Attack/action id
        /// </summary>
        public int AttackId
        {
            get;
            private set;
        }

        /// <summary>
        /// Selection weight
        /// </summary>
        public float Weight
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether this attack may repeat
        /// </summary>
        public bool DisallowRepeat
        {
            get;
            private set;
        }

        /// <summary>
        /// Fallback attack id
        /// </summary>
        public int FallbackAttackId
        {
            get;
            private set;
        }

        /// <summary>
        /// Animation semantic key
        /// </summary>
        public string AnimationKey
        {
            get;
            private set;
        }

        /// <summary>
        /// Sound cue id
        /// </summary>
        public string SoundCueId
        {
            get;
            private set;
        }

        /// <summary>
        /// Skill config id
        /// </summary>
        public int SkillId
        {
            get;
            private set;
        }

        /// <summary>
        /// Serialized animation event key
        /// </summary>
        public string AnimationEventKey
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
            BossId = int.Parse(columnStrings[index++]);
            ActorPart = columnStrings[index++];
            DistanceZone = int.Parse(columnStrings[index++]);
            AttackId = int.Parse(columnStrings[index++]);
            Weight = float.Parse(columnStrings[index++]);
            DisallowRepeat = bool.Parse(columnStrings[index++]);
            FallbackAttackId = int.Parse(columnStrings[index++]);
            AnimationKey = columnStrings[index++];
            SoundCueId = columnStrings[index++];
            SkillId = int.Parse(columnStrings[index++]);
            AnimationEventKey = columnStrings[index++];

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    BossId = binaryReader.Read7BitEncodedInt32();
                    ActorPart = binaryReader.ReadString();
                    DistanceZone = binaryReader.Read7BitEncodedInt32();
                    AttackId = binaryReader.Read7BitEncodedInt32();
                    Weight = binaryReader.ReadSingle();
                    DisallowRepeat = binaryReader.ReadBoolean();
                    FallbackAttackId = binaryReader.Read7BitEncodedInt32();
                    AnimationKey = binaryReader.ReadString();
                    SoundCueId = binaryReader.ReadString();
                    SkillId = binaryReader.Read7BitEncodedInt32();
                    AnimationEventKey = binaryReader.ReadString();
                }
            }

            return true;
        }
}
