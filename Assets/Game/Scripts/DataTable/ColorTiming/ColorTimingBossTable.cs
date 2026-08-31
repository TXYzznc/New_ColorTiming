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
/// ColorTiming boss tuning table
/// </summary>
public class ColorTimingBossTable : DataRowBase
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
        /// BattleKind
        /// </summary>
        public int BattleKind
        {
            get;
            private set;
        }

        /// <summary>
        /// Initial attack cooldown
        /// </summary>
        public float InitialCooldown
        {
            get;
            private set;
        }

        /// <summary>
        /// Minimum subsequent cooldown
        /// </summary>
        public float NextCooldownMin
        {
            get;
            private set;
        }

        /// <summary>
        /// Maximum subsequent cooldown
        /// </summary>
        public float NextCooldownMax
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
        /// Movement arrival distance
        /// </summary>
        public float ArrivalDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Red weakness count
        /// </summary>
        public int RedWeaknesses
        {
            get;
            private set;
        }

        /// <summary>
        /// Green weakness count
        /// </summary>
        public int GreenWeaknesses
        {
            get;
            private set;
        }

        /// <summary>
        /// Purple weakness count
        /// </summary>
        public int PurpleWeaknesses
        {
            get;
            private set;
        }

        /// <summary>
        /// Orange weakness count
        /// </summary>
        public int OrangeWeaknesses
        {
            get;
            private set;
        }

        /// <summary>
        /// HUD upcoming weakness limit
        /// </summary>
        public int UpcomingLimit
        {
            get;
            private set;
        }

        /// <summary>
        /// Remaining weaknesses that activate tail
        /// </summary>
        public int TailActivationRemaining
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 head far distance
        /// </summary>
        public float HeadFarDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 head melee distance
        /// </summary>
        public float HeadMeleeDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 head burrow weight
        /// </summary>
        public float HeadBurrowWeight
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 tail far distance
        /// </summary>
        public float TailFarDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 tail melee weight
        /// </summary>
        public float TailMeleeWeight
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 trail time interval
        /// </summary>
        public float TrailTimeInterval
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 trail distance interval
        /// </summary>
        public float TrailDistanceInterval
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 relocation anchor distance
        /// </summary>
        public float RelocationMinDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Inactive health slot brightness
        /// </summary>
        public float InactiveBrightness
        {
            get;
            private set;
        }

        /// <summary>
        /// Inactive health slot alpha
        /// </summary>
        public float InactiveAlpha
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 head emergence preparation delay
        /// </summary>
        public float HeadEmergenceDelay
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 tail hidden movement speed
        /// </summary>
        public float TailMoveSpeed
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 tail initial cooldown
        /// </summary>
        public float TailInitialCooldown
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 tail minimum cooldown
        /// </summary>
        public float TailNextCooldownMin
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 tail maximum cooldown
        /// </summary>
        public float TailNextCooldownMax
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 tail arrival distance
        /// </summary>
        public float TailArrivalDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 tail first reveal delay
        /// </summary>
        public float TailFirstRevealDelay
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 tail subsequent reveal delay
        /// </summary>
        public float TailRevealDelay
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss2 tail reveal fade duration
        /// </summary>
        public float TailFadeDuration
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss hit flash duration
        /// </summary>
        public float HitFlashDuration
        {
            get;
            private set;
        }

        /// <summary>
        /// Boss hit flash speed
        /// </summary>
        public float HitFlashSpeed
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
            BattleKind = int.Parse(columnStrings[index++]);
            InitialCooldown = float.Parse(columnStrings[index++]);
            NextCooldownMin = float.Parse(columnStrings[index++]);
            NextCooldownMax = float.Parse(columnStrings[index++]);
            MoveSpeed = float.Parse(columnStrings[index++]);
            ArrivalDistance = float.Parse(columnStrings[index++]);
            RedWeaknesses = int.Parse(columnStrings[index++]);
            GreenWeaknesses = int.Parse(columnStrings[index++]);
            PurpleWeaknesses = int.Parse(columnStrings[index++]);
            OrangeWeaknesses = int.Parse(columnStrings[index++]);
            UpcomingLimit = int.Parse(columnStrings[index++]);
            TailActivationRemaining = int.Parse(columnStrings[index++]);
            HeadFarDistance = float.Parse(columnStrings[index++]);
            HeadMeleeDistance = float.Parse(columnStrings[index++]);
            HeadBurrowWeight = float.Parse(columnStrings[index++]);
            TailFarDistance = float.Parse(columnStrings[index++]);
            TailMeleeWeight = float.Parse(columnStrings[index++]);
            TrailTimeInterval = float.Parse(columnStrings[index++]);
            TrailDistanceInterval = float.Parse(columnStrings[index++]);
            RelocationMinDistance = float.Parse(columnStrings[index++]);
            InactiveBrightness = float.Parse(columnStrings[index++]);
            InactiveAlpha = float.Parse(columnStrings[index++]);
            HeadEmergenceDelay = float.Parse(columnStrings[index++]);
            TailMoveSpeed = float.Parse(columnStrings[index++]);
            TailInitialCooldown = float.Parse(columnStrings[index++]);
            TailNextCooldownMin = float.Parse(columnStrings[index++]);
            TailNextCooldownMax = float.Parse(columnStrings[index++]);
            TailArrivalDistance = float.Parse(columnStrings[index++]);
            TailFirstRevealDelay = float.Parse(columnStrings[index++]);
            TailRevealDelay = float.Parse(columnStrings[index++]);
            TailFadeDuration = float.Parse(columnStrings[index++]);
            HitFlashDuration = float.Parse(columnStrings[index++]);
            HitFlashSpeed = float.Parse(columnStrings[index++]);

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    BattleKind = binaryReader.Read7BitEncodedInt32();
                    InitialCooldown = binaryReader.ReadSingle();
                    NextCooldownMin = binaryReader.ReadSingle();
                    NextCooldownMax = binaryReader.ReadSingle();
                    MoveSpeed = binaryReader.ReadSingle();
                    ArrivalDistance = binaryReader.ReadSingle();
                    RedWeaknesses = binaryReader.Read7BitEncodedInt32();
                    GreenWeaknesses = binaryReader.Read7BitEncodedInt32();
                    PurpleWeaknesses = binaryReader.Read7BitEncodedInt32();
                    OrangeWeaknesses = binaryReader.Read7BitEncodedInt32();
                    UpcomingLimit = binaryReader.Read7BitEncodedInt32();
                    TailActivationRemaining = binaryReader.Read7BitEncodedInt32();
                    HeadFarDistance = binaryReader.ReadSingle();
                    HeadMeleeDistance = binaryReader.ReadSingle();
                    HeadBurrowWeight = binaryReader.ReadSingle();
                    TailFarDistance = binaryReader.ReadSingle();
                    TailMeleeWeight = binaryReader.ReadSingle();
                    TrailTimeInterval = binaryReader.ReadSingle();
                    TrailDistanceInterval = binaryReader.ReadSingle();
                    RelocationMinDistance = binaryReader.ReadSingle();
                    InactiveBrightness = binaryReader.ReadSingle();
                    InactiveAlpha = binaryReader.ReadSingle();
                    HeadEmergenceDelay = binaryReader.ReadSingle();
                    TailMoveSpeed = binaryReader.ReadSingle();
                    TailInitialCooldown = binaryReader.ReadSingle();
                    TailNextCooldownMin = binaryReader.ReadSingle();
                    TailNextCooldownMax = binaryReader.ReadSingle();
                    TailArrivalDistance = binaryReader.ReadSingle();
                    TailFirstRevealDelay = binaryReader.ReadSingle();
                    TailRevealDelay = binaryReader.ReadSingle();
                    TailFadeDuration = binaryReader.ReadSingle();
                    HitFlashDuration = binaryReader.ReadSingle();
                    HitFlashSpeed = binaryReader.ReadSingle();
                }
            }

            return true;
        }
}
