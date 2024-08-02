using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace AssetsManagement
{
    public class AssetsManager
    {
        private static AssetsManager m_instance;

        public static UniTask<AssetsManager> Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new AssetsManager();
                    return m_instance.InitializeAsync();
                }

                return UniTask.FromResult(m_instance);
            }
        }

        private readonly Dictionary<string, AsyncOperationHandle> m_assetRequests;

        private AssetsManager()
        {
            m_assetRequests = new Dictionary<string, AsyncOperationHandle>();
        }

        private async UniTask<AssetsManager> InitializeAsync()
        {
            await Addressables.InitializeAsync().ToUniTask();
            return this;
        }

        public void Dispose()
        {
            m_instance = null;
        }

        public async UniTask<TAsset> Load<TAsset>(string key) where TAsset : class
        {
            if (!m_assetRequests.TryGetValue(key, value: out var handle))
            {
                handle = Addressables.LoadAssetAsync<TAsset>(key);
                m_assetRequests.Add(key, handle);
            }

            await handle.ToUniTask();

            return handle.Result as TAsset;
        }

        public UniTask<TAsset> Load<TAsset>(AssetReference assetReference) where TAsset : class
        {
            return Load<TAsset>(assetReference.AssetGUID);
        }

        public async UniTask<TAsset[]> LoadAll<TAsset>(IEnumerable<string> keys) where TAsset : class
        {
            var tasks = new List<UniTask<TAsset>>();

            foreach (var key in keys)
            {
                tasks.Add(Load<TAsset>(key));
            }

            return await UniTask.WhenAll(tasks);
        }

        public void CleanUp()
        {
            foreach (var assetRequest in m_assetRequests)
            {
                Addressables.Release(assetRequest.Value);
            }

            m_assetRequests.Clear();
        }
    }
}