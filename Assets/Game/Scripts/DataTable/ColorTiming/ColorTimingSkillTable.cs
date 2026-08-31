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
/// ColorTiming skill tuning table
/// </summary>
public class ColorTimingSkillTable : DataRowBase
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
        /// Semantic skill key
        /// </summary>
        public string Key
        {
            get;
            private set;
        }

        /// <summary>
        /// GF entity resource name
        /// </summary>
        public string EntityName
        {
            get;
            private set;
        }

        /// <summary>
        /// Lifetime seconds
        /// </summary>
        public float Lifetime
        {
            get;
            private set;
        }

        /// <summary>
        /// Damage amount
        /// </summary>
        public int Damage
        {
            get;
            private set;
        }

        /// <summary>
        /// Instant-kill rule
        /// </summary>
        public bool InstantKill
        {
            get;
            private set;
        }

        /// <summary>
        /// Travel speed
        /// </summary>
        public float Speed
        {
            get;
            private set;
        }

        /// <summary>
        /// Arrival distance epsilon
        /// </summary>
        public float ArrivalEpsilon
        {
            get;
            private set;
        }

        /// <summary>
        /// End delay
        /// </summary>
        public float EndDelay
        {
            get;
            private set;
        }

        /// <summary>
        /// Pattern parameter A
        /// </summary>
        public float PatternA
        {
            get;
            private set;
        }

        /// <summary>
        /// Pattern parameter B
        /// </summary>
        public float PatternB
        {
            get;
            private set;
        }

        /// <summary>
        /// Pattern parameter C
        /// </summary>
        public float PatternC
        {
            get;
            private set;
        }

        /// <summary>
        /// Pattern count A
        /// </summary>
        public int CountA
        {
            get;
            private set;
        }

        /// <summary>
        /// Pattern count B
        /// </summary>
        public int CountB
        {
            get;
            private set;
        }

        /// <summary>
        /// Use curved movement
        /// </summary>
        public bool UseCurve
        {
            get;
            private set;
        }

        /// <summary>
        /// Semantic sound cue id
        /// </summary>
        public string SoundCueId
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
            Key = columnStrings[index++];
            EntityName = columnStrings[index++];
            Lifetime = float.Parse(columnStrings[index++]);
            Damage = int.Parse(columnStrings[index++]);
            InstantKill = bool.Parse(columnStrings[index++]);
            Speed = float.Parse(columnStrings[index++]);
            ArrivalEpsilon = float.Parse(columnStrings[index++]);
            EndDelay = float.Parse(columnStrings[index++]);
            PatternA = float.Parse(columnStrings[index++]);
            PatternB = float.Parse(columnStrings[index++]);
            PatternC = float.Parse(columnStrings[index++]);
            CountA = int.Parse(columnStrings[index++]);
            CountB = int.Parse(columnStrings[index++]);
            UseCurve = bool.Parse(columnStrings[index++]);
            SoundCueId = columnStrings[index++];

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    Key = binaryReader.ReadString();
                    EntityName = binaryReader.ReadString();
                    Lifetime = binaryReader.ReadSingle();
                    Damage = binaryReader.Read7BitEncodedInt32();
                    InstantKill = binaryReader.ReadBoolean();
                    Speed = binaryReader.ReadSingle();
                    ArrivalEpsilon = binaryReader.ReadSingle();
                    EndDelay = binaryReader.ReadSingle();
                    PatternA = binaryReader.ReadSingle();
                    PatternB = binaryReader.ReadSingle();
                    PatternC = binaryReader.ReadSingle();
                    CountA = binaryReader.Read7BitEncodedInt32();
                    CountB = binaryReader.Read7BitEncodedInt32();
                    UseCurve = binaryReader.ReadBoolean();
                    SoundCueId = binaryReader.ReadString();
                }
            }

            return true;
        }
}
