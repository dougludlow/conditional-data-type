using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cob.Umb.DataTypes.ConditionalDataType;
using Cob.Umb.DataTypes.ConditionalDataType.Configuration;
using Cob.Umb.DataTypes.ConditionalDataType.Extensions;
using uComponents.Core.Shared.PrevalueEditors;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.propertytype;
using umbraco.interfaces;

[assembly: WebResource(CDTConstants.PrevalueEditorCssResourcePath, "text/css")]
namespace Cob.Umb.DataTypes.ConditionalDataType
{
    class CDTPrevalueEditor : AbstractJsonPrevalueEditor
    {
        /// <summary>
        /// The debug boolean to be set by the constructor. Corresponds with umbracoDebugMode AppSetting.
        /// </summary>
        private static bool _debug = false;

        /// <summary>
        /// The DropDownList for the database data-type.
        /// </summary>
        private DropDownList _databaseDataTypes;

        /// <summary>
        /// The DropDownList control for the requested data type.
        /// </summary>
        private DropDownList _dataTypes;

        /// <summary>
        /// The DropDownList for the available property types.
        /// </summary>
        private DropDownList _triggerPropertyTypes;

        /// <summary>
        /// The field validator for _triggerPropertyTypes.
        /// </summary>
        private RequiredFieldValidator _triggerPropertyValidator;

        /// <summary>
        /// The Control that will set the trigger value.
        /// </summary>
        private ListBox _triggerValues;

        /// <summary>
        /// The field validator for _triggerValues.
        /// </summary>
        private RequiredFieldValidator _triggerValueValidator;

        /// <summary>
        /// A string that contains the error thrown when umbracoDebugMode is false.
        /// </summary>
        private string _error;

        /// <summary>
        /// The prevalue editor's options.
        /// </summary>
        private CDTOptions _options;

        /// <summary>
        /// The prevalue editor's options.
        /// </summary>
        public CDTOptions Options
        {
            get
            {
                if (_options == null)
                {
                    _options = this.GetPreValueOptions<CDTOptions>();

                    // If the prevalue options are still null, then load the defaults
                    if (_options == null)
                    {
                        _options = new CDTOptions(true);
                    }
                }

                return _options;
            }
        }

        static CDTPrevalueEditor()
        {
            string umbracoDebugMode = ConfigurationManager.AppSettings["umbracoDebugMode"];
            bool.TryParse(umbracoDebugMode, out _debug);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CDTPrevalueEditor"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        public CDTPrevalueEditor(BaseDataType dataType)
            : base(dataType)
        {
            
        }

        /// <summary>
        /// Saves the data-type PreValue options.
        /// </summary>
        public override void Save()
        {
            // set the database data-type
            this.m_DataType.DBType = (DBTypes)Enum.Parse(typeof(DBTypes), this._databaseDataTypes.SelectedValue);

            // Get the selected values and convert to csv.
            var triggerValues = this._triggerValues.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Value);
            string triggerValuesCsv = string.Join(",", triggerValues);

            // set the options
            var options = new CDTOptions()
            {
                DBType = (DBTypes)Enum.Parse(typeof(DBTypes), this._databaseDataTypes.SelectedValue),
                DataTypeId = this._dataTypes.SelectedValue,
                TriggerPropertyTypeId = this._triggerPropertyTypes.SelectedValue,
                TriggerValues = triggerValuesCsv
            };

            // save the options as JSON
            this.SaveAsJson(options);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.EnsureChildControls();
        }

        /// <summary>
        /// Creates child controls for this control
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Set-up child controls
            this._databaseDataTypes = new DropDownList() { ID = "DatabaseDataType" };
            this._dataTypes = new DropDownList() { ID = "DataType" };
            this._triggerPropertyTypes = new DropDownList() { ID = "TriggerPropertyType" };
            this._triggerValues = new ListBox() { ID = "TriggerValue", Visible = false };

            this._triggerPropertyValidator = new RequiredFieldValidator
            {
                ID = "TriggerPropertyValidator",
                CssClass = "cdt-validation",
                ControlToValidate = this._triggerPropertyTypes.ClientID,
                Text = "Choose a trigger property."
            };

            this._triggerValueValidator = new RequiredFieldValidator
            {
                ID = "TriggerValueValidator",
                CssClass = "cdt-validation",
                ControlToValidate = this._triggerValues.ClientID,
                Text = "Choose a one or more values."
            };

            if (!string.IsNullOrEmpty(this.Options.TriggerPropertyTypeId))
            {
                int propertyTypeId = int.Parse(this.Options.TriggerPropertyTypeId);
                FillListBoxWithTriggerValues(ref this._triggerValues, propertyTypeId);
                this._triggerValues.Visible = true;
            }

            // Add the database data-type options
            this._databaseDataTypes.Items.Clear();
            this._databaseDataTypes.Items.Add(DBTypes.Date.ToString());
            this._databaseDataTypes.Items.Add(DBTypes.Integer.ToString());
            this._databaseDataTypes.Items.Add(DBTypes.Ntext.ToString());
            this._databaseDataTypes.Items.Add(DBTypes.Nvarchar.ToString());

            // Get compatible data types from config
            IEnumerable<CompatibleDataType> CompatibleDataTypes = Enumerable.Empty<CompatibleDataType>();
            try
            {
                CompatibleDataTypes = CDTSection.Config.CompatibleDataTypes.Cast<CompatibleDataType>();
            }
            catch (Exception ex)
            {
                if (_debug)
                    throw ex;

                _error = ex.GetType().ToString() + " - " + ex.Message;
            }

            // Add the data type options
            this._dataTypes.Items.Clear();
            this._dataTypes.DataSource = (from d in DataTypeDefinition.GetAll()
                                          where CompatibleDataTypes.Any(x => d.DataType != null && d.DataType.Id == new Guid(x.Guid))
                                          select new DataItem
                                          {
                                              Value = d.Id.ToString(),
                                              Text = d.Text
                                          }).ToList();
            this._dataTypes.DataValueField = "Value";
            this._dataTypes.DataTextField = "Text";
            this._dataTypes.DataBind();

            // Add the property type options, only get datatypes that derive from ListControl or Checkbox.
            List<DataItem> properties = new List<DataItem>();
            properties = (from p in PropertyType.GetAll()
                          where p.DataTypeDefinition.DataType.DataEditor is ListControl ||
                            p.DataTypeDefinition.DataType.DataEditor is CheckBox
                          group p by new { p.DataTypeDefinition.DataType.Id, p.Alias } into g
                          select new DataItem
                          {
                              Value = g.First().Id.ToString(),
                              Text = g.Key.Alias
                          }).OrderBy(x => x.Text).ToList();
            properties.Insert(0, new DataItem { Value = "", Text = "Choose..." });

            this._triggerPropertyTypes.Items.Clear();
            this._triggerPropertyTypes.DataSource = properties;
            this._triggerPropertyTypes.DataValueField = "Value";
            this._triggerPropertyTypes.DataTextField = "Text";
            this._triggerPropertyTypes.DataBind();

            // Register an event hander for when the dropdownlist changes. Allow it to postback when the change is made.
            this._triggerPropertyTypes.SelectedIndexChanged += new EventHandler(_triggerPropertyTypes_SelectedIndexChanged);
            this._triggerPropertyTypes.AutoPostBack = true;
            this._triggerPropertyTypes.EnableViewState = true;

            // add the child controls
            this.Controls.AddPrevalueControls(this._databaseDataTypes, this._dataTypes, this._triggerPropertyTypes, this._triggerValues, this._triggerPropertyValidator, this._triggerValueValidator);
        }

        /// <summary>
        /// Handles when the trigger property changes.
        /// </summary>
        /// <param name="sender">The trigger propery dropdown list.</param>
        /// <param name="e">The EventArgs.</param>
        void _triggerPropertyTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList triggerProperties = sender as DropDownList;
            this._triggerValues = this.FindControl("TriggerValue") as ListBox;
            FillListBoxWithTriggerValues(ref this._triggerValues, int.Parse(triggerProperties.SelectedValue));
            this._triggerValues.Visible = true;
        }

        /// <summary>
        /// Fills a given ListBox with the prevalues of a datatype used by the trigger property.
        /// </summary>
        /// <param name="listbox">The ListBox</param>
        /// <param name="propertyTypeId">The id of the trigger property.</param>
        private void FillListBoxWithTriggerValues(ref ListBox listbox, int propertyTypeId)
        {
            PropertyType propertyType = PropertyType.GetPropertyType(propertyTypeId);
            IDataEditor triggerDataEditor = propertyType.DataTypeDefinition.DataType.DataEditor;
            
            listbox.Items.Clear();
            
            if (triggerDataEditor is ListControl)
            {
                var triggerListControl = (triggerDataEditor as ListControl);
                this.Controls.Add(triggerListControl);

                listbox.SelectionMode = ListSelectionMode.Multiple;
                listbox.DataSource = triggerListControl.Items.Cast<ListItem>().Where(i => i.Value != "").Select(x => new { Value = x.Value, Text = x.Text });
                listbox.DataValueField = "Value";
                listbox.DataTextField = "Text";
                listbox.DataBind();
            }
            else if (triggerDataEditor is CheckBox)
            {
                // True/False
                listbox.Items.Add(new ListItem("True", "true"));
                listbox.Items.Add(new ListItem("False", "false"));
            }
        }
        
        /// <summary>
        /// Renders the controls on the screen.
        /// </summary>
        /// <param name="writer">The HtmlTextWriter</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (this._error != null)
            {
                writer.Write("<div class=\"cdt-error\">");
                writer.AddPrevalueRow("Error:", new Literal() { Text = this._error });
                writer.Write("</div>");
            }

            string description = "This data type shows a property if a condition is met. The property will be hidden by default. When the trigger value is selected, the property will be shown.";
            writer.Write("<div class=\"cdt-description\">");
            writer.AddPrevalueRow("Description:", new Literal() { Text = description });
            writer.Write("</div>");
            writer.AddPrevalueRow("Database Type:", this._databaseDataTypes);
            writer.AddPrevalueRow("Data Type:", this._dataTypes);
            writer.AddPrevalueRow("Trigger Property:", this._triggerPropertyTypes, this._triggerPropertyValidator);

            if (this._triggerValues.Visible)
                writer.AddPrevalueRow("Trigger Value(s):", this._triggerValues, this._triggerValueValidator);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this._databaseDataTypes.SelectedValue = this.m_DataType.DBType.ToString();
            this._dataTypes.SelectedValue = this.Options.DataTypeId;

            if (!Page.IsPostBack)
            {
                this._triggerPropertyTypes.SelectedValue = this.Options.TriggerPropertyTypeId;

                List<string> values = this.Options.TriggerValues.Split(',').ToList();

                var selectedItems = this._triggerValues.Items.Cast<ListItem>().Where(i => values.Contains(i.Value));
                foreach (var item in selectedItems)
                {
                    item.Selected = true;
                }
            }

            // Add Prevalue Editor Css
            string url = Page.ClientScript.GetWebResourceUrl(typeof(CDTPrevalueEditor), CDTConstants.PrevalueEditorCssResourcePath);
            string css = string.Format("<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" />", url);
            Page.ClientScript.RegisterClientScriptBlock(typeof(CDTPrevalueEditor), CDTConstants.PrevalueEditorCssResourcePath, css, false);
        }

        private class DataItem
        {
            public string Value { get; set; }
            public string Text { get; set; }
        }
    }
}
