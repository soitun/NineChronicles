using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace NekoyumeEditor
{
    /// <summary>
    /// 원스토어 빌드를 가르는 <c>ONESTORE</c> 스크립팅 디파인 심볼을 켜고 끈다.
    /// </summary>
    /// <remarks>
    /// **기본값은 꺼짐이다.** 켜지 않은 Android 빌드는 지금까지처럼 Unity IAP → Google Play 로 간다.
    /// 켠 빌드만 원스토어 결제 경로를 탄다.
    ///
    /// 심볼은 프로젝트 설정에 저장되므로 <c>ProjectSettings.asset</c> 이 바뀐다. 켠 채로 커밋하면
    /// 팀 전체 Android 빌드가 원스토어가 되니, 로컬에서 확인한 뒤에는 다시 꺼야 한다.
    ///
    /// CI 는 메뉴를 못 누르므로 <see cref="EnableFromCommandLine"/> 를 <c>-executeMethod</c> 로 부른다.
    /// </remarks>
    public static class OneStoreDefine
    {
        private const string Symbol = "ONESTORE";
        private const string MenuEnable = "Build/OneStore/Enable ONESTORE define";
        private const string MenuDisable = "Build/OneStore/Disable ONESTORE define";

        private static readonly NamedBuildTarget Target = NamedBuildTarget.Android;

        [MenuItem(MenuEnable)]
        public static void Enable() => Set(true);

        [MenuItem(MenuEnable, isValidateFunction: true)]
        private static bool ValidateEnable()
        {
            Menu.SetChecked(MenuEnable, IsEnabled());
            return !IsEnabled();
        }

        [MenuItem(MenuDisable)]
        public static void Disable() => Set(false);

        [MenuItem(MenuDisable, isValidateFunction: true)]
        private static bool ValidateDisable()
        {
            Menu.SetChecked(MenuDisable, !IsEnabled());
            return IsEnabled();
        }

        /// <summary>CI 용 진입점. <c>-executeMethod NekoyumeEditor.OneStoreDefine.EnableFromCommandLine</c></summary>
        public static void EnableFromCommandLine() => Set(true);

        public static bool IsEnabled() => Current().Contains(Symbol);

        private static string[] Current()
        {
            return PlayerSettings.GetScriptingDefineSymbols(Target)
                .Split(';')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
        }

        private static void Set(bool enabled)
        {
            var symbols = Current().ToList();
            if (enabled)
            {
                if (symbols.Contains(Symbol))
                {
                    return;
                }

                symbols.Add(Symbol);
            }
            else if (!symbols.Remove(Symbol))
            {
                return;
            }

            PlayerSettings.SetScriptingDefineSymbols(Target, string.Join(";", symbols));
            Debug.Log($"[OneStoreDefine] Android ONESTORE = {enabled}. symbols: {string.Join(";", symbols)}");
        }
    }
}
