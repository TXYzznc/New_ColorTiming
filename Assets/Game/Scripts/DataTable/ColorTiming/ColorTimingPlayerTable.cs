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
/// ColorTiming player tuning table
/// </summary>
public class ColorTimingPlayerTable : DataRowBase
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
        /// Maximum health
        /// </summary>
        public int MaximumHealth
        {
            get;
            private set;
        }

        /// <summary>
        /// Damage taken from a normal hit
        /// </summary>
        public int DamagePerHit
        {
            get;
            private set;
        }

        /// <summary>
        /// Health restored by successful dash
        /// </summary>
        public int DashHeal
        {
            get;
            private set;
        }

        /// <summary>
        /// Movement speed
        /// </summary>
        public float MoveSpeed
        {
            get;
            private set;
        }

        /// <summary>
        /// Dash speed
        /// </summary>
        public float DashSpeed
        {
            get;
            private set;
        }

        /// <summary>
        /// Attack skill movement speed
        /// </summary>
        public float SkillMoveSpeed
        {
            get;
            private set;
        }

        /// <summary>
        /// Movement input deadzone
        /// </summary>
        public float MovementDeadzone
        {
            get;
            private set;
        }

        /// <summary>
        /// Vertical dash scale
        /// </summary>
        public float DashVerticalScale
        {
            get;
            private set;
        }

        /// <summary>
        /// Post-hit invulnerability seconds
        /// </summary>
        public float HitInvulnerability
        {
            get;
            private set;
        }

        /// <summary>
        /// Attack input resume guard seconds
        /// </summary>
        public float AttackResumeGuard
        {
            get;
            private set;
        }

        /// <summary>
        /// Held attack animator threshold
        /// </summary>
        public float HeldAnimatorThreshold
        {
            get;
            private set;
        }

        /// <summary>
        /// Hit displacement
        /// </summary>
        public float HitKnockback
        {
            get;
            private set;
        }

        /// <summary>
        /// Hit time scale
        /// </summary>
        public float HitTimeScale
        {
            get;
            private set;
        }

        /// <summary>
        /// Hit time scale duration
        /// </summary>
        public float HitTimeDuration
        {
            get;
            private set;
        }

        /// <summary>
        /// Hit animator speed
        /// </summary>
        public float HitAnimatorSpeed
        {
            get;
            private set;
        }

        /// <summary>
        /// Sorting order on defeat
        /// </summary>
        public int HitSortingOrder
        {
            get;
            private set;
        }

        /// <summary>
        /// Death presentation duration
        /// </summary>
        public float DeathShowTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Death camera target size
        /// </summary>
        public float DeathCameraSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Death presentation scale
        /// </summary>
        public float DeathScale
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
            MaximumHealth = int.Parse(columnStrings[index++]);
            DamagePerHit = int.Parse(columnStrings[index++]);
            DashHeal = int.Parse(columnStrings[index++]);
            MoveSpeed = float.Parse(columnStrings[index++]);
            DashSpeed = float.Parse(columnStrings[index++]);
            SkillMoveSpeed = float.Parse(columnStrings[index++]);
            MovementDeadzone = float.Parse(columnStrings[index++]);
            DashVerticalScale = float.Parse(columnStrings[index++]);
            HitInvulnerability = float.Parse(columnStrings[index++]);
            AttackResumeGuard = float.Parse(columnStrings[index++]);
            HeldAnimatorThreshold = float.Parse(columnStrings[index++]);
            HitKnockback = float.Parse(columnStrings[index++]);
            HitTimeScale = float.Parse(columnStrings[index++]);
            HitTimeDuration = float.Parse(columnStrings[index++]);
            HitAnimatorSpeed = float.Parse(columnStrings[index++]);
            HitSortingOrder = int.Parse(columnStrings[index++]);
            DeathShowTime = float.Parse(columnStrings[index++]);
            DeathCameraSize = float.Parse(columnStrings[index++]);
            DeathScale = float.Parse(columnStrings[index++]);

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    MaximumHealth = binaryReader.Read7BitEncodedInt32();
                    DamagePerHit = binaryReader.Read7BitEncodedInt32();
                    DashHeal = binaryReader.Read7BitEncodedInt32();
                    MoveSpeed = binaryReader.ReadSingle();
                    DashSpeed = binaryReader.ReadSingle();
                    SkillMoveSpeed = binaryReader.ReadSingle();
                    MovementDeadzone = binaryReader.ReadSingle();
                    DashVerticalScale = binaryReader.ReadSingle();
                    HitInvulnerability = binaryReader.ReadSingle();
                    AttackResumeGuard = binaryReader.ReadSingle();
                    HeldAnimatorThreshold = binaryReader.ReadSingle();
                    HitKnockback = binaryReader.ReadSingle();
                    HitTimeScale = binaryReader.ReadSingle();
                    HitTimeDuration = binaryReader.ReadSingle();
                    HitAnimatorSpeed = binaryReader.ReadSingle();
                    HitSortingOrder = binaryReader.Read7BitEncodedInt32();
                    DeathShowTime = binaryReader.ReadSingle();
                    DeathCameraSize = binaryReader.ReadSingle();
                    DeathScale = binaryReader.ReadSingle();
                }
            }

            return true;
        }
}
