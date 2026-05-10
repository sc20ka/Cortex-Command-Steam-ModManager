using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CortexCommandModManager.Services
{
    public class LocalizationService : INotifyPropertyChanged
    {
        private static LocalizationService _instance;
        public static LocalizationService Instance => _instance ??= new LocalizationService();

        private string _currentLanguage = "en";
        
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    OnPropertyChanged("Item[]"); // Notifies all indexer bindings
                }
            }
        }

        private readonly Dictionary<string, Dictionary<string, string>> _translations = new()
        {
            { "ru", new Dictionary<string, string> {
                { "UpdateList", "Обновить список модов" },
                { "EnableAll", "Включить всё" },
                { "DisableAll", "Выключить всё" },
                { "Settings", "Настройки" },
                { "SteamPath", "Путь к Steam" },
                { "GamePath", "Путь к игре" },
                { "ResetDefault", "Сбросить по дефолту" },
                { "ClearConfig", "Очистить конфиг" },
                { "Close", "Закрыть" },
                { "Installed", "Установлен" },
                { "SearchProgress", "Поиск игры и модов..." },
                { "FoundMods", "Найдено модов: {0}" },
                { "InstallProgress", "Установка {0}..." },
                { "UninstallProgress", "Удаление {0}..." },
                { "Error", "Ошибка: {0}" },
                { "GameNotFound", "Игра Cortex Command не найдена." },
                { "InstallSuccess", "{0} установлен." },
                { "UninstallSuccess", "{0} удален." },
                { "PathNotFound", "Путь не найден" }
            }},
            { "en", new Dictionary<string, string> {
                { "UpdateList", "Update mod list" },
                { "EnableAll", "Enable all" },
                { "DisableAll", "Disable all" },
                { "Settings", "Settings" },
                { "SteamPath", "Steam path" },
                { "GamePath", "Game path" },
                { "ResetDefault", "Reset to default" },
                { "ClearConfig", "Clear config" },
                { "Close", "Close" },
                { "Installed", "Installed" },
                { "SearchProgress", "Searching for game and mods..." },
                { "FoundMods", "Found mods: {0}" },
                { "InstallProgress", "Installing {0}..." },
                { "UninstallProgress", "Uninstalling {0}..." },
                { "Error", "Error: {0}" },
                { "GameNotFound", "Cortex Command game not found." },
                { "InstallSuccess", "{0} installed." },
                { "UninstallSuccess", "{0} uninstalled." },
                { "PathNotFound", "Path not found" }
            }},
            { "ar", new Dictionary<string, string> {
                { "UpdateList", "تحديث قائمة التعديلات" },
                { "EnableAll", "تمكين الكل" },
                { "DisableAll", "تعطيل الكل" },
                { "Settings", "إعدادات" },
                { "SteamPath", "مسار Steam" },
                { "GamePath", "مسار اللعبة" },
                { "ResetDefault", "إعادة تعيين للافتراضي" },
                { "ClearConfig", "مسح التكوين" },
                { "Close", "إغلاق" },
                { "Installed", "مُثبت" },
                { "SearchProgress", "البحث عن اللعبة والتعديلات..." },
                { "FoundMods", "تم العثور على تعديلات: {0}" },
                { "InstallProgress", "تثبيت {0}..." },
                { "UninstallProgress", "إزالة {0}..." },
                { "Error", "خطأ: {0}" },
                { "GameNotFound", "لم يتم العثور على اللعبة." },
                { "InstallSuccess", "تم تثبيت {0}." },
                { "UninstallSuccess", "تمت إزالة {0}." },
                { "PathNotFound", "المسار غير موجود" }
            }},
            { "zh", new Dictionary<string, string> {
                { "UpdateList", "更新模组列表" },
                { "EnableAll", "全部启用" },
                { "DisableAll", "全部禁用" },
                { "Settings", "设置" },
                { "SteamPath", "Steam 路径" },
                { "GamePath", "游戏路径" },
                { "ResetDefault", "恢复默认" },
                { "ClearConfig", "清除配置" },
                { "Close", "关闭" },
                { "Installed", "已安装" },
                { "SearchProgress", "正在搜索游戏和模组..." },
                { "FoundMods", "找到模组: {0}" },
                { "InstallProgress", "正在安装 {0}..." },
                { "UninstallProgress", "正在卸载 {0}..." },
                { "Error", "错误: {0}" },
                { "GameNotFound", "未找到游戏。" },
                { "InstallSuccess", "{0} 已安装。" },
                { "UninstallSuccess", "{0} 已卸载。" },
                { "PathNotFound", "找不到路径" }
            }},
            { "jp", new Dictionary<string, string> {
                { "UpdateList", "MODリストを更新" },
                { "EnableAll", "すべて有効にする" },
                { "DisableAll", "すべて無効にする" },
                { "Settings", "設定" },
                { "SteamPath", "Steamパス" },
                { "GamePath", "ゲームパス" },
                { "ResetDefault", "デフォルトにリセット" },
                { "ClearConfig", "設定をクリア" },
                { "Close", "閉じる" },
                { "Installed", "インストール済み" },
                { "SearchProgress", "ゲームとMODを検索中..." },
                { "FoundMods", "見つかったMOD: {0}" },
                { "InstallProgress", "{0}をインストール中..." },
                { "UninstallProgress", "{0}をアンインストール中..." },
                { "Error", "エラー: {0}" },
                { "GameNotFound", "ゲームが見つかりません。" },
                { "InstallSuccess", "{0}がインストールされました。" },
                { "UninstallSuccess", "{0}がアンインストールされました。" },
                { "PathNotFound", "パスが見つかりません" }
            }}
        };

        public string this[string key]
        {
            get
            {
                if (_translations.TryGetValue(_currentLanguage, out var langDict) && langDict.TryGetValue(key, out var val))
                {
                    return val;
                }
                // Fallback to English
                if (_translations["en"].TryGetValue(key, out val))
                {
                    return val;
                }
                return key;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
