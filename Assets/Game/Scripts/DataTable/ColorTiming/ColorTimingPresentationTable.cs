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
/// ColorTiming shared presentation tuning table
/// </summary>
public class ColorTimingPresentationTable : DataRowBase
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
        /// Loading fade duration
        /// </summary>
        public float LoadingFadeDuration
        {
            get;
            private set;
        }

        /// <summary>
        /// Default transition estimate
        /// </summary>
        public float TransitionDefaultDuration
        {
            get;
            private set;
        }

        /// <summary>
        /// Minimum transition estimate
        /// </summary>
        public float TransitionMinimumDuration
        {
            get;
            private set;
        }

        /// <summary>
        /// Maximum transition estimate
        /// </summary>
        public float TransitionMaximumDuration
        {
            get;
            private set;
        }

        /// <summary>
        /// Observed-duration history weight
        /// </summary>
        public float TransitionHistoryWeight
        {
            get;
            private set;
        }

        /// <summary>
        /// Scene load progress weight
        /// </summary>
        public float SceneProgressWeight
        {
            get;
            private set;
        }

        /// <summary>
        /// Resource preparation progress weight
        /// </summary>
        public float PreparationProgressWeight
        {
            get;
            private set;
        }

        /// <summary>
        /// Progress log bucket count
        /// </summary>
        public int ProgressBuckets
        {
            get;
            private set;
        }

        /// <summary>
        /// Earliest tutorial dismiss time
        /// </summary>
        public float TutorialDismissDelay
        {
            get;
            private set;
        }

        /// <summary>
        /// Player HP pip spacing
        /// </summary>
        public float PlayerPipSpacing
        {
            get;
            private set;
        }

        /// <summary>
        /// Alternate pip row offset
        /// </summary>
        public float PlayerPipAlternateOffset
        {
            get;
            private set;
        }

        /// <summary>
        /// Highlighted boss pip speed
        /// </summary>
        public float BossPipFloatSpeed
        {
            get;
            private set;
        }

        /// <summary>
        /// Highlighted boss pip minimum Y
        /// </summary>
        public float BossPipMinY
        {
            get;
            private set;
        }

        /// <summary>
        /// Highlighted boss pip maximum Y
        /// </summary>
        public float BossPipMaxY
        {
            get;
            private set;
        }

        /// <summary>
        /// Intro timeout
        /// </summary>
        public float MainMenuIntroTimeout
        {
            get;
            private set;
        }

        /// <summary>
        /// Weapon pickup fade duration
        /// </summary>
        public float PickupFadeDuration
        {
            get;
            private set;
        }

        /// <summary>
        /// Battle result fade speed
        /// </summary>
        public float ResultFadeSpeed
        {
            get;
            private set;
        }

        /// <summary>
        /// Held normal attack cursor index
        /// </summary>
        public int HeldNormalCursorIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// Pause cursor index
        /// </summary>
        public int PauseCursorIndex
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
            LoadingFadeDuration = float.Parse(columnStrings[index++]);
            TransitionDefaultDuration = float.Parse(columnStrings[index++]);
            TransitionMinimumDuration = float.Parse(columnStrings[index++]);
            TransitionMaximumDuration = float.Parse(columnStrings[index++]);
            TransitionHistoryWeight = float.Parse(columnStrings[index++]);
            SceneProgressWeight = float.Parse(columnStrings[index++]);
            PreparationProgressWeight = float.Parse(columnStrings[index++]);
            ProgressBuckets = int.Parse(columnStrings[index++]);
            TutorialDismissDelay = float.Parse(columnStrings[index++]);
            PlayerPipSpacing = float.Parse(columnStrings[index++]);
            PlayerPipAlternateOffset = float.Parse(columnStrings[index++]);
            BossPipFloatSpeed = float.Parse(columnStrings[index++]);
            BossPipMinY = float.Parse(columnStrings[index++]);
            BossPipMaxY = float.Parse(columnStrings[index++]);
            MainMenuIntroTimeout = float.Parse(columnStrings[index++]);
            PickupFadeDuration = float.Parse(columnStrings[index++]);
            ResultFadeSpeed = float.Parse(columnStrings[index++]);
            HeldNormalCursorIndex = int.Parse(columnStrings[index++]);
            PauseCursorIndex = int.Parse(columnStrings[index++]);

            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    LoadingFadeDuration = binaryReader.ReadSingle();
                    TransitionDefaultDuration = binaryReader.ReadSingle();
                    TransitionMinimumDuration = binaryReader.ReadSingle();
                    TransitionMaximumDuration = binaryReader.ReadSingle();
                    TransitionHistoryWeight = binaryReader.ReadSingle();
                    SceneProgressWeight = binaryReader.ReadSingle();
                    PreparationProgressWeight = binaryReader.ReadSingle();
                    ProgressBuckets = binaryReader.Read7BitEncodedInt32();
                    TutorialDismissDelay = binaryReader.ReadSingle();
                    PlayerPipSpacing = binaryReader.ReadSingle();
                    PlayerPipAlternateOffset = binaryReader.ReadSingle();
                    BossPipFloatSpeed = binaryReader.ReadSingle();
                    BossPipMinY = binaryReader.ReadSingle();
                    BossPipMaxY = binaryReader.ReadSingle();
                    MainMenuIntroTimeout = binaryReader.ReadSingle();
                    PickupFadeDuration = binaryReader.ReadSingle();
                    ResultFadeSpeed = binaryReader.ReadSingle();
                    HeldNormalCursorIndex = binaryReader.Read7BitEncodedInt32();
                    PauseCursorIndex = binaryReader.Read7BitEncodedInt32();
                }
            }

            return true;
        }
}
