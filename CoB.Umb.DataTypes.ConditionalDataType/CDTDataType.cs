using System;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace Cob.Umb.DataTypes.ConditionalDataType
{
    public class CDTDataType : AbstractDataEditor
    {
        #region Private Members

        /// <summary>
        /// The control for the data type.
        /// </summary>
        private CDTDataEditor _dataEditor = new CDTDataEditor();

        /// <summary>
        /// The PreValue Editor for the data type.
        /// </summary>
        private CDTPrevalueEditor _preValueEditor;

        #endregion

        #region AbstractDataEditor members

        /// <summary>
        /// Gets the id of the data type.
        /// </summary>
        /// <value>The id of the data-type.</value>
        public override Guid Id
        {
            get
            {
                return new Guid("B99B7DEF-3729-445A-8692-B7105F743143");
            }
        }

        /// <summary>
        /// Gets the name of the data type.
        /// </summary>
        /// <value>The name of the data type.</value>
        public override string DataTypeName
        {
            get
            {
                return "CoB: Conditional Data Type Wrapper";
            }
        }

        /// <summary>
        /// Gets the prevalue editor.
        /// </summary>
        /// <value>The prevalue editor.</value>
        public override IDataPrevalue PrevalueEditor
        {
            get
            {
                if (this._preValueEditor == null)
                {
                    this._preValueEditor = new CDTPrevalueEditor(this);
                }

                return this._preValueEditor;
            }
        }

        #endregion

        public CDTDataType()
            : base()
        {
            // set the render control as the placeholder
            this.RenderControl = this._dataEditor;
            //this.RenderControl = DataTypeDefinition.GetDataTypeDefinition(-88).DataType.DataEditor.Editor;

            // assign the initialise event for the control
            this._dataEditor.Init += new EventHandler(_dataEditor_Init);

            // assign the save event for the data-type/editor
            this.DataEditorControl.OnSave += new AbstractDataEditorControl.SaveEventHandler(DataEditorControl_OnSave);
        }

        void _dataEditor_Init(object sender, EventArgs e)
        {
            // get the options from the Prevalue Editor.
            this._dataEditor.Options = ((CDTPrevalueEditor)this.PrevalueEditor).GetPreValueOptions<CDTOptions>();

            // set the value of the control
            if (this.Data.Value != null)
            {
                this._dataEditor.Value = this.Data.Value;
            }
            else
            {
                this._dataEditor.Value = string.Empty;
            }
        }

        void DataEditorControl_OnSave(EventArgs e)
        {
            _dataEditor.m_DataType.DataEditor.Save();

            if (_dataEditor.m_DataType.Data.Value == null)
                this.Data.Value = string.Empty;
            else
                this.Data.Value = _dataEditor.m_DataType.Data.Value;
        }
    }
}
