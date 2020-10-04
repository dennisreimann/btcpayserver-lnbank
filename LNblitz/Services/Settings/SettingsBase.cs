using System;
using System.Linq;
using System.Reflection;
using LNblitz.Data;
using LNblitz.Data.Models;

namespace LNblitz.Services.Settings
{
    public abstract class SettingsBase
    {
        private readonly string _name;
        private readonly PropertyInfo[] _properties;

        public SettingsBase()
        {
            var type = GetType();
            _name = type.Name;
            _properties = type.GetProperties();
        }

        public virtual void Load(ApplicationDbContext dbContext)
        {
            var settings = dbContext.Settings.Where(w => w.Type == _name).ToList();

            foreach (var propertyInfo in _properties)
            {
                var setting = settings.SingleOrDefault(s => s.Name == propertyInfo.Name);
                if (setting != null)
                {
                    propertyInfo.SetValue(this, Convert.ChangeType(setting.Value, propertyInfo.PropertyType));
                }
            }
        }

        public virtual void Save(ApplicationDbContext dbContext)
        {
            var settings = dbContext.Settings.Where(w => w.Type == _name).ToList();

            foreach (var propertyInfo in _properties)
            {
                object propertyValue = propertyInfo.GetValue(this, null);
                string value = propertyValue?.ToString();

                var setting = settings.SingleOrDefault(s => s.Name == propertyInfo.Name);
                if (setting != null)
                {
                    setting.Value = value;
                }
                else
                {
                    dbContext.Settings.Add(new Setting
                    {
                        Name = propertyInfo.Name,
                        Type = _name,
                        Value = value,
                    });
                }
            }
        }
    }
}
