using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace App.UI
{
    public class LoggerController : SingletonMonoBehaviour<LoggerController>
    {
        [SerializeField] private Text logTextPrefab = null;
        [SerializeField] private Transform contentTransform = null;
        [SerializeField] private Scrollbar scroll = null;

        protected override void doAwake()
        {
            this.ObserveEveryValueChanged(_ => contentTransform.childCount).Subscribe(_ =>
            {
                ResetScroll().Forget();
            }).AddTo(gameObject);
        }

        public void AddSystemLog(string message)
        {
            var logText = Instantiate(logTextPrefab, parent: contentTransform);
            logText.color = Color.cyan;
            logText.text = $"[System] : {message}";
        }

        public void AddGameLog(string message)
        {
            var logText = Instantiate(logTextPrefab, parent: contentTransform);
            logText.color = Color.yellow;
            logText.text = $"[Game] : {message}";
        }

        private async UniTask ResetScroll()
        {
            // LogのGameObject生成、ContentFitterの適用に2フレーム掛かるので待たせる
            await UniTask.DelayFrame(2);
            scroll.value = 0;
        }
    }
}