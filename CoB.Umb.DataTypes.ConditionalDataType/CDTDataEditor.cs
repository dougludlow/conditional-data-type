using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cob.Umb.DataTypes.ConditionalDataType;
using Cob.Umb.DataTypes.ConditionalDataType.Extensions;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.propertytype;
using umbraco.interfaces;
using umbraco.uicontrols;

[assembly: WebResource(CDTConstants.DataEditorJavaScriptResourcePath, "text/javascript")]
namespace Cob.Umb.DataTypes.ConditionalDataType
{
    class CDTDataEditor : PlaceHolder
    {
        /// <summary>
        /// The saved options.
        /// </summary>
        public CDTOptions Options { get; set; }
        
        /// <summary>
        /// The data type that will be used. Chosen from the prevalue editor.
        /// </summary>
        public IDataType m_DataType { get; set; }

        /// <summary>
        /// The control used by the selected data type.
        /// </summary>
        public Control m_Control { get; set; }

        /// <summary>
        /// The value of the data type, passed to the control on init.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Initialize the control, make sure children are created
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.EnsureChildControls();

            try
            {
                // Get data type:
                int dataTypeId = int.Parse(this.Options.DataTypeId);

                try
                {
                    m_DataType = DataTypeDefinition.GetDataTypeDefinition(dataTypeId).DataType;
                }
                catch (ArgumentException)
                {
                    throw new Exception("An error occurred when attempting to load the Data Editor. Ensure that the data type exists and a Render Control is selected.");
                }

                try
                {
                    // Set the value:
                    m_DataType.Data.Value = this.Value;
                }
                catch (umbraco.DataLayer.SqlHelperException)
                {
                    throw new Exception("Change the Database data type to a more appropriate type for the control.");
                }

                // Add data type to placeholder:
                m_Control = m_DataType.DataEditor as Control;
                this.Controls.Add(m_Control);

                // Add script to placeholder:
                if (!string.IsNullOrEmpty(this.Options.TriggerPropertyTypeId))
                {
                    int propertyTypeId = int.Parse(this.Options.TriggerPropertyTypeId);

                    PropertyType propertyType = PropertyType.GetPropertyType(propertyTypeId);
                    IDataEditor triggerDataEditor = propertyType.DataTypeDefinition.DataType.DataEditor;
                    string triggerId = string.Format("#body_prop_{0}", propertyType.Alias);
                    string type = "";

                    if (triggerDataEditor is CheckBox)
                        type = "CheckBox";
                    else if (triggerDataEditor is RadioButtonList)
                        type = "RadioButtonList";
                    else if (triggerDataEditor is CheckBoxList)
                        type = "CheckBoxList";
                    else if (triggerDataEditor is DropDownList)
                        type = "DropDownList";

                    string script = string.Format("$(function(){{cdtInit({{ id: '{0}', showLabel: {1}, triggerId: '{2}', triggerValue: '{3}', type: '{4}' }})}});", 
                        "#" + m_Control.ClientID,
                        m_DataType.DataEditor.ShowLabel.ToString().ToLower(),
                        triggerId,
                        this.Options.TriggerValues,
                        type);

                    this.Controls.Add(new Literal { Text = String.Format("<script type=\"text/javascript\">{0}</script>", script) });
                }
                
            }
            catch (Exception ex)
            {
                this.Controls.Add(new Literal { Text = string.Format("<div style=\"color: #f00;\">{0}</div>", ex.Message) });
                //this.Controls.Add(new Literal { Text = string.Format("<div style=\"color: #f00;\">{0}: {1}</div>", ex.GetType().ToString(), ex.Message) });
            }
        }

        /// <summary>
        /// Add the resources (sytles/scripts)
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (m_Control != null)
            {
                if (m_Control.GetType() == typeof(umbraco.editorControls.tinyMCE3.TinyMCE))
                {
                    TabView tabView = Page.FindControl("TabView1", true) as TabView;
                    TabPage tabPage = this.FindAncestor(c => tabView.Controls.Cast<Control>().Any(t => t.ID == c.ID)) as TabPage;
                    tabPage.Menu.NewElement("div", "umbTinymceMenu_" + m_Control.ClientID, "tinymceMenuBar", 0);
                }
            }

            Page.ClientScript.RegisterClientScriptResource(typeof(CDTDataEditor), CDTConstants.DataEditorJavaScriptResourcePath);
        }
    }
}
