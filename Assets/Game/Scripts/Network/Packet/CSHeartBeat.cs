// 文件职责：定义 CSHeartBeat 的网络传输或处理行为。
// 所属模块：Network / Packet。

//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using ProtoBuf;
using System;

namespace GameFramework.Network
{
    [Serializable, ProtoContract(Name = @"CSHeartBeat")]
    public class CSHeartBeat : CSPacketBase
    {
        // 初始化CSHeartBeat实例及其核心依赖。
        public CSHeartBeat()
        {
        }

        public override int Id
        {
            get
            {
                return 1;
            }
        }

        // 清空当前保存的运行时状态，使对象可安全复用。
        public override void Clear()
        {
        }
    }
}
