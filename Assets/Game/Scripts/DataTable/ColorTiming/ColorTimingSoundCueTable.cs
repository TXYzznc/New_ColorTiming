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
/// ColorTiming semantic sound cue table
/// </summary>
public class ColorTimingSoundCueTable : DataRowBase
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
        /// Semantic cue id
        /// </summary>
        public string CueId
        {
            get;
            private set;
        }

        /// <summary>
        /// Animation event key
        /// </summary>
        public string AnimationEventKey
        {
            get;
            private set;
        }

        /// <summary>
        /// GF audio asset name
        /// </summary>
        public string AssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// GF sound group
        /// </summary>
        public string SoundGroup
        {
            get;
            private set;
        }

        /// <summary>
        /// Loop playback
        /// </summary>
        public bool Loop
        {
            get;
            private set;
        }

        /// <summary>
        /// Cue volume
        /// </summary>
        public float Volume
        {
            get;
            private set;
        }

        /// <summary>
        /// Playback priority
        /// </summary>
        public int Priority
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
            CueId = columnStrings[index++];
            AnimationEventKey = columnStrings[index++];
            AssetName = columnStrings[index++];
            SoundGroup = columnStrings[index++];
            Loop = bool.Parse(columnStrings[index++]);
            Volume = float.Parse(columnStrings[index++]);
            Priority = int.Parse(columnStrings[index++]);

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    CueId = binaryReader.ReadString();
                    AnimationEventKey = binaryReader.ReadString();
                    AssetName = binaryReader.ReadString();
                    SoundGroup = binaryReader.ReadString();
                    Loop = binaryReader.ReadBoolean();
                    Volume = binaryReader.ReadSingle();
                    Priority = binaryReader.Read7BitEncodedInt32();
                }
            }

            return true;
        }
}
