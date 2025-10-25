using System;
using System.Windows.Forms;
using CrystalTable.Data;

namespace CrystalTable.Dialogs
{
    public partial class CreateMapDialog : Form
    {
        private TextBox txtBatchName;
        private TextBox txtNotes;
        private ComboBox cmbTemplates;
        private Button btnOK;
        private Button btnCancel;
        private Label lblBatchName;
        private Label lblNotes;
        private Label lblTemplate;

        public string BatchName => txtBatchName.Text.Trim();
        public string Notes => txtNotes.Text.Trim();
        public string SelectedTemplate => cmbTemplates.SelectedItem?.ToString();
        public bool IsNewTemplate => cmbTemplates.SelectedIndex == 0;

        public CreateMapDialog(WaferTemplateManager templateManager, string defaultBatchName = null)
        {
            InitializeComponent();
            LoadTemplates(templateManager);
            
            if (!string.IsNullOrEmpty(defaultBatchName))
            {
                txtBatchName.Text = defaultBatchName;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Создание новой карты";
            this.ClientSize = new System.Drawing.Size(450, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Batch Name
            lblBatchName = new Label
            {
                Text = "Название партии:",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(120, 20)
            };

            txtBatchName = new TextBox
            {
                Location = new System.Drawing.Point(150, 20),
                Size = new System.Drawing.Size(280, 25)
            };

            // Notes
            lblNotes = new Label
            {
                Text = "Примечание:",
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(120, 20)
            };

            txtNotes = new TextBox
            {
                Location = new System.Drawing.Point(150, 60),
                Size = new System.Drawing.Size(280, 60),
                Multiline = true
            };

            // Template
            lblTemplate = new Label
            {
                Text = "Шаблон:",
                Location = new System.Drawing.Point(20, 140),
                Size = new System.Drawing.Size(120, 20)
            };

            cmbTemplates = new ComboBox
            {
                Location = new System.Drawing.Point(150, 140),
                Size = new System.Drawing.Size(280, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Buttons
            btnOK = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(250, 190),
                Size = new System.Drawing.Size(90, 30)
            };

            btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(350, 190),
                Size = new System.Drawing.Size(90, 30)
            };

            this.Controls.AddRange(new Control[]
            {
                lblBatchName, txtBatchName,
                lblNotes, txtNotes,
                lblTemplate, cmbTemplates,
                btnOK, btnCancel
            });

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void LoadTemplates(WaferTemplateManager templateManager)
        {
            cmbTemplates.Items.Clear();
            cmbTemplates.Items.Add("< Создать новый шаблон >");

            var templates = templateManager.LoadAllTemplates();
            foreach (var template in templates)
            {
                cmbTemplates.Items.Add(template.TemplateName);
            }

            cmbTemplates.SelectedIndex = 0;
        }
    }
}
