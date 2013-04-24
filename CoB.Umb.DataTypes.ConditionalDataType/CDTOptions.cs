using System.ComponentModel;
using uComponents.Core.Shared.PrevalueEditors;
using umbraco.cms.businesslogic.datatype;

namespace Cob.Umb.DataTypes.ConditionalDataType
{
    class CDTOptions : AbstractOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CDTOptions"/> class.
        /// </summary>
        public CDTOptions()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CDTOptions"/> class.
		/// </summary>
		/// <param name="loadDefaults">if set to <c>true</c> [loads defaults].</param>
        public CDTOptions(bool loadDefaults)
			: base(loadDefaults)
		{
		}

        /// <summary>
        /// Gets or sets the Database Storage Type 
        /// </summary>
        [DefaultValue(DBTypes.Ntext)]
        public DBTypes DBType { get; set; }

        /// <summary>
        /// Gets or sets the data type to be used. The default is -87, the Richtext editor.
        /// </summary>
        /// <value>The data type Id.</value>
        [DefaultValue("-87")]
        public string DataTypeId { get; set; }

        /// <summary>
        /// Gets or sets the trigger property type id.
        /// </summary>
        /// <value>The property type Id.</value>
        [DefaultValue("")]
        public string TriggerPropertyTypeId { get; set; }

        /// <summary>
        /// Gets or sets the trigger value to be used.
        /// </summary>
        /// <value>The trigger value.</value>
        [DefaultValue("")]
        public string TriggerValues { get; set; }
    }
}
