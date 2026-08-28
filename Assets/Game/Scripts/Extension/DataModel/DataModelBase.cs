// 文件职责：定义 数据模型基类，承担 数据模型 模块中的对应职责。
// 所属模块：Extension / DataModel。

namespace GameFramework
{
    public abstract class DataModelBase : IReference
    {
        [Newtonsoft.Json.JsonIgnore]
        public int Id { get; private set; } = 0;
        [Newtonsoft.Json.JsonIgnore]
        public RefParams Userdata { get; private set; } = null;

        /// <summary>
        /// 每次取用时自动调用
        /// </summary>
        /// <param name="userdata"></param>
        protected virtual void OnCreate(RefParams userdata) { }

        /// <summary>
        /// 当对象回收时自动调用
        /// </summary>
        protected virtual void OnRelease() { }
        internal void Init(int id, RefParams userdata)
        {
            this.Id = id;
            this.Userdata = userdata;
            OnCreate(userdata);
        }
        // 清空当前保存的运行时状态，使对象可安全复用。
        public void Clear()
        {
            OnRelease();
            this.Id = 0;
            ReleaseUserdata();
        }

        // 停止服务并释放其管理的运行时资源。
        internal void Shutdown()
        {
            ReferencePool.Release(this);
        }

        // 释放Userdata及其临时资源。
        protected void ReleaseUserdata()
        {
            if (Userdata != null)
            {
                ReferencePool.Release(Userdata);
                Userdata = null;
            }
        }
    }

}
