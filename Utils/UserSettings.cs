using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Utils
{
	public class UserSettings : ApplicationSettingsBase, IDisposable
	{
		private LocalFileSettingsProvider _defaultProvider;

		class SettingInfo
		{
			public object Container { get; set; }
			public PropertyInfo Property { get; set; }
		}

		private Dictionary<string, SettingInfo> _settings = new Dictionary<string, SettingInfo>();
	    private string _dataPath;

	    public SettingsProvider DefaultProvider
		{
			get { return _defaultProvider; }
		}

	    public UserSettings()
			: base("settings")
		{
			// provider
			_defaultProvider = new LocalFileSettingsProvider();
			_defaultProvider.Initialize("LocalFileSettingsProvider", null);
			base.Providers.Add(_defaultProvider);
		}

		public void AttachConfigObject(object configObject, string category = null)
		{
			//find all properties decorated with the UserScopedSetting attribute
			var configType = configObject.GetType();
			var properties = configType
				.GetProperties()
				.Where(p => p.GetCustomAttributes(typeof(UserScopedSettingAttribute), true).Any());

			foreach (var property in properties)
			{
				var name = category != null ? category + "." + property.Name : property.Name;
				CreateSettingProperty(name, property.PropertyType, GetSerializationType(property.PropertyType),
										   string.Empty);
				_settings[name] = new SettingInfo { Container = configObject, Property = property };
			}
		}

		private SettingsSerializeAs GetSerializationType(Type propertyType)
		{
			if (propertyType == typeof(string) || propertyType.IsPrimitive || propertyType.IsEnum)
				return SettingsSerializeAs.String;				
			return SettingsSerializeAs.Xml;
		}

		public void CreateSettingProperty(string name, Type type, SettingsSerializeAs serializeAs, object defaultValue)
		{
			var settingsProperty = Properties[name];
			if (settingsProperty != null)
				return;

			var attributes = new SettingsAttributeDictionary();
			var attribute = new UserScopedSettingAttribute();
			attributes.Add(attribute.TypeId, attribute);

			settingsProperty = new SettingsProperty(
				name, type, DefaultProvider,
				false, defaultValue, serializeAs,
				attributes, false, false);

			Properties.Add(settingsProperty);
		}

		public void CreateSettingPropertyValue(string name, Type type, SettingsSerializeAs serializeAs, object defaultValue)
		{
			CreateSettingProperty(name, type, serializeAs, defaultValue);

			var settingsProperty = PropertyValues[name];
			if (settingsProperty != null)
			{
				return; // already present
			}

			var settingsPropertyValue = new SettingsPropertyValue(Properties[name]);
			PropertyValues.Add(settingsPropertyValue);
		}


		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Save();
			GC.SuppressFinalize(this);
		}

		#endregion

		public void Load()
		{
			CreateSettingProperty("UpgradeChecked", typeof(bool), SettingsSerializeAs.String, false);
			object checkedFlag = this["UpgradeChecked"];
			if (!(bool)checkedFlag)
			{
				Upgrade();
				CreateSettingPropertyValue("UpgradeChecked", typeof(bool), SettingsSerializeAs.String, true);
				this["UpgradeChecked"] = true;
				Save();
			}

			foreach (var settingInfo in _settings)
			{
				settingInfo.Value.Property.SetValue(settingInfo.Value.Container, this[settingInfo.Key], null);
			}
		}

		public override void Save()
		{
			foreach (var settingInfo in _settings)
			{
				this[settingInfo.Key] = settingInfo.Value.Property.GetValue(settingInfo.Value.Container, null);
			}

			base.Save();
		}
	}

}
