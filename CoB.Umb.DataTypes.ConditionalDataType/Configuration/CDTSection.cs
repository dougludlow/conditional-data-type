using System;
using System.Configuration;
using System.IO;
using System.Web;

namespace Cob.Umb.DataTypes.ConditionalDataType.Configuration
{
    public class CDTSection : ConfigurationSection
    {
        #region Static Members

        /// <summary>
        /// An object to temporarily lock writing to the database/filesystem.
        /// </summary>
        private static readonly object m_Locker = new object();
        
        /// <summary>
        /// The Conditional Data Type configuration.
        /// </summary>
        private static System.Configuration.Configuration _config;

        /// <summary>
        /// Loads the configruation once.
        /// </summary>
        public static CDTSection Config
        {
            get
            {
                if (_config == null)
                {
                    // Write config file out if it doesn't exist.
                    string filepath = HttpContext.Current.Server.MapPath(CDTConstants.ConfigFilePath);
                    if (!File.Exists(filepath))
                    {
                        lock (m_Locker)
                        {
                            if (!File.Exists(filepath))
                            {
                                using (var writer = new StreamWriter(File.Create(filepath)))
                                {
                                    writer.Write(CDTConfiguration.ConditionalDataType);
                                }
                            }
                        }
                    }

                    var configFileMap = new ExeConfigurationFileMap
                    {
                        ExeConfigFilename = filepath
                    };

                    _config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                }
                return _config.GetSection("conditionalDataType") as CDTSection;
            }
        }

        #endregion

        [ConfigurationProperty("compatibleDataTypes")]
        [ConfigurationCollection(typeof(CompatibleDataTypeCollection))]
        public CompatibleDataTypeCollection CompatibleDataTypes
        {
            get
            {
                return (CompatibleDataTypeCollection)this["compatibleDataTypes"];
            }
            set
            {
                this["compatibleDataTypes"] = value; 
            }
        }
    }

    /// <summary>
    /// Collection for Compatible Data Type elements.
    /// </summary>
    public class CompatibleDataTypeCollection : ConfigurationElementCollection
    {
        public CompatibleDataType this[int index]
        {
            get { return (CompatibleDataType)BaseGet(index); }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CompatibleDataType();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CompatibleDataType)element).Guid;
        }
    }

    /// <summary>
    /// The Compatible Data Type element.
    /// </summary>
    public class CompatibleDataType : ConfigurationElement
    {
        [ConfigurationProperty("guid")]
        public String Guid
        {
            get
            {
                return (this["guid"] as string);
            }
            set
            {
                this["guid"] = value;
            }
        }
    }
}
