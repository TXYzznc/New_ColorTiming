// 文件职责：定义 数据模型Storage基类，承担 数据模型 模块中的对应职责。
// 所属模块：Extension / DataModel。

using GameFramework;
using UnityGameFramework.Runtime;
namespace GameFramework
{
    /// <summary>
    /// 数据模型, 可持久化保存
    /// </summary>
    public abstract class DataModelStorageBase : DataModelBase
    {
        protected string StorageKey { get; private set; } = null;
        // 初始化数据模型Storage基类实例及其核心依赖。
        public DataModelStorageBase()
        {
            StorageKey = this.GetType().FullName;
        }
        // 响应Create回调，并更新本对象状态。
        protected override void OnCreate(RefParams userdata)
        {
            base.OnCreate(userdata);
            Load();
        }

        // 响应Release回调，并更新本对象状态。
        protected override void OnRelease()
        {
            Save();
            base.OnRelease();
        }

        // 执行Load对应的主要流程。
        private void Load()
        {
            if (Id != 0)
            {
                OnInitialDataModel();
                return;
            }
            string dataJson = GF.Setting.GetString(StorageKey, null);
            if (!string.IsNullOrEmpty(dataJson))
            {
                Newtonsoft.Json.JsonConvert.PopulateObject(dataJson, this);
            }
            else
            {
                OnInitialDataModel();
            }
        }
        /// <summary>
        /// 从没有本地储存数据时, 回调此方法, 用于初始化变量
        /// </summary>
        protected virtual void OnInitialDataModel() { }

        public void Save()
        {
            if (Id != 0) return;
            string dataJson = Utility.Json.ToJson(this);
            if (!string.IsNullOrEmpty(dataJson))
            {
                GF.Setting.SetString(StorageKey, dataJson);
            }
        }
    }
}
