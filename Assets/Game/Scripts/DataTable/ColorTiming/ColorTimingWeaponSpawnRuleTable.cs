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
/// ColorTiming weapon spawn rule entries
/// </summary>
public class ColorTimingWeaponSpawnRuleTable : DataRowBase
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
        /// Spawn rule id
        /// </summary>
        public int RuleId
        {
            get;
            private set;
        }

        /// <summary>
        /// Spawn cadence
        /// </summary>
        public float SpawnInterval
        {
            get;
            private set;
        }

        /// <summary>
        /// Active weapon limit
        /// </summary>
        public int ActiveLimit
        {
            get;
            private set;
        }

        /// <summary>
        /// Weakness guarantee threshold
        /// </summary>
        public int GuaranteeThreshold
        {
            get;
            private set;
        }

        /// <summary>
        /// Minimum distance to an occupied anchor
        /// </summary>
        public float MinimumAnchorDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Damage count after which tips stop
        /// </summary>
        public int TutorialDamageLimit
        {
            get;
            private set;
        }

        /// <summary>
        /// Allowed WeaponColor
        /// </summary>
        public int Color
        {
            get;
            private set;
        }

        /// <summary>
        /// Allowed WeaponType
        /// </summary>
        public int Type
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
            RuleId = int.Parse(columnStrings[index++]);
            SpawnInterval = float.Parse(columnStrings[index++]);
            ActiveLimit = int.Parse(columnStrings[index++]);
            GuaranteeThreshold = int.Parse(columnStrings[index++]);
            MinimumAnchorDistance = float.Parse(columnStrings[index++]);
            TutorialDamageLimit = int.Parse(columnStrings[index++]);
            Color = int.Parse(columnStrings[index++]);
            Type = int.Parse(columnStrings[index++]);

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    RuleId = binaryReader.Read7BitEncodedInt32();
                    SpawnInterval = binaryReader.ReadSingle();
                    ActiveLimit = binaryReader.Read7BitEncodedInt32();
                    GuaranteeThreshold = binaryReader.Read7BitEncodedInt32();
                    MinimumAnchorDistance = binaryReader.ReadSingle();
                    TutorialDamageLimit = binaryReader.Read7BitEncodedInt32();
                    Color = binaryReader.Read7BitEncodedInt32();
                    Type = binaryReader.Read7BitEncodedInt32();
                }
            }

            return true;
        }
}
