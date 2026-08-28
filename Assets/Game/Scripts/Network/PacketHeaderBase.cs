// 文件职责：定义 网络包Header基类 的网络传输或处理行为。
// 所属模块：Network。

//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Network;

namespace GameFramework.Network
{
    public abstract class PacketHeaderBase : IPacketHeader, IReference
    {
        public abstract PacketType PacketType
        {
            get;
        }

        public int Id
        {
            get;
            set;
        }

        public int PacketLength
        {
            get;
            set;
        }

        public bool IsValid
        {
            get
            {
                return PacketType != PacketType.Undefined && Id > 0 && PacketLength >= 0;
            }
        }

        // 清空当前保存的运行时状态，使对象可安全复用。
        public void Clear()
        {
            Id = 0;
            PacketLength = 0;
        }
    }
}
