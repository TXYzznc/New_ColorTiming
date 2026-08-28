// 文件职责：定义 网络包处理器基类 的网络传输或处理行为。
// 所属模块：Network。

//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework.Network;

namespace GameFramework.Network
{
    public abstract class PacketHandlerBase : IPacketHandler
    {
        public abstract int Id
        {
            get;
        }

        // 处理收到的数据或事件，并更新相关状态。
        public abstract void Handle(object sender, Packet packet);
    }
}
