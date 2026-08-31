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
/// ColorTiming weapon resource and presentation table
/// </summary>
public class ColorTimingWeaponTable : DataRowBase
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
        /// WeaponColor
        /// </summary>
        public int Color
        {
            get;
            private set;
        }

        /// <summary>
        /// WeaponType
        /// </summary>
        public int Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Authored animator parameter value
        /// </summary>
        public int AnimatorIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// Authored HUD icon index
        /// </summary>
        public int IconIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// Authored cursor index
        /// </summary>
        public int CursorIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether charge hint is shown
        /// </summary>
        public bool UsesChargeHint
        {
            get;
            private set;
        }

        /// <summary>
        /// GF animator controller asset name
        /// </summary>
        public string ControllerAsset
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
            Color = int.Parse(columnStrings[index++]);
            Type = int.Parse(columnStrings[index++]);
            AnimatorIndex = int.Parse(columnStrings[index++]);
            IconIndex = int.Parse(columnStrings[index++]);
            CursorIndex = int.Parse(columnStrings[index++]);
            UsesChargeHint = bool.Parse(columnStrings[index++]);
            ControllerAsset = columnStrings[index++];
            SkillId = int.Parse(columnStrings[index++]);

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    Color = binaryReader.Read7BitEncodedInt32();
                    Type = binaryReader.Read7BitEncodedInt32();
                    AnimatorIndex = binaryReader.Read7BitEncodedInt32();
                    IconIndex = binaryReader.Read7BitEncodedInt32();
                    CursorIndex = binaryReader.Read7BitEncodedInt32();
                    UsesChargeHint = binaryReader.ReadBoolean();
                    ControllerAsset = binaryReader.ReadString();
                    SkillId = binaryReader.Read7BitEncodedInt32();
                }
            }

            return true;
        }
}
