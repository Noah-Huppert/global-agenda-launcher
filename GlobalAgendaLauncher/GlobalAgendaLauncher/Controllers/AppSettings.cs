using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalAgendaLauncher.Controllers
{
    /// <summary>
    /// Collection of persistent app settings.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Hi-Rez account username.
        /// </summary>
        public Setting Username;

        /// <summary>
        /// Hi-Rez account password.
        /// </summary>
        public Setting Password;

        /// <summary>
        /// Location of GA game binary.
        /// </summary>
        public Setting GABinaryPath;

        public Setting GALaunchOptions;

        /// <summary>
        /// Internal singleton store.
        /// </summary>
        private static AppSettings? instance;

        /// <summary>
        /// Singleton of AppSettings.
        /// </summary>
        public static AppSettings Instance
        {
            get
            {
                if (instance is null)
                {
                    instance = new AppSettings();
                }

                return instance;
            }
        }

        /// <summary>
        /// Initialize AppSettings.
        /// </summary>
        public AppSettings() {
            Username = new Setting("username");
            Password = new Setting("password", true);
            GABinaryPath = new Setting("global_agenda_binary_location");
            GALaunchOptions = new Setting("global_agenda_launch_options");
        }

        /// <summary>
        /// Retrieve all stored settings values.
        /// </summary>
        public async Task Load()
        {
            await Username.GetValue();
            await Password.GetValue();
            await GABinaryPath.GetValue();
            await GALaunchOptions.GetValue();
        }
    }

    /// <summary>
    /// Get and set an individual setting field.
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// Internal store for Name.
        /// </summary>
        private string name;

        /// <summary>
        /// Key in which setting will be stored.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Internal store for Secret.
        /// </summary>
        private bool secret;

        /// <summary>
        /// If true then the setting will be saved in a secure store, otherwise stored in a non-encrypted store.
        /// </summary>
        public bool Secret
        {
            get
            {
                return secret;
            }
        }

        /// <summary>
        /// Setting value.
        /// </summary>
        private string? value;

        /// <summary>
        /// Initialize Setting.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="secret"></param>
        /// <param name="value"></param>
        public Setting(string name, bool secret=false, string? value=null)
        {
            this.name = name;
            this.secret = secret;
            this.value = value;
        }

        /// <summary>
        /// Either retrieve value from store or return already retrieved value.
        /// </summary>
        /// <param name="reload">If true then any previously retrieved value is ignored and setting is retrieved from its store no matter what.</param>
        /// <returns>Setting value.</returns>
        public async Task<string?> GetValue(bool reload=false) { 
            // Check for cached value
            if (value is not null && !reload)
            {
                return value;
            }

            // Otherwise load from store
            if (secret)
            {
                value = await SecureStorage.Default.GetAsync(name);
            } else
            {
                value = Preferences.Default.Get<string?>(name, null);
            }

            return value;
        }

        /// <summary>
        /// Save value to store.
        /// </summary>
        public async Task Save(string? newValue)
        {
            if (newValue == value)
            {
                return;
            }

            value = newValue;

            if (secret)
            {
                await SecureStorage.Default.SetAsync(name, value);
            } else
            {
                Preferences.Default.Set(name, value);
            }
        }

        public void Clear()
        {
            if (secret)
            {
                SecureStorage.Default.Remove(Name);
            } else {
                Preferences.Default.Remove(Name); 
            }
        }
    }
}
